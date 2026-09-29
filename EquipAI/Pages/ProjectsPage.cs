using Microsoft.Playwright;

namespace EquipAI.Pages;

public class ProjectsPage : BasePage
{
    public ProjectsPage(IPage page) : base(page) { }

    private ILocator PageTitle => Page.Locator("//h1[contains(@id,'title')]")
        .Filter(new LocatorFilterOptions { HasTextString = "Projects" });
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

    private async Task<IReadOnlyList<(string Code, string Name)>> GetCodesAndNamesOnCurrentPageAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var rows = await Grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => !tr.querySelector('.table__skeleton-bar'))
              .map(tr => [...tr.querySelectorAll('td')]
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

        if (!string.IsNullOrEmpty(firstCodeBefore))
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < deadline)
            {
                var firstCodeAfter = (await GetCodesAndNamesOnCurrentPageAsync()).FirstOrDefault().Code ?? string.Empty;
                if (!string.Equals(firstCodeAfter, firstCodeBefore, StringComparison.Ordinal))
                    return true;
                await Task.Delay(200);
            }
        }

        return showingAfter is not null
            && showingBefore is not null
            && showingAfter.Value.Start != showingBefore.Value.Start;
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
