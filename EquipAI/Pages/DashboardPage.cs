using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class DashboardPage : BasePage
{
    public DashboardPage(IPage page) : base(page) { }

    private ILocator Logo => Page.Locator("//div[@class='header-left-bar']//app-logo");
    private ILocator OpenMenuBtn => Page.Locator("//div[@class='header-left-bar']//button[@aria-label='Open menu']");
    private ILocator DashboardTitle => Page.Locator("#dashboard-title");
    private ILocator Subtitle => Page.Locator("//p[@class='page-header__lead']");
    private ILocator TotalCarbonEmissionCard => Page.Locator("//article[@aria-labelledby='dashboard-emissions-title']");
    private ILocator TotalActiveProjectsCard => Page.Locator("//article[@aria-labelledby='dashboard-projects-title']");
    private ILocator TotalCarbonEmissionsByScopeCard => Page.Locator("//section[@class='dashboard__chart']");
    private ILocator ActiveProjectsCard => Page.Locator("//section[@class='dashboard__panel']");
    private ILocator CarbonEmissionsByCategoryCard => Page.Locator("//section[@class='dashboard__scopes']");
    private ILocator SupportCard => Page.Locator("//app-support-card[@class='dashboard__support']");

    public async Task OpenAsync()
    {
        if (Page.Url.Contains("login") || !IsOnDashboardHome())
        {
            await Page.GotoAsync(Config.BaseUrl);
        }

        await DashboardTitle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Logo.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    private bool IsOnDashboardHome()
    {
        var path = new Uri(Page.Url).AbsolutePath.TrimEnd('/');
        return path.Length == 0;
    }

    public new async Task<string> GetPageTitleAsync()
    {
        await DashboardTitle.WaitForAsync();
        return (await DashboardTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetSubtitleAsync()
    {
        await Subtitle.WaitForAsync();
        return (await Subtitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalCarbonEmissionCardVisibleAsync() => TotalCarbonEmissionCard.IsVisibleAsync();
    public Task<bool> IsTotalActiveProjectsCardVisibleAsync() => TotalActiveProjectsCard.IsVisibleAsync();
    public Task<bool> IsTotalCarbonEmissionsByScopeCardVisibleAsync() => TotalCarbonEmissionsByScopeCard.IsVisibleAsync();
    public Task<bool> IsActiveProjectsCardVisibleAsync() => ActiveProjectsCard.IsVisibleAsync();
    public Task<bool> IsCarbonEmissionsByCategoryCardVisibleAsync() => CarbonEmissionsByCategoryCard.IsVisibleAsync();
    public Task<bool> IsSupportCardVisibleAsync() => SupportCard.IsVisibleAsync();
    public Task<bool> IsLogoVisibleAsync() => Logo.IsVisibleAsync();
    public Task<bool> IsOpenMenuBtnVisibleAsync() => OpenMenuBtn.IsVisibleAsync();
}
