using Microsoft.Playwright;

namespace EquipAI.Pages;

public abstract class InvoiceFormPage : BasePage
{
    protected InvoiceFormPage(IPage page) : base(page) { }

    protected ILocator BackBtn => Page.Locator("//a[contains(text(),'Back')]");
    protected ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    protected ILocator HeaderSection => Page.Locator("//section[@aria-labelledby='invoice-header-heading']");
    protected ILocator InvoiceNumberInput => Page.Locator("//input[@id='invoice-number']");
    protected ILocator InvoiceNumberError => Page.Locator("//input[@id='invoice-number']/following-sibling::span");
    protected ILocator CompanyNameInput => Page.Locator("//input[@id='company-name']");
    protected ILocator CompanyNameError => Page.Locator("//input[@id='company-name']/following-sibling::span");
    protected ILocator AddressInput => Page.Locator("//input[@id='address']");
    protected ILocator AddressError => Page.Locator("//input[@id='address']/following-sibling::span");
    protected ILocator ProjectDropdown => Page.Locator("#invoice-project");
    protected ILocator InvoiceDateInput => Page.Locator("//input[@id='invoice-date']");
    protected ILocator EmissionCategoryDropdown => Page.Locator("#invoice-category");
    protected ILocator TotalCostInput => Page.Locator("//input[@id='total-cost']");
    protected ILocator TotalCostError => Page.Locator("//input[@id='total-cost']/following-sibling::span");
    protected ILocator CurrencyDropdown => Page.Locator("#currency-code");
    protected ILocator LineItemsSection => Page.Locator("//section[@aria-labelledby='invoice-lines-heading']");
    protected ILocator AddRowBtn => Page.Locator("//button[normalize-space()='Add row']");
    protected ILocator LineNumber => Page.Locator("//legend[@class='invoice-create__line-legend']");
    protected ILocator Description1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'line-description')]");
    protected ILocator Description1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'line-description')]/following-sibling::span");
    protected ILocator Quantity1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'quantity')]");
    protected ILocator Quantity1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'quantity')]/following-sibling::span");
    protected ILocator UnitPrice1Input => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'unit-price')]");
    protected ILocator UnitPrice1Error => Page.Locator("//legend[normalize-space()='Line 1']/following-sibling::div//input[contains(@id,'unit-price')]/following-sibling::span");
    protected ILocator EmissionType1Dropdown => Page.Locator(
        "//legend[normalize-space()='Line 1']/following-sibling::div//*[@id[contains(.,'emission-type')] and (self::select or self::button)]");
    protected ILocator Unit1Dropdown => Page.Locator(
        "//legend[normalize-space()='Line 1']/following-sibling::div//*[@id[contains(.,'unit-of-measure')] and (self::select or self::button)]");
    protected ILocator SearchableSelectList => Page.Locator("//div[@role='listbox' and contains(@class,'select__list')]");

    protected ILocator RemoveRowBtn(int lineNumber) =>
        Page.Locator($"//legend[normalize-space()='Line {lineNumber}']/following-sibling::div//button[normalize-space()='Remove row']");

    public Task<bool> IsBackBtnVisibleAsync() => BackBtn.IsVisibleAsync();
    public Task<bool> IsHeaderSectionVisibleAsync() => HeaderSection.IsVisibleAsync();
    public Task<bool> IsLineItemsSectionVisibleAsync() => LineItemsSection.IsVisibleAsync();
    public Task<bool> IsCancelBtnVisibleAsync() => CancelBtn.IsVisibleAsync();

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

    public async Task ClearInvoiceNumberAsync() => await InvoiceNumberInput.FillAsync(string.Empty);
    public async Task ClearCompanyNameAsync() => await CompanyNameInput.FillAsync(string.Empty);
    public async Task ClearAddressAsync() => await AddressInput.FillAsync(string.Empty);
    public async Task ClearTotalCostAsync() => await TotalCostInput.FillAsync(string.Empty);
    public async Task ClearDescription1Async() => await Description1Input.FillAsync(string.Empty);
    public async Task ClearQuantity1Async() => await Quantity1Input.FillAsync(string.Empty);
    public async Task ClearUnitPrice1Async() => await UnitPrice1Input.FillAsync(string.Empty);

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

    public Task<int> GetLineNumberCountAsync() => LineNumber.CountAsync();

    public async Task<bool> IsRemoveRowBtnDisabledAsync(int lineNumber)
    {
        var removeRowBtn = RemoveRowBtn(lineNumber);
        if (await removeRowBtn.GetAttributeAsync("disabled") is not null)
            return true;

        return !await removeRowBtn.IsEnabledAsync();
    }

    public async Task<bool> IsRemoveRowBtnVisibleAsync(int lineNumber) =>
        await RemoveRowBtn(lineNumber).IsVisibleAsync();

    public async Task<bool> IsAddRowBtnVisibleAsync() => await AddRowBtn.IsVisibleAsync();

    public async Task<bool> IsAddRowBtnEnabledAsync()
    {
        if (await AddRowBtn.GetAttributeAsync("disabled") is not null)
            return false;

        return await AddRowBtn.IsEnabledAsync();
    }

    public async Task<string> GetInvoiceNumberErrorAsync() => await GetErrorTextAsync(InvoiceNumberError);
    public async Task<string> GetCompanyNameErrorAsync() => await GetErrorTextAsync(CompanyNameError);
    public async Task<string> GetAddressErrorAsync() => await GetErrorTextAsync(AddressError);
    public Task<bool> IsAddressErrorVisibleAsync() => AddressError.IsVisibleAsync();
    public async Task<string> GetTotalCostErrorAsync() => await GetErrorTextAsync(TotalCostError);
    public async Task<string> GetDescription1ErrorAsync() => await GetErrorTextAsync(Description1Error);
    public async Task<string> GetQuantity1ErrorAsync() => await GetErrorTextAsync(Quantity1Error);
    public async Task<string> GetUnitPrice1ErrorAsync() => await GetErrorTextAsync(UnitPrice1Error);

    public async Task<InvoicesPage> ClickBackBtnAsync()
    {
        await BackBtn.ClickAsync();
        return await ReturnToInvoicesAsync();
    }

    protected async Task<InvoicesPage> ReturnToInvoicesAsync()
    {
        await Page.WaitForURLAsync(
            url => url.Contains("/invoices", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/invoices/new", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/edit", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 60_000 });

        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.OpenAsync();
        return invoicesPage;
    }

    protected async Task SelectOptionByTextAsync(ILocator dropdown, string optionText)
    {
        await dropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        if (await IsNativeSelectAsync(dropdown))
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

        await OpenSearchableSelectAsync(dropdown);
        var searchableOptions = SearchableSelectList.Locator("[role='option']");
        var count = await searchableOptions.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var option = searchableOptions.Nth(i);
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
            if (!text.Equals(optionText, StringComparison.Ordinal))
                continue;

            await option.ClickAsync();
            return;
        }

        throw new InvalidOperationException($"Option '{optionText}' was not found.");
    }

    protected async Task<string> GetSelectedOptionTextAsync(ILocator dropdown)
    {
        await dropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        if (await IsNativeSelectAsync(dropdown))
        {
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

        var selectedLabel = dropdown.Locator(".select__value:not(.select__value--placeholder), .select__value");
        if (await selectedLabel.CountAsync() > 0)
            return (await selectedLabel.First.TextContentAsync())?.Trim() ?? string.Empty;

        return (await dropdown.TextContentAsync())?.Trim() ?? string.Empty;
    }

    protected static async Task<string> GetErrorTextAsync(ILocator errorLocator)
    {
        await errorLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await errorLocator.TextContentAsync())?.Trim() ?? string.Empty;
    }

    protected async Task SelectFirstNonEmptyOptionAsync(ILocator dropdown)
    {
        await SelectFirstNonEmptyOptionAndGetTextAsync(dropdown);
    }

    protected async Task<IReadOnlyList<string>> GetDropdownOptionsAsync(ILocator dropdown)
    {
        await dropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        if (await IsNativeSelectAsync(dropdown))
        {
            var options = await dropdown.Locator("option").AllInnerTextsAsync();
            return options
                .Select(option => option.Trim())
                .Where(option => !string.IsNullOrEmpty(option)
                                 && !option.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        await OpenSearchableSelectAsync(dropdown);
        var searchableOptions = await SearchableSelectList.Locator("[role='option']").AllInnerTextsAsync();
        return searchableOptions
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrEmpty(option)
                             && !option.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    protected async Task<string?> SelectFirstNonEmptyOptionAndGetTextAsync(ILocator dropdown)
    {
        await dropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        if (await IsNativeSelectAsync(dropdown))
        {
            var options = await dropdown.Locator("option").AllAsync();
            foreach (var option in options)
            {
                var value = await option.GetAttributeAsync("value");
                var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(value) || value.Contains(": null", StringComparison.Ordinal))
                    continue;

                if (IsPlaceholderOption(text))
                    continue;

                await dropdown.SelectOptionAsync(value);
                return text;
            }

            return null;
        }

        await OpenSearchableSelectAsync(dropdown);
        var searchableOptions = SearchableSelectList.Locator("[role='option']");
        var count = await searchableOptions.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var option = searchableOptions.Nth(i);
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
            if (IsPlaceholderOption(text))
                continue;

            await option.ClickAsync();
            return text;
        }

        return null;
    }

    private static bool IsPlaceholderOption(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return true;

        if (text.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            return true;

        if (text.Equals("No project", StringComparison.OrdinalIgnoreCase))
            return true;

        if (text is "—" or "-" or "–")
            return true;

        return false;
    }

    private static async Task<bool> IsNativeSelectAsync(ILocator dropdown)
    {
        var tagName = await dropdown.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
        return tagName == "select";
    }

    private async Task OpenSearchableSelectAsync(ILocator dropdown)
    {
        var expanded = await dropdown.GetAttributeAsync("aria-expanded");
        if (string.Equals(expanded, "true", StringComparison.OrdinalIgnoreCase)
            && await SearchableSelectList.IsVisibleAsync())
            return;

        if (await SearchableSelectList.IsVisibleAsync()
            || await Page.Locator("button.overlay[aria-label='Close dropdown']").IsVisibleAsync())
        {
            await Page.Keyboard.PressAsync("Escape");
            try
            {
                await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = 2_000,
                });
            }
            catch (TimeoutException)
            {
                // Dropdown may already be closed.
            }
        }

        await dropdown.ClickAsync();
        await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }
}
