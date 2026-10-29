using System.Globalization;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AddOfficePage : BasePage
{
    public AddOfficePage(IPage page) : base(page) { }

    private ILocator Title => Page.Locator("//h1[contains(@class,'page-title')]");
    private ILocator BackBtn => Page.Locator("//a[contains(normalize-space(),'Back')]");
    private ILocator DetailsTab => Page.Locator("//button[normalize-space()='Details']");
    private ILocator EmissionsTab => Page.Locator("//button[contains(text(),'Emissions · ')]");
    private ILocator CreateOfficeBtn => Page.Locator("//button[normalize-space()='Create office']");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    private ILocator AlertMessage => Page.Locator("//div[@class='alert__body']");
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
    private ILocator EndDateMessage => Page.Locator("//label[normalize-space()='End Date']/following-sibling::p[@class='form-hint']");
    private ILocator AreaInput => Page.Locator("//input[@id='office-area']");
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
    private ILocator SearchableSelectList => Page.Locator("//div[@role='listbox' and contains(@class,'select__list')]");
    private ILocator CalculateByFootageToggle => Page.Locator("//span[@class='form-toggle__control']/input");
    private ILocator EmissionTypeDropdown => Page.Locator(
        "//label[contains(normalize-space(),'Emission')]/following-sibling::app-searchable-select");
    private ILocator EmissionsMessage => Page.Locator("//p[@id='office-method-hint']");

    public async Task WaitForLoadedAsync()
    {
        await Title.WaitForAsync();
        await DetailsTab.WaitForAsync();
    }

    public new async Task<string> GetPageTitleAsync()
    {
        await Title.WaitForAsync();
        return (await Title.InnerTextAsync()).Trim();
    }

    public Task<bool> IsBackBtnVisibleAsync() => IsVisibleAsync(BackBtn);
    public Task<bool> IsDetailsTabVisibleAsync() => IsVisibleAsync(DetailsTab);
    public Task<bool> IsEmissionsTabVisibleAsync() => IsVisibleAsync(EmissionsTab);
    public Task<bool> IsCreateOfficeBtnVisibleAsync() => IsVisibleAsync(CreateOfficeBtn);
    public Task<bool> IsCancelBtnVisibleAsync() => IsVisibleAsync(CancelBtn);
    public Task<bool> IsProjectDropdownVisibleAsync() => IsVisibleAsync(ProjectDropdown);
    public Task<bool> IsNameInputVisibleAsync() => IsVisibleAsync(NameInput);
    public Task<bool> IsAddressLine1InputVisibleAsync() => IsVisibleAsync(AddressLine1Input);
    public Task<bool> IsAddressLine2InputVisibleAsync() => IsVisibleAsync(AddressLine2Input);
    public Task<bool> IsCityInputVisibleAsync() => IsVisibleAsync(CityInput);
    public Task<bool> IsStateInputVisibleAsync() => IsVisibleAsync(StateInput);
    public Task<bool> IsPostalCodeInputVisibleAsync() => IsVisibleAsync(PostalCodeInput);
    public Task<bool> IsCountryDropdownVisibleAsync() => IsVisibleAsync(CountryDropdown);

    public Task<string> GetSelectedProjectAsync() => GetSelectedOptionAsync(ProjectDropdown);
    public Task<string> GetSelectedCountryAsync() => GetSelectedOptionAsync(CountryDropdown);
    public Task<bool> IsStartDateInputVisibleAsync() => IsVisibleAsync(StartDateInput);
    public Task<bool> IsEndDateInputVisibleAsync() => IsVisibleAsync(EndDateInput);
    public Task<bool> IsAreaInputVisibleAsync() => IsVisibleAsync(AreaInput);

    public async Task<string> GetEndDateMessageAsync()
    {
        await EndDateMessage.WaitForAsync();
        return (await EndDateMessage.InnerTextAsync()).Trim();
    }

    public async Task ClickCreateOfficeBtnAsync() => await CreateOfficeBtn.ClickAsync();

    public async Task<string> GetAlertMessageAsync()
    {
        await AlertMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertMessage.InnerTextAsync()).Trim();
    }

    public async Task<OfficesPage> ClickCancelBtnAsync()
    {
        await CancelBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/admin/offices", StringComparison.OrdinalIgnoreCase)
                   && !url.Contains("/new", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 30_000 });
        var officesPage = new OfficesPage(Page);
        await officesPage.WaitForLoadedAsync();
        return officesPage;
    }

    public async Task FillNameAsync(string value) => await NameInput.FillAsync(value);
    public async Task FillAddressLine1Async(string value) => await AddressLine1Input.FillAsync(value);
    public async Task FillAddressLine2Async(string value) => await AddressLine2Input.FillAsync(value);
    public async Task FillCityAsync(string value) => await CityInput.FillAsync(value);
    public async Task FillStateAsync(string value) => await StateInput.FillAsync(value);
    public async Task FillPostalCodeAsync(string value) => await PostalCodeInput.FillAsync(value);
    public Task FillStartDateAsync(DateTime value) => FillMaskedDateAsync(StartDateInput, value);
    public Task FillEndDateAsync(DateTime value) => FillMaskedDateAsync(EndDateInput, value);
    public async Task FillAreaAsync(string value) => await AreaInput.FillAsync(value);

    public async Task<string> SelectProjectAsync(string projectName)
    {
        await ProjectDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var current = await GetSelectedProjectAsync();
        if (IsProjectOption(current, projectName))
            return current;

        await OpenSearchableSelectAsync(ProjectDropdown);

        var search = VisibleDropdownList().Locator("input");
        if (await search.CountAsync() == 0)
            search = ProjectDropdown.Locator("input");

        if (await search.CountAsync() > 0 && await search.First.IsVisibleAsync())
            await search.First.FillAsync(projectName);

        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var options = VisibleDropdownList().Locator("[role='option']");
            var count = await options.CountAsync();
            for (var i = 0; i < count; i++)
            {
                var candidate = options.Nth(i);
                var text = NormalizeOption(await candidate.InnerTextAsync());
                if (!IsProjectOption(text, projectName))
                    continue;

                await candidate.ClickAsync();
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

                return await GetSelectedProjectAsync();
            }

            if (count > 0)
                await options.Last.ScrollIntoViewIfNeededAsync();

            await Task.Delay(150);
        }

        throw new InvalidOperationException($"Project '{projectName}' was not found.");
    }

    public async Task SelectCountryAsync(string country)
    {
        await CountryDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var current = NormalizeOption(await GetSelectedCountryAsync());
        if (current.Equals(country, StringComparison.Ordinal))
            return;

        await OpenSearchableSelectAsync(CountryDropdown);

        var search = VisibleDropdownList().Locator("input");
        if (await search.CountAsync() == 0)
            search = CountryDropdown.Locator("input");

        if (await search.CountAsync() > 0 && await search.First.IsVisibleAsync())
            await search.First.FillAsync(country);

        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var options = VisibleDropdownList().Locator("[role='option']");
            var count = await options.CountAsync();
            for (var i = 0; i < count; i++)
            {
                var candidate = options.Nth(i);
                var text = NormalizeOption(await candidate.InnerTextAsync());
                if (!text.Equals(country, StringComparison.Ordinal))
                    continue;

                await candidate.ClickAsync();
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

                return;
            }

            if (count > 0)
                await options.Last.ScrollIntoViewIfNeededAsync();

            await Task.Delay(150);
        }

        throw new InvalidOperationException($"Country '{country}' was not found.");
    }

    public async Task<OfficesPage> CreateOfficeAsync()
    {
        var responseTask = Page.WaitForResponseAsync(
            response => response.Request.Method is "POST" or "PUT",
            new PageWaitForResponseOptions { Timeout = 30_000 });
        await CreateOfficeBtn.ClickAsync();
        var response = await responseTask;
        if (!response.Ok)
        {
            var body = await response.TextAsync();
            throw new InvalidOperationException(
                $"Create office failed with status {(int)response.Status} {response.Url}: {body}");
        }

        try
        {
            await Page.WaitForURLAsync(
                url => url.Contains("/admin/offices", StringComparison.OrdinalIgnoreCase)
                       && !url.Contains("/new", StringComparison.OrdinalIgnoreCase),
                new PageWaitForURLOptions { Timeout = 30_000 });
        }
        catch (TimeoutException)
        {
            var errors = await Page.Locator(".error, .alert, [role='alert']").AllInnerTextsAsync();
            throw new TimeoutException(
                $"Create office stayed on {Page.Url} after {(int)response.Status} {response.Url}. {string.Join(" | ", errors.Select(error => error.Trim()))}");
        }

        var officesPage = new OfficesPage(Page);
        await officesPage.WaitForLoadedAsync();
        return officesPage;
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

    public Task<bool> IsDetailsTabSelectedAsync() => IsTabSelectedAsync(DetailsTab);
    public Task<bool> IsEmissionsTabSelectedAsync() => IsTabSelectedAsync(EmissionsTab);

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

    public async Task<bool> IsCalculateByFootageToggleVisibleAsync()
    {
        try
        {
            await CalculateByFootageToggle.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Attached,
                Timeout = 10_000
            });
            return await CalculateByFootageToggle
                .Locator("xpath=ancestor::span[@class='form-toggle__control']")
                .IsVisibleAsync();
        }
        catch (TimeoutException)
        {
            return false;
        }
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

    public async Task SelectEmissionTypeAsync(string code, string displayName)
    {
        await EmissionTypeDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await OpenSearchableSelectAsync(EmissionTypeDropdown);

        var search = VisibleDropdownList().Locator("input");
        if (await search.CountAsync() == 0)
            search = EmissionTypeDropdown.Locator("input");

        if (await search.CountAsync() > 0 && await search.First.IsVisibleAsync())
            await search.First.FillAsync(displayName);

        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var options = VisibleDropdownList().Locator("[role='option']");
            var count = await options.CountAsync();
            for (var i = 0; i < count; i++)
            {
                var candidate = options.Nth(i);
                var text = NormalizeOption(await candidate.InnerTextAsync());
                if (!IsEmissionTypeOption(text, code, displayName))
                    continue;

                await candidate.ClickAsync();
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

                return;
            }

            if (count > 0)
                await options.Last.ScrollIntoViewIfNeededAsync();

            await Task.Delay(150);
        }

        throw new InvalidOperationException($"Emission type '{displayName}' was not found.");
    }

    public async Task<string> GetEmissionsMessageAsync()
    {
        await EmissionsMessage.WaitForAsync();
        return (await EmissionsMessage.InnerTextAsync()).Trim();
    }

    public async Task<IReadOnlyList<string>> GetProjectDropdownOptionsAsync()
    {
        await ProjectDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await OpenSearchableSelectAsync(ProjectDropdown);
        return await ReadVisibleDropdownOptionsAsync();
    }

    public async Task<IReadOnlyList<string>> GetEmissionTypeDropdownOptionsAsync()
    {
        await EmissionTypeDropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await OpenSearchableSelectAsync(EmissionTypeDropdown);
        return await ReadVisibleDropdownOptionsAsync();
    }

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

    private static async Task<bool> IsTabSelectedAsync(ILocator tab)
    {
        await tab.WaitForAsync();
        var className = await tab.GetAttributeAsync("class") ?? string.Empty;
        return className.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Contains("tabs__item--active");
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

    private async Task<IReadOnlyList<string>> ReadVisibleDropdownOptionsAsync()
    {
        var list = VisibleDropdownList();
        var seen = new List<string>();
        var stablePasses = 0;
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline && stablePasses < 3)
        {
            var added = false;
            foreach (var raw in await list.Locator("[role='option']").AllInnerTextsAsync())
            {
                var option = System.Text.RegularExpressions.Regex.Replace(raw.Replace('\u00A0', ' '), @"\s+", " ").Trim();
                if (option.Length == 0 || seen.Contains(option, StringComparer.Ordinal))
                    continue;

                seen.Add(option);
                added = true;
            }

            var options = list.Locator("[role='option']");
            if (await options.CountAsync() > 0)
                await options.Last.ScrollIntoViewIfNeededAsync();

            if (added)
                stablePasses = 0;
            else
                stablePasses++;

            await Task.Delay(150);
        }

        return seen;
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

    private static string NormalizeOption(string value) =>
        System.Text.RegularExpressions.Regex.Replace(value.Replace('\u00A0', ' '), @"\s+", " ").Trim();

    private static bool IsEmissionTypeOption(string option, string code, string displayName)
    {
        var text = NormalizeOption(option);
        return text.Equals(code, StringComparison.Ordinal)
            || text.StartsWith(code + " —", StringComparison.Ordinal)
            || text.StartsWith(code + " -", StringComparison.Ordinal)
            || text.Equals(displayName, StringComparison.Ordinal)
            || text.EndsWith("— " + displayName, StringComparison.Ordinal)
            || text.EndsWith("- " + displayName, StringComparison.Ordinal);
    }

    private static bool IsProjectOption(string option, string projectName)
    {
        var text = NormalizeOption(option);
        return text.Equals(projectName, StringComparison.Ordinal)
            || text.EndsWith("— " + projectName, StringComparison.Ordinal)
            || text.EndsWith("- " + projectName, StringComparison.Ordinal);
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
