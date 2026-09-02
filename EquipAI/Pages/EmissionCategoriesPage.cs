using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EmissionCategoriesPage : BasePage
{
    public EmissionCategoriesPage(IPage page) : base(page) { }

    private ILocator PageTitle => Page.Locator("//h1[contains(@id,'title')]")
        .Filter(new LocatorFilterOptions { HasTextString = "Emission Categories" });
    private ILocator Grid => Page.Locator("table.table");
    private ILocator DisplayNameCells => Grid.Locator("tbody tr td:nth-child(1)");
    private ILocator GhgScopeCells => Grid.Locator("tbody tr td:nth-child(2)");

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
        await Grid.Locator("tbody tr").First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
        });
    }

    public async Task<IReadOnlyList<string>> GetAllDisplayNamesWithGhgScopesAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var displayNames = await DisplayNameCells.AllInnerTextsAsync();
        var scopes = await GhgScopeCells.AllInnerTextsAsync();

        return displayNames
            .Select((name, index) => (DisplayName: name.Trim(), Scope: scopes[index].Trim()))
            .Where(row => !string.IsNullOrEmpty(row.DisplayName))
            .Select(row => $"{row.DisplayName} ({row.Scope})")
            .ToList();
    }
}
