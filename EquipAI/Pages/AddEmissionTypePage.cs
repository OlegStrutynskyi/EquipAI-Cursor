using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AddEmissionTypePage : EmissionTypeFormPage
{
    public AddEmissionTypePage(IPage page) : base(page) { }

    private ILocator CreateEmissionTypeBtn => Page.Locator("//button[normalize-space()='Create emission type']");

    public Task<bool> IsCreateEmissionTypeBtnVisibleAsync() => CreateEmissionTypeBtn.IsVisibleAsync();

    public async Task ClickCreateEmissionTypeBtnAsync()
    {
        await CreateEmissionTypeBtn.ClickAsync();
    }

    public async Task<EmissionTypesPage> CreateEmissionTypeAsync()
    {
        await CreateEmissionTypeBtn.ClickAsync();
        return await ReturnToEmissionTypesAsync();
    }
}
