using Microsoft.Playwright;

namespace EquipAI.Pages;

public class ProjectsPage : BasePage
{
    public ProjectsPage(IPage page) : base(page) { }

    private ILocator PageTitle => Page.Locator("//h1[contains(@id,'title')]")
        .Filter(new LocatorFilterOptions { HasTextString = "Projects" });
    private ILocator SynchronizeFromProcoreBtn => Page.Locator("//button[normalize-space()='Synchronize from Procore']");
    private ILocator SynchDate => Page.Locator("//span[@class='text-muted']");
    private ILocator Grid => Page.Locator("table.table");
    private ILocator NextPageBtn => Page.Locator(
        "//nav[contains(@class,'pagination')]//button[@aria-label='Next page' or normalize-space()='Next']");
    private ILocator PreviousPageBtn => Page.Locator(
        "//nav[contains(@class,'pagination')]//button[@aria-label='Previous page' or normalize-space()='Previous']");

    public async Task OpenAsync()
    {
        var sideMenuPage = new SideMenuPage(Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickProjectsAsync();
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await PageTitle.WaitForAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
    }

    public Task<bool> IsSynchronizeFromProcoreBtnVisibleAsync() => SynchronizeFromProcoreBtn.IsVisibleAsync();
    public Task<bool> IsSynchronizeFromProcoreBtnEnabledAsync() => SynchronizeFromProcoreBtn.IsEnabledAsync();
    public Task<bool> IsSynchDateVisibleAsync() => SynchDate.IsVisibleAsync();
    public Task<bool> IsGridVisibleAsync() => Grid.IsVisibleAsync();

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await Grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Replace('\u00A0', ' ').Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header))
            .ToList();
    }

    public async Task ClickSynchronizeFromProcoreBtnAsync() =>
        await SynchronizeFromProcoreBtn.ClickAsync();

    public async Task<string> GetSynchDateTextAsync()
    {
        await SynchDate.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var text = (await SynchDate.InnerTextAsync())?.Replace('\u00A0', ' ').Replace("…", "...") ?? string.Empty;
        return System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
    }

    public async Task WaitForSynchDateTextAsync(string expectedText)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            if (string.Equals(await GetSynchDateTextAsync(), expectedText, StringComparison.Ordinal))
                return;

            await Task.Delay(100);
        }

        throw new TimeoutException($"Synch date did not become '{expectedText}'. Actual: '{await GetSynchDateTextAsync()}'.");
    }

    public async Task<string> WaitForSynchDateChangedFromAsync(string previousText)
    {
        var deadline = DateTime.UtcNow.AddMinutes(3);
        while (DateTime.UtcNow < deadline)
        {
            var text = await GetSynchDateTextAsync();
            if (!string.Equals(text, previousText, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(text))
                return text;

            await Task.Delay(500);
        }

        throw new TimeoutException($"Synch date stayed '{previousText}'.");
    }

    public async Task<IReadOnlyList<(string Code, string Name)>> GetAllCodesAndNamesAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        await GoToFirstGridPageAsync();

        var results = new List<(string Code, string Name)>();
        var seenCodes = new HashSet<string>(StringComparer.Ordinal);
        while (true)
        {
            foreach (var row in await GetCodesAndNamesOnCurrentPageAsync())
            {
                if (seenCodes.Add(row.Code))
                    results.Add(row);
            }

            if (!await TryGoToNextGridPageAsync())
                break;
        }

        return results;
    }

    public async Task<IReadOnlyList<ProjectGridRecord>> GetAllGridRecordsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        await GoToFirstGridPageAsync();

        var results = new List<ProjectGridRecord>();
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

    private async Task<IReadOnlyList<ProjectGridRecord>> GetGridRecordsOnCurrentPageAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var rows = await Grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => !tr.querySelector(':scope > td .table__skeleton-bar'))
              .map(tr => [...tr.querySelectorAll(':scope > td')].map(td => {
                const rendered = (td.innerText || '').replace(/\u00a0/g, ' ').trim();
                const full = (td.textContent || '').replace(/\u00a0/g, ' ').trim();
                return full.length > rendered.length ? full : rendered;
              }))
              .filter(cells => cells.length > 2 && cells[1])
            """);

        return rows
            .Select(cells => new ProjectGridRecord(
                Code: Cell(cells, 1),
                Name: Cell(cells, 2),
                Address: CollapseWhitespace(Cell(cells, 3)),
                StartDate: Cell(cells, 4),
                Status: Cell(cells, 5)))
            .Where(row => !string.IsNullOrEmpty(row.Code))
            .ToList();
    }

    private static string Cell(string[] cells, int index) =>
        index < cells.Length ? cells[index] : string.Empty;

    private static string CollapseWhitespace(string value) =>
        System.Text.RegularExpressions.Regex.Replace(value, @"\s+", " ").Trim();

    private async Task<IReadOnlyList<(string Code, string Name)>> GetCodesAndNamesOnCurrentPageAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var rows = await Grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => !tr.querySelector(':scope > td .table__skeleton-bar'))
              .map(tr => [...tr.querySelectorAll(':scope > td')]
                .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').trim()))
              .filter(cells => cells.length > 2 && cells[1])
            """);

        return rows
            .Select(cells => (Code: cells[1], Name: cells.Length > 2 ? cells[2] : string.Empty))
            .Where(row => !string.IsNullOrEmpty(row.Code))
            .ToList();
    }

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
                    const cells = tr.querySelectorAll('td');
                    return cells.length > 1 && (cells[1].innerText || '').trim().length > 0;
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
            await WaitForGridDataAsync();
        }
    }

    private async Task<bool> TryGoToNextGridPageAsync()
    {
        if (!await CanGoToAdjacentPageAsync(NextPageBtn))
            return false;

        var showingBefore = await TryGetShowingRangeAsync();
        var firstCodeBefore = (await GetCodesAndNamesOnCurrentPageAsync()).FirstOrDefault().Code ?? string.Empty;
        try
        {
            await NextPageBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000, Force = true });
        }
        catch (TimeoutException)
        {
            return false;
        }

        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var showingAfter = await TryGetShowingRangeAsync();
            var showingAdvanced = showingBefore is not null
                && showingAfter is not null
                && showingAfter.Value.Start != showingBefore.Value.Start;

            if (!string.IsNullOrEmpty(firstCodeBefore))
            {
                var firstCodeAfter = (await GetCodesAndNamesOnCurrentPageAsync()).FirstOrDefault().Code ?? string.Empty;
                if (!string.Equals(firstCodeAfter, firstCodeBefore, StringComparison.Ordinal))
                {
                    await WaitForGridDataAsync();
                    return true;
                }
            }
            else if (showingAdvanced)
            {
                await WaitForGridDataAsync();
                return true;
            }

            await Task.Delay(150);
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

        return await IsPagerButtonEnabledAsync(button);
    }

    private async Task<(int Start, int End, int Total)?> TryGetShowingRangeAsync()
    {
        var showing = Page.Locator(".pagination__info").First;
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

public sealed record ProjectGridRecord(string Code, string Name, string Address, string StartDate, string Status);
