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
    private ILocator TotalEmissionsLabel => Page.Locator("//h2[@id='dashboard-emissions-title']");
    private ILocator TotalEmissionsValue => Page.Locator("//article[@aria-labelledby='dashboard-emissions-title']//span[@class='metric__value']");
    private ILocator TotalEmissionsUnit => Page.Locator("//article[@aria-labelledby='dashboard-emissions-title']//span[@class='metric__unit']");
    private ILocator TotalEmissionsScope1Label => Page.Locator("//span[normalize-space()='Scope 1']");
    private ILocator TotalEmissionsScope1Value => Page.Locator("//dd[@class='stat__value stat__value--scope-1']");
    private ILocator TotalEmissionsScope2Label => Page.Locator("//span[normalize-space()='Scope 2']");
    private ILocator TotalEmissionsScope2Value => Page.Locator("//dd[@class='stat__value stat__value--scope-2']");
    private ILocator TotalEmissionsScope3Label => Page.Locator("//span[normalize-space()='Scope 3']");
    private ILocator TotalEmissionsScope3Value => Page.Locator("//dd[@class='stat__value stat__value--scope-3']");
    private ILocator TotalActiveProjectsCard => Page.Locator("//article[@aria-labelledby='dashboard-projects-title']");
    private ILocator TotalProjectsTitle => Page.Locator("#dashboard-projects-title");
    private ILocator TotalProjectsValue => Page.Locator("//article[@aria-labelledby='dashboard-projects-title']//span[@class='metric__value']");
    private ILocator TotalProjectsReportingLabel => Page.Locator("//span[normalize-space()='Reporting']");
    private ILocator TotalProjectsReportingValue => Page.Locator("//dd[@class='stat__value stat__value--primary']");
    private ILocator TotalProjectsNonReportingLabel => Page.Locator("//span[normalize-space()='Non Reporting']");
    private ILocator TotalProjectsNonReportingValue => Page.Locator("//dd[@class='stat__value stat__value--danger']");
    private ILocator TotalCarbonEmissionsByScopeCard => Page.Locator("//section[@class='dashboard__chart']");
    private ILocator ActiveProjectsCard => Page.Locator("//section[@class='dashboard__panel']");
    private ILocator ActiveProjectsLabel => Page.Locator("//h2[@id='dashboard-projects-heading']");
    private ILocator ActiveProjectsMessage => Page.Locator("//section[@class='dashboard__panel']//p[@class='section-header__lead']");
    private ILocator ProjectsList => Page.Locator("//div[@class='card card--subtle dashboard__panel-card']");
    private ILocator ProjectsUl => Page.Locator("//ul[@class='projects']");
    private ILocator ProjectNameLinks => Page.Locator("//p[@class='projects__name']/a");
    private ILocator ProjectTonnesValues => Page.Locator("//span[@class='projects__tonnes']");
    private ILocator ViewAllProjectsLink => Page.Locator("//a[normalize-space()='View all projects']");
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

    public async Task<string> GetTotalEmissionsLabelAsync()
    {
        await TotalEmissionsLabel.WaitForAsync();
        return (await TotalEmissionsLabel.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalEmissionsValueVisibleAsync() => TotalEmissionsValue.IsVisibleAsync();

    public async Task<string> GetTotalEmissionsValueAsync()
    {
        await TotalEmissionsValue.WaitForAsync();
        return (await TotalEmissionsValue.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetTotalEmissionsUnitAsync()
    {
        await TotalEmissionsUnit.WaitForAsync();
        return (await TotalEmissionsUnit.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalEmissionsScope1LabelVisibleAsync() => TotalEmissionsScope1Label.IsVisibleAsync();
    public Task<bool> IsTotalEmissionsScope1ValueVisibleAsync() => TotalEmissionsScope1Value.IsVisibleAsync();

    public async Task<string> GetTotalEmissionsScope1ValueAsync()
    {
        await TotalEmissionsScope1Value.WaitForAsync();
        return (await TotalEmissionsScope1Value.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalEmissionsScope2LabelVisibleAsync() => TotalEmissionsScope2Label.IsVisibleAsync();
    public Task<bool> IsTotalEmissionsScope2ValueVisibleAsync() => TotalEmissionsScope2Value.IsVisibleAsync();

    public async Task<string> GetTotalEmissionsScope2ValueAsync()
    {
        await TotalEmissionsScope2Value.WaitForAsync();
        return (await TotalEmissionsScope2Value.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalEmissionsScope3LabelVisibleAsync() => TotalEmissionsScope3Label.IsVisibleAsync();
    public Task<bool> IsTotalEmissionsScope3ValueVisibleAsync() => TotalEmissionsScope3Value.IsVisibleAsync();

    public async Task<string> GetTotalEmissionsScope3ValueAsync()
    {
        await TotalEmissionsScope3Value.WaitForAsync();
        return (await TotalEmissionsScope3Value.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetTotalProjectsTitleAsync()
    {
        await TotalProjectsTitle.WaitForAsync();
        return (await TotalProjectsTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalProjectsValueVisibleAsync() => TotalProjectsValue.IsVisibleAsync();

    public async Task<string> GetTotalProjectsValueAsync()
    {
        await TotalProjectsValue.WaitForAsync();
        return (await TotalProjectsValue.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalProjectsReportingLabelVisibleAsync() => TotalProjectsReportingLabel.IsVisibleAsync();
    public Task<bool> IsTotalProjectsReportingValueVisibleAsync() => TotalProjectsReportingValue.IsVisibleAsync();

    public async Task<string> GetTotalProjectsReportingValueAsync()
    {
        await TotalProjectsReportingValue.WaitForAsync();
        return (await TotalProjectsReportingValue.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalProjectsNonReportingLabelVisibleAsync() => TotalProjectsNonReportingLabel.IsVisibleAsync();
    public Task<bool> IsTotalProjectsNonReportingValueVisibleAsync() => TotalProjectsNonReportingValue.IsVisibleAsync();

    public async Task<string> GetTotalProjectsNonReportingValueAsync()
    {
        await TotalProjectsNonReportingValue.WaitForAsync();
        return (await TotalProjectsNonReportingValue.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetActiveProjectsLabelAsync()
    {
        await ActiveProjectsLabel.WaitForAsync();
        return (await ActiveProjectsLabel.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetActiveProjectsMessageAsync()
    {
        await ActiveProjectsMessage.WaitForAsync();
        return (await ActiveProjectsMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsProjectsListVisibleAsync() => ProjectsList.IsVisibleAsync();
    public Task<bool> IsViewAllProjectsLinkVisibleAsync() => ViewAllProjectsLink.IsVisibleAsync();

    public async Task<bool> IsActiveProjectsListEmptyAsync()
    {
        await ProjectsUl.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (true)
        {
            if (await ProjectsUl.Locator("li").CountAsync() == 0)
                return true;

            if (DateTime.UtcNow >= deadline)
                return false;

            await Task.Delay(250);
        }
    }

    public async Task<IReadOnlyList<string>> GetActiveProjectNamesAsync()
    {
        await ProjectsUl.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var names = await ProjectNameLinks.AllInnerTextsAsync();
        return names.Select(name => name.Trim()).ToList();
    }

    public async Task<IReadOnlyList<string>> GetActiveProjectTonnesValuesAsync()
    {
        await ProjectsUl.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var values = await ProjectTonnesValues.AllInnerTextsAsync();
        return values.Select(value => value.Trim()).ToList();
    }

    public async Task<string> GetFirstActiveProjectNameAsync()
    {
        await ProjectNameLinks.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await ProjectNameLinks.First.InnerTextAsync()).Trim();
    }

    public async Task<ProjectDashboardPage> ClickFirstActiveProjectAsync()
    {
        await ProjectNameLinks.First.ClickAsync();
        var projectDashboardPage = new ProjectDashboardPage(Page);
        await projectDashboardPage.WaitForLoadedAsync();
        return projectDashboardPage;
    }

    public async Task<ProjectsPage> ClickViewAllProjectsLinkAsync()
    {
        await ViewAllProjectsLink.ClickAsync();
        var projectsPage = new ProjectsPage(Page);
        await projectsPage.WaitForLoadedAsync();
        return projectsPage;
    }
}
