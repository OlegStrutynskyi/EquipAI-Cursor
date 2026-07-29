using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AdministrationPage : BasePage
{
    public AdministrationPage(IPage page) : base(page) { }

    private ILocator Title => Page.Locator("//h1[@class='admin-shell__title']");
    private ILocator Message => Page.Locator("//p[@class='admin-shell__lead']");
    private ILocator UsersTab => Page.Locator("//span[normalize-space()='Users']");
    private ILocator ProjectsTab => Page.Locator("//span[normalize-space()='Projects']");
    private ILocator UnitsTab => Page.Locator("//span[normalize-space()='Units']");
    private ILocator EmissionTypesTab => Page.Locator("//span[normalize-space()='Emission types']");
    private ILocator AliasesTab => Page.Locator("//span[normalize-space()='Aliases']");

    public async Task OpenAsync()
    {
        var dashboardPage = new DashboardPage(Page);
        await dashboardPage.OpenAsync();
        await dashboardPage.ClickAdministrationBtnAsync();
        await Title.WaitForAsync();
    }

    public async Task<string> GetTitleAsync()
    {
        await Title.WaitForAsync();
        return (await Title.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessageAsync()
    {
        await Message.WaitForAsync();
        return (await Message.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTitleVisibleAsync() => Title.IsVisibleAsync();
    public Task<bool> IsAdministrationTitleVisibleAsync() => Title.IsVisibleAsync();
    public Task<bool> IsMessageVisibleAsync() => Message.IsVisibleAsync();
    public Task<bool> IsUsersTabVisibleAsync() => UsersTab.IsVisibleAsync();
    public Task<bool> IsProjectsTabVisibleAsync() => ProjectsTab.IsVisibleAsync();
    public Task<bool> IsUnitsTabVisibleAsync() => UnitsTab.IsVisibleAsync();
    public Task<bool> IsEmissionTypesTabVisibleAsync() => EmissionTypesTab.IsVisibleAsync();
    public Task<bool> IsAliasesTabVisibleAsync() => AliasesTab.IsVisibleAsync();

    public async Task ClickUsersTabAsync()
    {
        await UsersTab.ClickAsync();
        await Page.WaitForURLAsync("**/admin/users**");
    }

    public async Task ClickProjectsTabAsync()
    {
        await ProjectsTab.ClickAsync();
        await Page.WaitForURLAsync("**/admin/projects**");
    }

    public async Task<UnitsPage> ClickUnitsTabAsync()
    {
        await UnitsTab.ClickAsync();
        await Page.WaitForURLAsync("**/admin/units**");
        var unitsPage = new UnitsPage(Page);
        await unitsPage.WaitForLoadedAsync();
        return unitsPage;
    }

    public async Task<EmissionTypesPage> ClickEmissionTypesTabAsync()
    {
        await EmissionTypesTab.ClickAsync();
        await Page.WaitForURLAsync("**/admin/emission-types**");
        var emissionTypesPage = new EmissionTypesPage(Page);
        await emissionTypesPage.WaitForLoadedAsync();
        return emissionTypesPage;
    }

    public async Task ClickAliasesTabAsync()
    {
        await AliasesTab.ClickAsync();
        await Page.WaitForURLAsync("**/admin/aliases**");
    }
}
