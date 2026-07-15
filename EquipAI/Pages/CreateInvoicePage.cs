using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class CreateInvoicePage : BasePage
{
    public CreateInvoicePage(IPage page) : base(page) { }

    private ILocator CreateInvoiceTitle => Page.Locator("//h1[@id='invoice-create-title']");
    private ILocator CreateInvoiceMessage => Page.Locator("//p[@class='invoice-create__lead']");
    private ILocator BackBtn => Page.Locator("//a[contains(@class,'invoice-edit-toolbar__button') and contains(normalize-space(),'Back')]");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    private ILocator CreateInvoiceBtn => Page.Locator("//button[normalize-space()='Create invoice']");
    private ILocator HeaderSection => Page.Locator("//section[@aria-labelledby='invoice-header-heading']");
    private ILocator InvoiceNumberInput => Page.Locator("//input[@id='invoice-number']");
    private ILocator InvoiceNumberError => Page.Locator("//input[@id='invoice-number']/following-sibling::span");
    private ILocator CompanyNameInput => Page.Locator("//input[@id='company-name']");
    private ILocator CompanyNameError => Page.Locator("//input[@id='company-name']/following-sibling::span");
    private ILocator AddressInput => Page.Locator("//input[@id='address']");
    private ILocator AddressError => Page.Locator("//input[@id='address']/following-sibling::span");
    private ILocator ProjectDropdown => Page.Locator("//select[@id='invoice-project']");
    private ILocator ProjectOptions => Page.Locator("//select[@id='invoice-project']/option");
    private ILocator ProjectError => Page.Locator("//select[@id='invoice-project']/following-sibling::span");
    private ILocator InvoiceDateInput => Page.Locator("//input[@id='invoice-date']");
    private ILocator InvoiceDateError => Page.Locator("//input[@id='invoice-date']/following-sibling::span");
    private ILocator InvoiceCategoryDropdown => Page.Locator("//select[@id='invoice-category']");
    private ILocator InvoiceCategoryOptions => Page.Locator("//select[@id='invoice-category']/option");
    private ILocator InvoiceCategoryError => Page.Locator("//select[@id='invoice-category']/following-sibling::span");
    private ILocator TotalCostInput => Page.Locator("//input[@id='total-cost']");
    private ILocator TotalCostError => Page.Locator("//input[@id='total-cost']/following-sibling::span");
    private ILocator CurrencyDropdown => Page.Locator("//select[@id='currency-code']");
    private ILocator CurrencyError => Page.Locator("//select[@id='currency-code']/following-sibling::span");
    private ILocator CurrencyOptions => Page.Locator("//select[@id='currency-code']/option");
    private ILocator LineItemsSection => Page.Locator("//section[@aria-labelledby='invoice-lines-heading']");
    private ILocator AddRowBtn => Page.Locator("//button[normalize-space()='Add row']");
    private ILocator LineNumber => Page.Locator("//legend[@class='invoice-create__line-legend']");
    private ILocator Description1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'line-description')]");
    private ILocator Description1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'line-description')]/following-sibling::span");
    private ILocator Quantity1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'quantity')]");
    private ILocator Quantity1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'quantity')]/following-sibling::span");
    private ILocator UnitPrice1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'unit-price')]");
    private ILocator UnitPrice1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'unit-price')]/following-sibling::span");
    private ILocator Cost1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'line-cost')]");
    private ILocator EmissionType1Dropdown => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'emission-type')]");
    private ILocator EmissionType1Options => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'emission-type')]/option");
    private ILocator Emissiontype1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'emission-type')]/following-sibling::span");
    private ILocator Unit1Dropdown => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'unit-of-measure')]");
    private ILocator Unit1Options => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'unit-of-measure')]/option");
    private ILocator Unit1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//select[contains(@id,'unit-of-measure')]/following-sibling::span");
    private ILocator RemoveRowBtn1 => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//button[normalize-space()='Remove row']");

    private ILocator RemoveRowBtn(int lineNumber) =>
        Page.Locator($"//legend[normalize-space()='Line {lineNumber}']/following-sibling::div//button[normalize-space()='Remove row']");
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
    public Task<bool> IsBackBtnVisibleAsync() => BackBtn.IsVisibleAsync();
    public Task<bool> IsHeaderSectionVisibleAsync() => HeaderSection.IsVisibleAsync();
    public Task<bool> IsLineItemsSectionVisibleAsync() => LineItemsSection.IsVisibleAsync();
    public Task<bool> IsCancelBtnVisibleAsync() => CancelBtn.IsVisibleAsync();
    public Task<bool> IsCreateInvoiceBtnVisibleAsync() => CreateInvoiceBtn.IsVisibleAsync();

    public async Task ClickCreateInvoiceBtnAsync()
    {
        await CreateInvoiceBtn.ClickAsync();
        await InvoiceNumberError.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
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
    public async Task ClickProjectDropdownAsync()
    {
        await ProjectDropdown.ClickAsync();
    }

    public async Task<IReadOnlyList<string>> GetProjectOptionsAsync()
    {
        var options = await ProjectOptions.AllInnerTextsAsync();
        return options.Select(option => option.Trim()).Where(option => !string.IsNullOrEmpty(option)).ToList();
    }
    public async Task FillDescriptionAsync(string description)
    {
        await Description1Input.FillAsync(description);
    }

    public async Task FillInvoiceDateAsync(string invoiceDate)
    {
        await InvoiceDateInput.FillAsync(invoiceDate);
    }

    public async Task FillTotalCostAsync(string totalCost)
    {
        await TotalCostInput.FillAsync(totalCost);
    }

    public async Task FillQuantityAsync(string quantity)
    {
        await Quantity1Input.FillAsync(quantity);
    }

    public async Task FillUnitPriceAsync(string unitPrice)
    {
        await UnitPrice1Input.FillAsync(unitPrice);
    }

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
    
    public Task<int> GetLineNumberCountAsync() => LineNumber.CountAsync();

    public async Task<bool> IsRemoveRowBtnDisabledAsync(int lineNumber)
    {
        var removeRowBtn = RemoveRowBtn(lineNumber);
        if (await removeRowBtn.GetAttributeAsync("disabled") is not null)
            return true;

        return !await removeRowBtn.IsEnabledAsync();
    }

    public async Task ClickAddRowBtnAsync()
    {
        var currentLineCount = await LineNumber.CountAsync();
        await AddRowBtn.ClickAsync();
        await Page.Locator($"//legend[normalize-space()='Line {currentLineCount + 1}']").WaitForAsync();
    }

    public async Task ClickRemoveRowBtnAsync(int lineNumber)
    {
        await RemoveRowBtn(lineNumber).ClickAsync();
        await Page.Locator($"//legend[normalize-space()='Line {lineNumber}']").WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }

    public async Task<string> GetInvoiceNumberErrorAsync() => await GetErrorTextAsync(InvoiceNumberError);
    public async Task<string> GetCompanyNameErrorAsync() => await GetErrorTextAsync(CompanyNameError);
    public async Task<string> GetAddressErrorAsync() => await GetErrorTextAsync(AddressError);
    public Task<bool> IsAddressErrorVisibleAsync() => AddressError.IsVisibleAsync();
    public async Task<string> GetProjectErrorAsync() => await GetErrorTextAsync(ProjectError);
    public async Task<string> GetInvoiceDateErrorAsync() => await GetErrorTextAsync(InvoiceDateError);
    public async Task<string> GetInvoiceCategoryErrorAsync() => await GetErrorTextAsync(InvoiceCategoryError);
    public async Task<string> GetTotalCostErrorAsync() => await GetErrorTextAsync(TotalCostError);
    public async Task<string> GetCurrencyErrorAsync() => await GetErrorTextAsync(CurrencyError);
    public async Task<string> GetDescription1ErrorAsync() => await GetErrorTextAsync(Description1Error);
    public async Task<string> GetQuantity1ErrorAsync() => await GetErrorTextAsync(Quantity1Error);
    public async Task<string> GetUnitPrice1ErrorAsync() => await GetErrorTextAsync(UnitPrice1Error);
    public async Task<string> GetEmissionType1ErrorAsync() => await GetErrorTextAsync(Emissiontype1Error);
    public async Task<string> GetUnit1ErrorAsync() => await GetErrorTextAsync(Unit1Error);

    private static async Task<string> GetErrorTextAsync(ILocator errorLocator)
    {
        await errorLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await errorLocator.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<InvoicesPage> ClickBackBtnAsync()
    {
        await BackBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/invoices", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/invoices/new", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 60_000 });

        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.OpenAsync();
        return invoicesPage;
    }

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
        await Page.WaitForURLAsync(
            url => url.Contains("/invoices", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/invoices/new", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 60_000 });

        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.OpenAsync();
        return invoicesPage;
    }

    public async Task<InvoicesPage> ClickCancelAsync()
    {
        await CancelBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/invoices", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/invoices/new", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 60_000 });

        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.OpenAsync();
        return invoicesPage;
    }

    private static async Task SelectFirstNonEmptyOptionAsync(ILocator dropdown)
    {
        await SelectFirstNonEmptyOptionAndGetTextAsync(dropdown);
    }

    private static async Task SelectOptionByTextAsync(ILocator dropdown, string optionText)
    {
        var options = await dropdown.Locator("option").AllAsync();
        foreach (var option in options)
        {
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
            if (!text.Equals(optionText, StringComparison.Ordinal))
                continue;

            var value = await option.GetAttributeAsync("value");
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Option '{optionText}' has no value.");

            await dropdown.SelectOptionAsync(value);
            return;
        }

        throw new InvalidOperationException($"Option '{optionText}' was not found.");
    }

    private static async Task<string?> SelectFirstNonEmptyOptionAndGetTextAsync(ILocator dropdown)
    {
        var options = await dropdown.Locator("option").AllAsync();
        foreach (var option in options)
        {
            var value = await option.GetAttributeAsync("value");
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(value) || value.Contains(": null", StringComparison.Ordinal))
                continue;

            if (text.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
                continue;

            await dropdown.SelectOptionAsync(value);
            return text;
        }

        return null;
    }
}