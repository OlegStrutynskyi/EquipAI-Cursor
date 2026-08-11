using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class DashboardPage : BasePage
{
    public DashboardPage(IPage page) : base(page) { }

    private ILocator Logo => Page.Locator("//div[@class='header-left-bar']//app-logo");
    private ILocator OpenMenuBtn => Page.Locator("//div[@class='header-left-bar']//button[@aria-label='Open menu']");
    private ILocator Title => Page.Locator("//h1/a");
    private ILocator SignedAsText => Page.Locator("//p[@class='app-page-header__welcome']");
    private ILocator WelcomeText => Page.Locator("//p[@class='home__body']");
    private ILocator InvoicesBtn => Page.Locator("//a[normalize-space()='Invoices']");
    private ILocator SignOutBtn => Page.Locator("//button[normalize-space()='Sign out']");

    public async Task OpenAsync()
    {
        if (Page.Url.Contains("login") || !IsOnDashboardHome())
        {
            await Page.GotoAsync(Config.BaseUrl);
        }

        await Logo.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    private bool IsOnDashboardHome()
    {
        var path = new Uri(Page.Url).AbsolutePath.TrimEnd('/');
        return path.Length == 0;
    }

    public Task<bool> IsLogoVisibleAsync() => Logo.IsVisibleAsync();
    public Task<bool> IsOpenMenuBtnVisibleAsync() => OpenMenuBtn.IsVisibleAsync();
    public Task<bool> IsTitleVisibleAsync() => Title.IsVisibleAsync();
    public Task<bool> IsSignedAsTextVisibleAsync() => SignedAsText.IsVisibleAsync();
    public Task<bool> IsWelcomeTextVisibleAsync() => WelcomeText.IsVisibleAsync();
    public Task<bool> IsInvoicesBtnVisibleAsync() => InvoicesBtn.IsVisibleAsync();
    public Task<bool> IsSignOutButtonVisibleAsync() => SignOutBtn.IsVisibleAsync();

    public async Task<string> GetTitleAsync()
    {
        await Title.WaitForAsync();
        return (await Title.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetSignedAsTextAsync()
    {
        await SignedAsText.WaitForAsync();
        return (await SignedAsText.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetWelcomeTextAsync()
    {
        await WelcomeText.WaitForAsync();
        return (await WelcomeText.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task WaitForSignOutButtonAsync()
    {
        await SignOutBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetSignOutButtonTextAsync()
    {
        await WaitForSignOutButtonAsync();
        return (await SignOutBtn.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task ClickSignOutAsync()
    {
        await SignOutBtn.ClickAsync();
        await Page.WaitForURLAsync("**/login**");
    }

    public async Task<InvoicesPage> ClickInvoicesBtnAsync()
    {
        await InvoicesBtn.ClickAsync();
        await Page.WaitForURLAsync("**/invoices**");
        return new InvoicesPage(Page);
    }
}