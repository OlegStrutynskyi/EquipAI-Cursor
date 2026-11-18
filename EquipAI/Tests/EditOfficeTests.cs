using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class EditOfficeTests : BaseTest
{
    [Test]
    public async Task T01_EditOffice_DefaultView_Office1()
    {
        const string expectedTitle = "Edit Office";
        var officeName = Config.SetupOfficeName1;
        var expectedProject = (await SqlHelper.GetOfficeProjectDropdownOptionsAsync())
            .Single(option =>
                option.Equals(Config.SetupProjectName1, StringComparison.Ordinal)
                || option.EndsWith("— " + Config.SetupProjectName1, StringComparison.Ordinal)
                || option.EndsWith("- " + Config.SetupProjectName1, StringComparison.Ordinal));

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        var editOfficePage = await officesPage.ClickEditBtnAsync(officeName);

        (await editOfficePage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await editOfficePage.GetPageTitleAsync()).Should().Be(expectedTitle);
        (await editOfficePage.GetSubtitleAsync()).Should().Be(officeName);
        (await editOfficePage.GetSelectedProjectAsync()).Should().Be(expectedProject);
        (await editOfficePage.GetNameAsync()).Should().Be(officeName);
        (await editOfficePage.GetAddressLine1Async()).Should().Be(Config.SetupAddressLine1);
        (await editOfficePage.GetAddressLine2Async()).Should().Be(Config.SetupAddressLine2);
        (await editOfficePage.GetCityAsync()).Should().Be(Config.SetupCity);
        (await editOfficePage.GetStateAsync()).Should().Be(Config.SetupState);
        (await editOfficePage.GetPostalCodeAsync()).Should().Be(Config.SetupPostalCode);
        (await editOfficePage.GetSelectedCountryAsync()).Should().Be(Config.SetupCountry);
        (await editOfficePage.GetStartDateAsync()).Should().Be(Config.SetupOfficeStartDate1);
        (await editOfficePage.GetEndDateAsync()).Should().Be(Config.SetupOfficeEndDate1);
        (await editOfficePage.GetAreaAsync()).Should().Be(Config.SetupOfficeArea1);

        await editOfficePage.ClickEmissionsTabAsync();
        (await editOfficePage.IsCalculateByFootageToggleOnAsync()).Should().BeFalse();
    }

    [Test]
    public async Task T02_EditOffice_DefaultView_Office2()
    {
        const string expectedTitle = "Edit Office";
        var officeName = Config.SetupOfficeName2;
        var expectedProject = (await SqlHelper.GetOfficeProjectDropdownOptionsAsync())
            .Single(option =>
                option.Equals(Config.SetupProjectName1, StringComparison.Ordinal)
                || option.EndsWith("— " + Config.SetupProjectName1, StringComparison.Ordinal)
                || option.EndsWith("- " + Config.SetupProjectName1, StringComparison.Ordinal));

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        var editOfficePage = await officesPage.ClickEditBtnAsync(officeName);

        (await editOfficePage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await editOfficePage.GetPageTitleAsync()).Should().Be(expectedTitle);
        (await editOfficePage.GetSubtitleAsync()).Should().Be(officeName);
        (await editOfficePage.GetSelectedProjectAsync()).Should().Be(expectedProject);
        (await editOfficePage.GetNameAsync()).Should().Be(officeName);
        (await editOfficePage.GetAddressLine1Async()).Should().Be(Config.SetupAddressLine1);
        (await editOfficePage.GetAddressLine2Async()).Should().Be(Config.SetupAddressLine2);
        (await editOfficePage.GetCityAsync()).Should().Be(Config.SetupCity);
        (await editOfficePage.GetStateAsync()).Should().Be(Config.SetupState);
        (await editOfficePage.GetPostalCodeAsync()).Should().Be(Config.SetupPostalCode);
        (await editOfficePage.GetSelectedCountryAsync()).Should().Be(Config.SetupCountry);
        (await editOfficePage.GetStartDateAsync()).Should().Be(Config.SetupOfficeStartDate2);
        (await editOfficePage.GetEndDateAsync()).Should().Be(Config.SetupOfficeEndDate2);
        (await editOfficePage.GetAreaAsync()).Should().Be(Config.SetupOfficeArea2);

        await editOfficePage.ClickEmissionsTabAsync();
        (await editOfficePage.IsCalculateByFootageToggleOnAsync()).Should().BeTrue();
        (await editOfficePage.GetSelectedEmissionTypeAsync()).Should().Be(Config.SetupEmissionTypeName4);
    }

    [Test]
    public async Task T03_EditOffice_ClickBack()
    {
        const string expectedPageTitle = "Offices";

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        var editOfficePage = await officesPage.ClickEditBtnAsync(Config.SetupOfficeName2);
        officesPage = await editOfficePage.ClickBackBtnAsync();

        (await officesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T04_EditOffice_EmptyFields()
    {
        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        var editOfficePage = await officesPage.ClickEditBtnAsync(Config.SetupOfficeName1);
        await editOfficePage.ClearAllInputsAsync();
        await editOfficePage.ClickEmissionsTabAsync();
        await editOfficePage.TurnCalculateByFootageOnAsync();
        await editOfficePage.ClickSaveOfficeBtnAsync();
        await editOfficePage.ClickDetailsTabAsync();

        (await editOfficePage.GetNameErrorAsync()).Should().Be("Name is required.");
        (await editOfficePage.IsProjectErrorVisibleAsync()).Should().BeFalse();

        (await editOfficePage.GetAddressLine1ErrorAsync()).Should().Be("Address line 1 is required.");
        (await editOfficePage.IsAddressLine2ErrorVisibleAsync()).Should().BeFalse();
        (await editOfficePage.GetCityErrorAsync()).Should().Be("City is required.");
        (await editOfficePage.GetStateErrorAsync()).Should().Be("State or province is required.");
        (await editOfficePage.GetPostalCodeErrorAsync()).Should().Be("Postal code is required.");
        (await editOfficePage.GetStartDateErrorAsync()).Should().Be("Start date is required.");
        (await editOfficePage.IsEndDateErrorVisibleAsync()).Should().BeFalse();
        (await editOfficePage.GetAreaErrorAsync()).Should().Be("Area is required.");

        await editOfficePage.ClickEmissionsTabAsync();
        (await editOfficePage.GetEmissionTypeErrorAsync()).Should().Be("Emission type is required.");
    }

    [Test]
    public async Task T05_EditOffice_AddressLine1TooLong()
    {
        const string expectedAddressLine1Error = "Address line 1 must be at most 256 characters.";

        var editOfficePage = await OpenEditOffice1Async();
        await editOfficePage.FillAddressLine1Async(GenerateRandomAlphanumericString(257));
        await editOfficePage.ClickSaveOfficeBtnAsync();

        (await editOfficePage.GetAddressLine1ErrorAsync()).Should().Be(expectedAddressLine1Error);
    }

    [Test]
    public async Task T06_EditOffice_AddressLine2TooLong()
    {
        const string expectedAddressLine2Error = "Address line 2 must be at most 256 characters.";

        var editOfficePage = await OpenEditOffice1Async();
        await editOfficePage.FillAddressLine2Async(GenerateRandomAlphanumericString(257));
        await editOfficePage.ClickSaveOfficeBtnAsync();

        (await editOfficePage.GetAddressLine2ErrorAsync()).Should().Be(expectedAddressLine2Error);
    }

    [Test]
    public async Task T07_EditOffice_CityTooLong()
    {
        const string expectedCityError = "City must be at most 128 characters.";

        var editOfficePage = await OpenEditOffice1Async();
        await editOfficePage.FillCityAsync(GenerateRandomAlphanumericString(129));
        await editOfficePage.ClickSaveOfficeBtnAsync();

        (await editOfficePage.GetCityErrorAsync()).Should().Be(expectedCityError);
    }

    [Test]
    public async Task T08_EditOffice_StateTooLong()
    {
        const string expectedStateError = "State or province must be at most 64 characters.";

        var editOfficePage = await OpenEditOffice1Async();
        await editOfficePage.FillStateAsync(GenerateRandomAlphanumericString(65));
        await editOfficePage.ClickSaveOfficeBtnAsync();

        (await editOfficePage.GetStateErrorAsync()).Should().Be(expectedStateError);
    }

    [Test]
    public async Task T09_EditOffice_PostalCodeTooLong()
    {
        const string expectedPostalCodeError = "Postal code must be at most 32 characters.";

        var editOfficePage = await OpenEditOffice1Async();
        await editOfficePage.FillPostalCodeAsync(GenerateRandomAlphanumericString(33));
        await editOfficePage.ClickSaveOfficeBtnAsync();

        (await editOfficePage.GetPostalCodeErrorAsync()).Should().Be(expectedPostalCodeError);
    }

    [Test]
    public async Task T10_EditOffice_AreaIncorrect()
    {
        const string expectedAreaError = "Area must have no more than 2 decimal places.";

        var editOfficePage = await OpenEditOffice1Async();
        await editOfficePage.FillAreaAsync("123.123");
        await editOfficePage.ClickSaveOfficeBtnAsync();

        (await editOfficePage.GetAreaErrorAsync()).Should().Be(expectedAreaError);
    }

    [Test]
    public async Task T11_EditOffice_ExistingName()
    {
        const string expectedAlertMessage = "An office or jobsite trailer with this name already exists in the project.";

        var editOfficePage = await OpenEditOffice1Async();
        await editOfficePage.FillNameAsync(Config.SetupOfficeName2);
        await editOfficePage.ClickSaveOfficeBtnAsync();

        (await editOfficePage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T12_EditOffice_ClickCancel()
    {
        const string expectedTitle = "Offices";
        var (officeId, addressId, countryCode) = await SqlHelper.GetOfficeIdAndAddressIdByNameAsync(Config.SetupOfficeName1);
        var expected = (await SqlHelper.GetOfficeGridRowsAsync())
            .Select(ToGridRecord)
            .Single(row => row.Name.Equals(Config.SetupOfficeName1, StringComparison.Ordinal));

        try
        {
            var editOfficePage = await OpenEditOffice1Async();
            await ChangeOffice1Async(editOfficePage);
            var officesPage = await editOfficePage.ClickCancelBtnAsync();

            (await officesPage.GetPageTitleAsync()).Should().Be(expectedTitle);
            await officesPage.SearchAsync(Config.SetupOfficeName1);
            var actual = await officesPage.GetAllGridRecordsAsync();
            actual.Should().ContainSingle();
            actual[0].Should().Be(expected);
        }
        finally
        {
            await SqlHelper.RestoreSetupOffice1Async(officeId, addressId, countryCode);
        }
    }

    [Test]
    public async Task T13_EditOffice_Office1_Success()
    {
        const string expectedTitle = "Offices";
        var (officeId, addressId, countryCode) = await SqlHelper.GetOfficeIdAndAddressIdByNameAsync(Config.SetupOfficeName1);
        var name = Config.SetupOfficeName1 + "UPDATED";
        var addressLine1 = Config.SetupAddressLine1 + "UPDATED";
        var addressLine2 = Config.SetupAddressLine2 + "UPDATED";
        var city = Config.SetupCity + "UPDATED";
        var state = Config.SetupState + "UPDATED";
        var postalCode = Config.SetupPostalCode + "UPDATED";
        var startDate = DateTime.ParseExact(Config.SetupOfficeStartDate2, "MM/dd/yyyy", CultureInfo.InvariantCulture);
        var area = decimal.Parse(Config.SetupOfficeArea2, CultureInfo.InvariantCulture);

        try
        {
            var editOfficePage = await OpenEditOffice1Async();
            var project = await ChangeOffice1Async(editOfficePage);
            var officesPage = await editOfficePage.SaveOfficeAsync();

            (await officesPage.GetPageTitleAsync()).Should().Be(expectedTitle);
            await officesPage.SearchAsync(name);
            var actual = await officesPage.GetAllGridRecordsAsync();
            actual.Should().ContainSingle();
            actual[0].Should().Be(new OfficeGridRecord(
                project,
                name,
                "Office",
                FormatAddress($"{addressLine1}, {addressLine2}, {city}, {state} {postalCode}", "CA"),
                FormatDate(startDate),
                "—",
                FormatArea(area)));
        }
        finally
        {
            await SqlHelper.RestoreSetupOffice1Async(officeId, addressId, countryCode);
        }
    }

    private async Task<string> ChangeOffice1Async(EditOfficePage editOfficePage)
    {
        var project = await editOfficePage.SelectDifferentProjectAsync();
        await editOfficePage.FillNameAsync(Config.SetupOfficeName1 + "UPDATED");
        await editOfficePage.FillAddressLine1Async(Config.SetupAddressLine1 + "UPDATED");
        await editOfficePage.FillAddressLine2Async(Config.SetupAddressLine2 + "UPDATED");
        await editOfficePage.FillCityAsync(Config.SetupCity + "UPDATED");
        await editOfficePage.FillStateAsync(Config.SetupState + "UPDATED");
        await editOfficePage.FillPostalCodeAsync(Config.SetupPostalCode + "UPDATED");
        await editOfficePage.SelectCountryAsync("Canada");
        await editOfficePage.FillStartDateAsync(
            DateTime.ParseExact(Config.SetupOfficeStartDate2, "MM/dd/yyyy", CultureInfo.InvariantCulture));
        await editOfficePage.ClearEndDateAsync();
        await editOfficePage.FillAreaAsync(Config.SetupOfficeArea2);
        await editOfficePage.ClickEmissionsTabAsync();
        await editOfficePage.TurnCalculateByFootageOnAsync();
        await editOfficePage.SelectEmissionTypeAsync(Config.SetupEmissionTypeName4);
        return project;
    }

    private async Task<EditOfficePage> OpenEditOffice1Async()
    {
        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        return await officesPage.ClickEditBtnAsync(Config.SetupOfficeName1);
    }

    private static OfficeGridRecord ToGridRecord(OfficeGridDbRow row) =>
        new(
            row.Project,
            row.Name,
            string.IsNullOrWhiteSpace(row.Kind) ? "—" : row.Kind,
            FormatAddress(row.Address, row.CountryCode),
            FormatDate(row.StartDate),
            row.EndDate is null ? "—" : FormatDate(row.EndDate),
            FormatArea(row.AreaSquareFeet));

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
