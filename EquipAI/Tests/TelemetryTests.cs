using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class TelemetryTests : BaseTest
{
    [Test]
    public async Task T01_Telemetry_DefaultView()
    {
        const string expectedTitle = "Telemetry Upload";
        const string expectedMessage = "Browse imported equipment telemetry readings.";
        var expectedMonth = DateTime.Today.AddMonths(-1).ToString("yyyy-MM", CultureInfo.InvariantCulture);

        var telemetryPage = new TelemetryPage(Fixture.Page);
        await telemetryPage.OpenAsync();

        (await telemetryPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await telemetryPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await telemetryPage.IsImportBtnVisibleAsync()).Should().BeTrue();
        (await telemetryPage.IsMonthLabelVisibleAsync()).Should().BeTrue();
        (await telemetryPage.IsMonthFieldVisibleAsync()).Should().BeTrue();
        (await telemetryPage.GetMonthFieldValueAsync()).Should().Be(expectedMonth);
        (await telemetryPage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Telemetry_ClickImportBtn()
    {
        const string expectedTitle = "Import Telemetry";

        var telemetryPage = new TelemetryPage(Fixture.Page);
        await telemetryPage.OpenAsync();
        var ImportPage = await telemetryPage.ClickImportBtnAsync();

        (await ImportPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_Telemetry_GridColumns()
    {
        var expectedColumns = new[]
        {
            "MONTH",
            "EQUIPMENT TAG",
            "EQUIPMENT TYPE",
            "LOCATION",
            "OPERATING HOURS",
            "FUEL TYPE",
            "IMPORT DATE",
            "SOURCE",
        };

        var telemetryPage = new TelemetryPage(Fixture.Page);
        await telemetryPage.OpenAsync();

        (await telemetryPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T04_Telemetry_GridRecords()
    {
        const string monthValue = "2022-06";
        const string monthDisplay = "June 2022";

        var telemetryPage = new TelemetryPage(Fixture.Page);
        await telemetryPage.OpenAsync();
        await telemetryPage.SetMonthAsync(monthValue);
        (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(monthDisplay);

        var expected = (await SqlHelper.GetTelemetryGridRowsAsync(new DateTime(2022, 6, 1)))
            .Select(ToGridRow)
            .OrderBy(GridKey, StringComparer.Ordinal)
            .ToList();
        var actual = (await telemetryPage.GetGridRowsAsync())
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
    public async Task T05_Telemetry_GridSorting()
    {
        const string monthValue = "2022-06";
        var columns = new[]
        {
            "MONTH",
            "EQUIPMENT TAG",
            "EQUIPMENT TYPE",
            "LOCATION",
            "OPERATING HOURS",
            "FUEL TYPE",
            "IMPORT DATE",
            "SOURCE",
        };

        var telemetryPage = new TelemetryPage(Fixture.Page);
        await telemetryPage.OpenAsync();
        await telemetryPage.SetMonthAsync(monthValue);

        var readings = await SqlHelper.GetTelemetryGridRowsAsync(new DateTime(2022, 6, 1));

        foreach (var column in columns)
        {
            await telemetryPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(
                telemetryPage,
                ExpectedSortedReadings(readings, column, ascending: true),
                $"{column} ASC");

            await telemetryPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(
                telemetryPage,
                ExpectedSortedReadings(readings, column, ascending: false),
                $"{column} DESC");

            await telemetryPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(
                telemetryPage,
                ExpectedSortedReadings(readings, "EQUIPMENT TAG", ascending: true),
                $"{column} reset to EQUIPMENT TAG ASC");
        }
    }

    private static async Task AssertCurrentPageSortedAsync(
        TelemetryPage telemetryPage,
        IReadOnlyList<(string Month, string EquipmentTag, string EquipmentType, string Location, string OperatingHours, string FuelType, string ImportDate, string Source)> expected,
        string because)
    {
        var actual = (await telemetryPage.GetCurrentPageGridRowsAsync())
            .Select(ToActualRow)
            .ToList();
        actual.Should().Equal(expected.Take(actual.Count), because);
    }

    private static List<(string Month, string EquipmentTag, string EquipmentType, string Location, string OperatingHours, string FuelType, string ImportDate, string Source)> ExpectedSortedReadings(
        IReadOnlyList<TelemetryGridDbRow> readings,
        string column,
        bool ascending)
    {
        IOrderedEnumerable<TelemetryGridDbRow> ordered = column switch
        {
            "MONTH" => OrderByDate(readings, row => row.ReportingMonth, ascending),
            "EQUIPMENT TAG" => OrderByText(readings, row => row.EquipmentTag, ascending),
            "EQUIPMENT TYPE" => OrderByText(readings, row => row.EquipmentType, ascending),
            "LOCATION" => OrderByText(readings, row => row.Location, ascending),
            "OPERATING HOURS" => OrderByHours(readings, ascending),
            "FUEL TYPE" => OrderByText(readings, row => row.FuelType, ascending),
            "IMPORT DATE" => OrderByDate(readings, row => row.CreatedAt, ascending),
            "SOURCE" => OrderByText(readings, row => row.SourceType, ascending),
            _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Unknown telemetry grid column."),
        };

        return ordered.Select(ToGridRow).ToList();
    }

    private static IOrderedEnumerable<TelemetryGridDbRow> OrderByText(
        IReadOnlyList<TelemetryGridDbRow> readings,
        Func<TelemetryGridDbRow, string> key,
        bool ascending) =>
        ascending
            ? readings.OrderBy(key, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.EquipmentTag, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Id)
            : readings.OrderByDescending(key, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.EquipmentTag, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Id);

    private static IOrderedEnumerable<TelemetryGridDbRow> OrderByDate(
        IReadOnlyList<TelemetryGridDbRow> readings,
        Func<TelemetryGridDbRow, DateTime?> key,
        bool ascending) =>
        ascending
            ? readings.OrderBy(row => key(row) ?? DateTime.MinValue).ThenBy(row => row.EquipmentTag, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Id)
            : readings.OrderByDescending(row => key(row) ?? DateTime.MinValue).ThenBy(row => row.EquipmentTag, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Id);

    private static IOrderedEnumerable<TelemetryGridDbRow> OrderByHours(
        IReadOnlyList<TelemetryGridDbRow> readings,
        bool ascending) =>
        ascending
            ? readings.OrderBy(row => row.OperatingHours).ThenBy(row => row.EquipmentTag, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Id)
            : readings.OrderByDescending(row => row.OperatingHours).ThenBy(row => row.EquipmentTag, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Id);

    private static string GridKey(
        (string Month, string EquipmentTag, string EquipmentType, string Location, string OperatingHours, string FuelType, string ImportDate, string Source) row) =>
        $"{row.EquipmentTag}|{row.EquipmentType}|{row.Location}|{row.OperatingHours}|{row.FuelType}|{row.ImportDate}|{row.Source}|{row.Month}";

    private static (string Month, string EquipmentTag, string EquipmentType, string Location, string OperatingHours, string FuelType, string ImportDate, string Source) ToGridRow(
        TelemetryGridDbRow row) =>
        (
            row.ReportingMonth.ToString("MMM yyyy", CultureInfo.InvariantCulture),
            row.EquipmentTag,
            FormatEmpty(row.EquipmentType),
            FormatEmpty(row.Location),
            FormatHours(row.OperatingHours),
            FormatEmpty(row.FuelType),
            FormatDate(row.CreatedAt),
            FormatEmpty(row.SourceType)
        );

    private static (string Month, string EquipmentTag, string EquipmentType, string Location, string OperatingHours, string FuelType, string ImportDate, string Source) ToActualRow(
        TelemetryGridRow row) =>
        (
            row.Month,
            row.EquipmentTag,
            row.EquipmentType,
            row.Location,
            row.OperatingHours,
            row.FuelType,
            row.ImportDate,
            row.Source
        );

    private static string FormatDate(DateTime? value) =>
        value?.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) ?? "—";

    private static string FormatHours(decimal? value) =>
        value?.ToString("#,##0.################", CultureInfo.GetCultureInfo("en-US")) ?? "—";

    private static string FormatEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value;
}
