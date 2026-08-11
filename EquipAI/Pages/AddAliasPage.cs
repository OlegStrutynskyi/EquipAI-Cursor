using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AddAliasPage : BasePage
{
    public AddAliasPage(IPage page) : base(page) { }

    private ILocator ContextDropdown => Page.Locator("//select[@id='alias-context']");
    private ILocator ContextHelpMessage => Page.Locator("//select[@id='alias-context']/../following-sibling::p");
    private ILocator ContextOptions => Page.Locator("//select[@id='alias-context']/option");
    private ILocator AliasTextInput => Page.Locator("//input[@id='alias-text']");
    private ILocator AliasTextError => Page.Locator("//input[@id='alias-text']/following-sibling::span");
    private ILocator TargetKindDropdown => Page.Locator("//select[@id='alias-target-kind']");
    private ILocator TargetKindOptions => Page.Locator("//select[@id='alias-target-kind']/option");
    private ILocator UnitOfMeasureDropdown => Page.Locator("//select[@id='alias-unit']");
    private ILocator UnitOfMeasureOptions => Page.Locator("//select[@id='alias-unit']/option");
    private ILocator UnitOfMeasureError => Page.Locator("//select[@id='alias-unit']/../following-sibling::span");
    private ILocator EmissionTypeDropdown => Page.Locator("//select[@id='alias-emission-type']");
    private ILocator EmissionTypeError => Page.Locator("//select[@id='alias-emission-type']/../following-sibling::span");
    private ILocator FactorSourceTitle => Page.Locator("//legend[normalize-space()='Factor source']");
    private ILocator FactorSourceMessage => Page.Locator("//legend[normalize-space()='Factor source']/following-sibling::p");
    private ILocator FactorSourceError => Page.Locator("//legend[normalize-space()='Factor source']/following-sibling::span");
    private ILocator EpaBtn => Page.Locator("//label[normalize-space()='EPA']");
    private ILocator DefraBtn => Page.Locator("//label[normalize-space()='DEFRA']");
    private ILocator CreateAliasBtn => Page.Locator("//button[normalize-space()='Create alias']");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");

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
    public Task<bool> IsFactorSourceTitleVisibleAsync() => FactorSourceTitle.IsVisibleAsync();
    public Task<bool> IsFactorSourceMessageVisibleAsync() => FactorSourceMessage.IsVisibleAsync();
    public Task<bool> IsEpaBtnVisibleAsync() => EpaBtn.IsVisibleAsync();
    public Task<bool> IsDefraBtnVisibleAsync() => DefraBtn.IsVisibleAsync();
    public Task<bool> IsCreateAliasBtnVisibleAsync() => CreateAliasBtn.IsVisibleAsync();
    public Task<bool> IsCancelBtnVisibleAsync() => CancelBtn.IsVisibleAsync();

    public async Task<IReadOnlyList<string>> GetContextOptionsAsync()
    {
        var options = await ContextOptions.AllInnerTextsAsync();
        return options.Select(option => option.Trim()).Where(option => !string.IsNullOrEmpty(option)).ToList();
    }

    public async Task SelectContextAsync(string optionText)
    {
        await ContextDropdown.WaitForAsync();
        await ContextDropdown.SelectOptionAsync(new SelectOptionValue { Label = optionText });
    }

    public async Task<IReadOnlyList<string>> GetTargetKindOptionsAsync()
    {
        var options = await TargetKindOptions.AllInnerTextsAsync();
        return options.Select(option => option.Trim()).Where(option => !string.IsNullOrEmpty(option)).ToList();
    }

    public async Task SelectTargetKindAsync(string optionText)
    {
        await TargetKindDropdown.WaitForAsync();
        await TargetKindDropdown.SelectOptionAsync(new SelectOptionValue { Label = optionText });
    }

    public async Task<IReadOnlyList<string>> GetUnitOfMeasureOptionsAsync()
    {
        await UnitOfMeasureDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var options = await UnitOfMeasureOptions.AllInnerTextsAsync();
        return options
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrEmpty(option) && !option.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<string> GetContextHelpMessageAsync()
    {
        await ContextHelpMessage.WaitForAsync();
        return (await ContextHelpMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task FillAliasTextAsync(string aliasText)
    {
        await AliasTextInput.FillAsync(aliasText);
    }

    public async Task ClickCreateAliasBtnAsync()
    {
        await CreateAliasBtn.ClickAsync();
    }

    public async Task<string> GetAliasTextErrorAsync() => await GetErrorTextAsync(AliasTextError);
    public async Task<string> GetUnitOfMeasureErrorAsync() => await GetErrorTextAsync(UnitOfMeasureError);
    public async Task<string> GetEmissionTypeErrorAsync() => await GetErrorTextAsync(EmissionTypeError);
    public async Task<string> GetFactorSourceErrorAsync() => await GetErrorTextAsync(FactorSourceError);

    private static async Task<string> GetErrorTextAsync(ILocator errorLocator)
    {
        await errorLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await errorLocator.TextContentAsync())?.Trim() ?? string.Empty;
    }
}

