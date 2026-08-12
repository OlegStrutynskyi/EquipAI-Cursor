using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EditAliasPage : BasePage
{
    public EditAliasPage(IPage page) : base(page) { }

    private ILocator Subtitle => Page.Locator("//span[contains(@class,'admin-alias-edit__subtitle')]");
    private ILocator BackToAliasesBtn => Page.Locator("//a[contains(@class,'admin-alias-edit__back')] | //*[contains(@class,'admin-alias-edit__back')]");
    private ILocator ContextDropdown => Page.Locator("//select[@id='alias-context']");
    private ILocator AliasTextInput => Page.Locator("//input[@id='alias-text']");
    private ILocator AliasTextError => Page.Locator("//input[@id='alias-text']/following-sibling::span");
    private ILocator TargetKindDropdown => Page.Locator("//select[@id='alias-target-kind']");
    private ILocator UnitOfMeasureDropdown => Page.Locator("//select[@id='alias-unit']");
    private ILocator EmissionTypeDropdown => Page.Locator("//select[@id='alias-emission-type']");
    private ILocator EpaFactorSourceInput => Page.Locator("//label[normalize-space()='EPA']//input | //input[@type='radio' and (@value='EPA' or @value='Epa')]");
    private ILocator DefraFactorSourceInput => Page.Locator("//label[normalize-space()='DEFRA']//input | //input[@type='radio' and (@value='DEFRA' or @value='defra')]");
    private ILocator DefraFactorSourceBtn => Page.Locator("//label[normalize-space()='DEFRA']");
    private ILocator PreparedFactorsPreviewText => Page.Locator("//h2[@id='alias-prepared-factors-title']");
    private ILocator SaveAliasBtn => Page.Locator("//button[normalize-space()='Save alias']");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    private ILocator AlertMessage => Page.Locator("//p[@role='alert']");

    public async Task WaitForLoadedAsync()
    {
        await Page.Locator("//h1[contains(@id,'title')]")
            .Filter(new LocatorFilterOptions { HasTextString = "Edit alias" })
            .WaitForAsync();
        await AliasTextInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public new async Task<string> GetPageTitleAsync()
    {
        var title = Page.Locator("//h1[contains(@id,'title') or contains(@class,'page-title')]").First;
        await title.WaitForAsync();
        return await title.EvaluateAsync<string>(
            """
            el => {
              const clone = el.cloneNode(true);
              clone.querySelectorAll('[class*="admin-alias-edit__subtitle"]').forEach(node => node.remove());
              return (clone.textContent || '').trim();
            }
            """);
    }

    public async Task<string> GetSubtitleAsync()
    {
        await Subtitle.WaitForAsync();
        return (await Subtitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetContextAsync() => await GetSelectedOptionTextAsync(ContextDropdown);

    public async Task<string> GetAliasTextAsync()
    {
        await AliasTextInput.WaitForAsync();
        return (await AliasTextInput.InputValueAsync()).Trim();
    }

    public async Task ClearAliasTextAsync()
    {
        await AliasTextInput.FillAsync(string.Empty);
    }

    public async Task FillAliasTextAsync(string aliasText)
    {
        await AliasTextInput.FillAsync(aliasText);
    }

    public async Task ClickSaveAliasBtnAsync()
    {
        await SaveAliasBtn.ClickAsync();
    }

    public async Task<AliasesPage> SaveAliasAsync()
    {
        await SaveAliasBtn.ClickAsync();
        await Page.WaitForURLAsync(url =>
        {
            var path = new Uri(url).AbsolutePath.TrimEnd('/');
            return path.Equals("/admin/aliases", StringComparison.OrdinalIgnoreCase);
        });
        var aliasesPage = new AliasesPage(Page);
        await aliasesPage.WaitForLoadedAsync();
        return aliasesPage;
    }

    public async Task<string> GetAliasTextErrorAsync()
    {
        await AliasTextError.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AliasTextError.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetAlertMessageAsync()
    {
        await AlertMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetTargetKindAsync() => await GetSelectedOptionTextAsync(TargetKindDropdown);

    public async Task<string> GetUnitOfMeasureAsync() => await GetSelectedOptionTextAsync(UnitOfMeasureDropdown);

    public async Task<string> GetEmissionTypeAsync() => await GetSelectedOptionTextAsync(EmissionTypeDropdown);

    public Task<bool> IsPreparedFactorsPreviewTextVisibleAsync() => PreparedFactorsPreviewText.IsVisibleAsync();

    public async Task<bool> IsEpaFactorSourceSelectedAsync()
    {
        await EpaFactorSourceInput.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return await EpaFactorSourceInput.First.IsCheckedAsync();
    }

    public async Task SelectDefraFactorSourceAsync()
    {
        await DefraFactorSourceBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        if (await DefraFactorSourceInput.CountAsync() > 0)
        {
            await DefraFactorSourceInput.First.CheckAsync();
            return;
        }

        await DefraFactorSourceBtn.ClickAsync();
    }

    public async Task SelectUnitOfMeasureAsync(string code)
    {
        await UnitOfMeasureDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var options = await UnitOfMeasureDropdown.Locator("option").AllInnerTextsAsync();
        var option = options
            .Select(o => o.Trim())
            .Where(o => !string.IsNullOrEmpty(o) && !o.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(o =>
                o.Equals(code, StringComparison.Ordinal)
                || o.StartsWith(code + " —", StringComparison.Ordinal)
                || o.StartsWith(code + " -", StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Unit of measure '{code}' was not found in the dropdown.");

        await UnitOfMeasureDropdown.SelectOptionAsync(new SelectOptionValue { Label = option });
    }

    public async Task SelectEmissionTypeAsync(string code)
    {
        await EmissionTypeDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var options = await EmissionTypeDropdown.Locator("option").AllInnerTextsAsync();
        var option = options
            .Select(o => o.Trim())
            .Where(o => !string.IsNullOrEmpty(o) && !o.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(o =>
                o.Equals(code, StringComparison.Ordinal)
                || o.StartsWith(code + " —", StringComparison.Ordinal)
                || o.StartsWith(code + " -", StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Emission type '{code}' was not found in the dropdown.");

        await EmissionTypeDropdown.SelectOptionAsync(new SelectOptionValue { Label = option });
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

    public Task<bool> IsBackToAliasesBtnVisibleAsync() => BackToAliasesBtn.IsVisibleAsync();
    public Task<bool> IsSaveAliasBtnVisibleAsync() => SaveAliasBtn.IsVisibleAsync();
    public Task<bool> IsCancelBtnVisibleAsync() => CancelBtn.IsVisibleAsync();

    private static async Task<string> GetSelectedOptionTextAsync(ILocator dropdown)
    {
        await dropdown.WaitForAsync();
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
}
