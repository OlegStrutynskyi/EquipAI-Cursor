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
    private ILocator HeaderSection => Page.Locator("//section[@aria-labelledby='invoice-header-heading']");
    private ILocator InvoiceNumberInput => Page.Locator("//input[@id='invoice-number']");
    private ILocator CompanyNameInput => Page.Locator("//input[@id='company-name']");
    private ILocator AddressInput => Page.Locator("//input[@id='address']");
    private ILocator ProjectDropdown => Page.Locator("//select[@id='invoice-project']");
    private ILocator InvoiceDateInput => Page.Locator("//input[@id='invoice-date']");
    private ILocator InvoiceCategoryDropdown => Page.Locator("//select[@id='invoice-category']");
    private ILocator TotalCostInput => Page.Locator("//input[@id='total-cost']");
    private ILocator CurrencyDropdown => Page.Locator("//select[@id='currency-code']");
    private ILocator LineItemsSection => Page.Locator("//section[@aria-labelledby='invoice-lines-heading']");
    private ILocator AddRowBtn => Page.Locator("//button[normalize-space()='Add row']");
    private ILocator Description1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'line-description')]");
    private ILocator Quantity1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'quantity')]");
    private ILocator UnitPrice1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'unit-price')]");
    private ILocator Cost1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'cost')]");
    private ILocator EmissionType1Dropdown => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'emission-type')]");
    private ILocator Unit1Dropdown => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'unit-of-measure')]");
    private ILocator Toolbar => Page.Locator("//nav[contains(@class,'invoice-edit-toolbar')]");

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
}
