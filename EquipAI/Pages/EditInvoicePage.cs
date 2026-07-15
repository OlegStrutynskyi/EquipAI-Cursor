using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EditInvoicePage : BasePage
{
    public EditInvoicePage(IPage page) : base(page) { }

    private ILocator EditTitle => Page.Locator("//h1[@id='invoice-edit-title']");
    private ILocator EditMessage => Page.Locator("//p[@class='invoice-create__lead']");
    private ILocator BackBtn => Page.Locator("//a[contains(@class,'invoice-edit-toolbar__button') and contains(normalize-space(),'Back')]");
    private ILocator ApproveBtn => Page.Locator("//button[normalize-space()='Approve']");
    private ILocator RejectBtn => Page.Locator("//button[normalize-space()='Reject']");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    private ILocator SaveAsDraftBtn => Page.Locator("//button[normalize-space()='Save as draft']");

    public async Task<string> GetTitleAsync()
    {
        await EditTitle.WaitForAsync();
        return (await EditTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessageAsync()
    {
        await EditMessage.WaitForAsync();
        return (await EditMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsEditTitleVisibleAsync() => EditTitle.IsVisibleAsync();
    public Task<bool> IsEditMessageVisibleAsync() => EditMessage.IsVisibleAsync();
    public Task<bool> IsBackBtnVisibleAsync() => BackBtn.IsVisibleAsync();
    public Task<bool> IsApproveBtnVisibleAsync() => ApproveBtn.IsVisibleAsync();
    public Task<bool> IsRejectBtnVisibleAsync() => RejectBtn.IsVisibleAsync();
    public Task<bool> IsCancelBtnVisibleAsync() => CancelBtn.IsVisibleAsync();
    public Task<bool> IsSaveAsDraftBtnVisibleAsync() => SaveAsDraftBtn.IsVisibleAsync();
}
