using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class AddOfficeTests : BaseTest
{
    [Test]
    public async Task T01_AddOffice_DefaultView()
    {
        const string expectedTitle = "Add Office";

        var addOfficePage = await OpenAddOfficePageAsync();

        (await addOfficePage.GetPageTitleAsync()).Should().Be(expectedTitle);
        (await addOfficePage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsDetailsTabVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsDetailsTabSelectedAsync()).Should().BeTrue();
        (await addOfficePage.IsEmissionsTabVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsEmissionsTabSelectedAsync()).Should().BeFalse();
        (await addOfficePage.IsCreateOfficeBtnVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsCancelBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_AddOffice_ClickBack()
    {
        const string expectedPageTitle = "Offices";

        var addOfficePage = await OpenAddOfficePageAsync();
        var officesPage = await addOfficePage.ClickBackBtnAsync();

        (await officesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T03_AddOffice_DetailsTab()
    {
        const string expectedCountry = "United States";
        const string expectedEndDateMessage = "Leave blank while the office is in use.";

        var addOfficePage = await OpenAddOfficePageAsync();

        (await addOfficePage.IsProjectDropdownVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsNameInputVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsAddressLine1InputVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsAddressLine2InputVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsCityInputVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsStateInputVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsPostalCodeInputVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsCountryDropdownVisibleAsync()).Should().BeTrue();
        (await addOfficePage.GetSelectedCountryAsync()).Should().Be(expectedCountry);
        (await addOfficePage.IsStartDateInputVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsEndDateInputVisibleAsync()).Should().BeTrue();
        (await addOfficePage.GetEndDateMessageAsync()).Should().Be(expectedEndDateMessage);
        (await addOfficePage.IsAreaInputVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T04_AddOffice_EmissionsTab()
    {
        const string expectedEmissionsMessage =
            "Off: emissions come from utility bills. On: EquipAI calculates them from the area each month, and bills cannot be assigned to the office.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.ClickEmissionsTabAsync();

        (await addOfficePage.IsCalculateByFootageToggleVisibleAsync()).Should().BeTrue();
        (await addOfficePage.IsCalculateByFootageToggleOnAsync()).Should().BeFalse();
        (await addOfficePage.GetEmissionsMessageAsync()).Should().Be(expectedEmissionsMessage);
    }

    [Test]
    public async Task T05_AddOffice_ProjectDropdown_Options()
    {
        var expected = await SqlHelper.GetOfficeProjectDropdownOptionsAsync();

        var addOfficePage = await OpenAddOfficePageAsync();

        if (expected.Count == 1)
            (await addOfficePage.GetSelectedProjectAsync()).Should().Be(expected[0]);

        var actual = await addOfficePage.GetProjectDropdownOptionsAsync();

        if (expected.Count == 0)
        {
            actual.Should().BeEmpty();
            return;
        }

        actual.Should().Equal(expected);
    }

    [Test]
    public async Task T06_AddOffice_EmptyFields()
    {
        var expectedProjects = await SqlHelper.GetOfficeProjectDropdownOptionsAsync();

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.ClickEmissionsTabAsync();
        await addOfficePage.TurnCalculateByFootageOnAsync();
        await addOfficePage.ClickCreateOfficeBtnAsync();
        await addOfficePage.ClickDetailsTabAsync();

        (await addOfficePage.GetNameErrorAsync()).Should().Be("Name is required.");

        if (expectedProjects.Count == 1)
            (await addOfficePage.IsProjectErrorVisibleAsync()).Should().BeFalse();
        else
            (await addOfficePage.GetProjectErrorAsync()).Should().Be("Project is required.");

        (await addOfficePage.GetAddressLine1ErrorAsync()).Should().Be("Address line 1 is required.");
        (await addOfficePage.IsAddressLine2ErrorVisibleAsync()).Should().BeFalse();
        (await addOfficePage.GetCityErrorAsync()).Should().Be("City is required.");
        (await addOfficePage.GetStateErrorAsync()).Should().Be("State or province is required.");
        (await addOfficePage.GetPostalCodeErrorAsync()).Should().Be("Postal code is required.");
        (await addOfficePage.GetStartDateErrorAsync()).Should().Be("Start date is required.");
        (await addOfficePage.IsEndDateErrorVisibleAsync()).Should().BeFalse();
        (await addOfficePage.GetAreaErrorAsync()).Should().Be("Area is required.");

        await addOfficePage.ClickEmissionsTabAsync();
        (await addOfficePage.GetEmissionTypeErrorAsync()).Should().Be("Emission type is required.");
    }

    [Test]
    public async Task T07_AddOffice_NameTooLong()
    {
        const string expectedNameError = "Name must be at most 256 characters.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillNameAsync(GenerateRandomAlphanumericString(257));
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetNameErrorAsync()).Should().Be(expectedNameError);
    }

    [Test]
    public async Task T08_AddOffice_AddressLine1TooLong()
    {
        const string expectedAddressLine1Error = "Address line 1 must be at most 256 characters.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillAddressLine1Async(GenerateRandomAlphanumericString(257));
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetAddressLine1ErrorAsync()).Should().Be(expectedAddressLine1Error);
    }

    [Test]
    public async Task T09_AddOffice_AddressLine2TooLong()
    {
        const string expectedAddressLine2Error = "Address line 2 must be at most 256 characters.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillAddressLine2Async(GenerateRandomAlphanumericString(257));
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetAddressLine2ErrorAsync()).Should().Be(expectedAddressLine2Error);
    }

    [Test]
    public async Task T10_AddOffice_CityTooLong()
    {
        const string expectedCityError = "City must be at most 128 characters.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillCityAsync(GenerateRandomAlphanumericString(129));
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetCityErrorAsync()).Should().Be(expectedCityError);
    }

    [Test]
    public async Task T11_AddOffice_StateTooLong()
    {
        const string expectedStateError = "State or province must be at most 64 characters.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillStateAsync(GenerateRandomAlphanumericString(65));
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetStateErrorAsync()).Should().Be(expectedStateError);
    }

    [Test]
    public async Task T12_AddOffice_PostalCodeTooLong()
    {
        const string expectedPostalCodeError = "Postal code must be at most 32 characters.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillPostalCodeAsync(GenerateRandomAlphanumericString(33));
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetPostalCodeErrorAsync()).Should().Be(expectedPostalCodeError);
    }

    [Test]
    public async Task T13_AddOffice_AreaIncorrect()
    {
        const string expectedAreaError = "Area must have no more than 2 decimal places.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillAreaAsync("123.123");
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetAreaErrorAsync()).Should().Be(expectedAreaError);
    }

    [Test]
    public async Task T14_AddOffice_ExistingName()
    {
        const string expectedAlertMessage = "An office or jobsite trailer with this name already exists in the project.";
        var createdAt = DateTime.Now;
        var stamp = createdAt.ToString("yyyyMMddHHmmss");
        var addressLine1 = $"Address1{stamp}";
        var addressLine2 = $"Address2{stamp}";
        var city = $"City{stamp}";
        var state = $"State{stamp}";
        var postalCode = $"Code{stamp}";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.SelectProjectAsync(Config.SetupProjectName1);
        await addOfficePage.FillNameAsync(Config.SetupOfficeName1);
        await addOfficePage.FillAddressLine1Async(addressLine1);
        await addOfficePage.FillAddressLine2Async(addressLine2);
        await addOfficePage.FillCityAsync(city);
        await addOfficePage.FillStateAsync(state);
        await addOfficePage.FillPostalCodeAsync(postalCode);
        await addOfficePage.FillStartDateAsync(createdAt.Date);
        await addOfficePage.FillAreaAsync(Config.SetupOfficeArea1);
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T15_AddOffice_EmissionTypeOptions()
    {
        var expected = await SqlHelper.GetSqftEmissionTypeDisplayNamesAsync();

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.ClickEmissionsTabAsync();
        await addOfficePage.TurnCalculateByFootageOnAsync();

        var actual = await addOfficePage.GetEmissionTypeDropdownOptionsAsync();
        actual.Should().Equal(expected);
    }

    [Test]
    public async Task T16_AddOffice_ClickCancel()
    {
        const string expectedPageTitle = "Offices";
        var createdAt = DateTime.Now;
        var stamp = createdAt.ToString("yyyyMMddHHmmss");
        var name = $"Office{stamp}";
        var addressLine1 = $"Address1{stamp}";
        var addressLine2 = $"Address2{stamp}";
        var city = $"City{stamp}";
        var state = $"State{stamp}";
        var postalCode = $"Code{stamp}";

        try
        {
            var addOfficePage = await OpenAddOfficePageAsync();
            await addOfficePage.SelectProjectAsync(Config.SetupProjectName1);
            await addOfficePage.FillNameAsync(name);
            await addOfficePage.FillAddressLine1Async(addressLine1);
            await addOfficePage.FillAddressLine2Async(addressLine2);
            await addOfficePage.FillCityAsync(city);
            await addOfficePage.FillStateAsync(state);
            await addOfficePage.FillPostalCodeAsync(postalCode);
            await addOfficePage.FillStartDateAsync(createdAt.Date);
            await addOfficePage.FillAreaAsync(Config.SetupOfficeArea1);
            var officesPage = await addOfficePage.ClickCancelBtnAsync();

            (await officesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            (await officesPage.IsNameInGridAsync(name)).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.DeleteOfficeByNameAsync(name);
        }
    }

    [Test]
    public async Task T17_AddOffice_OfficeBills_Success()
    {
        var createdAt = DateTime.Now;
        var stamp = createdAt.ToString("yyyyMMddHHmmss");
        var name = $"Office{stamp}";
        var addressLine1 = $"Address1{stamp}";
        var addressLine2 = $"Address2{stamp}";
        var city = $"City{stamp}";
        var state = $"State{stamp}";
        var postalCode = $"Code{stamp}";
        var startDate = createdAt.Date;

        try
        {
            var addOfficePage = await OpenAddOfficePageAsync();
            var project = await addOfficePage.SelectProjectAsync(Config.SetupProjectName1);
            await addOfficePage.FillNameAsync(name);
            await addOfficePage.FillAddressLine1Async(addressLine1);
            await addOfficePage.FillAddressLine2Async(addressLine2);
            await addOfficePage.FillCityAsync(city);
            await addOfficePage.FillStateAsync(state);
            await addOfficePage.FillPostalCodeAsync(postalCode);
            await addOfficePage.FillStartDateAsync(startDate);
            await addOfficePage.FillAreaAsync(Config.SetupOfficeArea1);
            var officesPage = await addOfficePage.CreateOfficeAsync();

            await officesPage.SearchAsync(name);
            var actual = await officesPage.GetAllGridRecordsAsync();
            actual.Should().ContainSingle();
            actual[0].Should().Be(new OfficeGridRecord(
                project,
                name,
                "Office",
                FormatAddress($"{addressLine1}, {addressLine2}, {city}, {state} {postalCode}", "US"),
                FormatDate(startDate),
                "—",
                FormatArea(decimal.Parse(Config.SetupOfficeArea1, CultureInfo.InvariantCulture))));
        }
        finally
        {
            await SqlHelper.DeleteOfficeByNameAsync(name);
        }
    }

    [Test]
    public async Task T18_AddOffice_OfficeSQFT_Success()
    {
        var createdAt = DateTime.Now;
        var stamp = createdAt.ToString("yyyyMMddHHmmss");
        var name = $"Office{stamp}";
        var addressLine1 = $"Address1{stamp}";
        var addressLine2 = $"Address2{stamp}";
        var city = $"City{stamp}";
        var state = $"State{stamp}";
        var postalCode = $"Code{stamp}";
        var startDate = createdAt.Date;
        var endDate = startDate.AddYears(2);

        try
        {
            await SqlHelper.DeleteGeneratedTestOfficesAsync();
            var addOfficePage = await OpenAddOfficePageAsync();
            var project = await addOfficePage.SelectProjectAsync(Config.SetupProjectName1);
            await addOfficePage.FillNameAsync(name);
            await addOfficePage.FillAddressLine1Async(addressLine1);
            await addOfficePage.FillAddressLine2Async(addressLine2);
            await addOfficePage.FillCityAsync(city);
            await addOfficePage.FillStateAsync(state);
            await addOfficePage.FillPostalCodeAsync(postalCode);
            await addOfficePage.FillStartDateAsync(startDate);
            await addOfficePage.FillEndDateAsync(endDate);
            await addOfficePage.FillAreaAsync(Config.SetupOfficeArea1);
            await addOfficePage.ClickEmissionsTabAsync();
            await addOfficePage.TurnCalculateByFootageOnAsync();
            await addOfficePage.SelectEmissionTypeAsync(Config.SetupCode4, Config.SetupEmissionTypeName4);
            var officesPage = await addOfficePage.CreateOfficeAsync();

            await officesPage.SearchAsync(name);
            var actual = await officesPage.GetAllGridRecordsAsync();
            actual.Should().ContainSingle();
            actual[0].Should().Be(new OfficeGridRecord(
                project,
                name,
                "Office",
                FormatAddress($"{addressLine1}, {addressLine2}, {city}, {state} {postalCode}", "US"),
                FormatDate(startDate),
                FormatDate(endDate),
                FormatArea(decimal.Parse(Config.SetupOfficeArea1, CultureInfo.InvariantCulture))));
        }
        finally
        {
            await SqlHelper.DeleteOfficeByNameAsync(name);
        }
    }

    private async Task<AddOfficePage> OpenAddOfficePageAsync()
    {
        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        return await officesPage.ClickAddOfficeBtnAsync();
    }

    private static string FormatAddress(string address, string countryCode)
    {
        var country = CountryName(countryCode);
        if (string.IsNullOrEmpty(address))
            return country;
        if (string.IsNullOrEmpty(country))
            return address;

        return $"{address}, {country}";
    }

    private static string CountryName(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return string.Empty;

        try
        {
            return new RegionInfo(countryCode).EnglishName;
        }
        catch (ArgumentException)
        {
            return countryCode;
        }
    }

    private static string FormatDate(DateTime? value) =>
        value?.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string FormatArea(decimal? value)
    {
        if (value is null)
            return string.Empty;

        var rounded = decimal.Round(value.Value, 2, MidpointRounding.AwayFromZero);
        return rounded.ToString("#,##0.##", CultureInfo.GetCultureInfo("en-US"));
    }
}
