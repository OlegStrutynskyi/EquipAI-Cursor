using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EditAliasPage : BasePage
{
    public EditAliasPage(IPage page) : base(page) { }

    private ILocator Title => Page.Locator("//h1[@id='admin-alias-edit-title']").First;
    private ILocator Subtitle => Page.Locator("//span[contains(@class,'admin-alias-edit__subtitle')]");
    private ILocator BackToAliasesBtn => Page.Locator("//a[contains(@class,'admin-alias-edit__back')] | //*[contains(@class,'admin-alias-edit__back')]");
    private ILocator ContextDropdown => Page.Locator("#alias-context");
    private ILocator AliasTextInput => Page.Locator("//input[@id='alias-text']");
    private ILocator AliasTextError => Page.Locator("//input[@id='alias-text']/following-sibling::span");
    private ILocator TargetKindDropdown => Page.Locator("#alias-target-kind");
    private ILocator UnitOfMeasureDropdown => Page.Locator("#alias-unit");
    private ILocator EmissionTypeDropdown => Page.Locator("#alias-emission-type");
    private ILocator TelemetryProjectDropdown => Page.Locator("#alias-project");
    private ILocator EpaFactorSourceInput => Page.Locator("//label[normalize-space()='EPA']//input | //input[@type='radio' and (@value='EPA' or @value='epa')]");
    private ILocator DefraFactorSourceInput => Page.Locator("//label[normalize-space()='DEFRA']//input | //input[@type='radio' and (@value='DEFRA' or @value='defra')]");
    private ILocator CustomFactorSourceInput => Page.Locator("//label[normalize-space()='Custom']//input | //input[@type='radio' and (@value='Custom' or @value='CUSTOM' or @value='custom')]");
    private ILocator EpaFactorSourceBtn => Page.Locator("//label[normalize-space()='EPA']");
    private ILocator DefraFactorSourceBtn => Page.Locator("//label[normalize-space()='DEFRA']");
    private ILocator CustomFactorSourceBtn => Page.Locator("//label[normalize-space()='Custom']");
    private ILocator PreparedFactorsPreviewText => Page.Locator("//h2[@id='alias-prepared-factors-title']");
    private ILocator PreparedFactorsTable => Page.Locator("//h2[@id='alias-prepared-factors-title']/following::table[1]");
    private ILocator SelectedTargetTable => Page.Locator("//h3[normalize-space()='Selected Target']/following-sibling::*//table").First;
    private ILocator SaveAliasBtn => Page.Locator("//button[normalize-space()='Save alias']");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    private ILocator AlertMessage => Page.Locator("//div[@class='alert__body']");
    private ILocator SearchableSelectList => Page.Locator("//div[@role='listbox' and contains(@class,'select__list')]");

    public async Task WaitForLoadedAsync()
    {
        await Page.Locator("//h1[contains(@id,'title')]")
            .Filter(new LocatorFilterOptions { HasTextString = "Edit Alias" })
            .WaitForAsync();
        await AliasTextInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public new async Task<string> GetPageTitleAsync()
    {
        await Title.WaitForAsync();
        return (await Title.InnerTextAsync()).Replace("\n", " ").Replace("\r", " ").Trim();
    }

    public async Task<string> GetSubtitleAsync()
    {
        if (await Subtitle.CountAsync() > 0 && await Subtitle.IsVisibleAsync())
            return (await Subtitle.TextContentAsync())?.Trim() ?? string.Empty;

        var fullTitle = await GetPageTitleAsync();
        const string prefix = "Edit Alias";
        if (fullTitle.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && fullTitle.Length > prefix.Length)
            return fullTitle[prefix.Length..].Trim();

        return string.Empty;
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
        await AlertMessage.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertMessage.First.InnerTextAsync()).Replace('\u00A0', ' ').Trim();
    }

    public async Task<string> GetTargetKindAsync() => await GetSelectedOptionTextAsync(TargetKindDropdown);

    public async Task<string> GetUnitOfMeasureAsync() => await GetSelectedOptionTextAsync(UnitOfMeasureDropdown);

    public async Task<string> GetEmissionTypeAsync() => await GetSelectedOptionTextAsync(EmissionTypeDropdown);

    public async Task<string> GetTelemetryProjectAsync() => await GetSelectedOptionTextAsync(TelemetryProjectDropdown);

    public Task<bool> IsPreparedFactorsPreviewTextVisibleAsync() => PreparedFactorsPreviewText.IsVisibleAsync();
    public Task<bool> IsContextDropdownVisibleAsync() => ContextDropdown.IsVisibleAsync();
    public Task<bool> IsTelemetryProjectDropdownVisibleAsync() => TelemetryProjectDropdown.IsVisibleAsync();

    public async Task<bool> IsEpaFactorSourceSelectedAsync()
    {
        await EpaFactorSourceInput.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return await EpaFactorSourceInput.First.IsCheckedAsync();
    }

    public async Task<bool> IsDefraFactorSourceSelectedAsync()
    {
        await DefraFactorSourceInput.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return await DefraFactorSourceInput.First.IsCheckedAsync();
    }

    public async Task<bool> IsCustomFactorSourceSelectedAsync()
    {
        if (await CustomFactorSourceInput.CountAsync() == 0)
            return false;

        await CustomFactorSourceInput.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return await CustomFactorSourceInput.First.IsCheckedAsync();
    }

    public async Task SelectEpaFactorSourceAsync()
    {
        await EpaFactorSourceBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        if (await EpaFactorSourceInput.CountAsync() > 0)
            await EpaFactorSourceInput.First.CheckAsync();
        else
            await EpaFactorSourceBtn.ClickAsync();

        await WaitForFactorSourceSelectedAsync(EpaFactorSourceInput);
        await WaitForFactorSourcePreviewAsync();
    }

    public async Task SelectDefraFactorSourceAsync()
    {
        await DefraFactorSourceBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        if (await DefraFactorSourceInput.CountAsync() > 0)
            await DefraFactorSourceInput.First.CheckAsync();
        else
            await DefraFactorSourceBtn.ClickAsync();

        await WaitForFactorSourceSelectedAsync(DefraFactorSourceInput);
        await WaitForFactorSourcePreviewAsync();
    }

    public async Task SelectCustomFactorSourceAsync()
    {
        await CustomFactorSourceBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        if (await CustomFactorSourceInput.CountAsync() > 0)
            await CustomFactorSourceInput.First.CheckAsync();
        else
            await CustomFactorSourceBtn.ClickAsync();

        await WaitForFactorSourceSelectedAsync(CustomFactorSourceInput);
        await WaitForFactorSourcePreviewAsync();
    }

    private async Task WaitForFactorSourceSelectedAsync(ILocator factorSourceInput)
    {
        if (await factorSourceInput.CountAsync() == 0)
            return;

        await Assertions.Expect(factorSourceInput.First).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions
        {
            Timeout = 10_000,
        });
    }

    private async Task WaitForFactorSourcePreviewAsync()
    {
        var previewTable = Page.Locator(
            "//h2[@id='alias-prepared-factors-title']/following::table[1] | //h3[normalize-space()='Selected Target']/following-sibling::*//table");
        try
        {
            await previewTable.First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10_000,
            });
        }
        catch (TimeoutException)
        {
            // Preview grid is optional on edit when factors are not loaded yet.
        }
    }

    public async Task<IReadOnlyList<PreparedFactorGridRow>> GetPreparedFactorsRowsAsync()
    {
        await PreparedFactorsTable.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000,
        });
        return await ReadFactorRowsAsync(PreparedFactorsTable);
    }

    public async Task<IReadOnlyList<PreparedFactorGridRow>> GetSelectedTargetFactorRowsAsync()
    {
        await SelectedTargetTable.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000,
        });
        return await ReadFactorRowsAsync(SelectedTargetTable);
    }

    public async Task SelectUnitOfMeasureAsync(string code)
    {
        var options = await GetDropdownOptionsAsync(UnitOfMeasureDropdown);
        var option = options.FirstOrDefault(o =>
                o.Equals(code, StringComparison.Ordinal)
                || o.StartsWith(code + " —", StringComparison.Ordinal)
                || o.StartsWith(code + " -", StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Unit of measure '{code}' was not found in the dropdown.");

        await SelectOptionByTextAsync(UnitOfMeasureDropdown, option);
    }

    public async Task SelectEmissionTypeAsync(string code)
    {
        var options = await GetDropdownOptionsAsync(EmissionTypeDropdown);
        var option = options.FirstOrDefault(o =>
                o.Equals(code, StringComparison.Ordinal)
                || o.StartsWith(code + " —", StringComparison.Ordinal)
                || o.StartsWith(code + " -", StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Emission type '{code}' was not found in the dropdown.");

        await SelectOptionByTextAsync(EmissionTypeDropdown, option);
    }

    public async Task<string> SelectDifferentTelemetryProjectAsync(string excludeNameOrCode)
    {
        await OpenSearchableSelectAsync(TelemetryProjectDropdown);
        var options = SearchableSelectList.Locator("[role='option']");
        var count = await options.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var option = options.Nth(i);
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(text)
                || text.StartsWith("Select", StringComparison.OrdinalIgnoreCase)
                || text.Contains(excludeNameOrCode, StringComparison.Ordinal))
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

            await TelemetryProjectDropdown.Filter(new LocatorFilterOptions { HasTextString = text })
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
            return text;
        }

        throw new InvalidOperationException(
            $"No alternative project found excluding '{excludeNameOrCode}'.");
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

            return;
        }

        throw new InvalidOperationException($"Option '{optionText}' was not found.");
    }

    private static async Task<IReadOnlyList<PreparedFactorGridRow>> ReadFactorRowsAsync(ILocator table)
    {
        var headers = (await table.Locator("thead th").AllInnerTextsAsync())
            .Select(h => h.Replace('\u00A0', ' ').Trim().ToUpperInvariant())
            .ToList();
        var rows = table.Locator("tbody tr");
        var count = await rows.CountAsync();
        var result = new List<PreparedFactorGridRow>();

        for (var i = 0; i < count; i++)
        {
            var cells = rows.Nth(i).Locator("td");
            var cellCount = await cells.CountAsync();
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < cellCount && c < headers.Count; c++)
                values[headers[c]] = (await cells.Nth(c).InnerTextAsync()).Replace('\u00A0', ' ').Trim();

            result.Add(new PreparedFactorGridRow
            {
                Year = GetFactorCell(values, "YEAR"),
                Unit = GetFactorCell(values, "UNIT"),
                TCo2ePerActivityUnit = GetFactorCell(values, "TCO2E / ACTIVITY UNIT", "TCO2E/ACTIVITY UNIT"),
                TCo2 = GetFactorCell(values, "TCO2"),
                TCh4 = GetFactorCell(values, "TCH4"),
                TN2o = GetFactorCell(values, "TN2O", "TH2O"),
                SourceFileLine = GetFactorCell(values, "SOURCE FILE LINE"),
            });
        }

        return result;
    }

    private static string GetFactorCell(IReadOnlyDictionary<string, string> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value))
                return value;
        }

        return string.Empty;
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

    private async Task<string> GetSelectedOptionTextAsync(ILocator dropdown)
    {
        await dropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var tagName = await dropdown.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
        if (tagName == "select")
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
}

public sealed class PreparedFactorGridRow
{
    public required string Year { get; init; }
    public required string Unit { get; init; }
    public required string TCo2ePerActivityUnit { get; init; }
    public required string TCo2 { get; init; }
    public required string TCh4 { get; init; }
    public required string TN2o { get; init; }
    public required string SourceFileLine { get; init; }
}
