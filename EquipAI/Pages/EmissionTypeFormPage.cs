using Microsoft.Playwright;

namespace EquipAI.Pages;

public abstract class EmissionTypeFormPage : BasePage
{
    protected EmissionTypeFormPage(IPage page) : base(page) { }

    protected ILocator CodeInput => Page.Locator("//input[@id='emission-type-code']");
    protected ILocator CodeError => Page.Locator("//input[@id='emission-type-code']/following-sibling::span");
    protected ILocator DisplayNameInput => Page.Locator("//input[@id='emission-type-display-name']");
    protected ILocator DisplayNameError => Page.Locator("//input[@id='emission-type-display-name']/following-sibling::span");
    protected ILocator DefaultUnitDropdown => Page.Locator("//select[@id='emission-type-default-unit']");
    protected ILocator DefaultUnitOptions => Page.Locator("//select[@id='emission-type-default-unit']/option");
    protected ILocator DefaultUnitError => Page.Locator("//select[@id='emission-type-default-unit']/../following-sibling::span");
    protected ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    protected ILocator AlertMessage => Page.Locator("//p[@role='alert']");

    public Task<bool> IsCodeInputVisibleAsync() => CodeInput.IsVisibleAsync();
    public Task<bool> IsDisplayNameInputVisibleAsync() => DisplayNameInput.IsVisibleAsync();
    public Task<bool> IsDefaultUnitDropdownVisibleAsync() => DefaultUnitDropdown.IsVisibleAsync();
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

    public async Task FillCodeAsync(string code)
    {
        await CodeInput.FillAsync(code);
    }

    public async Task FillDisplayNameAsync(string displayName)
    {
        await DisplayNameInput.FillAsync(displayName);
    }

    public async Task SelectDefaultUnitByCodeAsync(string code)
    {
        await DefaultUnitDropdown.WaitForAsync();
        var options = await DefaultUnitDropdown.Locator("option").AllAsync();
        foreach (var option in options)
        {
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
            if (!text.Contains(code, StringComparison.Ordinal))
                continue;

            var value = await option.GetAttributeAsync("value");
            if (string.IsNullOrWhiteSpace(value) || value.Contains("null", StringComparison.Ordinal))
                continue;

            await DefaultUnitDropdown.SelectOptionAsync(value);
            return;
        }

        throw new InvalidOperationException($"Default unit option containing '{code}' was not found.");
    }

    public async Task<IReadOnlyList<string>> GetDefaultUnitOptionsAsync()
    {
        await DefaultUnitDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var options = await DefaultUnitOptions.AllInnerTextsAsync();
        return options
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrEmpty(option) && !option.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task ClearCodeAsync() => await CodeInput.FillAsync(string.Empty);
    public async Task ClearDisplayNameAsync() => await DisplayNameInput.FillAsync(string.Empty);

    public async Task<string> GetCodeErrorAsync() => await GetErrorTextAsync(CodeError);
    public async Task<string> GetDisplayNameErrorAsync() => await GetErrorTextAsync(DisplayNameError);
    public async Task<string> GetDefaultUnitErrorAsync() => await GetErrorTextAsync(DefaultUnitError);

    public async Task<string> GetAlertMessageAsync()
    {
        await AlertMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<EmissionTypesPage> ClickCancelBtnAsync()
    {
        await CancelBtn.ClickAsync();
        return await ReturnToEmissionTypesAsync();
    }

    protected async Task<EmissionTypesPage> ReturnToEmissionTypesAsync()
    {
        await Page.WaitForURLAsync(
            url => url.Contains("/admin/emission-types", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/new", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/edit", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 60_000 });
        var emissionTypesPage = new EmissionTypesPage(Page);
        await emissionTypesPage.WaitForLoadedAsync();
        return emissionTypesPage;
    }

    private static async Task<string> GetErrorTextAsync(ILocator errorLocator)
    {
        await errorLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await errorLocator.TextContentAsync())?.Trim() ?? string.Empty;
    }
}
