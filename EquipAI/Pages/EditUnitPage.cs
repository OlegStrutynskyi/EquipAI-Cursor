using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EditUnitPage : UnitFormPage
{
    public EditUnitPage(IPage page) : base(page) { }

    private ILocator Subtitle => Page.Locator("//span[@class='admin-unit-edit__subtitle']");
    private ILocator SaveUnitBtn => Page.Locator("//button[normalize-space()='Save unit']");

    public async Task WaitForLoadedAsync()
    {
        await Page.Locator("//h1[contains(@id,'title')]")
            .Filter(new LocatorFilterOptions { HasTextString = "Edit unit" })
            .WaitForAsync();
    }

    public async Task<string> GetSubtitleAsync()
    {
        await Subtitle.WaitForAsync();
        return (await Subtitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task ClickSaveUnitBtnAsync()
    {
        await SaveUnitBtn.ClickAsync();
    }

    public async Task<UnitsPage> SaveUnitAsync()
    {
        await SaveUnitBtn.ClickAsync();
        return await ReturnToUnitsAsync();
    }

    public Task<bool> IsSaveUnitBtnVisibleAsync() => SaveUnitBtn.IsVisibleAsync();
    public Task<bool> IsSaveUnitBtnEnabledAsync() => SaveUnitBtn.IsEnabledAsync();
}
