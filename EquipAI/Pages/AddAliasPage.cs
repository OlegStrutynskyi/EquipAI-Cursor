using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AddAliasPage : BasePage
{
    public AddAliasPage(IPage page) : base(page) { }

    private ILocator ContextDropdown => Page.Locator("#alias-context");
    private ILocator ContextHelpMessage => Page.Locator(
        "//app-searchable-select[@controlid='alias-context']/following-sibling::p");
    private ILocator AliasTextInput => Page.Locator("//input[@id='alias-text']");
    private ILocator AliasTextError => Page.Locator("//input[@id='alias-text']/following-sibling::span");
    private ILocator TargetKindDropdown => Page.Locator("#alias-target-kind");
    private ILocator UnitOfMeasureDropdown => Page.Locator("#alias-unit");
    private ILocator UnitOfMeasureError => Page.Locator(
        "//*[@id='alias-unit']/ancestor::app-searchable-select/following-sibling::span[contains(@class,'form-hint--error')] | //*[@id='alias-unit']/ancestor::*[contains(@class,'form-group')][1]//span[contains(@class,'form-hint--error')]");
    private ILocator EmissionTypeDropdown => Page.Locator("#alias-emission-type");
    private ILocator EmissionTypeError => Page.Locator(
        "//*[@id='alias-emission-type']/ancestor::app-searchable-select/following-sibling::span[contains(@class,'form-hint--error')] | //*[@id='alias-emission-type']/ancestor::*[contains(@class,'form-group')][1]//span[contains(@class,'form-hint--error')]");
    private ILocator TelemetryProjectDropdown => Page.Locator("#alias-project");
    private ILocator TelemetryProjectError => Page.Locator(
        "//*[@id='alias-project']/ancestor::app-searchable-select/following-sibling::span[contains(@class,'form-hint--error')] | //*[@id='alias-project']/ancestor::*[contains(@class,'form-group')][1]//span[contains(@class,'form-hint--error')]");
    private ILocator FactorSourceTitle => Page.Locator("//legend")
        .Filter(new LocatorFilterOptions { HasTextString = "Factor Source" });
    private ILocator FactorSourceMessage => FactorSourceTitle.Locator("xpath=following-sibling::p[1]");
    private ILocator FactorSourceError => FactorSourceTitle.Locator("xpath=following-sibling::span[1]");
    private ILocator EpaBtn => Page.Locator("//label[normalize-space()='EPA']");
    private ILocator DefraBtn => Page.Locator("//label[normalize-space()='DEFRA']");
    private ILocator CreateAliasBtn => Page.Locator("//button[normalize-space()='Create alias']");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    private ILocator SearchableSelectList => Page.Locator("//div[@role='listbox' and contains(@class,'select__list')]");

    public async Task WaitForLoadedAsync()
    {
        await Page.Locator("//h1[contains(@id,'title')]")
            .Filter(new LocatorFilterOptions { HasTextString = "Add alias" })
            .WaitForAsync();
        await AliasTextInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public Task<bool> IsContextDropdownVisibleAsync() => ContextDropdown.IsVisibleAsync();
    public Task<bool> IsAliasTextInputVisibleAsync() => AliasTextInput.IsVisibleAsync();
    public Task<bool> IsTargetKindDropdownVisibleAsync() => TargetKindDropdown.IsVisibleAsync();
    public Task<bool> IsUnitOfMeasureDropdownVisibleAsync() => UnitOfMeasureDropdown.IsVisibleAsync();
    public Task<bool> IsEmissionTypeDropdownVisibleAsync() => EmissionTypeDropdown.IsVisibleAsync();
    public Task<bool> IsTelemetryProjectDropdownVisibleAsync() => TelemetryProjectDropdown.IsVisibleAsync();
    public Task<bool> IsFactorSourceTitleVisibleAsync() => FactorSourceTitle.IsVisibleAsync();
    public Task<bool> IsFactorSourceMessageVisibleAsync() => FactorSourceMessage.IsVisibleAsync();
    public Task<bool> IsEpaBtnVisibleAsync() => EpaBtn.IsVisibleAsync();
    public Task<bool> IsDefraBtnVisibleAsync() => DefraBtn.IsVisibleAsync();
    public Task<bool> IsCreateAliasBtnVisibleAsync() => CreateAliasBtn.IsVisibleAsync();
    public Task<bool> IsCancelBtnVisibleAsync() => CancelBtn.IsVisibleAsync();

    public async Task SelectEpaFactorSourceAsync()
    {
        await EpaBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var epaInput = Page.Locator("//label[normalize-space()='EPA']//input | //input[@type='radio' and (@value='EPA' or @value='epa')]");
        if (await epaInput.CountAsync() > 0)
        {
            await epaInput.First.CheckAsync();
            return;
        }

        await EpaBtn.ClickAsync();
    }

    public async Task<IReadOnlyList<string>> GetContextOptionsAsync() =>
        await GetDropdownOptionsAsync(ContextDropdown);

    public async Task SelectContextAsync(string optionText) =>
        await SelectOptionByTextAsync(ContextDropdown, optionText);

    public async Task<IReadOnlyList<string>> GetTargetKindOptionsAsync() =>
        await GetDropdownOptionsAsync(TargetKindDropdown);

    public async Task SelectTargetKindAsync(string optionText) =>
        await SelectOptionByTextAsync(TargetKindDropdown, optionText);

    public async Task<IReadOnlyList<string>> GetUnitOfMeasureOptionsAsync() =>
        await GetDropdownOptionsAsync(UnitOfMeasureDropdown);

    public async Task<string> GetContextHelpMessageAsync()
    {
        await ContextHelpMessage.WaitForAsync();
        return (await ContextHelpMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task FillAliasTextAsync(string aliasText)
    {
        await AliasTextInput.FillAsync(aliasText);
    }

    public async Task<string> SelectUnitOfMeasureAsync(string code)
    {
        var options = await GetUnitOfMeasureOptionsAsync();
        var option = options.FirstOrDefault(o =>
                o.Equals(code, StringComparison.Ordinal)
                || o.StartsWith(code + " —", StringComparison.Ordinal)
                || o.StartsWith(code + " -", StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Unit of measure '{code}' was not found in the dropdown.");

        await SelectOptionByTextAsync(UnitOfMeasureDropdown, option);
        return option;
    }

    public async Task<string> SelectEmissionTypeAsync(string code)
    {
        var options = await GetDropdownOptionsAsync(EmissionTypeDropdown);
        var option = options.FirstOrDefault(o =>
                o.Equals(code, StringComparison.Ordinal)
                || o.StartsWith(code + " —", StringComparison.Ordinal)
                || o.StartsWith(code + " -", StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Emission type '{code}' was not found in the dropdown.");

        await SelectOptionByTextAsync(EmissionTypeDropdown, option);
        return option;
    }

    public async Task ClickCreateAliasBtnAsync()
    {
        await CreateAliasBtn.ClickAsync();
    }

    public async Task<AliasesPage> CreateAliasAsync()
    {
        await CreateAliasBtn.ClickAsync();
        await Page.WaitForURLAsync(url =>
        {
            var path = new Uri(url).AbsolutePath.TrimEnd('/');
            return path.Equals("/admin/aliases", StringComparison.OrdinalIgnoreCase);
        });
        var aliasesPage = new AliasesPage(Page);
        await aliasesPage.WaitForLoadedAsync();
        return aliasesPage;
    }

    public async Task<AliasesPage> ClickCancelBtnAsync()
    {
        await CancelBtn.ClickAsync();
        await Page.WaitForURLAsync(url =>
        {
            var path = new Uri(url).AbsolutePath.TrimEnd('/');
            return path.Equals("/admin/aliases", StringComparison.OrdinalIgnoreCase);
        });
        var aliasesPage = new AliasesPage(Page);
        await aliasesPage.WaitForLoadedAsync();
        return aliasesPage;
    }

    public async Task<string> GetAliasTextErrorAsync() => await GetErrorTextAsync(AliasTextError);
    public async Task<string> GetUnitOfMeasureErrorAsync() => await GetErrorTextAsync(UnitOfMeasureError);
    public async Task<string> GetEmissionTypeErrorAsync() => await GetErrorTextAsync(EmissionTypeError);
    public async Task<string> GetFactorSourceErrorAsync() => await GetErrorTextAsync(FactorSourceError);
    public async Task<string> GetTelemetryProjectErrorAsync() => await GetErrorTextAsync(TelemetryProjectError);

    private async Task<IReadOnlyList<string>> GetDropdownOptionsAsync(ILocator dropdown)
    {
        await OpenSearchableSelectAsync(dropdown);
        var options = await SearchableSelectList.Locator("[role='option']").AllInnerTextsAsync();
        var result = options
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrEmpty(option)
                             && !option.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            .ToList();

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

        return result;
    }

    private async Task SelectOptionByTextAsync(ILocator dropdown, string optionText)
    {
        await OpenSearchableSelectAsync(dropdown);
        var options = SearchableSelectList.Locator("[role='option']");
        var count = await options.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var option = options.Nth(i);
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
            if (!text.Equals(optionText, StringComparison.OrdinalIgnoreCase))
                continue;

            await option.ClickAsync();
            try
            {
                await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = 5_000,
                });
            }
            catch (TimeoutException)
            {
                // List may close without animation.
            }

            await dropdown.Filter(new LocatorFilterOptions { HasTextString = text }).WaitForAsync(
                new LocatorWaitForOptions { Timeout = 5_000 });
            return;
        }

        throw new InvalidOperationException($"Option '{optionText}' was not found.");
    }

    private async Task OpenSearchableSelectAsync(ILocator dropdown)
    {
        await dropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
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

    private static async Task<string> GetErrorTextAsync(ILocator errorLocator)
    {
        await errorLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await errorLocator.TextContentAsync())?.Trim() ?? string.Empty;
    }
}
