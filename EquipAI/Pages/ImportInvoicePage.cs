using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class ImportInvoicePage : BasePage
{
    public ImportInvoicePage(IPage page) : base(page) { }

    private ILocator ImportTitle => Page.Locator("//h1[@id='invoice-import-title']");
    private ILocator ImportMessage => Page.Locator("//p[@class='invoice-import__lead']");
    private ILocator ChooseFileBtn => Page.Locator("//label[normalize-space()='Choose file']");
    private ILocator ImportBtn => Page.Locator("//button[normalize-space()='Import']");

    public async Task OpenAsync()
    {
        await Page.GotoAsync(Config.BaseUrl + "invoices/import");
        await ImportTitle.WaitForAsync();
    }

    public async Task<string> GetTitleAsync()
    {
        await ImportTitle.WaitForAsync();
        return (await ImportTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessageAsync()
    {
        await ImportMessage.WaitForAsync();
        return (await ImportMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsImportTitleVisibleAsync() => ImportTitle.IsVisibleAsync();
    public Task<bool> IsImportMessageVisibleAsync() => ImportMessage.IsVisibleAsync();
    public Task<bool> IsChooseFileBtnVisibleAsync() => ChooseFileBtn.IsVisibleAsync();
    public Task<bool> IsImportBtnVisibleAsync() => ImportBtn.IsVisibleAsync();
}
