using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class UtilityBillUploadPage : BasePage
{
    public UtilityBillUploadPage(IPage page) : base(page) { }

    private ILocator PageTitle => Page.Locator("//h1[@id='invoice-list-title']");
    private ILocator Message => Page.Locator("//p[@class='page-header__lead']");
    private ILocator ImportPDFBtn => Page.Locator("//a[normalize-space()='Import PDF']");
    private ILocator Grid => Page.Locator("//table[contains(@class,'table')]");

    public async Task OpenAsync()
    {
        await Page.GotoAsync(Config.BaseUrl + "utility-bills");
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await PageTitle.WaitForAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetTitleAsync()
    {
        await PageTitle.WaitForAsync();
        return (await PageTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessageAsync()
    {
        await Message.WaitForAsync();
        return (await Message.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsImportPDFBtnVisibleAsync() => ImportPDFBtn.IsVisibleAsync();
    public Task<bool> IsGridVisibleAsync() => Grid.IsVisibleAsync();

    public async Task<ImportPage> ClickImportPDFBtnAsync()
    {
        await ImportPDFBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/utility-bills/import", StringComparison.OrdinalIgnoreCase));
        var importPage = new ImportPage(Page);
        await importPage.WaitForLoadedAsync();
        return importPage;
    }

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await Grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header))
            .ToList();
    }
}
