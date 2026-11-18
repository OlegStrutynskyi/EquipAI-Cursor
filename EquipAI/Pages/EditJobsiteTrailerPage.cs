using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EditJobsiteTrailerPage : EditOfficePage
{
    public EditJobsiteTrailerPage(IPage page) : base(page) { }

    private ILocator Title => Page.Locator("//h1[contains(@class,'page-title')]");
    private ILocator DetailsTab => Page.Locator("//button[normalize-space()='Details']");
    private ILocator NameInput => Page.Locator("//input[@id='office-name']");

    public new async Task WaitForLoadedAsync()
    {
        await Title.Filter(new LocatorFilterOptions { HasTextString = "Edit Jobsite Trailer" }).WaitForAsync();
        await DetailsTab.WaitForAsync();
        await NameInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }
}
