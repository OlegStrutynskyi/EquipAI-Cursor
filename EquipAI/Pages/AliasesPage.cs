using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AliasesPage : BasePage
{
    public AliasesPage(IPage page) : base(page) { }

    private ILocator Message => Page.Locator("//p[@class='admin-aliases__lead']");
    private ILocator AddAliasBtn => Page.Locator("//button[normalize-space()='Add alias']");
    private ILocator Grid => Page.Locator("//table[@class='admin-aliases__table']");

    public async Task OpenAsync()
    {
        var administrationPage = new AdministrationPage(Page);
        await administrationPage.OpenAsync();
        await administrationPage.ClickAliasesTabAsync();
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

    public async Task<AddAliasPage> ClickAddAliasBtnAsync()
    {
        await AddAliasBtn.ClickAsync();
        var addAliasPage = new AddAliasPage(Page);
        await addAliasPage.WaitForLoadedAsync();
        return addAliasPage;
    }
}
