using Microsoft.Playwright;

namespace EquipAI.Pages;

public class ProjectDashboardPage : BasePage
{
    public ProjectDashboardPage(IPage page) : base(page) { }

    private ILocator ProjectDashboardTitle => Page.Locator("#project-dashboard-title");

    public async Task WaitForLoadedAsync()
    {
        await ProjectDashboardTitle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public new async Task<string> GetPageTitleAsync()
    {
        await ProjectDashboardTitle.WaitForAsync();
        return (await ProjectDashboardTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }
}
