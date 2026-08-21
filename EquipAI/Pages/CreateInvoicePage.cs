using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class CreateInvoicePage : InvoiceFormPage
{
    public CreateInvoicePage(IPage page) : base(page) { }

    private ILocator CreateInvoiceTitle => Page.Locator("//h1[@id='invoice-create-title']");
    private ILocator CreateInvoiceMessage => Page.Locator("//p[@class='invoice-create__lead']");
    private ILocator CreateInvoiceBtn => Page.Locator("//button[normalize-space()='Create invoice']");
    private ILocator ProjectError => Page.Locator(
        "//*[@id='invoice-project']/ancestor::app-searchable-select/following-sibling::span[contains(@class,'form-hint--error')] | //select[@id='invoice-project']/following-sibling::span");
    private ILocator InvoiceDateError => Page.Locator("//input[@id='invoice-date']/following-sibling::span");
    private ILocator InvoiceCategoryError => Page.Locator(
        "//*[@id='invoice-category']/ancestor::app-searchable-select/following-sibling::span[contains(@class,'form-hint--error')] | //select[@id='invoice-category']/../following-sibling::span");
    private ILocator CurrencyError => Page.Locator(
        "//*[@id='currency-code']/ancestor::app-searchable-select/following-sibling::span[contains(@class,'form-hint--error')] | //select[@id='currency-code']/../following-sibling::span");
    private ILocator Cost1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'line-cost')]");
    private ILocator Emissiontype1Error => Page.Locator(
        "//legend[normalize-space()='Line 1']/following-sibling::div//*[@id[contains(.,'emission-type')]]/ancestor::app-searchable-select/following-sibling::span[contains(@class,'form-hint--error')] | //legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'emission-type')]/../following-sibling::span");
    private ILocator Unit1Error => Page.Locator(
        "//legend[normalize-space()='Line 1']/following-sibling::div//*[@id[contains(.,'unit-of-measure')]]/ancestor::app-searchable-select/following-sibling::span[contains(@class,'form-hint--error')] | //legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'unit-of-measure')]/../following-sibling::span");

    public async Task OpenAsync()
    {
        await Page.GotoAsync(Config.BaseUrl + "invoices/new");
        await CreateInvoiceTitle.WaitForAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await BackBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetTitleAsync()
    {
        await CreateInvoiceTitle.WaitForAsync();
        return (await CreateInvoiceTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessageAsync()
    {
        await CreateInvoiceMessage.WaitForAsync();
        return (await CreateInvoiceMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsCreateInvoiceTitleVisibleAsync() => CreateInvoiceTitle.IsVisibleAsync();
    public Task<bool> IsCreateInvoiceBtnVisibleAsync() => CreateInvoiceBtn.IsVisibleAsync();

    public async Task ClickCreateInvoiceBtnAsync()
    {
        await CreateInvoiceBtn.ClickAsync();
        await InvoiceNumberError.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task ClickProjectDropdownAsync()
    {
        await ProjectDropdown.ClickAsync();
    }

    public async Task<IReadOnlyList<string>> GetProjectOptionsAsync() =>
        await GetDropdownOptionsAsync(ProjectDropdown);

    public async Task FillQuantityAsync(string quantity) => await FillQuantity1Async(quantity);

    public async Task FillUnitPriceAsync(string unitPrice) => await FillUnitPrice1Async(unitPrice);

    public async Task FillInvoiceFormAsync(
        string invoiceNumber,
        string companyName,
        string address,
        string invoiceDate,
        string invoiceCategory,
        string totalCost,
        string currency,
        string description,
        string quantity,
        string unitPrice,
        string project,
        string emissionType,
        string unitOfMeasure)
    {
        await HeaderSection.WaitForAsync();
        await LineItemsSection.WaitForAsync();

        await InvoiceNumberInput.FillAsync(invoiceNumber);
        await CompanyNameInput.FillAsync(companyName);
        await AddressInput.FillAsync(address);
        await InvoiceDateInput.FillAsync(invoiceDate);
        await SelectOptionByTextAsync(InvoiceCategoryDropdown, invoiceCategory);
        await TotalCostInput.FillAsync(totalCost);
        await SelectOptionByTextAsync(CurrencyDropdown, currency);
        await Description1Input.FillAsync(description);
        await Quantity1Input.FillAsync(quantity);
        await UnitPrice1Input.FillAsync(unitPrice);
        await SelectOptionByTextAsync(ProjectDropdown, project);
        await SelectOptionByTextAsync(EmissionType1Dropdown, emissionType);
        await SelectOptionByTextAsync(Unit1Dropdown, unitOfMeasure);

        // Re-apply text fields in case dropdown interactions reset bound inputs.
        await InvoiceNumberInput.FillAsync(invoiceNumber);
        await CompanyNameInput.FillAsync(companyName);
        await AddressInput.FillAsync(address);
        await InvoiceDateInput.FillAsync(invoiceDate);
        await TotalCostInput.FillAsync(totalCost);
        await Description1Input.FillAsync(description);
        await Quantity1Input.FillAsync(quantity);
        await UnitPrice1Input.FillAsync(unitPrice);
    }

    public async Task ClickInvoiceCategoryDropdownAsync()
    {
        await InvoiceCategoryDropdown.ClickAsync();
    }

    public async Task<IReadOnlyList<string>> GetInvoiceCategoryOptionsAsync() =>
        await GetDropdownOptionsAsync(InvoiceCategoryDropdown);

    public async Task ClickCurrencyDropdownAsync()
    {
        await CurrencyDropdown.ClickAsync();
    }

    public async Task<IReadOnlyList<string>> GetCurrencyOptionsAsync() =>
        await GetDropdownOptionsAsync(CurrencyDropdown);

    public async Task ClickEmissionType1DropdownAsync()
    {
        await EmissionType1Dropdown.ClickAsync();
    }

    public async Task<IReadOnlyList<string>> GetEmissionType1OptionsAsync() =>
        await GetDropdownOptionsAsync(EmissionType1Dropdown);

    public async Task ClickUnit1DropdownAsync()
    {
        await Unit1Dropdown.ClickAsync();
    }

    public async Task<IReadOnlyList<string>> GetUnit1OptionsAsync() =>
        await GetDropdownOptionsAsync(Unit1Dropdown);

    public async Task<string> GetProjectErrorAsync() => await GetErrorTextAsync(ProjectError);
    public async Task<string> GetInvoiceDateErrorAsync() => await GetErrorTextAsync(InvoiceDateError);
    public async Task<string> GetInvoiceCategoryErrorAsync() => await GetErrorTextAsync(InvoiceCategoryError);
    public async Task<string> GetCurrencyErrorAsync() => await GetErrorTextAsync(CurrencyError);
    public async Task<string> GetEmissionType1ErrorAsync() => await GetErrorTextAsync(Emissiontype1Error);
    public async Task<string> GetUnit1ErrorAsync() => await GetErrorTextAsync(Unit1Error);

    public async Task<FilledInvoiceFormData> FillAllFieldsAsync(
        string? invoiceNumber = null,
        string? companyName = null,
        string? address = null,
        string? description = null)
    {
        await HeaderSection.WaitForAsync();
        await LineItemsSection.WaitForAsync();

        invoiceNumber ??= $"TEST-{DateTime.UtcNow:yyyyMMddHHmmss}";
        companyName ??= "Test Company";
        address ??= "123 Test Street";
        description ??= "Test line item";
        const string totalCost = "100";
        var invoiceDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        await InvoiceNumberInput.FillAsync(invoiceNumber);
        await CompanyNameInput.FillAsync(companyName);
        await AddressInput.FillAsync(address);
        await InvoiceDateInput.FillAsync(invoiceDate);
        await SelectFirstNonEmptyOptionAsync(InvoiceCategoryDropdown);
        await TotalCostInput.FillAsync(totalCost);
        var currency = await SelectFirstNonEmptyOptionAndGetTextAsync(CurrencyDropdown)
            ?? throw new InvalidOperationException("Currency option was not selected.");

        await Description1Input.FillAsync(description);
        await Quantity1Input.FillAsync("1");
        await UnitPrice1Input.FillAsync("100");
        if (await Cost1Input.CountAsync() > 0)
            await Cost1Input.FillAsync("100");
        var project = await SelectFirstNonEmptyOptionAndGetTextAsync(ProjectDropdown)
            ?? throw new InvalidOperationException("Project option was not selected.");
        await SelectFirstNonEmptyOptionAsync(EmissionType1Dropdown);
        await SelectFirstNonEmptyOptionAsync(Unit1Dropdown);

        // Re-apply text fields in case dropdown interactions reset bound inputs.
        await InvoiceNumberInput.FillAsync(invoiceNumber);
        await CompanyNameInput.FillAsync(companyName);
        await AddressInput.FillAsync(address);
        await Description1Input.FillAsync(description);

        return new FilledInvoiceFormData
        {
            InvoiceNumber = invoiceNumber,
            CompanyName = companyName,
            Project = project,
            InvoiceDate = invoiceDate,
            TotalCost = totalCost,
            Currency = currency,
        };
    }

    public async Task<InvoicesPage> SaveInvoiceAsync()
    {
        if (await SearchableSelectList.IsVisibleAsync()
            || await Page.Locator("button.overlay[aria-label='Close dropdown']").IsVisibleAsync())
        {
            await Page.Keyboard.PressAsync("Escape");
        }

        await CreateInvoiceBtn.ClickAsync();
        return await ReturnToInvoicesAsync();
    }

    public async Task<InvoicesPage> ClickCancelAsync()
    {
        await CancelBtn.ClickAsync();
        return await ReturnToInvoicesAsync();
    }
}
