using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AddOfficePage : BasePage
{
    public AddOfficePage(IPage page) : base(page) { }

    private ILocator Title => Page.Locator("//h1[contains(@class,'page-title')]");

    public async Task WaitForLoadedAsync()
    {
        await Title.WaitForAsync();
    }

    public new async Task<string> GetPageTitleAsync()
    {
        await Title.WaitForAsync();
        return (await Title.InnerTextAsync()).Trim();
    }
}
