using Microsoft.Playwright;

namespace EquipAI.Pages;

public class FooterPage : BasePage
{
    public FooterPage(IPage page) : base(page) { }

    private ILocator FooterSection => Page.Locator("//footer[@class='footer']");
    private ILocator FooterMessage => Page.Locator("//p[@class='footer__copyright']");
    private ILocator PrivacyPolicyLink => Page.Locator("//li[normalize-space()='Privacy Policy']");
    private ILocator TermsOfServiceLink => Page.Locator("//li[normalize-space()='Terms of Service']");

    public Task<bool> IsFooterSectionVisibleAsync() => FooterSection.IsVisibleAsync();
    public Task<bool> IsPrivacyPolicyLinkVisibleAsync() => PrivacyPolicyLink.IsVisibleAsync();
    public Task<bool> IsTermsOfServiceLinkVisibleAsync() => TermsOfServiceLink.IsVisibleAsync();

    public async Task<string> GetFooterMessageAsync()
    {
        await FooterMessage.WaitForAsync();
        return (await FooterMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }
}
