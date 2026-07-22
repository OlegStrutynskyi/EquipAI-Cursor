using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AddUnitPage : BasePage
{
    public AddUnitPage(IPage page) : base(page) { }

    private ILocator CodeInput => Page.Locator("//input[@id='unit-code']");
    private ILocator CodeError => Page.Locator("//input[@id='unit-code']/following-sibling::span");
    private ILocator DisplayNameInput => Page.Locator("//input[@id='unit-display-name']");
    private ILocator DisplayNameError => Page.Locator("//input[@id='unit-display-name']/following-sibling::span");
    private ILocator CreateUnitBtn => Page.Locator("//button[normalize-space()='Create unit']");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    private ILocator AlertMessage => Page.Locator("//p[@role='alert']");

    public Task<bool> IsCodeInputVisibleAsync() => CodeInput.IsVisibleAsync();
    public Task<bool> IsDisplayNameInputVisibleAsync() => DisplayNameInput.IsVisibleAsync();
    public Task<bool> IsCreateUnitBtnVisibleAsync() => CreateUnitBtn.IsVisibleAsync();
    public Task<bool> IsCancelBtnVisibleAsync() => CancelBtn.IsVisibleAsync();

    public async Task FillCodeAsync(string code)
    {
        await CodeInput.FillAsync(code);
    }

    public async Task FillDisplayNameAsync(string displayName)
    {
        await DisplayNameInput.FillAsync(displayName);
    }

    public async Task ClickCreateUnitBtnAsync()
    {
        await CreateUnitBtn.ClickAsync();
    }

    public async Task<UnitsPage> CreateUnitAsync()
    {
        await CreateUnitBtn.ClickAsync();
        await Page.WaitForURLAsync("**/admin/units");
        var unitsPage = new UnitsPage(Page);
        await unitsPage.WaitForLoadedAsync();
        return unitsPage;
    }

    public async Task<UnitsPage> ClickCancelBtnAsync()
    {
        await CancelBtn.ClickAsync();
        await Page.WaitForURLAsync("**/admin/units");
        var unitsPage = new UnitsPage(Page);
        await unitsPage.WaitForLoadedAsync();
        return unitsPage;
    }

    public async Task<string> GetCodeErrorAsync() => await GetErrorTextAsync(CodeError);
    public async Task<string> GetDisplayNameErrorAsync() => await GetErrorTextAsync(DisplayNameError);

    public async Task<string> GetAlertMessageAsync()
    {
        await AlertMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    private static async Task<string> GetErrorTextAsync(ILocator errorLocator)
    {
        await errorLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await errorLocator.TextContentAsync())?.Trim() ?? string.Empty;
    }
}
