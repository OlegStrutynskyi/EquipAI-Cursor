using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class WasteTests : BaseTest
{
    [Test]
    public async Task T01_Waste_DefaultView()
    {
        const string expectedTitle = "Waste Report Upload";
        const string expectedMessage = "Browse imported waste tonnage-report readings.";
        var expectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
            .AddMonths(-1)
            .ToString("yyyy-MM", CultureInfo.InvariantCulture);

        var wastePage = new WasteReportPage(Fixture.Page);
        await wastePage.OpenAsync();

        (await wastePage.GetTitleAsync()).Should().Be(expectedTitle);
        (await wastePage.GetMessageAsync()).Should().Be(expectedMessage);
        (await wastePage.IsImportBtnVisibleAsync()).Should().BeTrue();
        (await wastePage.IsMonthLabelVisibleAsync()).Should().BeTrue();
        (await wastePage.IsMonthFieldVisibleAsync()).Should().BeTrue();
        (await wastePage.GetMonthFieldValueAsync()).Should().Be(expectedMonth);
        (await wastePage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Waste_ClickImportBtn()
    {
        const string expectedTitle = "Import Waste Report";

        var wastePage = new WasteReportPage(Fixture.Page);
        await wastePage.OpenAsync();
        var importPage = await wastePage.ClickImportBtnAsync();

        (await importPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_Waste_Grid_Columns()
    {
        var expectedColumns = new[]
        {
            "TICKET NUMBER",
            "CONTAINER ID",
            "SITE NAME",
            "LOCATION",
            "PROJECT",
            "HAUL DATE",
            "TONNAGE (SHORT TON)",
            "EMISSION TYPE",
            "COMPANY",
        };

        var wastePage = new WasteReportPage(Fixture.Page);
        await wastePage.OpenAsync();

        (await wastePage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T04_Waste_Grid_Records()
    {
        var month = await ReportingMonthAsync();
        var wastePage = new WasteReportPage(Fixture.Page);
        await wastePage.OpenAsync();
        await wastePage.SetMonthAsync(month.ToString("yyyy-MM", CultureInfo.InvariantCulture));

        var expected = (await SqlHelper.GetWasteReportGridRowsAsync(month))
            .Select(ToGridRow)
            .OrderBy(GridKey, StringComparer.Ordinal)
            .ToList();
        var actual = (await wastePage.GetGridRowsAsync())
            .Select(ToActualRow)
            .OrderBy(GridKey, StringComparer.Ordinal)
            .ToList();

        if (actual.Count != expected.Count)
        {
            var extra = actual.Select(GridKey).Except(expected.Select(GridKey)).Take(15).ToList();
            var missing = expected.Select(GridKey).Except(actual.Select(GridKey)).Take(15).ToList();
            throw new AssertionException(
                $"Grid has {actual.Count} rows, database has {expected.Count}. Missing: {string.Join(" || ", missing)}. Extra: {string.Join(" || ", extra)}.");
        }

        actual.Should().Equal(expected);
    }

    [Test]
    public async Task T05_Waste_Grid_Sorting()
    {
        var columns = new[]
        {
            "TICKET NUMBER",
            "CONTAINER ID",
            "SITE NAME",
            "LOCATION",
            "PROJECT",
            "HAUL DATE",
            "TONNAGE",
            "EMISSION TYPE",
            "COMPANY",
        };

        var month = await ReportingMonthAsync();
        var wastePage = new WasteReportPage(Fixture.Page);
        await wastePage.OpenAsync();
        await wastePage.SetMonthAsync(month.ToString("yyyy-MM", CultureInfo.InvariantCulture));

        var readings = await SqlHelper.GetWasteReportGridRowsAsync(month);
        var defaultPage = (await wastePage.GetCurrentPageGridRowsAsync())
            .Select(ToActualRow)
            .ToList();

        foreach (var column in columns)
        {
            await wastePage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(
                wastePage,
                ExpectedSortedReadings(readings, column, ascending: true),
                $"{column} ASC");

            await wastePage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(
                wastePage,
                ExpectedSortedReadings(readings, column, ascending: false),
                $"{column} DESC");

            await wastePage.ClickGridColumnAsync(column);
            var reset = (await wastePage.GetCurrentPageGridRowsAsync())
                .Select(ToActualRow)
                .ToList();
            reset.Should().Equal(defaultPage, $"{column} reset to the default order");
        }
    }

    [Test]
    public async Task T06_Waste_Grid_Search()
    {
        var month = await ReportingMonthAsync();
        var wastePage = new WasteReportPage(Fixture.Page);
        await wastePage.OpenAsync();

        (await wastePage.IsSearchInputVisibleAsync()).Should().BeTrue();
        (await wastePage.GetSearchPlaceholderAsync()).Should().Be("Search site, container, or ticket...");

        await wastePage.SetMonthAsync(month.ToString("yyyy-MM", CultureInfo.InvariantCulture));

        await AssertWasteSearchAsync(wastePage, month, RandomWasteSearch());
        await AssertWasteSearchAsync(wastePage, month, "haskell");
        await AssertWasteSearchAsync(wastePage, month, "\"site\"");
    }

    private static async Task AssertWasteSearchAsync(WasteReportPage wastePage, DateTime month, string term)
    {
        await wastePage.SearchAsync(term);

        var phrase = SearchPhrase(term);
        var expected = (await SqlHelper.GetWasteReportGridRowsAsync(month))
            .Select(ToGridRow)
            .Where(row => WasteRowContains(row, phrase))
            .ToList();

        if (expected.Count == 0)
        {
            (await wastePage.GetGridEmptyMessageAsync()).Should().Be("No waste-report readings match your search.");
            return;
        }

        var actual = (await wastePage.GetGridRowsAsync())
            .Select(ToActualRow)
            .ToList();
        actual.Should().BeEquivalentTo(expected);
        actual.Should().OnlyContain(row => WasteRowContains(row, phrase));
    }

    private static async Task<DateTime> ReportingMonthAsync() =>
        await SqlHelper.GetLatestWasteReportMonthAsync()
        ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);

    private static string SearchPhrase(string term)
    {
        var trimmed = term.Trim();
        return trimmed.Length >= 2 && trimmed[0] == '"' && trimmed[^1] == '"'
            ? trimmed[1..^1]
            : trimmed;
    }

    private static bool WasteRowContains(WasteGridValues row, string term) =>
        string.Join(' ', row.TicketNumber, row.ContainerId, row.SiteName, row.Location, row.Project, row.HaulDate, row.Tonnage, row.EmissionType, row.Company)
            .Contains(term, StringComparison.OrdinalIgnoreCase);

    private static string RandomWasteSearch()
    {
        var chars = GenerateRandomAlphanumericString(10).ToCharArray();
        chars[0] = (char)('a' + Random.Shared.Next(26));
        chars[1] = (char)('0' + Random.Shared.Next(10));
        return new string(chars);
    }

    private static async Task AssertCurrentPageSortedAsync(
        WasteReportPage wastePage,
        IReadOnlyList<WasteGridValues> expected,
        string because)
    {
        var actual = (await wastePage.GetCurrentPageGridRowsAsync())
            .Select(ToActualRow)
            .ToList();
        actual.Should().Equal(expected.Take(actual.Count), because);
    }

    private static List<WasteGridValues> ExpectedSortedReadings(
        IReadOnlyList<WasteReportGridDbRow> readings,
        string column,
        bool ascending)
    {
        IOrderedEnumerable<WasteReportGridDbRow> ordered = column switch
        {
            "TICKET NUMBER" => OrderByText(readings, row => row.TicketNumber, ascending),
            "CONTAINER ID" => OrderByText(readings, row => row.ContainerId, ascending),
            "SITE NAME" => OrderByText(readings, row => row.SiteName, ascending),
            "LOCATION" => OrderByText(readings, row => row.SiteAddress, ascending),
            "PROJECT" => OrderByText(readings, row => row.ProjectCode, ascending),
            "HAUL DATE" => OrderByDate(readings, row => row.HaulDate, ascending),
            "TONNAGE" => OrderByTonnage(readings, ascending),
            "EMISSION TYPE" => OrderByText(readings, row => row.EmissionType, ascending),
            "COMPANY" => OrderByText(readings, row => row.Company, ascending),
            _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Unknown waste report grid column."),
        };

        return ordered.Select(ToGridRow).ToList();
    }

    private static IOrderedEnumerable<WasteReportGridDbRow> OrderByText(
        IReadOnlyList<WasteReportGridDbRow> readings,
        Func<WasteReportGridDbRow, string> key,
        bool ascending) =>
        ascending
            ? readings.OrderBy(key, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.TicketNumber, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Id)
            : readings.OrderByDescending(key, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.TicketNumber, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Id);

    private static IOrderedEnumerable<WasteReportGridDbRow> OrderByDate(
        IReadOnlyList<WasteReportGridDbRow> readings,
        Func<WasteReportGridDbRow, DateTime?> key,
        bool ascending) =>
        ascending
            ? readings.OrderBy(row => key(row) ?? DateTime.MinValue).ThenBy(row => row.TicketNumber, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Id)
            : readings.OrderByDescending(row => key(row) ?? DateTime.MinValue).ThenBy(row => row.TicketNumber, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Id);

    private static IOrderedEnumerable<WasteReportGridDbRow> OrderByTonnage(
        IReadOnlyList<WasteReportGridDbRow> readings,
        bool ascending) =>
        ascending
            ? readings.OrderBy(row => row.Tonnage ?? decimal.MinValue).ThenBy(row => row.TicketNumber, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Id)
            : readings.OrderByDescending(row => row.Tonnage ?? decimal.MinValue).ThenBy(row => row.TicketNumber, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Id);

    private static string GridKey(WasteGridValues row) =>
        $"{row.TicketNumber}|{row.ContainerId}|{row.SiteName}|{row.Location}|{row.Project}|{row.HaulDate}|{row.Tonnage}|{row.EmissionType}|{row.Company}";

    private static WasteGridValues ToGridRow(WasteReportGridDbRow row) =>
        new(
            row.TicketNumber,
            row.ContainerId,
            row.SiteName,
            FormatEmpty(row.SiteAddress),
            FormatProject(row.ProjectCode, row.ProjectName),
            FormatDate(row.HaulDate),
            FormatTonnage(row.Tonnage),
            FormatEmpty(row.EmissionType),
            row.Company);

    private static WasteGridValues ToActualRow(WasteReportGridRow row) =>
        new(
            row.TicketNumber,
            row.ContainerId,
            row.SiteName,
            row.Location,
            row.Project,
            row.HaulDate,
            row.Tonnage,
            row.EmissionType,
            row.Company);

    private static string FormatProject(string code, string name)
    {
        code = code.Trim();
        name = name.Trim();
        if (code.Length > 0)
            return name.Length > 0 ? $"{code} \u2014 {name}" : code;

        return name.Length > 0 ? name : "\u2014";
    }

    private static string FormatDate(DateTime? value) =>
        value?.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string FormatTonnage(decimal? value)
    {
        if (value is null)
            return string.Empty;

        var rounded = decimal.Round(value.Value, 4, MidpointRounding.AwayFromZero);
        return rounded.ToString("#,##0.####", CultureInfo.GetCultureInfo("en-US"));
    }

    private static string FormatEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? "\u2014" : value;

    private readonly record struct WasteGridValues(
        string TicketNumber,
        string ContainerId,
        string SiteName,
        string Location,
        string Project,
        string HaulDate,
        string Tonnage,
        string EmissionType,
        string Company);
}
