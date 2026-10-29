using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class OfficesTests : BaseTest
{
    [Test]
    public async Task T01_Offices_DefaultView()
    {
        const string expectedTitle = "Offices";
        const string expectedSubtitle = "Manage the offices recorded under projects.";

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();

        (await officesPage.GetPageTitleAsync()).Should().Be(expectedTitle);
        (await officesPage.GetMessageAsync()).Should().Be(expectedSubtitle);
        (await officesPage.IsAddOfficeBtnVisibleAsync()).Should().BeTrue();
        (await officesPage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Offices_ClickAddOfficeBtn()
    {
        const string expectedTitle = "Add Office";

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        var addOfficePage = await officesPage.ClickAddOfficeBtnAsync();

        (await addOfficePage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_Offices_Grid_Columns()
    {
        var expectedColumns = new[]
        {
            "PROJECT",
            "NAME",
            "ADDRESS",
            "START DATE",
            "END DATE",
            "AREA (SQ FT)",
            "ACTIONS",
        };

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();

        (await officesPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T04_Offices_Grid_Records()
    {
        var offices = await SqlHelper.GetOfficeGridRowsAsync();
        if (offices.Count == 0)
            Assert.Pass("No Offices");

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();

        var expected = offices.Select(ToGridRecord).ToList();
        var actual = await officesPage.GetAllGridRecordsAsync();
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public async Task T05_Offices_Grid_Sorting()
    {
        var columns = new[]
        {
            "PROJECT",
            "NAME",
            "ADDRESS",
            "START DATE",
            "END DATE",
            "AREA (SQ FT)",
        };

        var offices = await SqlHelper.GetOfficeGridRowsAsync();
        if (offices.Count == 0)
            Assert.Pass("There are no Offices.");

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        await officesPage.GetCurrentPageGridRecordsAsync();

        foreach (var column in columns)
        {
            await officesPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(officesPage, offices, column, ascending: true);

            await officesPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(officesPage, offices, column, ascending: false);

            await officesPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(officesPage, offices, "NAME", ascending: true);
        }
    }

    [Test]
    public async Task T06_Offices_Grid_Search()
    {
        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();

        (await officesPage.IsSearchInputVisibleAsync()).Should().BeTrue();
        (await officesPage.GetSearchPlaceholderAsync()).Should().Be("Search offices or data...");

        await AssertOfficeSearchAsync(officesPage, RandomOfficeSearch());
        await AssertOfficeSearchAsync(officesPage, "000");
        await AssertOfficeSearchAsync(officesPage, "Haskell");
        await AssertOfficeSearchAsync(officesPage, "delete");
        await AssertOfficeSearchAsync(officesPage, "office");
    }

    [Test]
    public async Task T07_Offices_Add_DefaultView()
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
    public async Task T08_Offices_Add_DetailsTab()
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
    public async Task T09_Offices_Add_EmissionsTab()
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
    public async Task T10_Offices_Add_ProjectDropdown_Options()
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
    public async Task T11_Offices_Add_EmptyFields()
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
    public async Task T12_Offices_Add_NameTooLong()
    {
        const string expectedNameError = "Name must be at most 256 characters.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillNameAsync(GenerateRandomAlphanumericString(257));
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetNameErrorAsync()).Should().Be(expectedNameError);
    }

    [Test]
    public async Task T13_Offices_Add_AddressLine1TooLong()
    {
        const string expectedAddressLine1Error = "Address line 1 must be at most 256 characters.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillAddressLine1Async(GenerateRandomAlphanumericString(257));
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetAddressLine1ErrorAsync()).Should().Be(expectedAddressLine1Error);
    }

    [Test]
    public async Task T14_Offices_Add_AddressLine2TooLong()
    {
        const string expectedAddressLine2Error = "Address line 2 must be at most 256 characters.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillAddressLine2Async(GenerateRandomAlphanumericString(257));
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetAddressLine2ErrorAsync()).Should().Be(expectedAddressLine2Error);
    }

    [Test]
    public async Task T15_Offices_Add_CityTooLong()
    {
        const string expectedCityError = "City must be at most 128 characters.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillCityAsync(GenerateRandomAlphanumericString(129));
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetCityErrorAsync()).Should().Be(expectedCityError);
    }

    [Test]
    public async Task T16_Offices_Add_StateTooLong()
    {
        const string expectedStateError = "State or province must be at most 64 characters.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillStateAsync(GenerateRandomAlphanumericString(65));
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetStateErrorAsync()).Should().Be(expectedStateError);
    }

    [Test]
    public async Task T17_Offices_Add_PostalCodeTooLong()
    {
        const string expectedPostalCodeError = "Postal code must be at most 32 characters.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillPostalCodeAsync(GenerateRandomAlphanumericString(33));
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetPostalCodeErrorAsync()).Should().Be(expectedPostalCodeError);
    }

    [Test]
    public async Task T18_Offices_Add_AreaIncorrect()
    {
        const string expectedAreaError = "Area must have no more than 2 decimal places.";

        var addOfficePage = await OpenAddOfficePageAsync();
        await addOfficePage.FillAreaAsync("123.123");
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetAreaErrorAsync()).Should().Be(expectedAreaError);
    }

    [Test]
    public async Task T19_Offices_Add_ExistingName()
    {
        const string expectedAlertMessage = "An office with this name already exists in the project.";
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
        await addOfficePage.FillAreaAsync("1000");
        await addOfficePage.ClickCreateOfficeBtnAsync();

        (await addOfficePage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T20_Offices_Add_ClickCancel()
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
            await addOfficePage.FillAreaAsync("1000");
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
    public async Task T21_Offices_Add_OfficeBills_Success()
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
            await addOfficePage.FillAreaAsync("1000");
            var officesPage = await addOfficePage.CreateOfficeAsync();

            await officesPage.SearchAsync(name);
            var actual = await officesPage.GetAllGridRecordsAsync();
            actual.Should().ContainSingle();
            actual[0].Should().Be(new OfficeGridRecord(
                project,
                name,
                FormatAddress($"{addressLine1}, {addressLine2}, {city}, {state} {postalCode}", "US"),
                FormatDate(startDate),
                "—",
                FormatArea(1000m)));
        }
        finally
        {
            await SqlHelper.DeleteOfficeByNameAsync(name);
        }
    }

    [Test]
    public async Task T22_Offices_Add_OfficeSQFT_Success()
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
            await addOfficePage.FillAreaAsync("1000");
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
                FormatAddress($"{addressLine1}, {addressLine2}, {city}, {state} {postalCode}", "US"),
                FormatDate(startDate),
                FormatDate(endDate),
                FormatArea(1000m)));
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

    private static async Task AssertCurrentPageSortedAsync(
        OfficesPage officesPage,
        IReadOnlyList<OfficeGridDbRow> offices,
        string column,
        bool ascending)
    {
        var actual = await officesPage.GetCurrentPageGridRecordsAsync();
        var expected = ExpectedSortedOffices(offices, column, ascending).Take(actual.Count).ToList();
        actual.Should().Equal(expected, $"{column} {(ascending ? "ASC" : "DESC")}");
    }

    private static List<OfficeGridRecord> ExpectedSortedOffices(
        IReadOnlyList<OfficeGridDbRow> offices,
        string column,
        bool ascending)
    {
        IOrderedEnumerable<OfficeGridDbRow> ordered = column switch
        {
            "PROJECT" => OrderByText(offices, row => row.Project, ascending),
            "NAME" => OrderByText(offices, row => row.Name, ascending),
            "ADDRESS" => OrderByText(offices, row => FormatAddress(row.Address, row.CountryCode), ascending),
            "START DATE" => OrderByDate(offices, row => row.StartDate, ascending),
            "END DATE" => OrderByDate(offices, row => row.EndDate, ascending),
            "AREA (SQ FT)" => OrderByArea(offices, ascending),
            _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Unknown offices grid column."),
        };

        return ordered.Select(ToGridRecord).ToList();
    }

    private static IOrderedEnumerable<OfficeGridDbRow> OrderByText(
        IReadOnlyList<OfficeGridDbRow> offices,
        Func<OfficeGridDbRow, string> key,
        bool ascending)
    {
        return ascending
            ? offices.OrderBy(key, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            : offices.OrderByDescending(key, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase);
    }

    private static IOrderedEnumerable<OfficeGridDbRow> OrderByDate(
        IReadOnlyList<OfficeGridDbRow> offices,
        Func<OfficeGridDbRow, DateTime?> key,
        bool ascending)
    {
        return ascending
            ? offices.OrderBy(row => key(row) ?? DateTime.MinValue).ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            : offices.OrderByDescending(row => key(row) ?? DateTime.MinValue).ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase);
    }

    private static IOrderedEnumerable<OfficeGridDbRow> OrderByArea(
        IReadOnlyList<OfficeGridDbRow> offices,
        bool ascending)
    {
        return ascending
            ? offices.OrderBy(row => row.AreaSquareFeet ?? decimal.MinValue).ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            : offices.OrderByDescending(row => row.AreaSquareFeet ?? decimal.MinValue).ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase);
    }

    private static OfficeGridRecord ToGridRecord(OfficeGridDbRow row) =>
        new(
            row.Project,
            row.Name,
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

    private static async Task AssertOfficeSearchAsync(OfficesPage officesPage, string term)
    {
        await officesPage.SearchAsync(term);

        var expected = (await SqlHelper.GetOfficeGridRowsAsync())
            .Select(ToGridRecord)
            .Where(row => OfficeRowContains(row, term))
            .ToList();

        if (expected.Count == 0)
        {
            (await officesPage.GetGridEmptyMessageAsync()).Should().Be("No offices match your search.");
            return;
        }

        var actual = await officesPage.GetAllGridRecordsAsync();
        actual.Should().BeEquivalentTo(expected);
        actual.Should().OnlyContain(row => OfficeRowContains(row, term));
    }

    private static bool OfficeRowContains(OfficeGridRecord row, string term) =>
        string.Join(' ', row.Project, row.Name, row.Address)
            .Contains(term, StringComparison.OrdinalIgnoreCase);

    private static string RandomOfficeSearch()
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Range(0, 10).Select(_ => letters[Random.Shared.Next(letters.Length)]).ToArray());
    }
}
