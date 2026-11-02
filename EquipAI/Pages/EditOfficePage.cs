using System.Globalization;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EditOfficePage : BasePage
{
    public EditOfficePage(IPage page) : base(page) { }

    private ILocator Title => Page.Locator("//h1[contains(@class,'page-title')]");
    private ILocator Subtitle => Page.Locator("//span[@class='page-header__subtitle']");
    private ILocator BackBtn => Page.Locator("//a[contains(normalize-space(),'Back')]");
    private ILocator DetailsTab => Page.Locator("//button[normalize-space()='Details']");
    private ILocator EmissionsTab => Page.Locator("//button[contains(text(),'Emissions · ')]");
    private ILocator ProjectDropdown => Page.Locator("//label[normalize-space()='Project']/following-sibling::app-searchable-select");
    private ILocator NameInput => Page.Locator("//input[@id='office-name']");
    private ILocator AddressLine1Input => Page.Locator("//input[@id='office-address-line-1']");
    private ILocator AddressLine2Input => Page.Locator("//input[@id='office-address-line-2']");
    private ILocator CityInput => Page.Locator("//input[@id='office-city']");
    private ILocator StateInput => Page.Locator("//input[@id='office-state-province']");
    private ILocator PostalCodeInput => Page.Locator("//input[@id='office-postal-code']");
    private ILocator CountryDropdown => Page.Locator("//label[normalize-space()='Country']/following-sibling::app-searchable-select");
    private ILocator StartDateInput => Page.Locator("//input[@id='office-start-date']");
    private ILocator EndDateInput => Page.Locator("//input[@id='office-end-date']");
    private ILocator AreaInput => Page.Locator("//input[@id='office-area']");
    private ILocator SaveOfficeBtn => Page.Locator("//button[contains(normalize-space(),'Save')]");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    private ILocator AlertMessage => Page.Locator("//div[@class='alert__body']");
    private ILocator ProjectError => FieldError("Project");
    private ILocator NameError => FieldError("Name");
    private ILocator AddressLine1Error => FieldError("Address Line 1");
    private ILocator AddressLine2Error => FieldError("Address Line 2");
    private ILocator CityError => FieldError("City");
    private ILocator StateError => FieldError("State / Province");
    private ILocator PostalCodeError => FieldError("Postal Code");
    private ILocator StartDateError => FieldError("Start Date");
    private ILocator EndDateError => FieldError("End Date");
    private ILocator AreaError => FieldError("Area (sq ft)");
    private ILocator EmissionTypeError => Page.Locator(
        "//label[normalize-space()='Emission type']/following::span[contains(@class,'form-hint--error')][1]");
    private ILocator CalculateByFootageToggle => Page.Locator("//span[@class='form-toggle__control']/input");
    private ILocator EmissionTypeDropdown => Page.Locator(
        "//label[normalize-space()='Emission type']/following-sibling::app-searchable-select");
    private ILocator EmissionsMessage => Page.Locator("//p[@id='office-method-hint']");
    private ILocator SearchableSelectList => Page.Locator("//div[@role='listbox' and contains(@class,'select__list')]");

    public async Task WaitForLoadedAsync()
    {
        await Title.Filter(new LocatorFilterOptions { HasTextString = "Edit Office" }).WaitForAsync();
        await DetailsTab.WaitForAsync();
        await NameInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public new async Task<string> GetPageTitleAsync()
    {
        await Title.WaitForAsync();
        return await Title.EvaluateAsync<string>(
            """
            el => {
              const clone = el.cloneNode(true);
              clone.querySelectorAll('.page-header__subtitle').forEach(node => node.remove());
              return (clone.textContent || '').replace(/\s+/g, ' ').trim();
            }
            """);
    }

    public Task<bool> IsBackBtnVisibleAsync() => IsVisibleAsync(BackBtn);

    public async Task<OfficesPage> ClickBackBtnAsync()
    {
        await BackBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                   && uri.AbsolutePath.TrimEnd('/').Equals("/admin/offices", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 30_000 });
        var officesPage = new OfficesPage(Page);
        await officesPage.WaitForLoadedAsync();
        return officesPage;
    }

    public async Task<string> GetSubtitleAsync()
    {
        await Subtitle.WaitForAsync();
        return (await Subtitle.InnerTextAsync()).Trim();
    }

    public Task<string> GetSelectedProjectAsync() => GetSelectedOptionAsync(ProjectDropdown);
    public Task<string> GetSelectedCountryAsync() => GetSelectedOptionAsync(CountryDropdown);
    public Task<string> GetSelectedEmissionTypeAsync() => GetSelectedOptionAsync(EmissionTypeDropdown);
    public Task<string> GetNameAsync() => ReadInputAsync(NameInput);
    public Task<string> GetAddressLine1Async() => ReadInputAsync(AddressLine1Input);
    public Task<string> GetAddressLine2Async() => ReadInputAsync(AddressLine2Input);
    public Task<string> GetCityAsync() => ReadInputAsync(CityInput);
    public Task<string> GetStateAsync() => ReadInputAsync(StateInput);
    public Task<string> GetPostalCodeAsync() => ReadInputAsync(PostalCodeInput);
    public Task<string> GetStartDateAsync() => ReadInputAsync(StartDateInput);
    public Task<string> GetEndDateAsync() => ReadInputAsync(EndDateInput);
    public Task<string> GetAreaAsync() => ReadInputAsync(AreaInput);

    public async Task FillNameAsync(string value) => await NameInput.FillAsync(value);
    public async Task FillAddressLine1Async(string value) => await AddressLine1Input.FillAsync(value);
    public async Task FillAddressLine2Async(string value) => await AddressLine2Input.FillAsync(value);
    public async Task FillCityAsync(string value) => await CityInput.FillAsync(value);
    public async Task FillStateAsync(string value) => await StateInput.FillAsync(value);
    public async Task FillPostalCodeAsync(string value) => await PostalCodeInput.FillAsync(value);
    public async Task FillAreaAsync(string value) => await AreaInput.FillAsync(value);
    public Task FillStartDateAsync(DateTime value) => FillMaskedDateAsync(StartDateInput, value);
    public Task ClearEndDateAsync() => ClearInputAsync(EndDateInput);

    public async Task<string> SelectDifferentProjectAsync()
    {
        var current = NormalizeOption(await GetSelectedProjectAsync());
        await OpenSearchableSelectAsync(ProjectDropdown);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var options = VisibleDropdownList().Locator("[role='option']");
            var count = await options.CountAsync();
            var added = false;
            for (var i = 0; i < count; i++)
            {
                var candidate = options.Nth(i);
                var text = NormalizeOption(await candidate.InnerTextAsync());
                if (text.Length == 0 || !seen.Add(text))
                    continue;

                added = true;
                if (text.Equals(current, StringComparison.Ordinal)
                    || text.StartsWith("Select ", StringComparison.OrdinalIgnoreCase))
                    continue;

                await candidate.ClickAsync();
                await WaitForSelectListToCloseAsync();
                return await GetSelectedProjectAsync();
            }

            if (!added)
                break;

            if (count > 0)
                await options.Last.ScrollIntoViewIfNeededAsync();

            await Task.Delay(150);
        }

        await Page.Keyboard.PressAsync("Escape");
        return current;
    }

    public Task SelectCountryAsync(string country) => SelectOptionAsync(CountryDropdown, country);

    public Task SelectEmissionTypeAsync(string emissionType) => SelectOptionAsync(EmissionTypeDropdown, emissionType);

    public async Task<OfficesPage> SaveOfficeAsync()
    {
        var responseTask = Page.WaitForResponseAsync(
            response => response.Request.Method is "PUT" or "POST" or "PATCH"
                && response.Url.Contains("/api/admin/offices", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = 30_000 });
        await SaveOfficeBtn.ClickAsync();
        var response = await responseTask;
        if (!response.Ok)
        {
            var body = await response.TextAsync();
            throw new InvalidOperationException(
                $"Save office failed with status {(int)response.Status} {response.Url}: {body}");
        }

        await Page.WaitForURLAsync(
            url => url.Contains("/admin/offices", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/edit", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/new", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 30_000 });
        var officesPage = new OfficesPage(Page);
        await officesPage.WaitForLoadedAsync();
        return officesPage;
    }

    public async Task<OfficesPage> ClickCancelBtnAsync()
    {
        await CancelBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                   && uri.AbsolutePath.TrimEnd('/').Equals("/admin/offices", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 30_000 });
        var officesPage = new OfficesPage(Page);
        await officesPage.WaitForLoadedAsync();
        return officesPage;
    }

    public async Task ClearAllInputsAsync()
    {
        await ClearInputAsync(NameInput);
        await ClearInputAsync(AddressLine1Input);
        await ClearInputAsync(AddressLine2Input);
        await ClearInputAsync(CityInput);
        await ClearInputAsync(StateInput);
        await ClearInputAsync(PostalCodeInput);
        await ClearInputAsync(StartDateInput);
        await ClearInputAsync(EndDateInput);
        await ClearInputAsync(AreaInput);
    }

    public async Task ClickSaveOfficeBtnAsync() => await SaveOfficeBtn.ClickAsync();

    public async Task<string> GetAlertMessageAsync()
    {
        await AlertMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertMessage.InnerTextAsync()).Trim();
    }

    public async Task ClickDetailsTabAsync()
    {
        await DetailsTab.ClickAsync();
        await NameInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task ClickEmissionsTabAsync()
    {
        await EmissionsTab.ClickAsync();
        await EmissionsMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<bool> IsCalculateByFootageToggleOnAsync()
    {
        await CalculateByFootageToggle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
        return await CalculateByFootageToggle.IsCheckedAsync();
    }

    public async Task TurnCalculateByFootageOnAsync()
    {
        await CalculateByFootageToggle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
        if (await CalculateByFootageToggle.IsCheckedAsync())
            return;

        await CalculateByFootageToggle
            .Locator("xpath=ancestor::span[@class='form-toggle__control']")
            .ClickAsync();

        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            if (await CalculateByFootageToggle.IsCheckedAsync())
            {
                await EmissionTypeDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
                return;
            }

            await Task.Delay(100);
        }

        throw new InvalidOperationException("Calculate by footage toggle did not turn on.");
    }

    public Task<string> GetProjectErrorAsync() => GetFieldErrorAsync(ProjectError);
    public Task<string> GetNameErrorAsync() => GetFieldErrorAsync(NameError);
    public Task<string> GetAddressLine1ErrorAsync() => GetFieldErrorAsync(AddressLine1Error);
    public Task<string> GetAddressLine2ErrorAsync() => GetFieldErrorAsync(AddressLine2Error);
    public Task<string> GetCityErrorAsync() => GetFieldErrorAsync(CityError);
    public Task<string> GetStateErrorAsync() => GetFieldErrorAsync(StateError);
    public Task<string> GetPostalCodeErrorAsync() => GetFieldErrorAsync(PostalCodeError);
    public Task<string> GetStartDateErrorAsync() => GetFieldErrorAsync(StartDateError);
    public Task<string> GetAreaErrorAsync() => GetFieldErrorAsync(AreaError);
    public Task<string> GetEmissionTypeErrorAsync() => GetFieldErrorAsync(EmissionTypeError);

    public Task<bool> IsProjectErrorVisibleAsync() => IsFieldErrorVisibleAsync(ProjectError);
    public Task<bool> IsAddressLine2ErrorVisibleAsync() => IsFieldErrorVisibleAsync(AddressLine2Error);
    public Task<bool> IsEndDateErrorVisibleAsync() => IsFieldErrorVisibleAsync(EndDateError);

    private async Task SelectOptionAsync(ILocator dropdown, string optionText)
    {
        await dropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        if (NormalizeOption(await GetSelectedOptionAsync(dropdown)).Equals(optionText, StringComparison.Ordinal))
            return;

        await OpenSearchableSelectAsync(dropdown);
        var search = VisibleDropdownList().Locator("input");
        if (await search.CountAsync() == 0)
            search = dropdown.Locator("input");
        if (await search.CountAsync() > 0 && await search.First.IsVisibleAsync())
            await search.First.FillAsync(optionText);

        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var options = VisibleDropdownList().Locator("[role='option']");
            var count = await options.CountAsync();
            for (var i = 0; i < count; i++)
            {
                var candidate = options.Nth(i);
                if (!NormalizeOption(await candidate.InnerTextAsync()).Equals(optionText, StringComparison.Ordinal))
                    continue;

                await candidate.ClickAsync();
                await WaitForSelectListToCloseAsync();
                return;
            }

            if (count > 0)
                await options.Last.ScrollIntoViewIfNeededAsync();

            await Task.Delay(150);
        }

        throw new InvalidOperationException($"Option '{optionText}' was not found.");
    }

    private async Task FillMaskedDateAsync(ILocator input, DateTime value)
    {
        var digits = value.ToString("MMddyyyy", CultureInfo.InvariantCulture);
        var expected = value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
        await input.ClickAsync();
        await input.FillAsync(string.Empty);
        await input.PressSequentiallyAsync(digits, new LocatorPressSequentiallyOptions { Delay = 40 });
        var actual = (await input.InputValueAsync()).Trim();
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
            throw new InvalidOperationException($"Date is '{actual}', expected '{expected}'.");
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
                    Timeout = 2_000
                });
            }
            catch (TimeoutException)
            {
            }
        }

        await dropdown.ClickAsync();
        await VisibleDropdownList().WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    private ILocator VisibleDropdownList() =>
        SearchableSelectList.Filter(new LocatorFilterOptions { Visible = true });

    private async Task WaitForSelectListToCloseAsync()
    {
        try
        {
            await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 5_000
            });
        }
        catch (TimeoutException)
        {
        }
    }

    private static string NormalizeOption(string value) =>
        System.Text.RegularExpressions.Regex.Replace(value.Replace('\u00A0', ' '), @"\s+", " ").Trim();

    private async Task<string> GetSelectedOptionAsync(ILocator dropdown)
    {
        await dropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var selected = dropdown.Locator(".select__value:not(.select__value--placeholder)");
        if (await selected.CountAsync() == 0)
            selected = dropdown.Locator(".select__value");

        return (await selected.First.InnerTextAsync()).Trim();
    }

    private ILocator FieldError(string label) =>
        Page.Locator($"//label[normalize-space()='{label}']/following-sibling::span[contains(@class,'error')]");

    private static async Task ClearInputAsync(ILocator input)
    {
        await input.ClickAsync();
        await input.FillAsync(string.Empty);
        await input.PressAsync("Control+A");
        await input.PressAsync("Backspace");
    }

    private static async Task<string> GetFieldErrorAsync(ILocator error)
    {
        await error.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await error.InnerTextAsync()).Trim();
    }

    private static async Task<bool> IsFieldErrorVisibleAsync(ILocator error)
    {
        if (await error.CountAsync() == 0)
            return false;

        return await error.First.IsVisibleAsync();
    }

    private static async Task<string> ReadInputAsync(ILocator input)
    {
        await input.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await input.InputValueAsync()).Trim();
    }

    private static async Task<bool> IsVisibleAsync(ILocator locator)
    {
        try
        {
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10_000
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
