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
    private ILocator SaveAsDraftBtn => Page.Locator("//button[normalize-space()='Save as Draft']");
    private ILocator DraftSavedMessage => Page.Locator("//p[@class='invoice-create__success' and normalize-space()='Draft saved successfully.']");
    private ILocator HeaderSection => Page.Locator("//section[@aria-labelledby='invoice-header-heading']");
    private ILocator InvoiceNumberInput => Page.Locator("//input[@id='invoice-number']");
    private ILocator InvoiceNumberError => Page.Locator("//input[@id='invoice-number']/following-sibling::span");
    private ILocator CompanyNameInput => Page.Locator("//input[@id='company-name']");
    private ILocator CompanyNameError => Page.Locator("//input[@id='company-name']/following-sibling::span");
    private ILocator AddressInput => Page.Locator("//input[@id='address']");
    private ILocator AddressError => Page.Locator("//input[@id='address']/following-sibling::span");
    private ILocator ProjectDropdown => Page.Locator("//select[@id='invoice-project']");
    private ILocator InvoiceDateInput => Page.Locator("//input[@id='invoice-date']");
    private ILocator InvoiceCategoryDropdown => Page.Locator("//select[@id='invoice-category']");
    private ILocator TotalCostInput => Page.Locator("//input[@id='total-cost']");
    private ILocator TotalCostError => Page.Locator("//input[@id='total-cost']/following-sibling::span");
    private ILocator CurrencyDropdown => Page.Locator("//select[@id='currency-code']");
    private ILocator LineItemsSection => Page.Locator("//section[@aria-labelledby='invoice-lines-heading']");
    private ILocator AddRowBtn => Page.Locator("//button[normalize-space()='Add row']");
    private ILocator Description1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'line-description')]");
    private ILocator Description1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'line-description')]/following-sibling::span");
    private ILocator Quantity1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'quantity')]");
    private ILocator Quantity1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'quantity')]/following-sibling::span");
    private ILocator UnitPrice1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'unit-price')]");
    private ILocator UnitPrice1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'unit-price')]/following-sibling::span");
    private ILocator Cost1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'cost')]");
    private ILocator EmissionType1Dropdown => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'emission-type')]");
    private ILocator Unit1Dropdown => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'unit-of-measure')]");
    private ILocator Description2Input => Page.Locator("//legend[normalize-space()='Line 2']/following-sibling::div//input[contains(@id,'line-description')]");
    private ILocator Quantity2Input => Page.Locator("//legend[normalize-space()='Line 2']/following-sibling::div//input[contains(@id,'quantity')]");
    private ILocator UnitPrice2Input => Page.Locator("//legend[normalize-space()='Line 2']/following-sibling::div//input[contains(@id,'unit-price')]");
    private ILocator EmissionType2Dropdown => Page.Locator("//legend[normalize-space()='Line 2']/following-sibling::div//select[contains(@id,'emission-type')]");
    private ILocator Unit2Dropdown => Page.Locator("//legend[normalize-space()='Line 2']/following-sibling::div//select[contains(@id,'unit-of-measure')]");
    private ILocator LineNumber => Page.Locator("//legend[@class='invoice-create__line-legend']");
    private ILocator Toolbar => Page.Locator("//nav[contains(@class,'invoice-edit-toolbar')]");
    private ILocator RejectDialog => Page.Locator("//div[@role='dialog']");
    private ILocator RejectDialogTitle => Page.Locator("//h2[@id='invoice-reject-dialog-title']");
    private ILocator RejectDialogLabel => Page.Locator("//label[@class='invoice-reject-dialog__label']");
    private ILocator RejectionReasonInput => Page.Locator("//textarea[@id='invoice-rejection-reason']");
    private ILocator RejectDialogCancelBtn => RejectDialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" });
    private ILocator RejectDialogConfirmBtn => RejectDialog.GetByRole(AriaRole.Button, new() { Name = "Confirm" });

    private ILocator RemoveRowBtn(int lineNumber) =>
        Page.Locator($"//legend[normalize-space()='Line {lineNumber}']/following-sibling::div//button[normalize-space()='Remove row']");

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

    public async Task<string> GetInvoiceNumberAsync()
    {
        await InvoiceNumberInput.WaitForAsync();
        return (await InvoiceNumberInput.InputValueAsync()).Trim();
    }

    public async Task FillInvoiceNumberAsync(string invoiceNumber)
    {
        await InvoiceNumberInput.FillAsync(invoiceNumber);
    }

    public async Task FillCompanyNameAsync(string companyName)
    {
        await CompanyNameInput.FillAsync(companyName);
    }

    public async Task FillAddressAsync(string address)
    {
        await AddressInput.FillAsync(address);
    }

    public async Task FillDescriptionAsync(string description)
    {
        await Description1Input.FillAsync(description);
    }

    public async Task FillTotalCostAsync(string totalCost)
    {
        await TotalCostInput.FillAsync(totalCost);
    }

    public async Task FillInvoiceDateAsync(string invoiceDate)
    {
        await InvoiceDateInput.FillAsync(invoiceDate);
    }

    public async Task FillQuantity1Async(string quantity)
    {
        await Quantity1Input.FillAsync(quantity);
    }

    public async Task FillUnitPrice1Async(string unitPrice)
    {
        await UnitPrice1Input.FillAsync(unitPrice);
    }

    public async Task SelectProjectAsync(string project) =>
        await SelectOptionByTextAsync(ProjectDropdown, project);

    public async Task SelectCurrencyAsync(string currency) =>
        await SelectOptionByTextAsync(CurrencyDropdown, currency);

    public async Task SelectEmissionType1Async(string emissionType) =>
        await SelectOptionByTextAsync(EmissionType1Dropdown, emissionType);

    public async Task SelectUnit1Async(string unitOfMeasure) =>
        await SelectOptionByTextAsync(Unit1Dropdown, unitOfMeasure);

    public async Task ClickAddRowBtnAsync()
    {
        var currentLineCount = await LineNumber.CountAsync();
        await AddRowBtn.ClickAsync();
        await Page.Locator($"//legend[normalize-space()='Line {currentLineCount + 1}']").WaitForAsync();
    }

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

    public async Task ClearInvoiceNumberAsync() => await InvoiceNumberInput.FillAsync(string.Empty);

    public async Task ClearCompanyNameAsync() => await CompanyNameInput.FillAsync(string.Empty);

    public async Task ClearAddressAsync() => await AddressInput.FillAsync(string.Empty);

    public async Task ClearTotalCostAsync() => await TotalCostInput.FillAsync(string.Empty);

    public async Task ClearDescription1Async() => await Description1Input.FillAsync(string.Empty);

    public async Task ClearQuantity1Async() => await Quantity1Input.FillAsync(string.Empty);

    public async Task ClearUnitPrice1Async() => await UnitPrice1Input.FillAsync(string.Empty);

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

    public async Task<string> GetInvoiceNumberErrorAsync() => await GetErrorTextAsync(InvoiceNumberError);
    public async Task<string> GetCompanyNameErrorAsync() => await GetErrorTextAsync(CompanyNameError);
    public async Task<string> GetAddressErrorAsync() => await GetErrorTextAsync(AddressError);
    public async Task<string> GetTotalCostErrorAsync() => await GetErrorTextAsync(TotalCostError);
    public async Task<string> GetDescription1ErrorAsync() => await GetErrorTextAsync(Description1Error);
    public async Task<string> GetQuantity1ErrorAsync() => await GetErrorTextAsync(Quantity1Error);
    public async Task<string> GetUnitPrice1ErrorAsync() => await GetErrorTextAsync(UnitPrice1Error);

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

    public async Task<bool> IsAddRowBtnVisibleAsync() => await AddRowBtn.IsVisibleAsync();

    public async Task<bool> IsAddRowBtnEnabledAsync()
    {
        if (await AddRowBtn.GetAttributeAsync("disabled") is not null)
            return false;

        return await AddRowBtn.IsEnabledAsync();
    }

    public async Task<bool> IsRemoveRowBtnVisibleAsync(int lineNumber) =>
        await RemoveRowBtn(lineNumber).IsVisibleAsync();

    public async Task<bool> IsRemoveRowBtnDisabledAsync(int lineNumber)
    {
        var removeRowBtn = RemoveRowBtn(lineNumber);
        if (await removeRowBtn.GetAttributeAsync("disabled") is not null)
            return true;

        return !await removeRowBtn.IsEnabledAsync();
    }

    public Task<bool> IsEditTitleVisibleAsync() => EditTitle.IsVisibleAsync();
    public Task<bool> IsEditMessageVisibleAsync() => EditMessage.IsVisibleAsync();
    public Task<bool> IsBackBtnVisibleAsync() => BackBtn.IsVisibleAsync();
    public Task<bool> IsApproveBtnVisibleAsync() => ApproveBtn.IsVisibleAsync();
    public Task<bool> IsRejectBtnVisibleAsync() => RejectBtn.IsVisibleAsync();
    public Task<bool> IsCancelBtnVisibleAsync() => CancelBtn.IsVisibleAsync();
    public Task<bool> IsSaveAsDraftBtnVisibleAsync() => SaveAsDraftBtn.IsVisibleAsync();
    public Task<bool> IsHeaderSectionVisibleAsync() => HeaderSection.IsVisibleAsync();
    public Task<bool> IsLineItemsSectionVisibleAsync() => LineItemsSection.IsVisibleAsync();

    public async Task<InvoicesPage> ClickBackBtnAsync()
    {
        await BackBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/invoices", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/invoices/new", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/edit", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 60_000 });

        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.OpenAsync();
        return invoicesPage;
    }

    public async Task<InvoicesPage> ClickCancelBtnAsync()
    {
        await CancelBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/invoices", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/invoices/new", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/edit", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 60_000 });

        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.OpenAsync();
        return invoicesPage;
    }

    private static async Task<string> GetSelectedOptionTextAsync(ILocator dropdown)
    {
        await dropdown.WaitForAsync();
        var selectedOption = dropdown.Locator("option:checked");
        if (await selectedOption.CountAsync() == 0)
            selectedOption = dropdown.Locator("option[selected]");

        if (await selectedOption.CountAsync() > 0)
            return (await selectedOption.First.TextContentAsync())?.Trim() ?? string.Empty;

        var value = await dropdown.InputValueAsync();
        var optionByValue = dropdown.Locator($"option[value='{value}']");
        if (await optionByValue.CountAsync() > 0)
            return (await optionByValue.First.TextContentAsync())?.Trim() ?? string.Empty;

        return string.Empty;
    }

    private static async Task SelectOptionByTextAsync(ILocator dropdown, string optionText)
    {
        await dropdown.WaitForAsync();
        await dropdown.SelectOptionAsync(new SelectOptionValue { Label = optionText });
    }

    private static async Task<string> GetErrorTextAsync(ILocator errorLocator)
    {
        await errorLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await errorLocator.TextContentAsync())?.Trim() ?? string.Empty;
    }
}
