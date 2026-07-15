using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AdministrationPage : BasePage
{
    public AdministrationPage(IPage page) : base(page) { }

    private ILocator AdministrationTitle => Page.Locator("//h1[@class='admin-shell__title']");
    private ILocator AdministrationMessage => Page.Locator("//p[@class='admin-shell__lead']");

    public Task<bool> IsAdministrationTitleVisibleAsync() => AdministrationTitle.IsVisibleAsync();
}
