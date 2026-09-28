using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class UtilityBillUploadPage : BasePage
{
    public UtilityBillUploadPage(IPage page) : base(page) { }

    private ILocator PageTitle => Page.Locator("//h1[@id='invoice-list-title']");
    private ILocator Message => Page.Locator("//p[@class='page-header__lead']");
    private ILocator ImportPDFBtn => Page.Locator("//a[normalize-space()='Import PDF']");
    private ILocator Grid => Page.Locator("//table[contains(@class,'table')]");
    private ILocator PreviousBtn => Page.Locator("//button[normalize-space()='Previous']");
    private ILocator NextBtn => Page.Locator("//button[normalize-space()='Next']");

    public async Task OpenAsync()
    {
        await Page.GotoAsync(Config.BaseUrl + "utility-bills");
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await PageTitle.WaitForAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
    }

    public async Task<string> GetTitleAsync()
    {
        await PageTitle.WaitForAsync();
        return (await PageTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessageAsync()
    {
        await Message.WaitForAsync();
        return (await Message.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsImportPDFBtnVisibleAsync() => ImportPDFBtn.IsVisibleAsync();
    public Task<bool> IsGridVisibleAsync() => Grid.IsVisibleAsync();

    public async Task<ImportPage> ClickImportPDFBtnAsync()
    {
        await ImportPDFBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/utility-bills/import", StringComparison.OrdinalIgnoreCase));
        var importPage = new ImportPage(Page);
        await importPage.WaitForLoadedAsync();
        return importPage;
    }

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await Grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header))
            .ToList();
    }

    public async Task<UtilityBillGridRow?> GetUtilityBillGridRowAsync(string company, string importDate)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        while (true)
        {
            var matchIndex = await Grid.EvaluateAsync<int?>(
                """
                (table, args) => {
                  const company = args.company;
                  const importDate = args.importDate;
                  const rows = [...table.querySelectorAll('tbody tr')];
                  for (let i = 0; i < rows.length; i++) {
                    const cells = [...rows[i].querySelectorAll('td')]
                      .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').trim());
                    if (cells.length < 5) continue;
                    if (!cells[1] && !cells[4]) continue;
                    if (cells[1] === company && cells[4] === importDate) return i;
                  }
                  return null;
                }
                """,
                new { company, importDate });

            if (matchIndex is not null)
            {
                var cells = await Grid.Locator("tbody tr").Nth(matchIndex.Value).EvaluateAsync<string[]>(
                    """
                    tr => [...tr.querySelectorAll('td')]
                      .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').trim())
                    """);
                return new UtilityBillGridRow
                {
                    Project = cells.ElementAtOrDefault(0) ?? string.Empty,
                    Company = cells.ElementAtOrDefault(1) ?? string.Empty,
                    Date = cells.ElementAtOrDefault(2) ?? string.Empty,
                    Status = cells.ElementAtOrDefault(3) ?? string.Empty,
                    ImportDate = cells.ElementAtOrDefault(4) ?? string.Empty,
                    ApproveRejectDate = cells.ElementAtOrDefault(5) ?? string.Empty,
                    Source = cells.ElementAtOrDefault(6) ?? string.Empty,
                };
            }

            if (!await CanGoToAdjacentPageAsync(NextBtn))
                break;

            var showingBefore = await TryGetShowingRangeAsync();
            try
            {
                await NextBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000, Force = true });
            }
            catch (TimeoutException)
            {
                break;
            }

            await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await WaitForGridDataAsync();

            var showingAfter = await TryGetShowingRangeAsync();
            if (showingBefore is not null
                && showingAfter is not null
                && showingAfter.Value.Start == showingBefore.Value.Start)
            {
                break;
            }
        }

        return null;
    }

    private async Task WaitForGridDataAsync()
    {
        try
        {
            await Page.WaitForFunctionAsync(
                """
                () => [...document.querySelectorAll('table.table tbody tr')]
                  .some(tr => {
                    const cells = tr.querySelectorAll('td');
                    return cells.length > 1 && (cells[1].innerText || '').trim().length > 0;
                  })
                """,
                null,
                new PageWaitForFunctionOptions { Timeout = 15_000 });
        }
        catch (TimeoutException)
        {
            // Grid may legitimately be empty.
        }
    }

    private async Task GoToFirstGridPageAsync()
    {
        while (await CanGoToAdjacentPageAsync(PreviousBtn))
        {
            try
            {
                await PreviousBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000, Force = true });
            }
            catch (TimeoutException)
            {
                break;
            }

            await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        }
    }

    private async Task<bool> CanGoToAdjacentPageAsync(ILocator button)
    {
        if (await button.CountAsync() == 0 || !await button.IsVisibleAsync())
            return false;

        var isNext = (await button.InnerTextAsync()).Contains("Next", StringComparison.OrdinalIgnoreCase);
        var showing = await TryGetShowingRangeAsync();
        if (showing is not null)
        {
            return isNext
                ? showing.Value.End < showing.Value.Total
                : showing.Value.Start > 1;
        }

        return await button.EvaluateAsync<bool>(
            """
            el => !(
              el.disabled
              || el.hasAttribute('disabled')
              || el.getAttribute('aria-disabled') === 'true'
              || el.classList.contains('disabled')
            )
            """);
    }

    private async Task<(int Start, int End, int Total)?> TryGetShowingRangeAsync()
    {
        var showing = Page.Locator("//*[contains(normalize-space(.),'Showing')]").First;
        if (await showing.CountAsync() == 0)
            return null;

        var text = (await showing.InnerTextAsync()).Replace('\u00A0', ' ').Replace('–', '-').Replace('—', '-');
        var match = System.Text.RegularExpressions.Regex.Match(
            text,
            @"Showing\s+(\d+)\s*-\s*(\d+)\s+of\s+(\d+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success)
            return null;

        return (
            int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[3].Value));
    }
}
