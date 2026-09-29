using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EmissionCategoriesPage : BasePage
{
    public EmissionCategoriesPage(IPage page) : base(page) { }

    private ILocator PageTitle => Page.Locator("//h1[contains(@id,'title')]")
        .Filter(new LocatorFilterOptions { HasTextString = "Emission Categories" });
    private ILocator Message => Page.Locator("//p[@class='page-header__lead']");
    private ILocator Grid => Page.Locator("table.table");
    private ILocator DisplayNameCells => Grid.Locator("tbody tr td:nth-child(1)");
    private ILocator GhgScopeCells => Grid.Locator("tbody tr td:nth-child(2)");
    private ILocator NextBtn => Page.Locator("nav.pagination button")
        .Filter(new LocatorFilterOptions { HasTextString = "Next" });
    private ILocator PreviousBtn => Page.Locator("nav.pagination button")
        .Filter(new LocatorFilterOptions { HasTextString = "Previous" });

    public async Task OpenAsync()
    {
        var sideMenuPage = new SideMenuPage(Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickEmissionCategoriesAsync();
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await PageTitle.WaitForAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Grid.Locator("xpath=.//tbody/tr/td[normalize-space()!='']")
            .First
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Grid.Locator("tbody tr").First
            .GetByRole(AriaRole.Button, new() { Name = "Edit" })
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetMessageAsync()
    {
        await Message.WaitForAsync();
        return (await Message.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsGridVisibleAsync() => Grid.IsVisibleAsync();

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await Grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header))
            .ToList();
    }

    public async Task<int> GetGridRowCountAsync()
    {
        var rows = await GetGridRowsAsync();
        return rows.Count;
    }

    public async Task<IReadOnlyList<(string DisplayName, string GhgScope)>> GetGridRowsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        var result = new List<(string DisplayName, string GhgScope)>();
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);

        for (var pageGuard = 0; pageGuard < 50; pageGuard++)
        {
            foreach (var row in await GetGridRowsOnCurrentPageAsync())
            {
                var key = $"{row.DisplayName}|{row.GhgScope}";
                if (seenKeys.Add(key))
                    result.Add(row);
            }

            var showing = await TryGetShowingRangeAsync();
            if (showing is not null && result.Count >= showing.Value.Total)
                break;
            if (showing is not null && showing.Value.End >= showing.Value.Total)
                break;
            if (!await TryGoToNextGridPageAsync())
                break;
        }

        await GoToFirstGridPageAsync();
        return result;
    }

    public async Task<(string DisplayName, string GhgScope)> GetFirstRowDisplayNameAndScopeAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();
        var firstRow = Grid.Locator("tbody tr").First;
        await firstRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var displayName = (await firstRow.Locator("td").Nth(0).TextContentAsync())?.Trim() ?? string.Empty;
        var ghgScope = (await firstRow.Locator("td").Nth(1).TextContentAsync())?.Trim() ?? string.Empty;
        return (displayName, ghgScope);
    }

    public async Task<bool> IsEditButtonVisibleForRowAsync(string displayName, string ghgScope)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        while (true)
        {
            var row = FindRowLocator(displayName, ghgScope);
            if (await row.CountAsync() > 0 && await row.First.IsVisibleAsync())
            {
                var visible = await row.First.GetByRole(AriaRole.Button, new() { Name = "Edit" }).IsVisibleAsync();
                await GoToFirstGridPageAsync();
                return visible;
            }

            if (!await TryGoToNextGridPageAsync())
                break;
        }

        await GoToFirstGridPageAsync();
        return false;
    }

    public async Task<bool> DoesEachRowHaveEditButtonAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        while (true)
        {
            var rows = Grid.Locator("tbody tr");
            var rowCount = await rows.CountAsync();
            for (var i = 0; i < rowCount; i++)
            {
                var cells = rows.Nth(i).Locator("td");
                if (await cells.CountAsync() == 0)
                    continue;

                var displayName = (await cells.Nth(0).InnerTextAsync()).Trim();
                if (string.IsNullOrWhiteSpace(displayName))
                    continue;

                var editButton = rows.Nth(i).GetByRole(AriaRole.Button, new() { Name = "Edit" });
                if (await editButton.CountAsync() == 0 || !await editButton.First.IsVisibleAsync())
                {
                    await GoToFirstGridPageAsync();
                    return false;
                }
            }

            if (!await TryGoToNextGridPageAsync())
                break;
        }

        await GoToFirstGridPageAsync();
        return true;
    }

    public async Task<EditEmissionCategoryPage> ClickEditBtnAsync(string displayName, string ghgScope)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        while (true)
        {
            var row = FindRowLocator(displayName, ghgScope);
            if (await row.CountAsync() > 0 && await row.First.IsVisibleAsync())
            {
                await row.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
                var editEmissionCategoryPage = new EditEmissionCategoryPage(Page);
                await editEmissionCategoryPage.WaitForLoadedAsync();
                return editEmissionCategoryPage;
            }

            if (!await TryGoToNextGridPageAsync())
                break;
        }

        throw new InvalidOperationException(
            $"Emission category row '{displayName}' / '{ghgScope}' was not found across grid pages.");
    }

    public async Task<EditEmissionCategoryPage> ClickEditBtnInFirstRowAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();
        var firstRow = Grid.Locator("tbody tr").First;
        await firstRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await firstRow.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        var editEmissionCategoryPage = new EditEmissionCategoryPage(Page);
        await editEmissionCategoryPage.WaitForLoadedAsync();
        return editEmissionCategoryPage;
    }

    public async Task<IReadOnlyList<string>> GetAllDisplayNamesWithGhgScopesAsync()
    {
        var rows = await GetGridRowsAsync();
        return rows
            .Select(row => $"{row.DisplayName} ({row.GhgScope})")
            .ToList();
    }

    private ILocator FindRowLocator(string displayName, string ghgScope) =>
        Grid.Locator(
            $"xpath=.//tbody/tr[td[1][normalize-space()='{displayName}'] and td[2][normalize-space()='{ghgScope}']]");

    private async Task<IReadOnlyList<(string DisplayName, string GhgScope)>> GetGridRowsOnCurrentPageAsync()
    {
        var rawRows = await Grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .map(tr => [...tr.querySelectorAll('td')]
                .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').trim()))
              .filter(cells => cells.length > 1 && cells[0])
            """);

        return rawRows
            .Select(cells => (DisplayName: cells[0], GhgScope: cells[1]))
            .ToList();
    }

    private async Task GoToFirstGridPageAsync()
    {
        while (await CanGoToAdjacentPageAsync(PreviousBtn))
        {
            var keysBefore = (await GetGridRowsOnCurrentPageAsync())
                .Select(r => $"{r.DisplayName}|{r.GhgScope}")
                .ToHashSet(StringComparer.Ordinal);

            await PreviousBtn.EvaluateAsync("el => el.click()");
            if (!await WaitForGridPageChangeAsync(keysBefore))
                break;
        }
    }

    private async Task<bool> TryGoToNextGridPageAsync()
    {
        if (!await CanGoToAdjacentPageAsync(NextBtn))
            return false;

        var keysBefore = (await GetGridRowsOnCurrentPageAsync())
            .Select(r => $"{r.DisplayName}|{r.GhgScope}")
            .ToHashSet(StringComparer.Ordinal);

        await NextBtn.EvaluateAsync("el => el.click()");
        return await WaitForGridPageChangeAsync(keysBefore);
    }

    private async Task<bool> WaitForGridPageChangeAsync(ISet<string> keysBefore)
    {
        var showingBefore = await TryGetShowingRangeAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            var pageRows = await GetGridRowsOnCurrentPageAsync();
            if (pageRows.Count > 0
                && pageRows.Any(r => !keysBefore.Contains($"{r.DisplayName}|{r.GhgScope}")))
                return true;

            var showingAfter = await TryGetShowingRangeAsync();
            if (showingBefore is not null
                && showingAfter is not null
                && showingAfter.Value.Start != showingBefore.Value.Start
                && pageRows.Count > 0
                && pageRows.All(r => keysBefore.Contains($"{r.DisplayName}|{r.GhgScope}")))
            {
                // Showing advanced but tbody is still stale — keep waiting.
                await Page.WaitForTimeoutAsync(150);
                continue;
            }

            await Page.WaitForTimeoutAsync(150);
        }

        return false;
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

        text = text
            .Replace('\u00A0', ' ')
            .Replace('\n', ' ')
            .Replace('\r', ' ')
            .Replace('–', '-')
            .Replace('—', '-')
            .Replace('−', '-');

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
