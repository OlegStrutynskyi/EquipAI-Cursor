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

    public async Task<ViewInvoicePage> ClickViewBtnAsync(string projectName)
    {
        await ClickRowActionAsync(projectName, "View");
        var viewInvoicePage = new ViewInvoicePage(Page);
        await viewInvoicePage.GetTitleAsync();
        return viewInvoicePage;
    }

    public async Task<ImportPage> ClickEditBtnAsync(string projectName)
    {
        await ClickRowActionAsync(projectName, "Edit");
        return await OpenReviewPdfImportPageAsync();
    }

    public async Task<UtilityBillGridRow?> FindGridRowAsync(
        string billDate,
        string? project = null,
        string? company = null,
        IReadOnlyCollection<string>? importDates = null,
        string? status = null)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        while (true)
        {
            foreach (var row in await ReadGridRowsOnCurrentPageAsync())
            {
                if (!row.Date.Equals(billDate, StringComparison.Ordinal))
                    continue;
                if (project is not null && !row.Project.Equals(project, StringComparison.Ordinal))
                    continue;
                if (company is not null && !row.Company.Equals(company, StringComparison.Ordinal))
                    continue;
                if (importDates is not null && !importDates.Contains(row.ImportDate, StringComparer.Ordinal))
                    continue;
                if (status is not null && !row.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    continue;

                return row;
            }

            if (!await TryGoToNextStablePageAsync())
                return null;
        }
    }

    public async Task<ViewInvoicePage> ClickViewBtnForRowAsync(string billDate, string project, string company, string status)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        while (true)
        {
            var row = Grid.Locator("tbody tr")
                .Filter(new LocatorFilterOptions { HasText = billDate })
                .Filter(new LocatorFilterOptions { HasText = project })
                .Filter(new LocatorFilterOptions { HasText = company })
                .Filter(new LocatorFilterOptions { HasText = status });
            if (await row.CountAsync() > 0)
            {
                var viewBtn = row.First.Locator("button, a").Filter(new LocatorFilterOptions { HasTextString = "View" }).First;
                await viewBtn.ClickAsync();

                var viewInvoicePage = new ViewInvoicePage(Page);
                await viewInvoicePage.GetTitleAsync();
                return viewInvoicePage;
            }

            if (!await TryGoToNextStablePageAsync())
                break;
        }

        throw new InvalidOperationException(
            $"Utility bill with Bill Date '{billDate}', Project '{project}', Company '{company}', Status '{status}' was not found in the grid.");
    }

    public async Task<ImportPage> ClickEditBtnForRowAsync(string billDate, string importDate)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        while (true)
        {
            var row = Grid.Locator("tbody tr")
                .Filter(new LocatorFilterOptions { HasText = billDate })
                .Filter(new LocatorFilterOptions { HasText = importDate });
            if (await row.CountAsync() > 0)
            {
                var editBtn = row.First.Locator("button, a").Filter(new LocatorFilterOptions { HasTextString = "Edit" }).First;
                await editBtn.ClickAsync();
                return await OpenReviewPdfImportPageAsync();
            }

            if (!await TryGoToNextStablePageAsync())
                break;
        }

        throw new InvalidOperationException(
            $"Utility bill with Bill Date '{billDate}' and Import Date '{importDate}' was not found in the grid.");
    }

    private async Task<ImportPage> OpenReviewPdfImportPageAsync()
    {
        var reviewTitle = Page.Locator("h1").Filter(new LocatorFilterOptions { HasTextString = "Review PDF Import" });
        await reviewTitle.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000,
        });
        return new ImportPage(Page);
    }

    private async Task ClickRowActionAsync(string projectName, string actionName)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        while (true)
        {
            var matchIndex = await Grid.EvaluateAsync<int?>(
                """
                (table, args) => {
                  const textOf = td => {
                    const title = (td.getAttribute('title') || '').replace(/\u00a0/g, ' ').trim();
                    const nested = td.querySelector('[title]');
                    const nestedTitle = nested ? (nested.getAttribute('title') || '').replace(/\u00a0/g, ' ').trim() : '';
                    const text = (td.textContent || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim();
                    const inner = (td.innerText || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim();
                    let best = inner;
                    for (const value of [title, nestedTitle, text]) {
                      if (value.length > best.length) best = value;
                    }
                    return best;
                  };
                  const rows = [...table.querySelectorAll('tbody tr')];
                  for (let i = 0; i < rows.length; i++) {
                    if (rows[i].querySelector('.table__skeleton-bar')) continue;
                    const cell = rows[i].querySelector(':scope > td');
                    if (!cell || textOf(cell) !== args.projectName) continue;
                    const hasAction = [...rows[i].querySelectorAll('button, a')]
                      .some(el => (el.innerText || '').replace(/\s+/g, ' ').trim() === args.actionName);
                    if (hasAction) return i;
                  }
                  return null;
                }
                """,
                new { projectName, actionName });

            if (matchIndex is not null)
            {
                var row = Grid.Locator("tbody tr").Nth(matchIndex.Value);
                var actionBtn = row.Locator("button, a").Filter(new LocatorFilterOptions { HasTextString = actionName }).First;
                await actionBtn.ClickAsync();
                return;
            }

            if (!await TryGoToNextStablePageAsync())
                break;
        }

        throw new InvalidOperationException($"Utility bill for project '{projectName}' with '{actionName}' was not found in the grid.");
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

    public async Task ClickGridColumnAsync(string columnName)
    {
        var header = Grid.Locator("thead th").Filter(new LocatorFilterOptions { HasTextString = columnName }).First;
        await header.ScrollIntoViewIfNeededAsync();
        var ariaBefore = await header.GetAttributeAsync("aria-sort");
        var fingerprintBefore = await GetPageFingerprintAsync();
        var showingBefore = await TryGetShowingRangeAsync();
        await header.Locator("button.table__sort").ClickAsync();

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            var ariaAfter = await header.GetAttributeAsync("aria-sort");
            if (string.Equals(ariaAfter, ariaBefore, StringComparison.Ordinal))
            {
                await Task.Delay(100);
                continue;
            }

            var refreshDeadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < refreshDeadline)
            {
                var showingAfter = await TryGetShowingRangeAsync();
                var onFirstPage = showingAfter is null || showingAfter.Value.Start == 1;
                var fingerprintAfter = await GetPageFingerprintAsync();
                if (onFirstPage
                    && fingerprintAfter.Length > 0
                    && !string.Equals(fingerprintAfter, fingerprintBefore, StringComparison.Ordinal))
                {
                    await WaitForGridSettledAsync();
                    var settled = await GetPageFingerprintAsync();
                    if (!string.Equals(settled, fingerprintBefore, StringComparison.Ordinal))
                        return;
                }

                await Task.Delay(100);
            }

            if (showingBefore is null || showingBefore.Value.Start == 1)
            {
                await WaitForGridSettledAsync();
                var settled = await GetPageFingerprintAsync();
                if (!string.Equals(settled, fingerprintBefore, StringComparison.Ordinal))
                    return;
            }

            break;
        }

        await WaitForGridSettledAsync();
    }

    public async Task<IReadOnlyList<UtilityBillGridRow>> GetCurrentPageGridRowsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return await ReadGridRowsOnCurrentPageAsync();
    }

    public async Task<IReadOnlyList<UtilityBillGridRow>> GetAllGridRowsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        var results = new List<UtilityBillGridRow>();
        while (true)
        {
            results.AddRange(await ReadGridRowsOnCurrentPageAsync());
            if (!await TryGoToNextStablePageAsync())
                break;
        }

        return results;
    }

    private async Task<IReadOnlyList<UtilityBillGridRow>> ReadGridRowsOnCurrentPageAsync()
    {
        var rows = await Grid.EvaluateAsync<string[][]>(
            """
            table => {
              const textOf = td => {
                const title = (td.getAttribute('title') || '').replace(/\u00a0/g, ' ').trim();
                const nested = td.querySelector('[title]');
                const nestedTitle = nested ? (nested.getAttribute('title') || '').replace(/\u00a0/g, ' ').trim() : '';
                const text = (td.textContent || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim();
                const inner = (td.innerText || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim();
                let best = inner;
                for (const value of [title, nestedTitle, text]) {
                  if (value.length > best.length) best = value;
                }
                return best;
              };
              return [...table.querySelectorAll('tbody tr')]
                .filter(tr => !tr.querySelector('.table__skeleton-bar'))
                .map(tr => [...tr.querySelectorAll(':scope > td')].slice(0, 7).map(textOf))
                .filter(cells => cells.some(cell => cell.length > 0));
            }
            """);

        return rows
            .Where(cells => cells.Length >= 7)
            .Select(cells => new UtilityBillGridRow
            {
                Project = cells[0],
                Company = cells[1],
                Date = cells[2],
                Status = cells[3],
                ImportDate = cells[4],
                ApproveRejectDate = cells[5],
                Source = cells[6],
            })
            .ToList();
    }

    private async Task<string> GetPageFingerprintAsync()
    {
        var rows = await ReadGridRowsOnCurrentPageAsync();
        return string.Join("||", rows.Take(3).Select(row => $"{row.Company}|{row.Date}|{row.ImportDate}"));
    }

    private async Task WaitForGridSettledAsync()
    {
        string? previous = null;
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var snapshot = await GetPageFingerprintAsync();
            if (previous is not null && snapshot.Length > 0 && snapshot == previous)
                return;

            previous = snapshot;
            await Task.Delay(200);
        }
    }

    private async Task<bool> TryGoToNextStablePageAsync()
    {
        var showingBefore = await TryGetShowingRangeAsync();
        if (showingBefore is not null && showingBefore.Value.End >= showingBefore.Value.Total)
            return false;

        var nextBtn = Page.Locator("//nav[contains(@class,'pagination')]//button[@aria-label='Next page' or normalize-space()='Next']");
        if (await nextBtn.CountAsync() == 0 || !await IsPagerEnabledAsync(nextBtn.First))
            return false;

        var pageKeyBefore = await GetPageKeyAsync();
        try
        {
            await nextBtn.First.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
        }
        catch (TimeoutException)
        {
            return false;
        }

        var deadline = DateTime.UtcNow.AddSeconds(12);
        while (DateTime.UtcNow < deadline)
        {
            if (await Grid.Locator(".table__skeleton-bar").CountAsync() > 0)
            {
                await Task.Delay(100);
                continue;
            }

            var showingAfter = await TryGetShowingRangeAsync();
            var pageKeyAfter = await GetPageKeyAsync();
            var contentMoved = pageKeyAfter.Length > 0
                && !string.Equals(pageKeyAfter, pageKeyBefore, StringComparison.Ordinal);
            var showingMoved = showingBefore is not null
                && showingAfter is not null
                && showingAfter.Value.Start > showingBefore.Value.Start;
            if (showingBefore is not null ? showingMoved && contentMoved : contentMoved)
            {
                var confirm = await GetPageKeyAsync();
                if (string.Equals(confirm, pageKeyAfter, StringComparison.Ordinal))
                    return true;
            }

            await Task.Delay(150);
        }

        return false;
    }

    private async Task<string> GetPageKeyAsync()
    {
        var rows = await ReadGridRowsOnCurrentPageAsync();
        return string.Join("||", rows.Select(row => $"{row.Company}|{row.Date}|{row.ImportDate}|{row.Status}|{row.Project}"));
    }

    private static async Task<bool> IsPagerEnabledAsync(ILocator button) =>
        await button.EvaluateAsync<bool>(
            """
            el => !(
              el.disabled
              || el.hasAttribute('disabled')
              || el.getAttribute('aria-disabled') === 'true'
              || el.classList.contains('disabled')
            )
            """);

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
