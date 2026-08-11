using Microsoft.Playwright;

namespace EquipAI.Pages;

public class SideMenuPage : BasePage
{
    public SideMenuPage(IPage page) : base(page) { }

    private ILocator OpenMenuBtn => Page.Locator("//div[@class='header-left-bar']//button[@aria-label='Open menu']");
    private ILocator PrimaryNav => Page.GetByRole(AriaRole.Navigation, new() { Name = "Primary" });
    private ILocator AccountNav => Page.GetByRole(AriaRole.Navigation, new() { Name = "Account" });
    private ILocator NavItems => Page.Locator(".nav-item");

    private ILocator EnterpriseLink => PrimaryNav.Locator("a.nav-item").Filter(new LocatorFilterOptions { HasTextString = "Enterprise" });
    private ILocator InvoicesLink => PrimaryNav.Locator("a.nav-item").Filter(new LocatorFilterOptions { HasTextString = "Invoices" });
    private ILocator TelemetryLink => PrimaryNav.Locator("a.nav-item").Filter(new LocatorFilterOptions { HasTextString = "Telemetry" });
    private ILocator UsersLink => PrimaryNav.Locator("a.nav-item").Filter(new LocatorFilterOptions { HasTextString = "Users" });
    private ILocator ProjectsLink => PrimaryNav.Locator("a.nav-item").Filter(new LocatorFilterOptions { HasTextString = "Projects" });
    private ILocator UnitsLink => PrimaryNav.Locator("a.nav-item").Filter(new LocatorFilterOptions { HasTextString = "Units" });
    private ILocator EmissionTypesLink => PrimaryNav.Locator("a.nav-item").Filter(new LocatorFilterOptions { HasTextString = "Emission types" });
    private ILocator AliasesLink => PrimaryNav.Locator("a.nav-item").Filter(new LocatorFilterOptions { HasTextString = "Aliases" });
    private ILocator FactorImportLink => PrimaryNav.Locator("a.nav-item").Filter(new LocatorFilterOptions { HasTextString = "Factor import" });
    private ILocator DarkModeLink => AccountNav.Locator("label.nav-item--theme");
    private ILocator DarkModeToggle => AccountNav.Locator("input[aria-label='Toggle dark mode']");
    private ILocator LogoutLink => AccountNav.Locator("button.nav-item").Filter(new LocatorFilterOptions { HasTextString = "Logout" });

    public async Task OpenAsync()
    {
        var dashboardPage = new DashboardPage(Page);
        await dashboardPage.OpenAsync();
        await ClickOpenMenuBtnAsync();
    }

    public async Task ClickOpenMenuBtnAsync()
    {
        if (await PrimaryNav.IsVisibleAsync())
            return;

        await OpenMenuBtn.ClickAsync();
        await PrimaryNav.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await AccountNav.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<IReadOnlyList<string>> GetLinkTextsAsync()
    {
        await PrimaryNav.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var texts = await NavItems.AllInnerTextsAsync();
        return texts
            .Select(text => text.Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();
    }

    public async Task<bool> IsLinkVisibleAsync(string linkText)
    {
        var link = Page.Locator(".nav-item").Filter(new LocatorFilterOptions { HasTextString = linkText });
        return await link.CountAsync() > 0 && await link.First.IsVisibleAsync();
    }

    public Task ClickEnterpriseAsync() => ClickLinkAsync(EnterpriseLink);
    public Task ClickInvoicesAsync() => ClickLinkAsync(InvoicesLink);
    public Task ClickTelemetryAsync() => ClickLinkAsync(TelemetryLink);
    public Task ClickUsersAsync() => ClickLinkAsync(UsersLink);
    public Task ClickProjectsAsync() => ClickLinkAsync(ProjectsLink);
    public Task ClickUnitsAsync() => ClickLinkAsync(UnitsLink);
    public Task ClickEmissionTypesAsync() => ClickLinkAsync(EmissionTypesLink);
    public Task ClickAliasesAsync() => ClickLinkAsync(AliasesLink);
    public Task ClickFactorImportAsync() => ClickLinkAsync(FactorImportLink);

    public async Task ClickDarkModeAsync()
    {
        await DarkModeLink.EvaluateAsync(
            """
            element => {
              let parent = element.parentElement;
              while (parent) {
                if (parent.scrollHeight > parent.clientHeight + 1) {
                  parent.scrollTop = element.offsetTop - parent.clientHeight / 2;
                  break;
                }
                parent = parent.parentElement;
              }
              element.click();
            }
            """);
    }

    public async Task<string> GetBodyBackgroundColorAsync()
    {
        return await Page.EvaluateAsync<string>("() => getComputedStyle(document.body).backgroundColor");
    }

    public Task<bool> IsDarkModeEnabledAsync() => DarkModeToggle.IsCheckedAsync();

    public async Task<LoginPage> ClickLogoutAsync()
    {
        await LogoutLink.EvaluateAsync(
            """
            element => {
              let parent = element.parentElement;
              while (parent) {
                if (parent.scrollHeight > parent.clientHeight + 1) {
                  parent.scrollTop = element.offsetTop - parent.clientHeight / 2;
                  break;
                }
                parent = parent.parentElement;
              }
              element.click();
            }
            """);
        await Page.WaitForURLAsync("**/login**");
        var loginPage = new LoginPage(Page);
        await loginPage.GetTitleAsync();
        return loginPage;
    }

    private async Task ClickLinkAsync(ILocator link)
    {
        var href = await link.GetAttributeAsync("href")
            ?? throw new InvalidOperationException("Side menu link href was not found.");

        await link.EvaluateAsync(
            """
            element => {
              let parent = element.parentElement;
              while (parent) {
                if (parent.scrollHeight > parent.clientHeight + 1) {
                  parent.scrollTop = element.offsetTop - parent.clientHeight / 2;
                  break;
                }
                parent = parent.parentElement;
              }
              element.click();
            }
            """);

        if (href is "/" or "")
        {
            await Page.WaitForURLAsync(url =>
            {
                var path = new Uri(url).AbsolutePath.TrimEnd('/');
                return path.Length == 0;
            });
        }
        else
        {
            await Page.WaitForURLAsync($"**{href}**");
        }

        await GetPageTitleAsync();
    }
}
