using Microsoft.Playwright;

namespace EquipAI.Pages;

public class UnitsPage : BasePage
{
    public UnitsPage(IPage page) : base(page) { }

    private ILocator Message => Page.Locator("//p[@class='page-header__lead']");
    private ILocator AddUnitBtn => Page.Locator("//button[normalize-space()='Add unit']");
    private ILocator SearchInput => Page.Locator("//input[@id='admin-units-search']");
    private ILocator Grid => Page.Locator("//table[@class='table']");
    private ILocator CodeCells => Page.Locator("//table[@class='table']//tbody/tr/td[1]");
    private ILocator DisplayNameCells => Page.Locator("//table[@class='table']//tbody/tr/td[2]");
    private ILocator NextPageBtn => Page.Locator("//button[normalize-space()='Next']").First;
    private ILocator PreviousPageBtn => Page.Locator("//button[normalize-space()='Previous']").First;
    private ILocator DeactivateDialog => Page.Locator("//div[@role='alertdialog']");
    private ILocator DeactivateDialogTitle => Page.Locator("//div[@role='alertdialog']//h2");
    private ILocator DeactivateDialogMessage => Page.Locator("//div[@role='alertdialog']//p[@id='confirm-dialog-message']");
    private ILocator DeactivateDialogCancelBtn => Page.Locator("//div[@role='alertdialog']//button[normalize-space()='Cancel']");
    private ILocator DeactivateDialogConfirmBtn => Page.Locator("//div[@role='alertdialog']//button[normalize-space()='Deactivate']");
    private ILocator AlertMessage => Page.Locator("//div[@class='alert__content']");

    public async Task OpenAsync()
    {
        var sideMenuPage = new SideMenuPage(Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickUnitsAsync();
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await Message.WaitForAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
    }

    public async Task<string> GetMessageAsync()
    {
        await Message.WaitForAsync();
        return (await Message.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsAddUnitBtnVisibleAsync() => AddUnitBtn.IsVisibleAsync();
    public Task<bool> IsAddUnitBtnEnabledAsync() => AddUnitBtn.IsEnabledAsync();
    public Task<bool> IsGridVisibleAsync() => Grid.IsVisibleAsync();

    public async Task<bool> IsSearchInputVisibleAsync()
    {
        try
        {
            await SearchInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10_000
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public async Task<string> GetSearchPlaceholderAsync() =>
        (await SearchInput.GetAttributeAsync("placeholder"))?.Trim() ?? string.Empty;

    public async Task SearchAsync(string text)
    {
        var before = NormalizeGridText(await Grid.Locator("tbody").InnerTextAsync());
        var response = Page.WaitForResponseAsync(
            response => IsUnitsSearchResponse(response, text),
            new PageWaitForResponseOptions { Timeout = 15_000 });
        await SearchInput.FillAsync(text);
        await response;

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            if (await IsGridLoadingAsync())
            {
                await Task.Delay(50);
                continue;
            }

            var current = NormalizeGridText(await Grid.Locator("tbody").InnerTextAsync());
            if (current == before)
            {
                await Task.Delay(50);
                continue;
            }

            if (await IsEmptySearchMessageVisibleAsync())
                return;

            var rows = await GetGridRecordsOnCurrentPageAsync();
            if (rows.Count > 0 && rows.All(row => RowContainsSearch(row, text)))
                return;

            await Task.Delay(50);
        }

        throw new TimeoutException($"Units search for '{text}' did not finish.");
    }

    public async Task<string> GetGridEmptyMessageAsync()
    {
        var message = Grid.Locator("tbody").GetByText(
            "No units match your search.",
            new LocatorGetByTextOptions { Exact = true });
        await message.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10_000
        });
        return NormalizeGridText(await message.First.InnerTextAsync());
    }

    public async Task ClickGridColumnAsync(string columnName)
    {
        var header = Grid.Locator("thead th").Filter(new LocatorFilterOptions { HasTextString = columnName });
        var ariaBefore = await header.GetAttributeAsync("aria-sort");
        var fingerprintBefore = await GetCurrentPageFingerprintAsync();
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
                var fingerprintAfter = await GetCurrentPageFingerprintAsync();
                if (fingerprintAfter.Length > 0
                    && !string.Equals(fingerprintAfter, fingerprintBefore, StringComparison.Ordinal))
                    return;

                await Task.Delay(100);
            }

            return;
        }

        throw new TimeoutException($"Column '{columnName}' sort did not change from '{ariaBefore ?? "none"}'.");
    }

    public async Task<IReadOnlyList<UnitGridRecord>> GetCurrentPageGridRecordsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        return await GetGridRecordsOnCurrentPageAsync();
    }

    public async Task<IReadOnlyList<UnitGridRecord>> GetAllGridRecordsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        await GoToFirstGridPageAsync();

        var results = new List<UnitGridRecord>();
        var seenCodes = new HashSet<string>(StringComparer.Ordinal);
        while (true)
        {
            foreach (var row in await GetGridRecordsOnCurrentPageAsync())
            {
                if (seenCodes.Add(row.Code))
                    results.Add(row);
            }

            if (!await TryGoToNextGridPageAsync())
                break;
        }

        return results;
    }

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await Grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header)
                             && !header.Equals("Actions", StringComparison.Ordinal))
            .ToList();
    }

    public async Task<bool> IsCodeInGridAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();
        return await FindCodeAcrossPagesAsync(code);
    }

    public async Task<bool> IsDisplayNameInGridAsync(string displayName)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();
        return await FindDisplayNameAcrossPagesAsync(displayName);
    }

    public async Task<(string Code, string DisplayName)> GetCodeAndDisplayNameAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var row = await FindRowByCodeAcrossPagesAsync(code)
            ?? throw new InvalidOperationException($"Code '{code}' was not found in the units grid.");
        var cells = await ReadRowCellsAsync(row);
        return (cells[0], cells[1]);
    }

    public async Task<string> GetSymbolByCodeAsync(string code)
    {
        var cells = await GetRowCellsByCodeAsync(code);
        return cells.Length > 2 ? cells[2] : string.Empty;
    }

    public async Task<(string Dimension, string Scale)> GetDimensionAndScaleByCodeAsync(string code)
    {
        var cells = await GetRowCellsByCodeAsync(code);
        return (cells.Length > 3 ? cells[3] : string.Empty, cells.Length > 4 ? cells[4] : string.Empty);
    }

    private async Task<string[]> GetRowCellsByCodeAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (true)
        {
            await GoToFirstGridPageAsync();
            while (true)
            {
                var match = await FindRowCellsByCodeOnCurrentPageAsync(code);
                if (match is not null)
                    return match;

                if (!await TryGoToNextGridPageAsync())
                    break;
            }

            if (DateTime.UtcNow >= deadline)
                break;

            await Task.Delay(250);
        }

        throw new InvalidOperationException($"Code '{code}' was not found in the units grid.");
    }

    public async Task<IReadOnlyList<(string Code, string DisplayName)>> GetAllCodesAndDisplayNamesAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var rows = await Grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .map(tr => [...tr.querySelectorAll('td')]
                .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').trim()))
              .filter(cells => cells.length > 0 && cells[0])
            """);
        return rows
            .Select(cells => (Code: cells[0], DisplayName: cells.Length > 1 ? cells[1] : string.Empty))
            .ToList();
    }

    public async Task<AddUnitPage> ClickAddUnitBtnAsync()
    {
        await AddUnitBtn.ClickAsync();
        await Page.Locator("//h1[contains(@id,'title')]")
            .Filter(new LocatorFilterOptions { HasTextString = "Add unit" })
            .WaitForAsync();
        return new AddUnitPage(Page);
    }

    public async Task<EditUnitPage> ClickEditBtnAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var row = await FindRowByCodeAcrossPagesAsync(code)
            ?? throw new InvalidOperationException($"Code '{code}' was not found in the units grid.");
        await row.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        var editUnitPage = new EditUnitPage(Page);
        await editUnitPage.WaitForLoadedAsync();
        return editUnitPage;
    }

    public async Task ClickDeactivateBtnAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var row = await FindRowByCodeAcrossPagesAsync(code)
            ?? throw new InvalidOperationException($"Code '{code}' was not found in the units grid.");
        await row.GetByRole(AriaRole.Button, new() { Name = "Deactivate" }).ClickAsync();
        await DeactivateDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetDeactivateDialogTitleAsync()
    {
        await DeactivateDialogTitle.WaitForAsync();
        return (await DeactivateDialogTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetDeactivateDialogMessageAsync()
    {
        await DeactivateDialogMessage.WaitForAsync();
        return (await DeactivateDialogMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsDeactivateDialogVisibleAsync() => DeactivateDialog.IsVisibleAsync();
    public Task<bool> IsDeactivateDialogCancelBtnVisibleAsync() => DeactivateDialogCancelBtn.IsVisibleAsync();
    public Task<bool> IsDeactivateDialogConfirmBtnVisibleAsync() => DeactivateDialogConfirmBtn.IsVisibleAsync();

    public async Task ClickDeactivateDialogCancelBtnAsync()
    {
        await DeactivateDialogCancelBtn.ClickAsync();
        await DeactivateDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
    }

    public async Task ClickDeactivateDialogConfirmBtnAsync(string code)
    {
        await DeactivateDialogConfirmBtn.ClickAsync();
        await DeactivateDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            if (!await IsCodeInGridAsync(code))
                return;
            await Task.Delay(250);
        }
    }

    public async Task ClickDeactivateDialogConfirmBtnExpectingErrorAsync()
    {
        await DeactivateDialogConfirmBtn.ClickAsync();
        await AlertMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetAlertMessageAsync()
    {
        await AlertMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    private async Task<bool> FindCodeAcrossPagesAsync(string code)
    {
        while (true)
        {
            if (await FindRowIndexByCodeOnCurrentPageAsync(code) is not null)
                return true;

            if (!await TryGoToNextGridPageAsync())
                return false;
        }
    }

    private async Task<bool> FindDisplayNameAcrossPagesAsync(string displayName)
    {
        while (true)
        {
            var found = await Grid.EvaluateAsync<bool>(
                """
                (table, displayName) => [...table.querySelectorAll('tbody tr')]
                  .some(tr => {
                    const cell = tr.querySelectorAll('td')[1];
                    if (!cell) return false;
                    const text = (cell.innerText || '').replace(/\u00a0/g, ' ').trim();
                    const title = (cell.getAttribute('title') || '').trim();
                    return text === displayName || title === displayName;
                  })
                """,
                displayName);
            if (found)
                return true;

            if (!await TryGoToNextGridPageAsync())
                return false;
        }
    }

    private async Task<ILocator?> FindRowByCodeAcrossPagesAsync(string code)
    {
        await GoToFirstGridPageAsync();
        while (true)
        {
            var index = await FindRowIndexByCodeOnCurrentPageAsync(code);
            if (index is not null)
                return Grid.Locator("tbody tr").Nth(index.Value);

            if (!await TryGoToNextGridPageAsync())
                return null;
        }
    }

    private async Task<int?> FindRowIndexByCodeOnCurrentPageAsync(string code)
    {
        return await Grid.EvaluateAsync<int?>(
            """
            (table, code) => {
              const rows = [...table.querySelectorAll('tbody tr')];
              for (let i = 0; i < rows.length; i++) {
                const cell = rows[i].querySelectorAll('td')[0];
                if (!cell) continue;
                const text = (cell.innerText || '').replace(/\u00a0/g, ' ').trim();
                const title = (cell.getAttribute('title') || '').trim();
                if (!text && !title) continue;
                if (text === code || title === code) return i;
              }
              return null;
            }
            """,
            code);
    }

    private async Task<string[]?> FindRowCellsByCodeOnCurrentPageAsync(string code)
    {
        return await Grid.EvaluateAsync<string[]?>(
            """
            (table, code) => {
              const rows = [...table.querySelectorAll('tbody tr')];
              for (const tr of rows) {
                const cells = [...tr.querySelectorAll('td')]
                  .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').trim());
                const title = (tr.querySelectorAll('td')[0]?.getAttribute('title') || '').trim();
                if (!cells[0] && !title) continue;
                if (cells[0] === code || title === code) return cells;
              }
              return null;
            }
            """,
            code);
    }

    private static async Task<string[]> ReadRowCellsAsync(ILocator row)
    {
        return await row.EvaluateAsync<string[]>(
            """
            tr => [...tr.querySelectorAll('td')]
              .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').trim())
            """);
    }

    private async Task<IReadOnlyList<UnitGridRecord>> GetGridRecordsOnCurrentPageAsync()
    {
        var rows = await Grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => !tr.querySelector(':scope > td .table__skeleton-bar'))
              .map(tr => [...tr.querySelectorAll(':scope > td')].slice(0, 5).map(td => {
                const rendered = (td.innerText || '').replace(/\u00a0/g, ' ').trim();
                const title = (td.getAttribute('title') || td.querySelector('[title]')?.getAttribute('title') || '').replace(/\u00a0/g, ' ').trim();
                const full = (td.textContent || '').replace(/\u00a0/g, ' ').trim();
                return [title, full, rendered].sort((a, b) => b.length - a.length)[0];
              }))
              .filter(cells => cells.length > 4 && cells[0])
            """);

        return rows
            .Select(cells => new UnitGridRecord(
                Code: cells[0],
                DisplayName: cells.Length > 1 ? cells[1] : string.Empty,
                Symbol: cells.Length > 2 ? cells[2] : string.Empty,
                Dimension: cells.Length > 3 ? cells[3] : string.Empty,
                Scale: cells.Length > 4 ? cells[4] : string.Empty))
            .ToList();
    }

    private async Task<string> GetCurrentPageFingerprintAsync() =>
        string.Join("|", (await GetGridRecordsOnCurrentPageAsync()).Select(row => row.Code));

    private static bool IsUnitsSearchResponse(IResponse response, string text)
    {
        if (!response.Ok || !response.Url.Contains("/api/admin/reference-data/units", StringComparison.OrdinalIgnoreCase))
            return false;

        var query = new Uri(response.Url).Query.TrimStart('?');
        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = part.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = Uri.UnescapeDataString(part[..separator]);
            var value = Uri.UnescapeDataString(part[(separator + 1)..].Replace("+", " "));
            if (key.Equals("Search", StringComparison.OrdinalIgnoreCase)
                && value.Equals(text, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private async Task<bool> IsGridLoadingAsync() =>
        await Page.Locator(".table-wrapper--loading").CountAsync() > 0;

    private async Task<bool> IsEmptySearchMessageVisibleAsync()
    {
        var message = Grid.Locator("tbody").GetByText(
            "No units match your search.",
            new LocatorGetByTextOptions { Exact = true });
        return await message.CountAsync() > 0 && await message.First.IsVisibleAsync();
    }

    private static bool RowContainsSearch(UnitGridRecord row, string term) =>
        string.Join(' ', row.Code, row.DisplayName, row.Symbol, row.Dimension)
            .Contains(term, StringComparison.OrdinalIgnoreCase);

    private static string NormalizeGridText(string value) =>
        System.Text.RegularExpressions.Regex.Replace(value.Replace('\u00A0', ' '), @"\s+", " ").Trim();

    private async Task WaitForGridDataAsync()
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
            var skeleton = Grid.Locator(".table__skeleton-bar").First;
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
                () => [...document.querySelectorAll('table.table tbody tr')]
                  .some(tr => {
                    if (tr.closest('.table-wrapper--loading')) return false;
                    if (tr.querySelector('.table__skeleton-bar')) return false;
                    const cell = tr.querySelectorAll('td')[0];
                    return cell && ((cell.innerText || '').trim().length > 0
                      || (cell.getAttribute('title') || '').trim().length > 0);
                  })
                """,
                null,
                new PageWaitForFunctionOptions { Timeout = 30_000 });
        }
        catch (TimeoutException)
        {
            // Grid may legitimately be empty.
        }
    }

    private async Task GoToFirstGridPageAsync()
    {
        while (await CanGoToAdjacentPageAsync(PreviousPageBtn))
        {
            try
            {
                await PreviousPageBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000, Force = true });
            }
            catch (TimeoutException)
            {
                break;
            }

            await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }

    private async Task<bool> TryGoToNextGridPageAsync()
    {
        if (!await CanGoToAdjacentPageAsync(NextPageBtn))
            return false;

        var showingBefore = await TryGetShowingRangeAsync();
        try
        {
            await NextPageBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000, Force = true });
        }
        catch (TimeoutException)
        {
            return false;
        }

        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForGridDataAsync();

        var showingAfter = await TryGetShowingRangeAsync();
        if (showingBefore is not null
            && showingAfter is not null
            && showingAfter.Value.Start == showingBefore.Value.Start)
        {
            return false;
        }

        return true;
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

        return await IsPagerButtonEnabledAsync(button);
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

    private static async Task<bool> IsPagerButtonEnabledAsync(ILocator button)
    {
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

public sealed record UnitGridRecord(
    string Code,
    string DisplayName,
    string Symbol,
    string Dimension,
    string Scale);
