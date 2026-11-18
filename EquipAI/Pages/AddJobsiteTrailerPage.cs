using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AddJobsiteTrailerPage : AddOfficePage
{
    public AddJobsiteTrailerPage(IPage page) : base(page) { }

    private ILocator Title => Page.Locator("//h1[contains(@class,'page-title')]");

    public new async Task WaitForLoadedAsync() =>
        await Title.Filter(new LocatorFilterOptions { HasText = "Add Jobsite Trailer" }).WaitForAsync();

    public new async Task<string> GetPageTitleAsync()
    {
        var title = Title.Filter(new LocatorFilterOptions { HasText = "Add Jobsite Trailer" });
        await title.WaitForAsync();
        return ((await title.InnerTextAsync()) ?? string.Empty).Trim();
    }
}
