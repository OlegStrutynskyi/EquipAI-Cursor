using Microsoft.Playwright;

namespace EquipAI.Pages;

public class ProjectsPage : BasePage
{
    public ProjectsPage(IPage page) : base(page) { }

    private ILocator PageTitle => Page.Locator("//h1[contains(@id,'title')]")
        .Filter(new LocatorFilterOptions { HasTextString = "Projects" });
    private ILocator Grid => Page.Locator("table.table");
    private ILocator CodeCells => Grid.Locator("tbody tr td:nth-child(2)");
    private ILocator NameCells => Grid.Locator("tbody tr td:nth-child(3)");
    private ILocator NextPageBtn => Page.Locator(
        "//nav[contains(@class,'pagination')]//button[@aria-label='Next page' or normalize-space()='Next']");

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
        await Grid.Locator("xpath=.//tbody/tr/td[normalize-space()!='']")
            .First
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<IReadOnlyList<(string Code, string Name)>> GetAllCodesAndNamesAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var results = new List<(string Code, string Name)>();
        while (true)
        {
            var codes = await CodeCells.AllInnerTextsAsync();
            var names = await NameCells.AllInnerTextsAsync();
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i].Trim();
                if (string.IsNullOrEmpty(code))
                    continue;
                results.Add((code, names[i].Trim()));
            }

            if (!await IsNextPageEnabledAsync())
                break;

            var firstCodeBefore = codes.FirstOrDefault()?.Trim() ?? string.Empty;
            await NextPageBtn.ClickAsync();
            if (!string.IsNullOrEmpty(firstCodeBefore))
            {
                await Grid.Locator($"tbody tr td:nth-child(2)")
                    .Filter(new LocatorFilterOptions { HasTextString = firstCodeBefore })
                    .First
                    .WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Detached,
                        Timeout = 10_000,
                    });
            }
            else
            {
                await Page.WaitForTimeoutAsync(500);
            }
        }

        return results;
    }

    private async Task<bool> IsNextPageEnabledAsync()
    {
        if (await NextPageBtn.CountAsync() == 0)
            return false;
        return await NextPageBtn.IsEnabledAsync();
    }
}
