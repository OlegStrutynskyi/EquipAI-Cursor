using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class EditJobsiteTrailerTests : BaseTest
{
    [Test]
    public async Task T01_EditJobsiteTrailer_DefaultView_Trailer1()
    {
        const string expectedTitle = "Edit Jobsite Trailer";
        var trailerName = Config.SetupJobsiteTrailerName1;
        var expectedProject = (await SqlHelper.GetActiveProjectDropdownOptionsAsync())
            .Single(option =>
                option.Equals(Config.SetupProjectName1, StringComparison.Ordinal)
                || option.EndsWith("— " + Config.SetupProjectName1, StringComparison.Ordinal)
                || option.EndsWith("- " + Config.SetupProjectName1, StringComparison.Ordinal));

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        var editJobsiteTrailerPage = await officesPage.ClickEditJobsiteTrailerBtnAsync(trailerName);

        (await editJobsiteTrailerPage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await editJobsiteTrailerPage.GetPageTitleAsync()).Should().Be(expectedTitle);
        (await editJobsiteTrailerPage.GetSubtitleAsync()).Should().Be(trailerName);
        (await editJobsiteTrailerPage.GetSelectedProjectAsync()).Should().Be(expectedProject);
        (await editJobsiteTrailerPage.GetNameAsync()).Should().Be(trailerName);
        (await editJobsiteTrailerPage.GetAddressLine1Async()).Should().Be(Config.SetupAddressLine1);
        (await editJobsiteTrailerPage.GetAddressLine2Async()).Should().Be(Config.SetupAddressLine2);
        (await editJobsiteTrailerPage.GetCityAsync()).Should().Be(Config.SetupCity);
        (await editJobsiteTrailerPage.GetStateAsync()).Should().Be(Config.SetupState);
        (await editJobsiteTrailerPage.GetPostalCodeAsync()).Should().Be(Config.SetupPostalCode);
        (await editJobsiteTrailerPage.GetSelectedCountryAsync()).Should().Be(Config.SetupCountry);
        (await editJobsiteTrailerPage.GetStartDateAsync()).Should().Be(Config.SetupJobsiteTrailerStartDate1);
        (await editJobsiteTrailerPage.GetEndDateAsync()).Should().Be(Config.SetupJobsiteTrailerEndDate1);
        (await editJobsiteTrailerPage.GetAreaAsync()).Should().Be(Config.SetupJobsiteTrailerArea1);

        await editJobsiteTrailerPage.ClickEmissionsTabAsync();
        (await editJobsiteTrailerPage.IsCalculateByFootageToggleOnAsync()).Should().BeFalse();
    }

    [Test]
    public async Task T02_EditJobsiteTrailer_DefaultView_Trailer2()
    {
        const string expectedTitle = "Edit Jobsite Trailer";
        var trailerName = Config.SetupJobsiteTrailerName2;
        var expectedProject = (await SqlHelper.GetActiveProjectDropdownOptionsAsync())
            .Single(option =>
                option.Equals(Config.SetupProjectName1, StringComparison.Ordinal)
                || option.EndsWith("— " + Config.SetupProjectName1, StringComparison.Ordinal)
                || option.EndsWith("- " + Config.SetupProjectName1, StringComparison.Ordinal));

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        var editJobsiteTrailerPage = await officesPage.ClickEditJobsiteTrailerBtnAsync(trailerName);

        (await editJobsiteTrailerPage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await editJobsiteTrailerPage.GetPageTitleAsync()).Should().Be(expectedTitle);
        (await editJobsiteTrailerPage.GetSubtitleAsync()).Should().Be(trailerName);
        (await editJobsiteTrailerPage.GetSelectedProjectAsync()).Should().Be(expectedProject);
        (await editJobsiteTrailerPage.GetNameAsync()).Should().Be(trailerName);
        (await editJobsiteTrailerPage.GetAddressLine1Async()).Should().Be(Config.SetupAddressLine1);
        (await editJobsiteTrailerPage.GetAddressLine2Async()).Should().Be(Config.SetupAddressLine2);
        (await editJobsiteTrailerPage.GetCityAsync()).Should().Be(Config.SetupCity);
        (await editJobsiteTrailerPage.GetStateAsync()).Should().Be(Config.SetupState);
        (await editJobsiteTrailerPage.GetPostalCodeAsync()).Should().Be(Config.SetupPostalCode);
        (await editJobsiteTrailerPage.GetSelectedCountryAsync()).Should().Be(Config.SetupCountry);
        (await editJobsiteTrailerPage.GetStartDateAsync()).Should().Be(Config.SetupJobsiteTrailerStartDate2);
        (await editJobsiteTrailerPage.GetEndDateAsync()).Should().Be(Config.SetupJobsiteTrailerEndDate2);
        (await editJobsiteTrailerPage.GetAreaAsync()).Should().Be(Config.SetupJobsiteTrailerArea2);

        await editJobsiteTrailerPage.ClickEmissionsTabAsync();
        (await editJobsiteTrailerPage.IsCalculateByFootageToggleOnAsync()).Should().BeTrue();
        (await editJobsiteTrailerPage.GetSelectedEmissionTypeAsync()).Should().Be(Config.SetupEmissionTypeName4);
    }

    [Test]
    public async Task T03_EditJobsiteTrailer_ClickBack()
    {
        const string expectedPageTitle = "Offices";

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        var editJobsiteTrailerPage = await officesPage.ClickEditJobsiteTrailerBtnAsync(Config.SetupJobsiteTrailerName2);
        officesPage = await editJobsiteTrailerPage.ClickBackBtnAsync();

        (await officesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T04_EditJobsiteTrailer_EmptyFields()
    {
        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        var editJobsiteTrailerPage = await officesPage.ClickEditJobsiteTrailerBtnAsync(Config.SetupJobsiteTrailerName1);
        await editJobsiteTrailerPage.ClearAllInputsAsync();
        await editJobsiteTrailerPage.ClickEmissionsTabAsync();
        await editJobsiteTrailerPage.TurnCalculateByFootageOnAsync();
        await editJobsiteTrailerPage.ClickSaveOfficeBtnAsync();
        await editJobsiteTrailerPage.ClickDetailsTabAsync();

        (await editJobsiteTrailerPage.GetNameErrorAsync()).Should().Be("Name is required.");
        (await editJobsiteTrailerPage.IsProjectErrorVisibleAsync()).Should().BeFalse();

        (await editJobsiteTrailerPage.GetAddressLine1ErrorAsync()).Should().Be("Address line 1 is required.");
        (await editJobsiteTrailerPage.IsAddressLine2ErrorVisibleAsync()).Should().BeFalse();
        (await editJobsiteTrailerPage.GetCityErrorAsync()).Should().Be("City is required.");
        (await editJobsiteTrailerPage.GetStateErrorAsync()).Should().Be("State or province is required.");
        (await editJobsiteTrailerPage.GetPostalCodeErrorAsync()).Should().Be("Postal code is required.");
        (await editJobsiteTrailerPage.GetStartDateErrorAsync()).Should().Be("Start date is required.");
        (await editJobsiteTrailerPage.IsEndDateErrorVisibleAsync()).Should().BeFalse();
        (await editJobsiteTrailerPage.GetAreaErrorAsync()).Should().Be("Area is required.");

        await editJobsiteTrailerPage.ClickEmissionsTabAsync();
        (await editJobsiteTrailerPage.GetEmissionTypeErrorAsync()).Should().Be("Emission type is required.");
    }

    [Test]
    public async Task T05_EditJobsiteTrailer_AddressLine1TooLong()
    {
        const string expectedAddressLine1Error = "Address line 1 must be at most 256 characters.";

        var editJobsiteTrailerPage = await OpenEditJobsiteTrailer1Async();
        await editJobsiteTrailerPage.FillAddressLine1Async(GenerateRandomAlphanumericString(257));
        await editJobsiteTrailerPage.ClickSaveOfficeBtnAsync();

        (await editJobsiteTrailerPage.GetAddressLine1ErrorAsync()).Should().Be(expectedAddressLine1Error);
    }

    [Test]
    public async Task T06_EditJobsiteTrailer_AddressLine2TooLong()
    {
        const string expectedAddressLine2Error = "Address line 2 must be at most 256 characters.";

        var editJobsiteTrailerPage = await OpenEditJobsiteTrailer1Async();
        await editJobsiteTrailerPage.FillAddressLine2Async(GenerateRandomAlphanumericString(257));
        await editJobsiteTrailerPage.ClickSaveOfficeBtnAsync();

        (await editJobsiteTrailerPage.GetAddressLine2ErrorAsync()).Should().Be(expectedAddressLine2Error);
    }

    [Test]
    public async Task T07_EditJobsiteTrailer_CityTooLong()
    {
        const string expectedCityError = "City must be at most 128 characters.";

        var editJobsiteTrailerPage = await OpenEditJobsiteTrailer1Async();
        await editJobsiteTrailerPage.FillCityAsync(GenerateRandomAlphanumericString(129));
        await editJobsiteTrailerPage.ClickSaveOfficeBtnAsync();

        (await editJobsiteTrailerPage.GetCityErrorAsync()).Should().Be(expectedCityError);
    }

    [Test]
    public async Task T08_EditJobsiteTrailer_StateTooLong()
    {
        const string expectedStateError = "State or province must be at most 64 characters.";

        var editJobsiteTrailerPage = await OpenEditJobsiteTrailer1Async();
        await editJobsiteTrailerPage.FillStateAsync(GenerateRandomAlphanumericString(65));
        await editJobsiteTrailerPage.ClickSaveOfficeBtnAsync();

        (await editJobsiteTrailerPage.GetStateErrorAsync()).Should().Be(expectedStateError);
    }

    [Test]
    public async Task T09_EditJobsiteTrailer_PostalCodeTooLong()
    {
        const string expectedPostalCodeError = "Postal code must be at most 32 characters.";

        var editJobsiteTrailerPage = await OpenEditJobsiteTrailer1Async();
        await editJobsiteTrailerPage.FillPostalCodeAsync(GenerateRandomAlphanumericString(33));
        await editJobsiteTrailerPage.ClickSaveOfficeBtnAsync();

        (await editJobsiteTrailerPage.GetPostalCodeErrorAsync()).Should().Be(expectedPostalCodeError);
    }

    [Test]
    public async Task T10_EditJobsiteTrailer_AreaIncorrect()
    {
        const string expectedAreaError = "Area must have no more than 2 decimal places.";

        var editJobsiteTrailerPage = await OpenEditJobsiteTrailer1Async();
        await editJobsiteTrailerPage.FillAreaAsync("123.123");
        await editJobsiteTrailerPage.ClickSaveOfficeBtnAsync();

        (await editJobsiteTrailerPage.GetAreaErrorAsync()).Should().Be(expectedAreaError);
    }

    [Test]
    public async Task T11_EditJobsiteTrailer_ExistingName()
    {
        const string expectedAlertMessage = "An office or jobsite trailer with this name already exists in the project.";

        var editJobsiteTrailerPage = await OpenEditJobsiteTrailer1Async();
        await editJobsiteTrailerPage.FillNameAsync(Config.SetupJobsiteTrailerName2);
        await editJobsiteTrailerPage.ClickSaveOfficeBtnAsync();

        (await editJobsiteTrailerPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T12_EditJobsiteTrailer_ClickCancel()
    {
        const string expectedTitle = "Offices";
        var (officeId, addressId, countryCode) = await SqlHelper.GetOfficeIdAndAddressIdByNameAsync(Config.SetupJobsiteTrailerName1);
        var expected = (await SqlHelper.GetOfficeGridRowsAsync())
            .Select(ToGridRecord)
            .Single(row => row.Name.Equals(Config.SetupJobsiteTrailerName1, StringComparison.Ordinal));

        try
        {
            var editJobsiteTrailerPage = await OpenEditJobsiteTrailer1Async();
            await ChangeJobsiteTrailer1Async(editJobsiteTrailerPage);
            var officesPage = await editJobsiteTrailerPage.ClickCancelBtnAsync();

            (await officesPage.GetPageTitleAsync()).Should().Be(expectedTitle);
            await officesPage.SearchAsync(Config.SetupJobsiteTrailerName1);
            var actual = await officesPage.GetAllGridRecordsAsync();
            actual.Should().ContainSingle();
            actual[0].Should().Be(expected);
        }
        finally
        {
            await SqlHelper.RestoreSetupJobsiteTrailer1Async(officeId, addressId, countryCode);
        }
    }

    [Test]
    public async Task T13_EditJobsiteTrailer_Trailer1_Success()
    {
        const string expectedTitle = "Offices";
        var (officeId, addressId, countryCode) = await SqlHelper.GetOfficeIdAndAddressIdByNameAsync(Config.SetupJobsiteTrailerName1);
        var name = Config.SetupJobsiteTrailerName1 + "UPDATED";
        var addressLine1 = Config.SetupAddressLine1 + "UPDATED";
        var addressLine2 = Config.SetupAddressLine2 + "UPDATED";
        var city = Config.SetupCity + "UPDATED";
        var state = Config.SetupState + "UPDATED";
        var postalCode = Config.SetupPostalCode + "UPDATED";
        var startDate = DateTime.ParseExact(Config.SetupJobsiteTrailerStartDate2, "MM/dd/yyyy", CultureInfo.InvariantCulture);
        var area = decimal.Parse(Config.SetupJobsiteTrailerArea2, CultureInfo.InvariantCulture);

        try
        {
            var editJobsiteTrailerPage = await OpenEditJobsiteTrailer1Async();
            var project = await ChangeJobsiteTrailer1Async(editJobsiteTrailerPage);
            var officesPage = await editJobsiteTrailerPage.SaveOfficeAsync();

            (await officesPage.GetPageTitleAsync()).Should().Be(expectedTitle);
            await officesPage.SearchAsync(name);
            var actual = await officesPage.GetAllGridRecordsAsync();
            actual.Should().ContainSingle();
            actual[0].Should().Be(new OfficeGridRecord(
                project,
                name,
                "Jobsite trailer",
                FormatAddress($"{addressLine1}, {addressLine2}, {city}, {state} {postalCode}", "CA"),
                FormatDate(startDate),
                "—",
                FormatArea(area)));
        }
        finally
        {
            await SqlHelper.RestoreSetupJobsiteTrailer1Async(officeId, addressId, countryCode);
        }
    }

    private async Task<string> ChangeJobsiteTrailer1Async(EditJobsiteTrailerPage editJobsiteTrailerPage)
    {
        var project = await editJobsiteTrailerPage.SelectDifferentProjectAsync();
        await editJobsiteTrailerPage.FillNameAsync(Config.SetupJobsiteTrailerName1 + "UPDATED");
        await editJobsiteTrailerPage.FillAddressLine1Async(Config.SetupAddressLine1 + "UPDATED");
        await editJobsiteTrailerPage.FillAddressLine2Async(Config.SetupAddressLine2 + "UPDATED");
        await editJobsiteTrailerPage.FillCityAsync(Config.SetupCity + "UPDATED");
        await editJobsiteTrailerPage.FillStateAsync(Config.SetupState + "UPDATED");
        await editJobsiteTrailerPage.FillPostalCodeAsync(Config.SetupPostalCode + "UPDATED");
        await editJobsiteTrailerPage.SelectCountryAsync("Canada");
        await editJobsiteTrailerPage.FillStartDateAsync(
            DateTime.ParseExact(Config.SetupJobsiteTrailerStartDate2, "MM/dd/yyyy", CultureInfo.InvariantCulture));
        await editJobsiteTrailerPage.ClearEndDateAsync();
        await editJobsiteTrailerPage.FillAreaAsync(Config.SetupJobsiteTrailerArea2);
        await editJobsiteTrailerPage.ClickEmissionsTabAsync();
        await editJobsiteTrailerPage.TurnCalculateByFootageOnAsync();
        await editJobsiteTrailerPage.SelectEmissionTypeAsync(Config.SetupEmissionTypeName4);
        return project;
    }

    private async Task<EditJobsiteTrailerPage> OpenEditJobsiteTrailer1Async()
    {
        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        return await officesPage.ClickEditJobsiteTrailerBtnAsync(Config.SetupJobsiteTrailerName1);
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
