using Microsoft.Playwright;

namespace EquipAI.Pages;

public abstract class EmissionTypeFormPage : BasePage
{
    protected EmissionTypeFormPage(IPage page) : base(page) { }

    protected ILocator CodeInput => Page.Locator("//input[@id='emission-type-code']");
    protected ILocator CodeError => Page.Locator("//input[@id='emission-type-code']/following-sibling::span");
    protected ILocator DisplayNameInput => Page.Locator("//input[@id='emission-type-display-name']");
    protected ILocator DisplayNameError => Page.Locator("//input[@id='emission-type-display-name']/following-sibling::span");
    protected ILocator DefaultUnitDropdown => Page.Locator("#emission-type-default-unit");
    protected ILocator DefaultEmissionCategoryDropdown => Page.Locator("#emission-type-default-category");
    protected ILocator SearchableSelectList => Page.Locator("//div[@role='listbox' and contains(@class,'select__list')]");
    protected ILocator DefaultUnitOptions => SearchableSelectList.Locator("[role='option']");
    protected ILocator DefaultUnitError => Page.Locator("//label[normalize-space()='Default Unit of Measure']/following-sibling::span");
    protected ILocator DefaultCategoryError => Page.Locator("//label[normalize-space()='Default Emission Category']/following-sibling::span");
    protected ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    protected ILocator AlertMessage => Page.Locator("//div[@class='alert__body']");

    public Task<bool> IsCodeInputVisibleAsync() => CodeInput.IsVisibleAsync();
    public Task<bool> IsDisplayNameInputVisibleAsync() => DisplayNameInput.IsVisibleAsync();
    public Task<bool> IsDefaultUnitDropdownVisibleAsync() => DefaultUnitDropdown.IsVisibleAsync();
    public Task<bool> IsDefaultEmissionCategoryDropdownVisibleAsync() => DefaultEmissionCategoryDropdown.IsVisibleAsync();
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

    public async Task<string> GetDefaultUnitAsync() => await GetSelectedOptionTextAsync(DefaultUnitDropdown);

    public async Task<string> GetDefaultEmissionCategoryAsync() =>
        await GetSelectedOptionTextAsync(DefaultEmissionCategoryDropdown);

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
        await OpenSearchableSelectAsync(DefaultUnitDropdown);
        var options = await DefaultUnitOptions.AllAsync();
        foreach (var option in options)
        {
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
            if (!(text.Equals(code, StringComparison.Ordinal)
                  || text.StartsWith(code + " —", StringComparison.Ordinal)
                  || text.StartsWith(code + " -", StringComparison.Ordinal)))
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

            return;
        }

        throw new InvalidOperationException($"Default unit option containing '{code}' was not found.");
    }

    public async Task<IReadOnlyList<string>> GetDefaultUnitOptionsAsync()
    {
        await OpenSearchableSelectAsync(DefaultUnitDropdown);
        var options = await DefaultUnitOptions.AllInnerTextsAsync();
        await CloseSearchableSelectAsync();
        return options
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrEmpty(option) && !option.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetDefaultEmissionCategoryOptionsAsync()
    {
        await OpenSearchableSelectAsync(DefaultEmissionCategoryDropdown);
        var options = await DefaultUnitOptions.AllInnerTextsAsync();
        await CloseSearchableSelectAsync();
        return options
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrEmpty(option) && !option.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<string> SelectRandomDefaultEmissionCategoryAsync()
    {
        await OpenSearchableSelectAsync(DefaultEmissionCategoryDropdown);
        var options = await DefaultUnitOptions.AllAsync();
        var validOptions = new List<(ILocator Option, string Text)>();
        foreach (var option in options)
        {
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(text) || text.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
                continue;
            validOptions.Add((option, text));
        }

        if (validOptions.Count == 0)
            throw new InvalidOperationException("No default emission category options were found.");

        var selected = validOptions[Random.Shared.Next(validOptions.Count)];
        await selected.Option.ClickAsync();
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

        return selected.Text;
    }

    public async Task SelectDefaultEmissionCategoryAsync(string optionText)
    {
        await OpenSearchableSelectAsync(DefaultEmissionCategoryDropdown);
        var options = await DefaultUnitOptions.AllAsync();
        foreach (var option in options)
        {
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
            if (!text.Equals(optionText, StringComparison.Ordinal))
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

            return;
        }

        throw new InvalidOperationException($"Default emission category '{optionText}' was not found.");
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
            await CloseSearchableSelectAsync();
        }

        await dropdown.ClickAsync();
        await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    private async Task CloseSearchableSelectAsync()
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

    public async Task ClearCodeAsync() => await CodeInput.FillAsync(string.Empty);
    public async Task ClearDisplayNameAsync() => await DisplayNameInput.FillAsync(string.Empty);

    public async Task<string> GetCodeErrorAsync() => await GetErrorTextAsync(CodeError);
    public async Task<string> GetDisplayNameErrorAsync() => await GetErrorTextAsync(DisplayNameError);
    public async Task<string> GetDefaultUnitErrorAsync() => await GetErrorTextAsync(DefaultUnitError);
    public async Task<string> GetDefaultCategoryErrorAsync() => await GetErrorTextAsync(DefaultCategoryError);

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

    private async Task<string> GetSelectedOptionTextAsync(ILocator dropdown)
    {
        await dropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var selectedLabel = dropdown.Locator(".select__value:not(.select__value--placeholder), .select__value");
        if (await selectedLabel.CountAsync() > 0)
            return (await selectedLabel.First.TextContentAsync())?.Trim() ?? string.Empty;

        return (await dropdown.TextContentAsync())?.Trim() ?? string.Empty;
    }
}
