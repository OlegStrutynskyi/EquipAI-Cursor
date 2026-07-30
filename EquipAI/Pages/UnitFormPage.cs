using Microsoft.Playwright;

namespace EquipAI.Pages;

public abstract class UnitFormPage : BasePage
{
    protected UnitFormPage(IPage page) : base(page) { }

    protected ILocator CodeInput => Page.Locator("//input[@id='unit-code']");
    protected ILocator CodeError => Page.Locator("//input[@id='unit-code']/following-sibling::span");
    protected ILocator DisplayNameInput => Page.Locator("//input[@id='unit-display-name']");
    protected ILocator DisplayNameError => Page.Locator("//input[@id='unit-display-name']/following-sibling::span");
    protected ILocator DimensionDropdown => Page.Locator("//select[@id='unit-dimension']");
    protected ILocator DimensionOptions => Page.Locator("//select[@id='unit-dimension']/option");
    protected ILocator ScaleInput => Page.Locator("//input[@id='unit-scale']");
    protected ILocator ScaleHelpMessage => Page.Locator("//input[@id='unit-scale']/following-sibling::p");
    protected ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    protected ILocator AlertMessage => Page.Locator("//p[@role='alert']");

    public Task<bool> IsCodeInputVisibleAsync() => CodeInput.IsVisibleAsync();
    public Task<bool> IsDisplayNameInputVisibleAsync() => DisplayNameInput.IsVisibleAsync();
    public Task<bool> IsDimensionDropdownVisibleAsync() => DimensionDropdown.IsVisibleAsync();
    public Task<bool> IsScaleInputVisibleAsync() => ScaleInput.IsVisibleAsync();
    public Task<bool> IsScaleHelpMessageVisibleAsync() => ScaleHelpMessage.IsVisibleAsync();
    public Task<bool> IsCancelBtnVisibleAsync() => CancelBtn.IsVisibleAsync();
    public Task<bool> IsCancelBtnEnabledAsync() => CancelBtn.IsEnabledAsync();

    public async Task<string> GetCodeAsync()
    {
        await CodeInput.WaitForAsync();
        return (await CodeInput.InputValueAsync()).Trim();
    }

    public async Task<string> GetDisplayNameAsync()
    {
        await DisplayNameInput.WaitForAsync();
        return (await DisplayNameInput.InputValueAsync()).Trim();
    }

    public async Task<string> GetScaleHelpMessageAsync()
    {
        await ScaleHelpMessage.WaitForAsync();
        return (await ScaleHelpMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<IReadOnlyList<string>> GetDimensionOptionsAsync()
    {
        await DimensionDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var options = await DimensionOptions.AllInnerTextsAsync();
        return options
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrEmpty(option) && !option.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task FillCodeAsync(string code)
    {
        await CodeInput.FillAsync(code);
    }

    public async Task FillDisplayNameAsync(string displayName)
    {
        await DisplayNameInput.FillAsync(displayName);
    }

    public async Task SelectDimensionAsync(string optionText)
    {
        await DimensionDropdown.WaitForAsync();
        await DimensionDropdown.SelectOptionAsync(new SelectOptionValue { Label = optionText });
    }

    public async Task FillScaleAsync(string scale)
    {
        await ScaleInput.FillAsync(scale);
    }

    public async Task ClearCodeAsync() => await CodeInput.FillAsync(string.Empty);
    public async Task ClearDisplayNameAsync() => await DisplayNameInput.FillAsync(string.Empty);

    public async Task<string> GetCodeErrorAsync() => await GetErrorTextAsync(CodeError);
    public async Task<string> GetDisplayNameErrorAsync() => await GetErrorTextAsync(DisplayNameError);

    public async Task<string> GetAlertMessageAsync()
    {
        await AlertMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<UnitsPage> ClickCancelBtnAsync()
    {
        await CancelBtn.ClickAsync();
        return await ReturnToUnitsAsync();
    }

    protected async Task<UnitsPage> ReturnToUnitsAsync()
    {
        await Page.WaitForURLAsync("**/admin/units");
        var unitsPage = new UnitsPage(Page);
        await unitsPage.WaitForLoadedAsync();
        return unitsPage;
    }

    private static async Task<string> GetErrorTextAsync(ILocator errorLocator)
    {
        await errorLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await errorLocator.TextContentAsync())?.Trim() ?? string.Empty;
    }
}
