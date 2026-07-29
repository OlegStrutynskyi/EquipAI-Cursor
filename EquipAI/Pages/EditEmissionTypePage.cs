using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EditEmissionTypePage : EmissionTypeFormPage
{
    public EditEmissionTypePage(IPage page) : base(page) { }

    private ILocator Subtitle => Page.Locator("//span[contains(@class,'admin-emission-type-edit__subtitle')]");
    private ILocator SaveEmissionTypeBtn => Page.Locator("//button[normalize-space()='Save emission type']");

    public async Task WaitForLoadedAsync()
    {
        await Page.Locator("//h1[contains(@id,'title')]")
            .Filter(new LocatorFilterOptions { HasTextString = "Edit emission type" })
            .WaitForAsync();
    }

    public async Task<string> GetSubtitleAsync()
    {
        await Subtitle.WaitForAsync();
        return (await Subtitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task ClickSaveEmissionTypeBtnAsync()
    {
        await SaveEmissionTypeBtn.ClickAsync();
    }

    public async Task<EmissionTypesPage> SaveEmissionTypeAsync()
    {
        await SaveEmissionTypeBtn.ClickAsync();
        return await ReturnToEmissionTypesAsync();
    }

    public Task<bool> IsSaveEmissionTypeBtnVisibleAsync() => SaveEmissionTypeBtn.IsVisibleAsync();
    public Task<bool> IsSaveEmissionTypeBtnEnabledAsync() => SaveEmissionTypeBtn.IsEnabledAsync();
}
