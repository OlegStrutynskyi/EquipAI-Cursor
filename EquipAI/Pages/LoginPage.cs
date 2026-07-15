using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class LoginPage : BasePage
{
    public LoginPage(IPage page) : base(page) { }

    private ILocator LoginTitle => Page.Locator("//h1").First;
    private ILocator LoginText => Page.Locator("//p[contains(text(),'Choose')]");
    private ILocator LoginBtn => Page.Locator("//button[contains(text(),'Sign in')]");

    public async Task OpenAsync()
    {
        await Page.GotoAsync(Config.BaseUrl + "login");
        await LoginTitle.WaitForAsync();
    }

    public async Task<string> GetTitleAsync()
    {
        await LoginTitle.WaitForAsync();
        return (await LoginTitle.TextContentAsync()) ?? string.Empty;
    }

    public async Task<string> GetTextAsync()
    {
        await LoginText.WaitForAsync();
        return (await LoginText.TextContentAsync()) ?? string.Empty;
    }

    public async Task<string> GetLoginBtnTextAsync()
    {
        await LoginBtn.WaitForAsync();
        return (await LoginBtn.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task ClickLoginBtnAsync() => LoginBtn.ClickAsync();
}
