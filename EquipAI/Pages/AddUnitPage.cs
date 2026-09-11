using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AddUnitPage : UnitFormPage
{
    public AddUnitPage(IPage page) : base(page) { }

    private ILocator CreateUnitBtn => Page.Locator("//button[normalize-space()='Create unit']");

    public Task<bool> IsCreateUnitBtnVisibleAsync() => CreateUnitBtn.IsVisibleAsync();

    public async Task ClickCreateUnitBtnAsync()
    {
        await CreateUnitBtn.ClickAsync();
    }

    public async Task<UnitsPage> CreateUnitAsync()
    {
        await CreateUnitBtn.ClickAsync();
        await Page.WaitForURLAsync("**/admin/units");
        await Page.ReloadAsync();
        var unitsPage = new UnitsPage(Page);
        await unitsPage.WaitForLoadedAsync();
        return unitsPage;
    }
}
