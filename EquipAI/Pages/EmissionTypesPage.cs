using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EmissionTypesPage : BasePage
{
    public EmissionTypesPage(IPage page) : base(page) { }

    private ILocator Message => Page.Locator("//p[@class='page-header__lead']");
    private ILocator AddEmissionTypeBtn => Page.Locator("//button[normalize-space()='Add emission type']");
    private ILocator Grid => Page.Locator("//table[@class='table']");
    private ILocator CodeCells => Page.Locator("//table[@class='table']//tr/td[1]");
    private ILocator DisplayNameCells => Page.Locator("//table[@class='table']//tr/td[2]");
    private ILocator DefaultUnitCells => Page.Locator("//table[@class='table']//tr/td[3]");
    private ILocator DeactivateDialog => Page.Locator("//div[@role='alertdialog']");
    private ILocator DeactivateDialogTitle => Page.Locator("//div[@role='alertdialog']//h2");
    private ILocator DeactivateDialogMessage => Page.Locator("//div[@role='alertdialog']//p[@id='confirm-dialog-message']");
    private ILocator DeactivateDialogCancelBtn => Page.Locator("//div[@role='alertdialog']//button[normalize-space()='Cancel']");
    private ILocator DeactivateDialogConfirmBtn => Page.Locator("//div[@role='alertdialog']//button[normalize-space()='Deactivate']");
    private ILocator NextPageBtn => Page.Locator("//button[normalize-space()='Next']").First;

    public async Task OpenAsync()
    {
        var sideMenuPage = new SideMenuPage(Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickEmissionTypesAsync();
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await Message.WaitForAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Page.Locator("//table[@class='table']//tbody/tr/td[1][normalize-space()!='']")
            .First
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetMessageAsync()
    {
        await Message.WaitForAsync();
        return (await Message.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsAddEmissionTypeBtnVisibleAsync() => AddEmissionTypeBtn.IsVisibleAsync();
    public Task<bool> IsAddEmissionTypeBtnEnabledAsync() => AddEmissionTypeBtn.IsEnabledAsync();
    public Task<bool> IsGridVisibleAsync() => Grid.IsVisibleAsync();

    public async Task ClickGridColumnAsync(string columnName)
    {
        var header = Grid.Locator("thead th").Filter(new LocatorFilterOptions { HasTextString = columnName });
        var ariaBefore = await header.GetAttributeAsync("aria-sort");
        var fingerprintBefore = await GetCurrentPageFingerprintAsync();
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
                var fingerprintAfter = await GetCurrentPageFingerprintAsync();
                if (onFirstPage
                    && fingerprintAfter.Length > 0
                    && !string.Equals(fingerprintAfter, fingerprintBefore, StringComparison.Ordinal))
                {
                    await WaitForGridSettledAsync();
                    return;
                }

                await Task.Delay(100);
            }

            if (showingBefore is null || showingBefore.Value.Start == 1)
            {
                await WaitForGridSettledAsync();
                return;
            }

            break;
        }

        throw new TimeoutException($"Column '{columnName}' sort did not change from '{ariaBefore ?? "none"}'.");
    }

    public async Task<IReadOnlyList<(string Code, string DisplayName, string DefaultUnit)>> GetCurrentPageGridRecordsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return await GetGridRecordsOnCurrentPageAsync();
    }

    public async Task<IReadOnlyList<(string Code, string DisplayName, string DefaultUnit)>> GetAllGridRecordsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        var results = new List<(string Code, string DisplayName, string DefaultUnit)>();
        var seenCodes = new HashSet<string>(StringComparer.Ordinal);
        while (true)
        {
            var pageRows = await GetGridRecordsOnCurrentPageAsync();
            foreach (var row in pageRows)
            {
                if (seenCodes.Add(row.Code))
                    results.Add(row);
            }

            var firstCode = pageRows.FirstOrDefault().Code ?? string.Empty;
            if (!await TryGoToNextGridPageAsync(firstCode))
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

        try
        {
            await Grid.Locator("tbody tr td:nth-child(1)")
                .Filter(new LocatorFilterOptions { HasTextString = code })
                .First
                .WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15_000,
                });
            return true;
        }
        catch (TimeoutException)
        {
            await GoToFirstGridPageAsync();
            return await FindCodeAcrossPagesAsync(code);
        }
    }

    public async Task<bool> IsDisplayNameInGridAsync(string displayName)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        try
        {
            await Grid.Locator("tbody tr td:nth-child(2)")
                .Filter(new LocatorFilterOptions { HasTextString = displayName })
                .First
                .WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15_000,
                });
            return true;
        }
        catch (TimeoutException)
        {
            await GoToFirstGridPageAsync();
            return await FindDisplayNameAcrossPagesAsync(displayName);
        }
    }

    private async Task<bool> FindCodeAcrossPagesAsync(string code)
    {
        while (true)
        {
            if (await IsCodeOnCurrentPageAsync(code))
                return true;

            var codes = await CodeCells.AllInnerTextsAsync();
            if (!await TryGoToNextGridPageAsync(codes.FirstOrDefault()?.Trim() ?? string.Empty))
                return false;
        }
    }

    private async Task<bool> FindDisplayNameAcrossPagesAsync(string displayName)
    {
        while (true)
        {
            if (await IsDisplayNameOnCurrentPageAsync(displayName))
                return true;

            if (!await TryGoToNextGridPageAsync(
                    (await CodeCells.AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? string.Empty))
                return false;
        }
    }

    private async Task<bool> IsCodeOnCurrentPageAsync(string code)
    {
        var cells = CodeCells;
        var count = await cells.CountAsync();
        for (var i = 0; i < count; i++)
        {
            if (await CellMatchesAsync(cells.Nth(i), code))
                return true;
        }

        return false;
    }

    private async Task<bool> IsDisplayNameOnCurrentPageAsync(string displayName)
    {
        var cells = DisplayNameCells;
        var count = await cells.CountAsync();
        for (var i = 0; i < count; i++)
        {
            if (await CellMatchesAsync(cells.Nth(i), displayName))
                return true;
        }

        return false;
    }

    private static async Task<bool> CellMatchesAsync(ILocator cell, string expected)
    {
        var text = (await cell.InnerTextAsync()).Trim();
        if (text.Equals(expected, StringComparison.Ordinal))
            return true;

        var title = (await cell.GetAttributeAsync("title"))?.Trim();
        if (!string.IsNullOrEmpty(title) && title.Equals(expected, StringComparison.Ordinal))
            return true;

        return false;
    }

    private async Task<string> GetCurrentPageFingerprintAsync() =>
        string.Join("|", (await GetGridRecordsOnCurrentPageAsync()).Select(row => row.Code));

    private async Task<(int Start, int End, int Total)?> TryGetShowingRangeAsync()
    {
        var showing = Page.Locator("nav.pagination .pagination__info, .pagination__info").First;
        if (await showing.CountAsync() == 0)
            return null;

        string text;
        try
        {
            text = await showing.InnerTextAsync();
        }
        catch (PlaywrightException)
        {
            return null;
        }

        text = text.Replace('\u00A0', ' ').Replace('–', '-').Replace('—', '-').Replace('−', '-');
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

    private async Task WaitForGridSettledAsync()
    {
        string? previous = null;
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var snapshot = string.Join("|", (await GetGridRecordsOnCurrentPageAsync()).Select(row => row.Code));
            if (previous is not null && snapshot.Length > 0 && snapshot == previous)
                return;

            previous = snapshot;
            await Task.Delay(200);
        }
    }

    private async Task<IReadOnlyList<(string Code, string DisplayName, string DefaultUnit)>> GetGridRecordsOnCurrentPageAsync()
    {
        var rows = await Grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => !tr.querySelector(':scope > td .table__skeleton-bar'))
              .map(tr => [...tr.querySelectorAll(':scope > td')].slice(0, 3).map(td => {
                const rendered = (td.innerText || '').replace(/\u00a0/g, ' ').trim();
                const title = (td.getAttribute('title') || td.querySelector('[title]')?.getAttribute('title') || '').replace(/\u00a0/g, ' ').trim();
                const full = (td.textContent || '').replace(/\u00a0/g, ' ').trim();
                return [title, full, rendered].sort((a, b) => b.length - a.length)[0];
              }))
              .filter(cells => cells.length >= 3 && cells[0])
            """);

        return rows
            .Select(cells => (
                Code: cells[0],
                DisplayName: cells.Length > 1 ? cells[1] : string.Empty,
                DefaultUnit: cells.Length > 2 ? cells[2] : string.Empty))
            .ToList();
    }

    private async Task GoToFirstGridPageAsync()
    {
        var previousBtn = Page.Locator("//button[normalize-space()='Previous']").First;
        for (var i = 0; i < 50; i++)
        {
            if (await previousBtn.CountAsync() == 0
                || !await previousBtn.IsVisibleAsync()
                || !await IsPagerButtonEnabledAsync(previousBtn))
                return;

            var codes = await CodeCells.AllInnerTextsAsync();
            var firstBefore = codes.FirstOrDefault()?.Trim() ?? string.Empty;
            await previousBtn.ClickAsync();
            if (!string.IsNullOrEmpty(firstBefore))
            {
                await CodeCells
                    .Filter(new LocatorFilterOptions { HasTextString = firstBefore })
                    .First
                    .WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Detached,
                        Timeout = 10_000,
                    });
            }
            else
            {
                await Page.WaitForTimeoutAsync(300);
            }
        }
    }

    private async Task<bool> TryGoToNextGridPageAsync(string firstCodeBefore)
    {
        if (await NextPageBtn.CountAsync() == 0
            || !await NextPageBtn.IsVisibleAsync()
            || !await IsPagerButtonEnabledAsync(NextPageBtn))
            return false;

        await NextPageBtn.ClickAsync();
        if (!string.IsNullOrEmpty(firstCodeBefore))
        {
            try
            {
                await CodeCells
                    .Filter(new LocatorFilterOptions { HasTextString = firstCodeBefore })
                    .First
                    .WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Detached,
                        Timeout = 10_000,
                    });
            }
            catch (TimeoutException)
            {
                await Page.Locator("//table[@class='table']//tbody/tr/td[1][normalize-space()!='']")
                    .First
                    .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            }
        }
        else
        {
            await Page.WaitForTimeoutAsync(500);
        }

        return true;
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

    public async Task<bool> IsDefaultUnitInGridAsync(string defaultUnit)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var defaultUnits = await DefaultUnitCells.AllInnerTextsAsync();
        return defaultUnits.Any(unit => unit.Trim().Equals(defaultUnit, StringComparison.Ordinal));
    }

    public async Task<string> GetDefaultUnitByCodeAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (true)
        {
            await GoToFirstGridPageAsync();
            while (true)
            {
                var codeCells = CodeCells;
                var defaultUnits = DefaultUnitCells;
                var count = await codeCells.CountAsync();
                for (var i = 0; i < count; i++)
                {
                    if (!await CellMatchesAsync(codeCells.Nth(i), code))
                        continue;

                    return (await defaultUnits.Nth(i).InnerTextAsync()).Trim();
                }

                var codes = await CodeCells.AllInnerTextsAsync();
                if (!await TryGoToNextGridPageAsync(codes.FirstOrDefault()?.Trim() ?? string.Empty))
                    break;
            }

            if (DateTime.UtcNow >= deadline)
                break;

            await Task.Delay(250);
        }

        throw new InvalidOperationException($"Code '{code}' was not found in the emission types grid.");
    }

    public async Task<(string Code, string DisplayName)> GetCodeAndDisplayNameAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var row = Page.Locator($"//table[@class='table']//tr[td[1][normalize-space()='{code}']]");
        await row.WaitForAsync();
        var codeText = (await row.Locator("td").Nth(0).InnerTextAsync()).Trim();
        var displayName = (await row.Locator("td").Nth(1).InnerTextAsync()).Trim();
        return (codeText, displayName);
    }

    public async Task<AddEmissionTypePage> ClickAddEmissionTypeBtnAsync()
    {
        await AddEmissionTypeBtn.ClickAsync();
        await Page.Locator("//h1[contains(@id,'title')]")
            .Filter(new LocatorFilterOptions { HasTextString = "Add emission type" })
            .WaitForAsync();
        var addEmissionTypePage = new AddEmissionTypePage(Page);
        await Page.Locator("//input[@id='emission-type-code']").WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return addEmissionTypePage;
    }

    public async Task<EditEmissionTypePage> ClickEditBtnAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var row = Page.Locator($"//table[@class='table']//tr[td[1][normalize-space()='{code}']]");
        await row.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        var editEmissionTypePage = new EditEmissionTypePage(Page);
        await editEmissionTypePage.WaitForLoadedAsync();
        return editEmissionTypePage;
    }

    public async Task ClickDeactivateBtnAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var row = Page.Locator($"//table[@class='table']//tr[td[1][normalize-space()='{code}']]");
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
        var row = Page.Locator($"//table[@class='table']//tr[td[1][normalize-space()='{code}']]");
        await row.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }
}
