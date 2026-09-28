using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class InvoicesPage : BasePage
{
    public InvoicesPage(IPage page) : base(page) { }

    private ILocator InvoicesTitle => Page.Locator("//h1[@id='invoice-list-title']");
    private ILocator InvoicesMessage => Page.Locator("//p[@class='page-header__lead']");
    private ILocator ImportCSVBtn => Page.Locator("//a[normalize-space()='Import CSV']");
    private ILocator ImportPDFBtn => Page.Locator("//a[normalize-space()='Import PDF']");
    private ILocator ImportUtilityBillBtn => Page.Locator("//a[normalize-space()='Import Utility Bill']");
    private ILocator FuelInvoicesTab => Page.Locator("//button[normalize-space()='Fuel Invoices'] | //a[normalize-space()='Fuel Invoices']");
    private ILocator UtilityBillsTab => Page.Locator("//button[normalize-space()='Utility Bills'] | //a[normalize-space()='Utility Bills']");
    private ILocator CreateBtn => Page.Locator("//a[normalize-space()='Create']");
    private ILocator InvoicesGrid => Page.Locator("table.table.invoice-list__table, table.table").Locator("visible=true").First;
    private ILocator PreviousBtn => Page.Locator("//button[normalize-space()='Previous']").Last;
    private ILocator NextBtn => Page.Locator("//button[normalize-space()='Next']").Last;
    private ILocator InvoiceNumberCells => InvoicesGrid.Locator("tbody tr td:nth-child(2)");
    private ILocator PaginationSummary => Page.Locator("//p[@class='pagination__info'] | //*[contains(@class,'pagination__info')]").First;

    public async Task OpenAsync()
    {
        await Page.GotoAsync(Config.BaseUrl + "invoices");
        await InvoicesTitle.WaitForAsync();
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
    }

    public async Task<string> GetTitleAsync()
    {
        await InvoicesTitle.WaitForAsync();
        return (await InvoicesTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessageAsync()
    {
        await InvoicesMessage.WaitForAsync();
        return (await InvoicesMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsInvoicesTitleVisibleAsync() => InvoicesTitle.IsVisibleAsync();
    public Task<bool> IsInvoicesMessageVisibleAsync() => InvoicesMessage.IsVisibleAsync();
    public Task<bool> IsImportCSVBtnVisibleAsync() => ImportCSVBtn.IsVisibleAsync();
    public Task<bool> IsImportPDFBtnVisibleAsync() => ImportPDFBtn.IsVisibleAsync();
    public Task<bool> IsImportUtilityBillBtnVisibleAsync() => ImportUtilityBillBtn.IsVisibleAsync();
    public Task<bool> IsCreateBtnVisibleAsync() => CreateBtn.IsVisibleAsync();
    public Task<bool> IsInvoicesGridVisibleAsync() => InvoicesGrid.IsVisibleAsync();

    public async Task SelectUtilityBillsTabAsync()
    {
        await UtilityBillsTab.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await UtilityBillsTab.First.ClickAsync();
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync(keyColumnIndex: 0);
        await WaitForPaginationStableAsync();
    }

    public async Task SelectFuelInvoicesTabAsync()
    {
        await FuelInvoicesTab.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await FuelInvoicesTab.First.ClickAsync();
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        await WaitForPaginationStableAsync();
    }

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await InvoicesGrid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header)
                             && !header.Equals("Actions", StringComparison.Ordinal))
            .ToList();
    }

    public async Task<CreateInvoicePage> ClickCreateBtnAsync()
    {
        await CreateBtn.ClickAsync();
        await Page.WaitForURLAsync("**/invoices/new**");
        return new CreateInvoicePage(Page);
    }

    public async Task<ImportPage> ClickImportCSVBtnAsync()
    {
        await ImportCSVBtn.ClickAsync();
        await Page.WaitForURLAsync("**/invoices/import**");
        var importPage = new ImportPage(Page);
        await importPage.WaitForLoadedAsync();
        return importPage;
    }

    public async Task<ImportPage> ClickImportPDFBtnAsync()
    {
        await ImportPDFBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/invoices/", StringComparison.OrdinalIgnoreCase)
                   && url.Contains("pdf", StringComparison.OrdinalIgnoreCase));
        var importPage = new ImportPage(Page);
        await importPage.WaitForLoadedAsync();
        return importPage;
    }

    public async Task<ImportPage> ClickImportUtilityBillBtnAsync()
    {
        await ImportUtilityBillBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/invoices/import-utility-bill", StringComparison.OrdinalIgnoreCase)
                   || url.Contains("/utility-bills/import", StringComparison.OrdinalIgnoreCase));
        var importPage = new ImportPage(Page);
        await importPage.WaitForLoadedAsync();
        return importPage;
    }

    public async Task<ViewInvoicePage> ClickViewBtnAsync(string invoiceNumber)
    {
        var row = await FindInvoiceRowAcrossPagesAsync(invoiceNumber)
            ?? throw new InvalidOperationException($"Invoice '{invoiceNumber}' was not found in the grid.");

        await row.GetByRole(AriaRole.Button, new() { Name = "View" }).ClickAsync();

        var viewInvoicePage = new ViewInvoicePage(Page);
        await viewInvoicePage.GetTitleAsync();
        return viewInvoicePage;
    }

    public async Task<EditInvoicePage> ClickEditBtnAsync(string invoiceNumber)
    {
        var row = await FindInvoiceRowAcrossPagesAsync(invoiceNumber)
            ?? throw new InvalidOperationException($"Invoice '{invoiceNumber}' was not found in the grid.");

        await row.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();

        var editInvoicePage = new EditInvoicePage(Page);
        await editInvoicePage.WaitForLoadedAsync();
        return editInvoicePage;
    }

    public async Task<bool> IsEditBtnVisibleForInvoiceAsync(string invoiceNumber)
    {
        var row = await FindInvoiceRowAcrossPagesAsync(invoiceNumber)
            ?? throw new InvalidOperationException($"Invoice '{invoiceNumber}' was not found in the grid.");

        return await row.GetByRole(AriaRole.Button, new() { Name = "Edit" }).IsVisibleAsync();
    }

    public async Task<bool> IsViewBtnVisibleForInvoiceAsync(string invoiceNumber)
    {
        var row = await FindInvoiceRowAcrossPagesAsync(invoiceNumber)
            ?? throw new InvalidOperationException($"Invoice '{invoiceNumber}' was not found in the grid.");

        return await row.GetByRole(AriaRole.Button, new() { Name = "View" }).IsVisibleAsync();
    }

    public async Task<int> GetTotalInvoiceCountFromPaginationSummaryAsync()
    {
        await PaginationSummary.WaitForAsync();
        var summaryText = (await PaginationSummary.TextContentAsync())?.Trim() ?? string.Empty;

        var matches = System.Text.RegularExpressions.Regex.Matches(summaryText, @"\d+");
        if (matches.Count == 0)
        {
            throw new InvalidOperationException(
                $"Could not parse invoice count from pagination summary: '{summaryText}'");
        }

        return int.Parse(matches[^1].Value);
    }

    public async Task<int> GetInvoiceNumberCountFromAllPagesAsync()
    {
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        await WaitForPaginationReadyAsync();

        var showing = await TryGetShowingRangeAsync();
        if (showing is not null)
            return showing.Value.Total;

        var totalCount = 0;
        var visitedNextPage = false;
        await GoToFirstGridPageAsync();

        while (true)
        {
            totalCount += await CountNonEmptyInvoiceRowsOnCurrentPageAsync();

            if (!await TryGoToNextGridPageAsync())
                break;

            visitedNextPage = true;
        }

        if (visitedNextPage)
            await GoToFirstGridPageAsync();

        return totalCount;
    }

    public async Task<bool> IsInvoiceNumberInGridAsync(string invoiceNumber)
    {
        return await FindInvoiceRowAcrossPagesAsync(invoiceNumber) is not null;
    }

    public async Task<InvoiceGridRow?> GetInvoiceGridRowAsync(string invoiceNumber)
    {
        var row = await FindInvoiceRowAcrossPagesAsync(invoiceNumber);
        if (row is null)
            return null;

        var cells = row.Locator("td");
        return new InvoiceGridRow
        {
            Project = (await cells.Nth(0).InnerTextAsync()).Trim(),
            InvoiceNumber = (await cells.Nth(1).InnerTextAsync()).Trim(),
            Company = (await cells.Nth(2).InnerTextAsync()).Trim(),
            Date = (await cells.Nth(3).InnerTextAsync()).Trim(),
            Status = (await cells.Nth(4).InnerTextAsync()).Trim(),
            ImportDate = (await cells.Nth(5).InnerTextAsync()).Trim(),
            ApproveRejectDate = (await cells.Nth(6).InnerTextAsync()).Trim(),
            Source = (await cells.Nth(7).InnerTextAsync()).Trim(),
        };
    }

    public async Task<UtilityBillGridRow?> GetUtilityBillGridRowAsync(string company, string importDate)
    {
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForPaginationStableAsync();
        await GoToFirstGridPageAsync();

        while (true)
        {
            var matchIndex = await InvoicesGrid.EvaluateAsync<int?>(
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
                var cells = InvoicesGrid.Locator("tbody tr").Nth(matchIndex.Value).Locator("td");
                return new UtilityBillGridRow
                {
                    Project = (await cells.Nth(0).InnerTextAsync()).Trim(),
                    Company = (await cells.Nth(1).InnerTextAsync()).Trim(),
                    Date = (await cells.Nth(2).InnerTextAsync()).Trim(),
                    Status = (await cells.Nth(3).InnerTextAsync()).Trim(),
                    ImportDate = (await cells.Nth(4).InnerTextAsync()).Trim(),
                    ApproveRejectDate = (await cells.Nth(5).InnerTextAsync()).Trim(),
                    Source = (await cells.Nth(6).InnerTextAsync()).Trim(),
                };
            }

            if (!await TryGoToNextGridPageAsync())
                break;
        }

        return null;
    }

    private async Task<ILocator?> FindInvoiceRowAcrossPagesAsync(string invoiceNumber)
    {
        var deadline = DateTime.UtcNow.AddSeconds(60);
        var attempt = 0;
        while (DateTime.UtcNow < deadline)
        {
            attempt++;
            await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await WaitForGridDataAsync();
            await WaitForPaginationReadyAsync();
            await GoToFirstGridPageAsync();

            while (true)
            {
                var row = await FindInvoiceRowOnCurrentPageAsync(invoiceNumber);
                if (row is not null)
                    return row;

                if (!await TryGoToNextGridPageAsync())
                    break;
            }

            if (DateTime.UtcNow >= deadline)
                break;

            await Task.Delay(500);
            await Page.ReloadAsync();
            await InvoicesTitle.WaitForAsync();
            await WaitForGridDataAsync();
            await WaitForPaginationReadyAsync();
        }

        return null;
    }

    private async Task<ILocator?> FindInvoiceRowOnCurrentPageAsync(string invoiceNumber)
    {
        var matchIndex = await InvoicesGrid.EvaluateAsync<int?>(
            """
            (table, invoiceNumber) => {
              const target = (invoiceNumber || '').trim().toLowerCase();
              const rows = [...table.querySelectorAll('tbody tr')];
              for (let i = 0; i < rows.length; i++) {
                if (rows[i].querySelector('.table__skeleton-bar')) continue;
                const cell = rows[i].querySelectorAll('td')[1];
                if (!cell) continue;
                const text = (cell.innerText || '').replace(/\u00a0/g, ' ').trim();
                const title = (cell.getAttribute('title') || '').trim();
                if (!text && !title) continue;
                if (text.toLowerCase() === target || title.toLowerCase() === target) return i;
                if (text.toLowerCase().includes(target) || title.toLowerCase().includes(target)) return i;
              }
              return null;
            }
            """,
            invoiceNumber);

        if (matchIndex is not null)
            return InvoicesGrid.Locator("tbody tr").Nth(matchIndex.Value);

        var exact = InvoicesGrid.Locator($"tbody tr:has(td:nth-child(2):text-is(\"{invoiceNumber}\"))");
        if (await exact.CountAsync() > 0)
            return exact.First;

        var byTitle = InvoicesGrid.Locator($"tbody tr:has(td[title=\"{invoiceNumber}\"])");
        if (await byTitle.CountAsync() > 0)
            return byTitle.First;

        return null;
    }

    private async Task<int> CountNonEmptyInvoiceRowsOnCurrentPageAsync()
    {
        return await InvoicesGrid.EvaluateAsync<int>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => {
                if (tr.querySelector('.table__skeleton-bar')) return false;
                const cell = tr.querySelectorAll('td')[1];
                if (!cell) return false;
                const text = (cell.innerText || '').replace(/\u00a0/g, ' ').trim();
                const title = (cell.getAttribute('title') || '').trim();
                return !!(text || title);
              }).length
            """);
    }

    private async Task WaitForGridDataAsync(int keyColumnIndex = 1)
    {
        try
        {
            var loading = Page.Locator(".table-wrapper--loading").First;
            if (await loading.CountAsync() > 0)
            {
                await loading.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Detached,
                    Timeout = 30_000,
                });
            }
        }
        catch (TimeoutException)
        {
        }

        try
        {
            var skeleton = InvoicesGrid.Locator(".table__skeleton-bar").First;
            if (await skeleton.CountAsync() > 0)
            {
                await skeleton.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Detached,
                    Timeout = 30_000,
                });
            }
        }
        catch (TimeoutException)
        {
        }

        try
        {
            await Page.WaitForFunctionAsync(
                """
                (keyColumnIndex) => {
                  const tables = [...document.querySelectorAll('table.table')]
                    .filter(t => t.offsetParent !== null);
                  return tables.some(table => {
                    if (table.closest('.table-wrapper--loading')) return false;
                    if (table.querySelector('.table__skeleton-bar')) return false;
                    return [...table.querySelectorAll('tbody tr')]
                      .some(tr => {
                        const cell = tr.querySelectorAll('td')[keyColumnIndex];
                        if (!cell) return false;
                        return ((cell.innerText || '').trim().length > 0
                          || (cell.getAttribute('title') || '').trim().length > 0);
                      });
                  });
                }
                """,
                keyColumnIndex,
                new PageWaitForFunctionOptions { Timeout = 30_000 });
        }
        catch (TimeoutException)
        {
            // Grid may legitimately be empty.
        }
    }

    private async Task WaitForPaginationReadyAsync()
    {
        (int Start, int End, int Total)? previous = null;
        for (var attempt = 0; attempt < 30; attempt++)
        {
            var showing = await TryGetShowingRangeAsync();
            if (showing is not null)
            {
                if (previous is not null
                    && previous.Value.Start == showing.Value.Start
                    && previous.Value.End == showing.Value.End
                    && previous.Value.Total == showing.Value.Total)
                {
                    return;
                }

                previous = showing;
            }
            else if (await InvoicesGrid.Locator("tbody tr td").CountAsync() > 0)
            {
                return;
            }

            await Task.Delay(150);
        }
    }

    private async Task GoToFirstGridPageAsync()
    {
        await WaitForPaginationReadyAsync();
        for (var guard = 0; guard < 100; guard++)
        {
            if (!await CanGoToAdjacentPageAsync(PreviousBtn))
                return;

            var showingBefore = await TryGetShowingRangeAsync();
            var fingerprintBefore = await GetPageFingerprintAsync();
            try
            {
                await PreviousBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000, Force = true });
            }
            catch (TimeoutException)
            {
                return;
            }

            if (!await WaitForPageAdvanceAsync(showingBefore, fingerprintBefore, expectIncrease: false))
                return;
        }
    }

    private async Task<bool> TryGoToNextGridPageAsync()
    {
        await WaitForPaginationReadyAsync();

        var showingBefore = await TryGetShowingRangeAsync();
        var canByShowing = showingBefore is not null && showingBefore.Value.End < showingBefore.Value.Total;
        var canByButton = await IsPaginationButtonEnabledAsync(NextBtn);
        if (!canByShowing && !canByButton)
            return false;

        var fingerprintBefore = await GetPageFingerprintAsync();
        try
        {
            await NextBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000, Force = true });
        }
        catch (TimeoutException)
        {
            return false;
        }

        return await WaitForPageAdvanceAsync(showingBefore, fingerprintBefore, expectIncrease: true);
    }

    private async Task<bool> WaitForPageAdvanceAsync(
        (int Start, int End, int Total)? showingBefore,
        string fingerprintBefore,
        bool expectIncrease)
    {
        var deadline = DateTime.UtcNow.AddSeconds(12);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var skeleton = InvoicesGrid.Locator(".table__skeleton-bar").First;
                if (await skeleton.CountAsync() > 0)
                {
                    await skeleton.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Detached,
                        Timeout = 5_000,
                    });
                }
            }
            catch (TimeoutException)
            {
            }

            var showingAfter = await TryGetShowingRangeAsync();
            if (showingBefore is not null && showingAfter is not null)
            {
                if (expectIncrease && showingAfter.Value.Start > showingBefore.Value.Start)
                    return true;
                if (!expectIncrease && showingAfter.Value.Start < showingBefore.Value.Start)
                    return true;
                if (expectIncrease
                    && showingAfter.Value.Start == showingBefore.Value.Start
                    && showingAfter.Value.End >= showingAfter.Value.Total
                    && !await IsPaginationButtonEnabledAsync(NextBtn))
                {
                    return false;
                }
            }

            var fingerprintAfter = await GetPageFingerprintAsync();
            if (!string.IsNullOrEmpty(fingerprintBefore)
                && !fingerprintBefore.Equals(fingerprintAfter, StringComparison.Ordinal)
                && !string.IsNullOrEmpty(fingerprintAfter))
            {
                return true;
            }

            await Task.Delay(200);
        }

        var finalShowing = await TryGetShowingRangeAsync();
        if (showingBefore is not null && finalShowing is not null)
        {
            return expectIncrease
                ? finalShowing.Value.Start > showingBefore.Value.Start
                : finalShowing.Value.Start < showingBefore.Value.Start;
        }

        var finalFingerprint = await GetPageFingerprintAsync();
        return !string.IsNullOrEmpty(fingerprintBefore)
               && !fingerprintBefore.Equals(finalFingerprint, StringComparison.Ordinal);
    }

    private async Task<string> GetPageFingerprintAsync()
    {
        return await InvoicesGrid.EvaluateAsync<string>(
            """
            table => {
              const rows = [...table.querySelectorAll('tbody tr')]
                .filter(tr => !tr.querySelector('.table__skeleton-bar'))
                .slice(0, 3);
              return rows.map(tr => {
                const cell = tr.querySelectorAll('td')[1];
                if (!cell) return '';
                return ((cell.innerText || '') + '|' + (cell.getAttribute('title') || ''))
                  .replace(/\u00a0/g, ' ').trim();
              }).join('||');
            }
            """) ?? string.Empty;
    }

    private async Task WaitForPaginationStableAsync()
    {
        await WaitForPaginationReadyAsync();
    }

    private async Task<bool> CanGoToAdjacentPageAsync(ILocator button)
    {
        if (await button.CountAsync() == 0 || !await button.IsVisibleAsync())
            return false;

        var isNext = (await button.InnerTextAsync()).Contains("Next", StringComparison.OrdinalIgnoreCase);
        var showing = await TryGetShowingRangeAsync();
        if (showing is not null)
        {
            var byShowing = isNext
                ? showing.Value.End < showing.Value.Total
                : showing.Value.Start > 1;
            if (byShowing)
                return true;

            return await IsPaginationButtonEnabledAsync(button);
        }

        return await IsPaginationButtonEnabledAsync(button);
    }

    private async Task<(int Start, int End, int Total)?> TryGetShowingRangeAsync()
    {
        var candidates = new[]
        {
            PaginationSummary,
            Page.Locator("//*[contains(normalize-space(.),'Showing') and contains(normalize-space(.),'of')]").First,
        };

        foreach (var showing in candidates)
        {
            try
            {
                if (await showing.CountAsync() == 0 || !await showing.IsVisibleAsync())
                    continue;

                var text = (await showing.InnerTextAsync()).Replace('\u00A0', ' ').Replace('–', '-').Replace('—', '-');
                var match = System.Text.RegularExpressions.Regex.Match(
                    text,
                    @"Showing\s+(\d+)\s*-\s*(\d+)\s+of\s+(\d+)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!match.Success)
                    continue;

                return (
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value));
            }
            catch (PlaywrightException)
            {
            }
        }

        return null;
    }

    private static async Task<bool> IsPaginationButtonEnabledAsync(ILocator button)
    {
        if (await button.CountAsync() == 0 || !await button.IsVisibleAsync())
            return false;

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
}
public sealed class InvoiceGridRow
{
    public required string Project { get; init; }
    public required string InvoiceNumber { get; init; }
    public required string Company { get; init; }
    public required string Date { get; init; }
    public required string Status { get; init; }
    public required string ImportDate { get; init; }
    public required string ApproveRejectDate { get; init; }
    public required string Source { get; init; }
}

public sealed class UtilityBillGridRow
{
    public required string Project { get; init; }
    public required string Company { get; init; }
    public required string Date { get; init; }
    public required string Status { get; init; }
    public required string ImportDate { get; init; }
    public required string ApproveRejectDate { get; init; }
    public required string Source { get; init; }
}