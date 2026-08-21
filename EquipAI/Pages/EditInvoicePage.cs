using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EditInvoicePage : InvoiceFormPage
{
    public EditInvoicePage(IPage page) : base(page) { }

    private ILocator EditTitle => Page.Locator("//h1[@id='invoice-edit-title']");
    private ILocator EditMessage => Page.Locator("//p[@class='invoice-create__lead']");
    private ILocator ApproveBtn => Page.Locator("//button[normalize-space()='Approve']");
    private ILocator RejectBtn => Page.Locator("//button[normalize-space()='Reject']");
    private ILocator SaveAsDraftBtn => Page.Locator("//button[normalize-space()='Save as Draft']");
    private ILocator DraftSavedMessage => Page.Locator("//p[contains(@class,'alert--success') and normalize-space()='Draft saved successfully.']");
    private ILocator Cost1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'cost')]");
    private ILocator Description2Input => Page.Locator("//legend[normalize-space()='Line 2']/following-sibling::div//input[contains(@id,'line-description')]");
    private ILocator Quantity2Input => Page.Locator("//legend[normalize-space()='Line 2']/following-sibling::div//input[contains(@id,'quantity')]");
    private ILocator UnitPrice2Input => Page.Locator("//legend[normalize-space()='Line 2']/following-sibling::div//input[contains(@id,'unit-price')]");
    private ILocator EmissionType2Dropdown => Page.Locator(
        "//legend[normalize-space()='Line 2']/following-sibling::div//*[@id[contains(.,'emission-type')] and (self::select or self::button)]");
    private ILocator Unit2Dropdown => Page.Locator(
        "//legend[normalize-space()='Line 2']/following-sibling::div//*[@id[contains(.,'unit-of-measure')] and (self::select or self::button)]");
    private ILocator Toolbar => Page.Locator("//nav[@class='toolbar']");
    private ILocator RejectDialog => Page.Locator("//div[@role='dialog']");
    private ILocator RejectDialogTitle => Page.Locator("//h2[@id='invoice-reject-dialog-title']");
    private ILocator RejectDialogLabel => Page.Locator("//label[@for='invoice-rejection-reason']");
    private ILocator RejectionReasonInput => Page.Locator("//textarea[@id='invoice-rejection-reason']");
    private ILocator RejectDialogCancelBtn => RejectDialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" });
    private ILocator RejectDialogConfirmBtn => RejectDialog.GetByRole(AriaRole.Button, new() { Name = "Confirm" });

    public async Task WaitForLoadedAsync()
    {
        await EditTitle.WaitForAsync();
        await Toolbar.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await BackBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await HeaderSection.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await LineItemsSection.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

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

    public async Task SelectProjectAsync(string project) =>
        await SelectOptionByTextAsync(ProjectDropdown, project);

    public async Task SelectCurrencyAsync(string currency) =>
        await SelectOptionByTextAsync(CurrencyDropdown, currency);

    public async Task SelectEmissionType1Async(string emissionType) =>
        await SelectOptionByTextAsync(EmissionType1Dropdown, emissionType);

    public async Task SelectUnit1Async(string unitOfMeasure) =>
        await SelectOptionByTextAsync(Unit1Dropdown, unitOfMeasure);

    public async Task FillDescription2Async(string description)
    {
        await Description2Input.FillAsync(description);
    }

    public async Task FillQuantity2Async(string quantity)
    {
        await Quantity2Input.FillAsync(quantity);
    }

    public async Task FillUnitPrice2Async(string unitPrice)
    {
        await UnitPrice2Input.FillAsync(unitPrice);
    }

    public async Task SelectEmissionType2Async(string emissionType) =>
        await SelectOptionByTextAsync(EmissionType2Dropdown, emissionType);

    public async Task SelectUnit2Async(string unitOfMeasure) =>
        await SelectOptionByTextAsync(Unit2Dropdown, unitOfMeasure);

    public async Task ClickSaveAsDraftBtnAsync()
    {
        await SaveAsDraftBtn.ClickAsync();
    }

    public async Task WaitForDraftSavedMessageAsync()
    {
        await DraftSavedMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public Task<bool> IsDraftSavedMessageVisibleAsync() => DraftSavedMessage.IsVisibleAsync();

    public async Task<ViewInvoicePage> SaveAsDraftAsync(string invoiceNumber)
    {
        await ClickSaveAsDraftBtnAsync();
        var invoicesPage = await ClickBackBtnAsync();
        return await invoicesPage.ClickViewBtnAsync(invoiceNumber);
    }

    public async Task<ViewInvoicePage> ClickApproveBtnAsync()
    {
        await ApproveBtn.ClickAsync();
        var viewInvoicePage = new ViewInvoicePage(Page);
        await viewInvoicePage.GetTitleAsync();
        return viewInvoicePage;
    }

    public async Task ClickRejectBtnAsync()
    {
        await RejectBtn.ClickAsync();
        await RejectDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetRejectDialogTitleAsync()
    {
        await RejectDialogTitle.WaitForAsync();
        return (await RejectDialogTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetRejectDialogLabelAsync()
    {
        await RejectDialogLabel.WaitForAsync();
        return (await RejectDialogLabel.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsRejectDialogVisibleAsync() => RejectDialog.IsVisibleAsync();
    public Task<bool> IsRejectionReasonInputVisibleAsync() => RejectionReasonInput.IsVisibleAsync();
    public Task<bool> IsRejectDialogCancelBtnVisibleAsync() => RejectDialogCancelBtn.IsVisibleAsync();
    public Task<bool> IsRejectDialogConfirmBtnVisibleAsync() => RejectDialogConfirmBtn.IsVisibleAsync();

    public async Task<bool> IsRejectDialogConfirmBtnDisabledAsync()
    {
        if (await RejectDialogConfirmBtn.GetAttributeAsync("disabled") is not null)
            return true;

        return !await RejectDialogConfirmBtn.IsEnabledAsync();
    }

    public async Task<bool> IsRejectDialogConfirmBtnEnabledAsync()
    {
        if (await RejectDialogConfirmBtn.GetAttributeAsync("disabled") is not null)
            return false;

        return await RejectDialogConfirmBtn.IsEnabledAsync();
    }

    public async Task FillRejectionReasonAsync(string reason)
    {
        await RejectionReasonInput.FillAsync(reason);
    }

    public async Task ClickRejectDialogCancelBtnAsync()
    {
        await RejectDialogCancelBtn.ClickAsync();
        await RejectDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
    }

    public async Task<ViewInvoicePage> ClickRejectDialogConfirmBtnAsync()
    {
        await RejectDialogConfirmBtn.ClickAsync();
        var viewInvoicePage = new ViewInvoicePage(Page);
        await viewInvoicePage.GetTitleAsync();
        return viewInvoicePage;
    }

    public async Task<string> GetCompanyNameAsync()
    {
        await CompanyNameInput.WaitForAsync();
        return (await CompanyNameInput.InputValueAsync()).Trim();
    }

    public async Task<string> GetAddressAsync()
    {
        await AddressInput.WaitForAsync();
        return (await AddressInput.InputValueAsync()).Trim();
    }

    public async Task<string> GetProjectAsync() => await GetSelectedOptionTextAsync(ProjectDropdown);

    public async Task<string> GetInvoiceDateAsync()
    {
        await InvoiceDateInput.WaitForAsync();
        return (await InvoiceDateInput.InputValueAsync()).Trim();
    }

    public async Task<string> GetInvoiceCategoryAsync() => await GetSelectedOptionTextAsync(InvoiceCategoryDropdown);

    public async Task<string> GetTotalCostAsync()
    {
        await TotalCostInput.WaitForAsync();
        return (await TotalCostInput.InputValueAsync()).Trim();
    }

    public async Task<string> GetCurrencyAsync() => await GetSelectedOptionTextAsync(CurrencyDropdown);

    public async Task<string> GetDescription1Async()
    {
        await Description1Input.WaitForAsync();
        return (await Description1Input.InputValueAsync()).Trim();
    }

    public async Task<string> GetQuantity1Async()
    {
        await Quantity1Input.WaitForAsync();
        return (await Quantity1Input.InputValueAsync()).Trim();
    }

    public async Task<string> GetUnitPrice1Async()
    {
        await UnitPrice1Input.WaitForAsync();
        return (await UnitPrice1Input.InputValueAsync()).Trim();
    }

    public async Task<string> GetCost1Async()
    {
        await Cost1Input.WaitForAsync();
        return (await Cost1Input.InputValueAsync()).Trim();
    }

    public async Task<string> GetEmissionType1Async() => await GetSelectedOptionTextAsync(EmissionType1Dropdown);

    public async Task<string> GetUnit1Async() => await GetSelectedOptionTextAsync(Unit1Dropdown);

    public Task<bool> IsEditTitleVisibleAsync() => EditTitle.IsVisibleAsync();
    public Task<bool> IsEditMessageVisibleAsync() => EditMessage.IsVisibleAsync();
    public Task<bool> IsApproveBtnVisibleAsync() => ApproveBtn.IsVisibleAsync();
    public Task<bool> IsRejectBtnVisibleAsync() => RejectBtn.IsVisibleAsync();
    public Task<bool> IsSaveAsDraftBtnVisibleAsync() => SaveAsDraftBtn.IsVisibleAsync();

    public async Task<InvoicesPage> ClickCancelBtnAsync()
    {
        await CancelBtn.ClickAsync();
        return await ReturnToInvoicesAsync();
    }
}
