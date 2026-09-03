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
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return await Grid.Locator("tbody tr").CountAsync();
    }

    public async Task<(string DisplayName, string GhgScope)> GetFirstRowDisplayNameAndScopeAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var firstRow = Grid.Locator("tbody tr").First;
        await firstRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var displayName = (await firstRow.Locator("td").Nth(0).TextContentAsync())?.Trim() ?? string.Empty;
        var ghgScope = (await firstRow.Locator("td").Nth(1).TextContentAsync())?.Trim() ?? string.Empty;
        return (displayName, ghgScope);
    }

    public async Task<bool> IsEditButtonVisibleForRowAsync(string displayName, string ghgScope)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var row = Grid.Locator(
            $"xpath=.//tbody/tr[td[1][normalize-space()='{displayName}'] and td[2][normalize-space()='{ghgScope}']]");
        if (await row.CountAsync() == 0)
            return false;

        return await row.First.GetByRole(AriaRole.Button, new() { Name = "Edit" }).IsVisibleAsync();
    }

    public async Task<bool> DoesEachRowHaveEditButtonAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var rows = Grid.Locator("tbody tr");
        var rowCount = await rows.CountAsync();
        for (var i = 0; i < rowCount; i++)
        {
            var editButton = rows.Nth(i).GetByRole(AriaRole.Button, new() { Name = "Edit" });
            if (await editButton.CountAsync() == 0 || !await editButton.First.IsVisibleAsync())
                return false;
        }

        return true;
    }

    public async Task<EditEmissionCategoryPage> ClickEditBtnAsync(string displayName, string ghgScope)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var row = Grid.Locator(
            $"xpath=.//tbody/tr[td[1][normalize-space()='{displayName}'] and td[2][normalize-space()='{ghgScope}']]");
        await row.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        var editEmissionCategoryPage = new EditEmissionCategoryPage(Page);
        await editEmissionCategoryPage.WaitForLoadedAsync();
        return editEmissionCategoryPage;
    }

    public async Task<EditEmissionCategoryPage> ClickEditBtnInFirstRowAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var firstRow = Grid.Locator("tbody tr").First;
        await firstRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await firstRow.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        var editEmissionCategoryPage = new EditEmissionCategoryPage(Page);
        await editEmissionCategoryPage.WaitForLoadedAsync();
        return editEmissionCategoryPage;
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
