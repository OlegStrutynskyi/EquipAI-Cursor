using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class AddJobsiteTrailerTests : BaseTest
{
    [Test]
    public async Task T01_AddJobsiteTrailer_DefaultView()
    {
        const string expectedTitle = "Add Jobsite Trailer";

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();

        (await addJobsiteTrailerPage.GetPageTitleAsync()).Should().Be(expectedTitle);
        (await addJobsiteTrailerPage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsDetailsTabVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsDetailsTabSelectedAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsEmissionsTabVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsEmissionsTabSelectedAsync()).Should().BeFalse();
        (await addJobsiteTrailerPage.IsCreateJobsiteTrailerBtnVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsCancelBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_AddJobsiteTrailer_ClickBack()
    {
        const string expectedPageTitle = "Offices";

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
        var officesPage = await addJobsiteTrailerPage.ClickBackBtnAsync();

        (await officesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T03_AddJobsiteTrailer_DetailsTab()
    {
        const string expectedCountry = "United States";
        const string expectedEndDateMessage = "Leave blank while the jobsite trailer is in use.";

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();

        (await addJobsiteTrailerPage.IsProjectDropdownVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsNameInputVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsAddressLine1InputVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsAddressLine2InputVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsCityInputVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsStateInputVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsPostalCodeInputVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsCountryDropdownVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.GetSelectedCountryAsync()).Should().Be(expectedCountry);
        (await addJobsiteTrailerPage.IsStartDateInputVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsEndDateInputVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.GetEndDateMessageAsync()).Should().Be(expectedEndDateMessage);
        (await addJobsiteTrailerPage.IsAreaInputVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T04_AddJobsiteTrailer_EmissionsTab()
    {
        const string expectedEmissionsMessage =
            "Off: emissions come from utility bills. On: EquipAI calculates them from the area each month, and bills cannot be assigned to the jobsite trailer.";

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
        await addJobsiteTrailerPage.ClickEmissionsTabAsync();

        (await addJobsiteTrailerPage.IsCalculateByFootageToggleVisibleAsync()).Should().BeTrue();
        (await addJobsiteTrailerPage.IsCalculateByFootageToggleOnAsync()).Should().BeFalse();
        (await addJobsiteTrailerPage.GetEmissionsMessageAsync()).Should().Be(expectedEmissionsMessage);
    }

    [Test]
    public async Task T05_AddJobsiteTrailer_ProjectDropdown_Options()
    {
        var expected = await SqlHelper.GetActiveProjectDropdownOptionsAsync();

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();

        if (expected.Count == 1)
            (await addJobsiteTrailerPage.GetSelectedProjectAsync()).Should().Be(expected[0]);

        var actual = await addJobsiteTrailerPage.GetProjectDropdownOptionsAsync();

        if (expected.Count == 0)
        {
            actual.Should().BeEmpty();
            return;
        }

        actual.Should().Equal(expected);
    }

    [Test]
    public async Task T06_AddJobsiteTrailer_EmptyFields()
    {
        var expectedProjects = await SqlHelper.GetActiveProjectDropdownOptionsAsync();

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
        await addJobsiteTrailerPage.ClickEmissionsTabAsync();
        await addJobsiteTrailerPage.TurnCalculateByFootageOnAsync();
        await addJobsiteTrailerPage.ClickCreateJobsiteTrailerBtnAsync();
        await addJobsiteTrailerPage.ClickDetailsTabAsync();

        (await addJobsiteTrailerPage.GetNameErrorAsync()).Should().Be("Name is required.");

        if (expectedProjects.Count == 1)
            (await addJobsiteTrailerPage.IsProjectErrorVisibleAsync()).Should().BeFalse();
        else
            (await addJobsiteTrailerPage.GetProjectErrorAsync()).Should().Be("Project is required.");

        (await addJobsiteTrailerPage.GetAddressLine1ErrorAsync()).Should().Be("Address line 1 is required.");
        (await addJobsiteTrailerPage.IsAddressLine2ErrorVisibleAsync()).Should().BeFalse();
        (await addJobsiteTrailerPage.GetCityErrorAsync()).Should().Be("City is required.");
        (await addJobsiteTrailerPage.GetStateErrorAsync()).Should().Be("State or province is required.");
        (await addJobsiteTrailerPage.GetPostalCodeErrorAsync()).Should().Be("Postal code is required.");
        (await addJobsiteTrailerPage.GetStartDateErrorAsync()).Should().Be("Start date is required.");
        (await addJobsiteTrailerPage.IsEndDateErrorVisibleAsync()).Should().BeFalse();
        (await addJobsiteTrailerPage.GetAreaErrorAsync()).Should().Be("Area is required.");

        await addJobsiteTrailerPage.ClickEmissionsTabAsync();
        (await addJobsiteTrailerPage.GetEmissionTypeErrorAsync()).Should().Be("Emission type is required.");
    }

    [Test]
    public async Task T07_AddJobsiteTrailer_NameTooLong()
    {
        const string expectedNameError = "Name must be at most 256 characters.";

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
        await addJobsiteTrailerPage.FillNameAsync(GenerateRandomAlphanumericString(257));
        await addJobsiteTrailerPage.ClickCreateJobsiteTrailerBtnAsync();

        (await addJobsiteTrailerPage.GetNameErrorAsync()).Should().Be(expectedNameError);
    }

    [Test]
    public async Task T08_AddJobsiteTrailer_AddressLine1TooLong()
    {
        const string expectedAddressLine1Error = "Address line 1 must be at most 256 characters.";

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
        await addJobsiteTrailerPage.FillAddressLine1Async(GenerateRandomAlphanumericString(257));
        await addJobsiteTrailerPage.ClickCreateJobsiteTrailerBtnAsync();

        (await addJobsiteTrailerPage.GetAddressLine1ErrorAsync()).Should().Be(expectedAddressLine1Error);
    }

    [Test]
    public async Task T09_AddJobsiteTrailer_AddressLine2TooLong()
    {
        const string expectedAddressLine2Error = "Address line 2 must be at most 256 characters.";

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
        await addJobsiteTrailerPage.FillAddressLine2Async(GenerateRandomAlphanumericString(257));
        await addJobsiteTrailerPage.ClickCreateJobsiteTrailerBtnAsync();

        (await addJobsiteTrailerPage.GetAddressLine2ErrorAsync()).Should().Be(expectedAddressLine2Error);
    }

    [Test]
    public async Task T10_AddJobsiteTrailer_CityTooLong()
    {
        const string expectedCityError = "City must be at most 128 characters.";

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
        await addJobsiteTrailerPage.FillCityAsync(GenerateRandomAlphanumericString(129));
        await addJobsiteTrailerPage.ClickCreateJobsiteTrailerBtnAsync();

        (await addJobsiteTrailerPage.GetCityErrorAsync()).Should().Be(expectedCityError);
    }

    [Test]
    public async Task T11_AddJobsiteTrailer_StateTooLong()
    {
        const string expectedStateError = "State or province must be at most 64 characters.";

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
        await addJobsiteTrailerPage.FillStateAsync(GenerateRandomAlphanumericString(65));
        await addJobsiteTrailerPage.ClickCreateJobsiteTrailerBtnAsync();

        (await addJobsiteTrailerPage.GetStateErrorAsync()).Should().Be(expectedStateError);
    }

    [Test]
    public async Task T12_AddJobsiteTrailer_PostalCodeTooLong()
    {
        const string expectedPostalCodeError = "Postal code must be at most 32 characters.";

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
        await addJobsiteTrailerPage.FillPostalCodeAsync(GenerateRandomAlphanumericString(33));
        await addJobsiteTrailerPage.ClickCreateJobsiteTrailerBtnAsync();

        (await addJobsiteTrailerPage.GetPostalCodeErrorAsync()).Should().Be(expectedPostalCodeError);
    }

    [Test]
    public async Task T13_AddJobsiteTrailer_AreaIncorrect()
    {
        const string expectedAreaError = "Area must have no more than 2 decimal places.";

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
        await addJobsiteTrailerPage.FillAreaAsync("123.123");
        await addJobsiteTrailerPage.ClickCreateJobsiteTrailerBtnAsync();

        (await addJobsiteTrailerPage.GetAreaErrorAsync()).Should().Be(expectedAreaError);
    }

    [Test]
    public async Task T14_AddJobsiteTrailer_ExistingName()
    {
        const string expectedAlertMessage = "An office or jobsite trailer with this name already exists in the project.";
        var createdAt = DateTime.Now;
        var stamp = createdAt.ToString("yyyyMMddHHmmss");
        var addressLine1 = $"Address1{stamp}";
        var addressLine2 = $"Address2{stamp}";
        var city = $"City{stamp}";
        var state = $"State{stamp}";
        var postalCode = $"Code{stamp}";

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
        await addJobsiteTrailerPage.SelectProjectAsync(Config.SetupProjectName1);
        await addJobsiteTrailerPage.FillNameAsync(Config.SetupJobsiteTrailerName1);
        await addJobsiteTrailerPage.FillAddressLine1Async(addressLine1);
        await addJobsiteTrailerPage.FillAddressLine2Async(addressLine2);
        await addJobsiteTrailerPage.FillCityAsync(city);
        await addJobsiteTrailerPage.FillStateAsync(state);
        await addJobsiteTrailerPage.FillPostalCodeAsync(postalCode);
        await addJobsiteTrailerPage.FillStartDateAsync(createdAt.Date);
        await addJobsiteTrailerPage.FillAreaAsync(Config.SetupJobsiteTrailerArea1);
        await addJobsiteTrailerPage.ClickCreateJobsiteTrailerBtnAsync();

        (await addJobsiteTrailerPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T15_AddJobsiteTrailer_EmissionTypeOptions()
    {
        var expected = await SqlHelper.GetSqftEmissionTypeDisplayNamesAsync();

        var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
        await addJobsiteTrailerPage.ClickEmissionsTabAsync();
        await addJobsiteTrailerPage.TurnCalculateByFootageOnAsync();

        var actual = await addJobsiteTrailerPage.GetEmissionTypeDropdownOptionsAsync();
        actual.Should().Equal(expected);
    }

    [Test]
    public async Task T16_AddJobsiteTrailer_ClickCancel()
    {
        const string expectedPageTitle = "Offices";
        var createdAt = DateTime.Now;
        var stamp = createdAt.ToString("yyyyMMddHHmmss");
        var name = $"Trailer{stamp}";
        var addressLine1 = $"Address1{stamp}";
        var addressLine2 = $"Address2{stamp}";
        var city = $"City{stamp}";
        var state = $"State{stamp}";
        var postalCode = $"Code{stamp}";

        try
        {
            var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
            await addJobsiteTrailerPage.SelectProjectAsync(Config.SetupProjectName1);
            await addJobsiteTrailerPage.FillNameAsync(name);
            await addJobsiteTrailerPage.FillAddressLine1Async(addressLine1);
            await addJobsiteTrailerPage.FillAddressLine2Async(addressLine2);
            await addJobsiteTrailerPage.FillCityAsync(city);
            await addJobsiteTrailerPage.FillStateAsync(state);
            await addJobsiteTrailerPage.FillPostalCodeAsync(postalCode);
            await addJobsiteTrailerPage.FillStartDateAsync(createdAt.Date);
            await addJobsiteTrailerPage.FillAreaAsync(Config.SetupJobsiteTrailerArea1);
            var officesPage = await addJobsiteTrailerPage.ClickCancelBtnAsync();

            (await officesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            (await officesPage.IsNameInGridAsync(name)).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.DeleteOfficeByNameAsync(name);
        }
    }

    [Test]
    public async Task T17_AddJobsiteTrailer_Bills_Success()
    {
        var createdAt = DateTime.Now;
        var stamp = createdAt.ToString("yyyyMMddHHmmss");
        var name = $"Trailer{stamp}";
        var addressLine1 = $"Address1{stamp}";
        var addressLine2 = $"Address2{stamp}";
        var city = $"City{stamp}";
        var state = $"State{stamp}";
        var postalCode = $"Code{stamp}";
        var startDate = createdAt.Date;

        try
        {
            var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
            var project = await addJobsiteTrailerPage.SelectProjectAsync(Config.SetupProjectName1);
            await addJobsiteTrailerPage.FillNameAsync(name);
            await addJobsiteTrailerPage.FillAddressLine1Async(addressLine1);
            await addJobsiteTrailerPage.FillAddressLine2Async(addressLine2);
            await addJobsiteTrailerPage.FillCityAsync(city);
            await addJobsiteTrailerPage.FillStateAsync(state);
            await addJobsiteTrailerPage.FillPostalCodeAsync(postalCode);
            await addJobsiteTrailerPage.FillStartDateAsync(startDate);
            await addJobsiteTrailerPage.FillAreaAsync(Config.SetupJobsiteTrailerArea1);
            var officesPage = await addJobsiteTrailerPage.CreateJobsiteTrailerAsync();

            await officesPage.SearchAsync(name);
            var actual = await officesPage.GetAllGridRecordsAsync();
            actual.Should().ContainSingle();
            actual[0].Should().Be(new OfficeGridRecord(
                project,
                name,
                "Jobsite trailer",
                FormatAddress($"{addressLine1}, {addressLine2}, {city}, {state} {postalCode}", "US"),
                FormatDate(startDate),
                "—",
                FormatArea(decimal.Parse(Config.SetupJobsiteTrailerArea1, CultureInfo.InvariantCulture))));
        }
        finally
        {
            await SqlHelper.DeleteOfficeByNameAsync(name);
        }
    }

    [Test]
    public async Task T18_AddJobsiteTrailer_SQFT_Success()
    {
        var createdAt = DateTime.Now;
        var stamp = createdAt.ToString("yyyyMMddHHmmss");
        var name = $"Trailer{stamp}";
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
            var addJobsiteTrailerPage = await OpenAddJobsiteTrailerPageAsync();
            var project = await addJobsiteTrailerPage.SelectProjectAsync(Config.SetupProjectName1);
            await addJobsiteTrailerPage.FillNameAsync(name);
            await addJobsiteTrailerPage.FillAddressLine1Async(addressLine1);
            await addJobsiteTrailerPage.FillAddressLine2Async(addressLine2);
            await addJobsiteTrailerPage.FillCityAsync(city);
            await addJobsiteTrailerPage.FillStateAsync(state);
            await addJobsiteTrailerPage.FillPostalCodeAsync(postalCode);
            await addJobsiteTrailerPage.FillStartDateAsync(startDate);
            await addJobsiteTrailerPage.FillEndDateAsync(endDate);
            await addJobsiteTrailerPage.FillAreaAsync(Config.SetupJobsiteTrailerArea1);
            await addJobsiteTrailerPage.ClickEmissionsTabAsync();
            await addJobsiteTrailerPage.TurnCalculateByFootageOnAsync();
            await addJobsiteTrailerPage.SelectEmissionTypeAsync(Config.SetupCode4, Config.SetupEmissionTypeName4);
            var officesPage = await addJobsiteTrailerPage.CreateJobsiteTrailerAsync();

            await officesPage.SearchAsync(name);
            var actual = await officesPage.GetAllGridRecordsAsync();
            actual.Should().ContainSingle();
            actual[0].Should().Be(new OfficeGridRecord(
                project,
                name,
                "Jobsite trailer",
                FormatAddress($"{addressLine1}, {addressLine2}, {city}, {state} {postalCode}", "US"),
                FormatDate(startDate),
                FormatDate(endDate),
                FormatArea(decimal.Parse(Config.SetupJobsiteTrailerArea1, CultureInfo.InvariantCulture))));
        }
        finally
        {
            await SqlHelper.DeleteOfficeByNameAsync(name);
        }
    }

    private async Task<AddJobsiteTrailerPage> OpenAddJobsiteTrailerPageAsync()
    {
        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        return await officesPage.ClickAddJobsiteTrailerBtnAsync();
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
