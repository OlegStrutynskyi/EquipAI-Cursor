using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AliasesPage : BasePage
{
    public AliasesPage(IPage page) : base(page) { }

    private ILocator Message => Page.Locator("//p[@class='admin-aliases__lead']");
    private ILocator AddAliasBtn => Page.Locator("//button[normalize-space()='Add alias']");
    private ILocator UnitsOfMeasureTab => Page.Locator("//button[normalize-space()='Units of measure']");
    private ILocator EmissionTypesTab => Page.Locator("//button[normalize-space()='Emission types']");
    private ILocator Grid => Page.Locator("//table[@class='table admin-aliases__table']");

    public async Task OpenAsync()
    {
        var sideMenuPage = new SideMenuPage(Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickAliasesAsync();
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await Message.WaitForAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetMessageAsync()
    {
        await Message.WaitForAsync();
        return (await Message.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsAddAliasBtnVisibleAsync() => AddAliasBtn.IsVisibleAsync();
    public Task<bool> IsAddAliasBtnEnabledAsync() => AddAliasBtn.IsEnabledAsync();
    public Task<bool> IsGridVisibleAsync() => Grid.IsVisibleAsync();

    public async Task ClickUnitsOfMeasureTabAsync()
    {
        await UnitsOfMeasureTab.ClickAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Resolves to" })
            .WaitForAsync();
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Factor source" })
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }

    public async Task ClickEmissionTypesTabAsync()
    {
        await EmissionTypesTab.ClickAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Factor source" })
            .WaitForAsync();
    }

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await Grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header)
                             && !header.Equals("ACTIONS", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<AddAliasPage> ClickAddAliasBtnAsync()
    {
        await AddAliasBtn.ClickAsync();
        var addAliasPage = new AddAliasPage(Page);
        await addAliasPage.WaitForLoadedAsync();
        return addAliasPage;
    }
}
