using Microsoft.Playwright;

namespace EquipAI.Pages;

public class HeaderPage : BasePage
{
    public HeaderPage(IPage page) : base(page) { }

    private ILocator MenuIcon => Page.Locator("//button[@aria-label='Open menu']//app-menu-toggle-icon");
    private ILocator AppLogo => Page.Locator("//div[@class='header-left-bar']//app-logo");
    private ILocator AskEquipAIBtn => Page.Locator("//button[normalize-space()='Ask EquipAI']");
    private ILocator NotificationIcon => Page.Locator("//div[@class='header-right-bar']//app-notification-icon");
    private ILocator UserName => Page.Locator("//div[@class='user-account-info-text']");
    private ILocator UserRole => Page.Locator("//div[@class='user-account-info-text-role']");
    private ILocator ProfilePicture => Page.Locator("//div[@class='profile-picture-initials']");

    public Task<bool> IsMenuIconVisibleAsync() => MenuIcon.IsVisibleAsync();
    public Task<bool> IsAppLogoVisibleAsync() => AppLogo.IsVisibleAsync();
    public Task<bool> IsAskEquipAIBtnVisibleAsync() => AskEquipAIBtn.IsVisibleAsync();
    public Task<bool> IsNotificationIconVisibleAsync() => NotificationIcon.IsVisibleAsync();
    public Task<bool> IsUserNameVisibleAsync() => UserName.IsVisibleAsync();
    public Task<bool> IsUserRoleVisibleAsync() => UserRole.IsVisibleAsync();
    public Task<bool> IsProfilePictureVisibleAsync() => ProfilePicture.IsVisibleAsync();

    public async Task<SideMenuPage> ClickMenuIconAsync()
    {
        await MenuIcon.ClickAsync();
        return new SideMenuPage(Page);
    }
}
