using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class LoginPage : BasePage
{
    public LoginPage(IPage page) : base(page) { }

    private ILocator LoginTitle => Page.Locator("//h1").First;
    private ILocator SignInBtn => Page.Locator("//button[contains(@class,'login__action')]");

    public async Task OpenAsync()
    {
        await Page.GotoAsync(Config.BaseUrl + "login");
        await SignInBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetTitleAsync()
    {
        await LoginTitle.WaitForAsync();
        return (await LoginTitle.TextContentAsync()) ?? string.Empty;
    }

    public async Task<string> GetSignInBtnTextAsync()
    {
        await SignInBtn.WaitForAsync();
        return (await SignInBtn.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsSignInBtnVisibleAsync() => SignInBtn.IsVisibleAsync();

    public async Task<bool> IsSignInBtnEnabledAsync() => await SignInBtn.IsEnabledAsync();

    public Task ClickLoginBtnAsync() => SignInBtn.ClickAsync();
}
