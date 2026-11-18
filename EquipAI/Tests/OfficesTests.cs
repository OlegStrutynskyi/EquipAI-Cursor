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
        const string expectedSubtitle = "Manage the offices and jobsite trailers recorded under projects.";

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();

        (await officesPage.GetPageTitleAsync()).Should().Be(expectedTitle);
        (await officesPage.GetMessageAsync()).Should().Be(expectedSubtitle);
        (await officesPage.IsAddJobsiteTrailerBtnVisibleAsync()).Should().BeTrue();
        (await officesPage.IsAddOfficeBtnVisibleAsync()).Should().BeTrue();
        (await officesPage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Offices_ClickAddJobsiteTrailerBtn()
    {
        const string expectedTitle = "Add Jobsite Trailer";

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        var addJobsiteTrailerPage = await officesPage.ClickAddJobsiteTrailerBtnAsync();

        (await addJobsiteTrailerPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_Offices_ClickAddOfficeBtn()
    {
        const string expectedTitle = "Add Office";

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();
        var addOfficePage = await officesPage.ClickAddOfficeBtnAsync();

        (await addOfficePage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T04_Offices_Grid_Tabs()
    {
        var expectedTabs = new[] { "All", "Offices", "Jobsite trailers" };
        const string expectedSelectedTab = "All";

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();

        (await officesPage.GetGridTabsAsync()).Should().Equal(expectedTabs);
        (await officesPage.GetSelectedGridTabAsync()).Should().Be(expectedSelectedTab);
    }

    [Test]
    public async Task T05_Offices_Grid_Columns()
    {
        var expectedColumns = new[]
        {
            "PROJECT",
            "NAME",
            "KIND",
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
    public async Task T06_Offices_Grid_Records()
    {
        var rows = await SqlHelper.GetOfficeGridRowsAsync();
        if (rows.Count == 0)
            Assert.Pass("No Offices");

        var officesPage = new OfficesPage(Fixture.Page);
        await officesPage.OpenAsync();

        await AssertGridTabAsync(officesPage, "All", rows);
        await AssertGridTabAsync(officesPage, "Offices", rows.Where(row => row.Kind == "Office"));
        await AssertGridTabAsync(officesPage, "Jobsite trailers", rows.Where(row => row.Kind == "Jobsite trailer"));
        await AssertGridTabAsync(officesPage, "All", rows);
    }

    [Test]
    public async Task T07_Offices_Grid_Sorting()
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
    public async Task T08_Offices_Grid_Search()
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
            "ADDRESS" => OrderByText(offices, row => FormatAddress(row.Address, row.CountryCode), ascending, SpaceInsensitiveComparer.Instance),
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
        bool ascending) =>
        OrderByText(offices, key, ascending, StringComparer.OrdinalIgnoreCase);

    private static IOrderedEnumerable<OfficeGridDbRow> OrderByText(
        IReadOnlyList<OfficeGridDbRow> offices,
        Func<OfficeGridDbRow, string> key,
        bool ascending,
        StringComparer comparer)
    {
        return ascending
            ? offices.OrderBy(key, comparer).ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            : offices.OrderByDescending(key, comparer).ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase);
    }

    private sealed class SpaceInsensitiveComparer : StringComparer
    {
        public static readonly SpaceInsensitiveComparer Instance = new();

        public override int Compare(string? x, string? y) =>
            string.Compare(StripSpaces(x), StripSpaces(y), StringComparison.OrdinalIgnoreCase);

        public override bool Equals(string? x, string? y) => Compare(x, y) == 0;

        public override int GetHashCode(string obj) =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(StripSpaces(obj));

        private static string StripSpaces(string? value) =>
            string.Concat((value ?? string.Empty).Where(character => !char.IsWhiteSpace(character)));
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

    private static async Task AssertGridTabAsync(
        OfficesPage officesPage,
        string tab,
        IEnumerable<OfficeGridDbRow> rows)
    {
        await officesPage.SelectGridTabAsync(tab);
        var actual = await officesPage.GetAllGridRecordsAsync();
        var expected = rows.Select(ToGridRecord).ToList();
        if (!await officesPage.HasKindColumnAsync())
            expected = expected.Select(row => row with { Kind = string.Empty }).ToList();
        actual.Should().BeEquivalentTo(expected);
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

    private static async Task AssertOfficeSearchAsync(OfficesPage officesPage, string term)
    {
        await officesPage.SearchAsync(term);

        var expected = (await SqlHelper.GetOfficeGridRowsAsync())
            .Select(ToGridRecord)
            .Where(row => OfficeRowContains(row, term))
            .ToList();

        if (expected.Count == 0)
        {
            (await officesPage.GetGridEmptyMessageAsync()).Should().Be("No offices or jobsite trailers match your search.");
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
