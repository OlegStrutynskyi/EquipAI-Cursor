using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class CreateInvoicePage : InvoiceFormPage
{
    public CreateInvoicePage(IPage page) : base(page) { }

    private ILocator CreateInvoiceTitle => Page.Locator("//h1[@id='invoice-create-title']");
    private ILocator CreateInvoiceMessage => Page.Locator("//p[@class='invoice-create__lead']");
    private ILocator CreateInvoiceBtn => Page.Locator("//button[normalize-space()='Create invoice']");
    private ILocator ProjectOptions => Page.Locator("//select[@id='invoice-project']/option");
    private ILocator ProjectError => Page.Locator("//select[@id='invoice-project']/following-sibling::span");
    private ILocator InvoiceDateError => Page.Locator("//input[@id='invoice-date']/following-sibling::span");
    private ILocator InvoiceCategoryOptions => Page.Locator("//select[@id='invoice-category']/option");
    private ILocator InvoiceCategoryError => Page.Locator("//select[@id='invoice-category']/following-sibling::span");
    private ILocator CurrencyError => Page.Locator("//select[@id='currency-code']/following-sibling::span");
    private ILocator CurrencyOptions => Page.Locator("//select[@id='currency-code']/option");
    private ILocator Cost1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'line-cost')]");
    private ILocator EmissionType1Options => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'emission-type')]/option");
    private ILocator Emissiontype1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'emission-type')]/following-sibling::span");
    private ILocator Unit1Options => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'unit-of-measure')]/option");
    private ILocator Unit1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'unit-of-measure')]/following-sibling::span");

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

    public async Task<IReadOnlyList<string>> GetProjectOptionsAsync()
    {
        var options = await ProjectOptions.AllInnerTextsAsync();
        return options.Select(option => option.Trim()).Where(option => !string.IsNullOrEmpty(option)).ToList();
    }

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
        await SelectOptionByTextAsync(ProjectDropdown, project);
        await InvoiceDateInput.FillAsync(invoiceDate);
        await SelectOptionByTextAsync(InvoiceCategoryDropdown, invoiceCategory);
        await TotalCostInput.FillAsync(totalCost);
        await SelectOptionByTextAsync(CurrencyDropdown, currency);
        await Description1Input.FillAsync(description);
        await Quantity1Input.FillAsync(quantity);
        await UnitPrice1Input.FillAsync(unitPrice);
        await SelectOptionByTextAsync(EmissionType1Dropdown, emissionType);
        await SelectOptionByTextAsync(Unit1Dropdown, unitOfMeasure);
    }

    public async Task ClickInvoiceCategoryDropdownAsync()
    {
        await InvoiceCategoryDropdown.ClickAsync();
    }

    public async Task<IReadOnlyList<string>> GetInvoiceCategoryOptionsAsync()
    {
        var options = await InvoiceCategoryOptions.AllInnerTextsAsync();
        return options.Select(option => option.Trim()).Where(option => !string.IsNullOrEmpty(option)).ToList();
    }

    public async Task ClickCurrencyDropdownAsync()
    {
        await CurrencyDropdown.ClickAsync();
    }

    public async Task<IReadOnlyList<string>> GetCurrencyOptionsAsync()
    {
        var options = await CurrencyOptions.AllInnerTextsAsync();
        return options.Select(option => option.Trim()).Where(option => !string.IsNullOrEmpty(option)).ToList();
    }

    public async Task ClickEmissionType1DropdownAsync()
    {
        await EmissionType1Dropdown.ClickAsync();
    }

    public async Task<IReadOnlyList<string>> GetEmissionType1OptionsAsync()
    {
        var options = await EmissionType1Options.AllInnerTextsAsync();
        return options.Select(option => option.Trim()).Where(option => !string.IsNullOrEmpty(option)).ToList();
    }

    public async Task ClickUnit1DropdownAsync()
    {
        await Unit1Dropdown.ClickAsync();
    }

    public async Task<IReadOnlyList<string>> GetUnit1OptionsAsync()
    {
        var options = await Unit1Options.AllInnerTextsAsync();
        return options.Select(option => option.Trim()).Where(option => !string.IsNullOrEmpty(option)).ToList();
    }

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
        await CreateInvoiceBtn.ClickAsync();
        return await ReturnToInvoicesAsync();
    }

    public async Task<InvoicesPage> ClickCancelAsync()
    {
        await CancelBtn.ClickAsync();
        return await ReturnToInvoicesAsync();
    }
}
