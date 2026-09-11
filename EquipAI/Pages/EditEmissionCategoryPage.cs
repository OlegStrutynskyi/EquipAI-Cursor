using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EditEmissionCategoryPage : BasePage
{
    public EditEmissionCategoryPage(IPage page) : base(page) { }

    private ILocator Subtitle => Page.Locator("//span[@class='page-header__subtitle']");
    private ILocator DisplayNameInput => Page.Locator("//input[@id='emission-category-display-name']");
    private ILocator GhgScopeDropdown => Page.Locator(
        "//*[@id='emission-category-ghg-scope' and (self::select or self::button)] | //label[normalize-space()='GHG Scope']/following-sibling::*[1]//*[contains(@class,'select__control')] | //label[normalize-space()='GHG Scope']/following-sibling::*[1][self::select or self::button]");
    private ILocator GhgScopeValue => Page.Locator("//span[@class='select__value']");
    private ILocator SearchableSelectList => Page.Locator("//div[@role='listbox' and contains(@class,'select__list')]");
    private ILocator RollupKeyText => Page.Locator("//p[@class='text-muted']");
    private ILocator DisplayNameError => Page.Locator("//label[normalize-space()='Display Name']/following-sibling::span");
    private ILocator SaveCategoryBtn => Page.Locator("//button[normalize-space()='Save category']");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");

    public async Task WaitForLoadedAsync()
    {
        await Page.Locator("//h1[contains(@id,'title')]")
            .Filter(new LocatorFilterOptions { HasTextString = "Edit Emission Category" })
            .WaitForAsync();
    }

    public async Task<string> GetSubtitleAsync()
    {
        await Subtitle.WaitForAsync();
        return (await Subtitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetDisplayNameAsync()
    {
        await DisplayNameInput.WaitForAsync();
        return (await DisplayNameInput.InputValueAsync()).Trim();
    }

    public async Task<string> GetGhgScopeAsync()
    {
        await GhgScopeValue.WaitForAsync();
        return (await GhgScopeValue.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> SelectAnyOtherGhgScopeAsync(string currentScope)
    {
        await GhgScopeDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        if (await IsNativeSelectAsync(GhgScopeDropdown))
        {
            var options = await GhgScopeDropdown.Locator("option").AllAsync();
            foreach (var option in options)
            {
                var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(text) ||
                    text.StartsWith("Select", StringComparison.OrdinalIgnoreCase) ||
                    text.Equals(currentScope, StringComparison.Ordinal))
                    continue;

                var value = await option.GetAttributeAsync("value");
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                await GhgScopeDropdown.SelectOptionAsync(value);
                return text;
            }

            throw new InvalidOperationException("No alternate GHG Scope option was found.");
        }

        await OpenSearchableSelectAsync();
        var searchableOptions = await SearchableSelectList.Locator("[role='option']").AllAsync();
        foreach (var option in searchableOptions)
        {
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(text) ||
                text.StartsWith("Select", StringComparison.OrdinalIgnoreCase) ||
                text.Equals(currentScope, StringComparison.Ordinal))
                continue;

            await option.ClickAsync();
            await WaitForSearchableSelectToCloseAsync();
            return text;
        }

        throw new InvalidOperationException("No alternate GHG Scope option was found.");
    }

    public async Task<string> GetRollupKeyTextAsync()
    {
        await RollupKeyText.WaitForAsync();
        return (await RollupKeyText.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task ClearDisplayNameAsync() => await DisplayNameInput.FillAsync(string.Empty);

    public async Task FillDisplayNameAsync(string displayName) => await DisplayNameInput.FillAsync(displayName);

    public async Task ClickSaveCategoryBtnAsync() => await SaveCategoryBtn.ClickAsync();

    public async Task<EmissionCategoriesPage> SaveCategoryAsync()
    {
        await SaveCategoryBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/admin/emission-categories", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/edit", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 60_000 });
        var emissionCategoriesPage = new EmissionCategoriesPage(Page);
        await emissionCategoriesPage.WaitForLoadedAsync();
        return emissionCategoriesPage;
    }

    public async Task<string> GetDisplayNameErrorAsync()
    {
        await DisplayNameError.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await DisplayNameError.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsSaveCategoryBtnVisibleAsync() => SaveCategoryBtn.IsVisibleAsync();
    public Task<bool> IsCancelBtnVisibleAsync() => CancelBtn.IsVisibleAsync();

    private async Task<bool> IsNativeSelectAsync(ILocator dropdown)
    {
        var tag = await dropdown.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
        return tag == "select";
    }

    private async Task OpenSearchableSelectAsync()
    {
        if (await SearchableSelectList.IsVisibleAsync())
            return;

        await GhgScopeDropdown.ClickAsync();
        await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    private async Task WaitForSearchableSelectToCloseAsync()
    {
        try
        {
            await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 3_000,
            });
        }
        catch (TimeoutException)
        {
            // Dropdown may close without transition.
        }
    }
}
