using Microsoft.Playwright;

namespace EquipAI.Pages;

public abstract class UnitFormPage : BasePage
{
    protected UnitFormPage(IPage page) : base(page) { }

    protected ILocator CodeInput => Page.Locator("//input[@id='unit-code']");
    protected ILocator CodeError => Page.Locator("//input[@id='unit-code']/following-sibling::span");
    protected ILocator DisplayNameInput => Page.Locator("//input[@id='unit-display-name']");
    protected ILocator DisplayNameError => Page.Locator("//input[@id='unit-display-name']/following-sibling::span");
    protected ILocator DimensionDropdown => Page.Locator("#unit-dimension");
    protected ILocator DimensionList => Page.Locator("//div[@role='listbox' and contains(@class,'select__list')]");
    protected ILocator DimensionOptions => DimensionList.Locator("[role='option']");
    protected ILocator ScaleInput => Page.Locator("//input[@id='unit-scale']");
    protected ILocator ScaleHelpMessage => Page.Locator("//input[@id='unit-scale']/following-sibling::p");
    protected ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    protected ILocator AlertMessage => Page.Locator("//div[@class='alert__content']");

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
        await OpenDimensionListAsync();
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
        await OpenDimensionListAsync();
        var options = await DimensionOptions.AllAsync();
        foreach (var option in options)
        {
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
            if (!text.Equals(optionText, StringComparison.Ordinal))
                continue;

            await option.ClickAsync();
            return;
        }

        throw new InvalidOperationException($"Dimension option '{optionText}' was not found.");
    }

    private async Task OpenDimensionListAsync()
    {
        await DimensionDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var expanded = await DimensionDropdown.GetAttributeAsync("aria-expanded");
        if (string.Equals(expanded, "true", StringComparison.OrdinalIgnoreCase)
            && await DimensionList.IsVisibleAsync())
            return;

        if (await DimensionList.IsVisibleAsync()
            || await Page.Locator("button.overlay[aria-label='Close dropdown']").IsVisibleAsync())
        {
            await Page.Keyboard.PressAsync("Escape");
            try
            {
                await DimensionList.WaitForAsync(new LocatorWaitForOptions
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

        await DimensionDropdown.ClickAsync();
        await DimensionList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
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
