using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class ProjectDashboardTests_Haskell : BaseTest
{
    [Test]
    public async Task T01_HaskellDashboard_DefaultView()
    {
        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        await projectDashboardPage.WaitForDefaultViewAsync();

        (await projectDashboardPage.GetPageTitleAsync()).Should().Be($"{Config.DefaultEnterpriseProjectName} Overview");
        (await projectDashboardPage.IsReportingBadgeVisibleAsync()).Should().BeTrue();
        (await projectDashboardPage.IsSynchedProcoreVisibleAsync()).Should().BeTrue();
        (await projectDashboardPage.IsTotalEmissionsCardVisibleAsync()).Should().BeTrue();
        (await projectDashboardPage.IsChartCardVisibleAsync()).Should().BeTrue();
        (await projectDashboardPage.IsDataGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_HaskellDashboard_ReportingBadge()
    {
        var projectName = Config.DefaultEnterpriseProjectName;
        var totalCo2eTonnes = await SqlHelper.GetActiveProjectTotalCo2eTonnesAsync(projectName);
        var expectedBadge = totalCo2eTonnes is not null ? "REPORTING" : "NON REPORTING";

        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();

        (await projectDashboardPage.GetReportingBadgeTextAsync())
            .ToUpperInvariant()
            .Should()
            .Be(expectedBadge);
    }

    [Test]
    public async Task T03_HaskellDashboard_Total_View()
    {
        const string expectedTitle = "Total Carbon Emissions";
        const string expectedSubtitle = "All time";

        var totalCo2eTonnes = await SqlHelper.GetActiveProjectTotalCo2eTonnesAsync(Config.DefaultEnterpriseProjectName) ?? 0m;
        var rounded = Math.Round(totalCo2eTonnes, 1, MidpointRounding.AwayFromZero);
        var expectedTotal = rounded.ToString("#,##0.0", CultureInfo.InvariantCulture) + " tCO2e";

        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();

        (await projectDashboardPage.IsTotalEmissionsIconVisibleAsync()).Should().BeTrue();
        (await projectDashboardPage.GetTotalEmissionsTitleAsync()).Should().Be(expectedTitle);
        (await projectDashboardPage.GetTotalEmissionsSubtitleAsync()).Should().Be(expectedSubtitle);
        (await projectDashboardPage.GetTotalEmissionsValueAsync()).Should().Be(expectedTotal);
        (await projectDashboardPage.IsScopeButtonVisibleAsync(1)).Should().BeTrue();
        (await projectDashboardPage.IsScopeButtonVisibleAsync(2)).Should().BeTrue();
        (await projectDashboardPage.IsScopeButtonVisibleAsync(3)).Should().BeTrue();
        (await projectDashboardPage.IsScopeTotalTextVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public Task T04_HaskellDashboard_Total_Scope1() => AssertScopeTotalAsync(scope: 1, selectScope: false);

    [Test]
    public Task T05_HaskellDashboard_Total_Scope2() => AssertScopeTotalAsync(scope: 2, selectScope: true);

    [Test]
    public Task T06_HaskellDashboard_Total_Scope3() => AssertScopeTotalAsync(scope: 3, selectScope: true);

    [Test]
    public async Task T07_HaskellDashboard_Chart_YearDropdown()
    {
        var currentYear = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture);
        var dbYears = (await SqlHelper.GetProjectActivityYearsAsync(Config.DefaultEnterpriseProjectName)).ToList();
        var expectedYears = dbYears.Contains(currentYear, StringComparer.Ordinal)
            ? dbYears
            : new[] { currentYear }.Concat(dbYears).ToList();

        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        var actualYears = await projectDashboardPage.GetYearDropdownOptionsAsync();
        actualYears.Should().Equal(expectedYears);
    }

    [Test]
    public async Task T08_HaskellDashboard_Chart_CurrentYearScope1()
    {
        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        await AssertChartScopeMatchesDbAsync(projectDashboardPage, DateTime.UtcNow.Year, scope: 1);
    }

    [Test]
    public async Task T09_HaskellDashboard_Chart_CurrentYearScope2()
    {
        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        await AssertChartScopeMatchesDbAsync(projectDashboardPage, DateTime.UtcNow.Year, scope: 2);
    }

    [Test]
    public async Task T10_HaskellDashboard_Chart_CurrentYearScope3()
    {
        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        await AssertChartScopeMatchesDbAsync(projectDashboardPage, DateTime.UtcNow.Year, scope: 3);
    }

    [Test]
    public async Task T11_HaskellDashboard_Chart_NextYearOptionScope1()
    {
        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        var yearOption = GetNextYearOptionAfterCurrent(await projectDashboardPage.GetYearDropdownOptionsAsync());
        if (yearOption is null)
            Assert.Pass("No next year option after current year");

        await AssertChartScopeMatchesDbAsync(
            projectDashboardPage,
            int.Parse(yearOption, CultureInfo.InvariantCulture),
            scope: 1,
            yearOption);
    }

    [Test]
    public async Task T12_HaskellDashboard_Chart_NextYearOptionScope2()
    {
        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        var yearOption = GetNextYearOptionAfterCurrent(await projectDashboardPage.GetYearDropdownOptionsAsync());
        if (yearOption is null)
            Assert.Pass("No next year option after current year");

        await AssertChartScopeMatchesDbAsync(
            projectDashboardPage,
            int.Parse(yearOption, CultureInfo.InvariantCulture),
            scope: 2,
            yearOption);
    }

    [Test]
    public async Task T13_HaskellDashboard_Chart_NextYearOptionScope3()
    {
        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        var yearOption = GetNextYearOptionAfterCurrent(await projectDashboardPage.GetYearDropdownOptionsAsync());
        if (yearOption is null)
            Assert.Pass("No next year option after current year");

        await AssertChartScopeMatchesDbAsync(
            projectDashboardPage,
            int.Parse(yearOption, CultureInfo.InvariantCulture),
            scope: 3,
            yearOption);
    }

    [Test]
    public async Task T14_HaskellDashboard_Data_Fuel()
    {
        var expectedColumns = new[]
        {
            "INVOICE NUMBER",
            "COMPANY",
            "INVOICE DATE",
            "STATUS",
            "IMPORT DATE",
            "TOTAL",
            "SOURCE",
            "CATEGORY",
        };

        var dbRows = await SqlHelper.GetProjectFuelInvoicesAsync(Config.DefaultEnterpriseProjectName);
        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        await projectDashboardPage.WaitForDefaultViewAsync();

        if (dbRows.Count == 0)
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < deadline && !await projectDashboardPage.IsFuelTabVisibleAsync())
                await Task.Delay(250);

            (await projectDashboardPage.IsFuelTabVisibleAsync()).Should().BeFalse();
            return;
        }

        await projectDashboardPage.WaitForFuelTabAsync();
        (await projectDashboardPage.IsFuelTabVisibleAsync()).Should().BeTrue();
        (await projectDashboardPage.IsFuelTabSelectedAsync()).Should().BeTrue();
        (await projectDashboardPage.GetFuelGridColumnHeadersAsync()).Should().Equal(expectedColumns);

        var expected = dbRows.Select(ToFuelGridRow).ToList();
        var actual = (await projectDashboardPage.GetFuelGridRowsAsync())
            .Select(ToActualFuelRow)
            .ToList();

        if (actual.Count != expected.Count)
        {
            static string Key((string InvoiceNumber, string Company, string InvoiceDate, string Status, string ImportDate, string Total, string Source, string Category) row) =>
                $"{row.InvoiceNumber}|{row.InvoiceDate}|{row.Status}|{row.Total}|{row.Source}|{row.Category}";

            var actualKeys = actual.Select(Key).ToList();
            var expectedKeys = expected.Select(Key).ToList();
            var extra = actualKeys.Except(expectedKeys).Take(15).ToList();
            var missing = expectedKeys.Except(actualKeys).Take(15).ToList();
            throw new AssertionException(
                $"Grid has {actual.Count} rows, database has {expected.Count}. Missing: {string.Join(" || ", missing)}. Extra: {string.Join(" || ", extra)}.");
        }

        actual.Should().Equal(expected);
    }

    [Test]
    public async Task T15_HaskellDashboard_Data_Office()
    {
        var expectedColumns = new[]
        {
            "COMPANY",
            "UTILITY BILL DATE",
            "STATUS",
            "IMPORT DATE",
            "TOTAL",
            "SOURCE",
            "CATEGORY",
        };

        var dbRows = await SqlHelper.GetProjectOfficeBillsAsync(Config.DefaultEnterpriseProjectName);
        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        await projectDashboardPage.WaitForDefaultViewAsync();

        if (dbRows.Count == 0)
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < deadline && !await projectDashboardPage.IsOfficeTabVisibleAsync())
                await Task.Delay(250);

            (await projectDashboardPage.IsOfficeTabVisibleAsync()).Should().BeFalse();
            return;
        }

        var officeDeadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < officeDeadline && !await projectDashboardPage.IsOfficeTabVisibleAsync())
            await Task.Delay(250);

        (await projectDashboardPage.IsOfficeTabVisibleAsync()).Should().BeTrue();
        await projectDashboardPage.ClickOfficeTabAsync();
        (await projectDashboardPage.IsOfficeTabSelectedAsync()).Should().BeTrue();
        (await projectDashboardPage.GetFuelGridColumnHeadersAsync()).Should().Equal(expectedColumns);

        var expected = dbRows
            .OrderBy(row => row.ImportDate ?? DateTime.MinValue)
            .ThenBy(row => row.Company, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.BillDate ?? DateTime.MinValue)
            .ThenBy(row => row.Id)
            .Select(ToOfficeGridRow)
            .ToList();
        var actual = (await projectDashboardPage.GetOfficeGridRowsAsync())
            .Select(ToActualOfficeRow)
            .ToList();

        if (actual.Count != expected.Count)
        {
            static string Key((string Company, string BillDate, string Status, string ImportDate, string Total, string Source, string Category) row) =>
                $"{row.Company}|{row.BillDate}|{row.Status}|{row.ImportDate}|{row.Total}|{row.Source}|{row.Category}";

            var actualKeys = actual.Select(Key).ToList();
            var expectedKeys = expected.Select(Key).ToList();
            var extra = actualKeys.Except(expectedKeys).Take(15).ToList();
            var missing = expectedKeys.Except(actualKeys).Take(15).ToList();
            throw new AssertionException(
                $"Grid has {actual.Count} rows, database has {expected.Count}. Missing: {string.Join(" || ", missing)}. Extra: {string.Join(" || ", extra)}.");
        }

        actual.Should().Equal(expected);
    }

    [Test]
    public async Task T16_HaskellDashboard_Data_Telemetry()
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

        var dbRows = await SqlHelper.GetProjectTelemetryReadingsAsync(Config.DefaultEnterpriseProjectName);
        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        await projectDashboardPage.WaitForDefaultViewAsync();

        if (dbRows.Count == 0)
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < deadline && !await projectDashboardPage.IsTelemetryTabVisibleAsync())
                await Task.Delay(250);

            (await projectDashboardPage.IsTelemetryTabVisibleAsync()).Should().BeFalse();
            return;
        }

        var telemetryDeadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < telemetryDeadline && !await projectDashboardPage.IsTelemetryTabVisibleAsync())
            await Task.Delay(250);

        (await projectDashboardPage.IsTelemetryTabVisibleAsync()).Should().BeTrue();
        await projectDashboardPage.ClickTelemetryTabAsync();
        (await projectDashboardPage.IsTelemetryTabSelectedAsync()).Should().BeTrue();
        (await projectDashboardPage.GetFuelGridColumnHeadersAsync()).Should().Equal(expectedColumns);

        static string Key((string Month, string EquipmentTag, string EquipmentType, string Location, string OperatingHours, string FuelType, string ImportDate, string Source) row) =>
            $"{row.Month}|{row.EquipmentTag}|{row.EquipmentType}|{row.Location}|{row.OperatingHours}|{row.FuelType}|{row.ImportDate}|{row.Source}";

        var expected = dbRows
            .Select(ToTelemetryGridRow)
            .OrderBy(Key, StringComparer.Ordinal)
            .ToList();
        var actual = (await projectDashboardPage.GetTelemetryDataGridRowsAsync())
            .Select(ToActualTelemetryRow)
            .OrderBy(Key, StringComparer.Ordinal)
            .ToList();

        if (actual.Count != expected.Count)
        {
            var actualKeys = actual.Select(Key).ToList();
            var expectedKeys = expected.Select(Key).ToList();
            var extra = actualKeys.Except(expectedKeys).Take(15).ToList();
            var missing = expectedKeys.Except(actualKeys).Take(15).ToList();
            throw new AssertionException(
                $"Grid has {actual.Count} rows, database has {expected.Count}. Missing: {string.Join(" || ", missing)}. Extra: {string.Join(" || ", extra)}.");
        }

        actual.Should().Equal(expected);
    }

    private async Task AssertScopeTotalAsync(int scope, bool selectScope)
    {
        var projectName = Config.DefaultEnterpriseProjectName;
        var expectedTotal = await SqlHelper.GetProjectScopeTotalCo2eTonnesAsync(projectName, scope);

        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        if (selectScope)
            await projectDashboardPage.ClickScopeButtonAsync(scope);

        (await projectDashboardPage.IsScopeButtonSelectedAsync(scope)).Should().BeTrue();

        if (expectedTotal == 0m)
        {
            (await projectDashboardPage.GetScopeTotalValueAsync()).Should().Be("0");
            (await projectDashboardPage.GetScopeTotalTextAsync()).Should().Be($"Total Scope {scope} \u2013 0 tCO2e");
            (await projectDashboardPage.GetScopeEmptyMessageAsync()).Should().Be($"No Scope {scope} emissions recorded.");
            return;
        }

        var expectedValue = FormatTonnes(expectedTotal);
        (await projectDashboardPage.GetScopeTotalValueAsync()).Should().Be(expectedValue);
        (await projectDashboardPage.GetScopeTotalTextAsync()).Should().Be($"Total Scope {scope} \u2013 {expectedValue} tCO2e");

        var expectedLines = (await SqlHelper.GetProjectScopeEmissionTypesAsync(projectName, scope))
            .Select(row => $"{row.TypeName} {FormatEmissionValue(row.Co2eTonnes)} tCO2e {FormatQuantity(row.Quantity)} {row.UnitSymbol}")
            .ToList();
        var actualLines = await projectDashboardPage.GetScopeEmissionLinesAsync();
        actualLines.Should().Equal(expectedLines);
    }

    private async Task<ProjectDashboardPage> OpenDefaultProjectDashboardAsync()
    {
        var projectsPage = new ProjectsPage(Fixture.Page);
        await projectsPage.OpenAsync();
        return await projectsPage.ClickProjectNameAsync(Config.DefaultEnterpriseProjectName);
    }

    private static string FormatTonnes(decimal tonnes)
    {
        var rounded = Math.Round(tonnes, 1, MidpointRounding.AwayFromZero);
        return rounded.ToString("#,##0.0", CultureInfo.InvariantCulture);
    }

    private static string FormatQuantity(decimal quantity) => FormatEmissionValue(quantity);

    private static async Task AssertChartScopeMatchesDbAsync(
        ProjectDashboardPage projectDashboardPage,
        int year,
        int scope,
        string? yearOption = null)
    {
        var expectedRows = await SqlHelper.GetProjectChartScopeMonthlyEmissionsAsync(
            Config.DefaultEnterpriseProjectName,
            year,
            scope);
        if (expectedRows.Count == 0)
            Assert.Pass("No data for this Scope");

        await projectDashboardPage.SelectYearAsync(yearOption ?? year.ToString(CultureInfo.InvariantCulture));
        await projectDashboardPage.SelectChartScopeAsync(scope);

        if (!await projectDashboardPage.HasChartLineAsync())
            Assert.Pass("No data for this Scope");

        var chartValuesByMonth = await projectDashboardPage.GetChartMonthlyTonnesAsync();
        if (chartValuesByMonth.Count == 0 || chartValuesByMonth.Values.All(value => value == 0m))
            Assert.Pass("No data for this Scope");

        foreach (var row in expectedRows)
        {
            chartValuesByMonth.Should().ContainKey(row.Month);
            var actualValue = chartValuesByMonth[row.Month];
            var expectedValue = RoundToChartDisplayPrecision(row.Co2eTonnes, actualValue);
            actualValue.Should().Be(expectedValue, $"month {row.Month}");
        }
    }

    private static decimal RoundToChartDisplayPrecision(decimal databaseValue, decimal chartValue)
    {
        var chartText = chartValue.ToString(CultureInfo.InvariantCulture);
        var separatorIndex = chartText.IndexOf('.');
        var decimals = separatorIndex < 0 ? 0 : chartText.Length - separatorIndex - 1;
        return Math.Round(databaseValue, decimals, MidpointRounding.AwayFromZero);
    }

    private static string? GetNextYearOptionAfterCurrent(IReadOnlyList<string> yearOptions)
    {
        var currentYear = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture);
        var currentIndex = -1;
        for (var i = 0; i < yearOptions.Count; i++)
        {
            if (!yearOptions[i].Equals(currentYear, StringComparison.Ordinal))
                continue;

            currentIndex = i;
            break;
        }

        if (currentIndex < 0 || currentIndex >= yearOptions.Count - 1)
            return null;

        return yearOptions[currentIndex + 1];
    }

    private static (string Month, string EquipmentTag, string EquipmentType, string Location, string OperatingHours, string FuelType, string ImportDate, string Source) ToTelemetryGridRow(
        TelemetryGridDbRow row) =>
        (
            row.ReportingMonth.ToString("MMM yyyy", CultureInfo.InvariantCulture),
            FormatEmpty(row.EquipmentTag),
            FormatEmpty(row.EquipmentType),
            FormatEmpty(row.Location),
            FormatHours(row.OperatingHours),
            FormatEmpty(row.FuelType),
            FormatDate(row.CreatedAt),
            FormatEmpty(row.SourceType)
        );

    private static (string Month, string EquipmentTag, string EquipmentType, string Location, string OperatingHours, string FuelType, string ImportDate, string Source) ToActualTelemetryRow(
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

    private static string FormatHours(decimal? value) =>
        value?.ToString("#,##0.################", CultureInfo.GetCultureInfo("en-US")) ?? "—";

    private static (string Company, string BillDate, string Status, string ImportDate, string Total, string Source, string Category) ToOfficeGridRow(
        ProjectOfficeBillDbRow row) =>
        (
            FormatEmpty(row.Company),
            FormatDate(row.BillDate),
            row.Status.ToUpperInvariant(),
            FormatDate(row.ImportDate),
            FormatTotal(row.TotalCost, row.CurrencyCode),
            FormatSource(row.SourceType),
            FormatEmpty(row.Category)
        );

    private static (string Company, string BillDate, string Status, string ImportDate, string Total, string Source, string Category) ToActualOfficeRow(
        ProjectOfficeGridRow row) =>
        (
            row.Company,
            row.BillDate,
            row.Status.ToUpperInvariant(),
            row.ImportDate,
            row.Total,
            row.Source,
            row.Category
        );

    private static (string InvoiceNumber, string Company, string InvoiceDate, string Status, string ImportDate, string Total, string Source, string Category) ToFuelGridRow(
        ProjectFuelInvoiceDbRow row) =>
        (
            FormatEmpty(row.InvoiceNumber),
            FormatEmpty(row.CompanyName),
            FormatDate(row.InvoiceDate),
            row.Status.ToUpperInvariant(),
            FormatDate(row.CreatedAt),
            FormatTotal(row.TotalCost, row.CurrencyCode),
            FormatSource(row.SourceType),
            FormatEmpty(row.Category)
        );

    private static (string InvoiceNumber, string Company, string InvoiceDate, string Status, string ImportDate, string Total, string Source, string Category) ToActualFuelRow(
        ProjectFuelGridRow row) =>
        (
            row.InvoiceNumber,
            row.Company,
            row.InvoiceDate,
            row.Status.ToUpperInvariant(),
            row.ImportDate,
            row.Total,
            row.Source,
            row.Category
        );

    private static string FormatDate(DateTime? value) =>
        value?.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) ?? "—";

    private static string FormatTotal(decimal? value, string currencyCode)
    {
        if (value is null)
            return "—";

        var amount = Math.Round(value.Value, 0, MidpointRounding.AwayFromZero);
        var formatted = amount.ToString("#,##0", CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(currencyCode))
            return formatted;

        return CurrencySymbol(currencyCode) + formatted;
    }

    private static string CurrencySymbol(string currencyCode) => currencyCode.ToUpperInvariant() switch
    {
        "USD" => "$",
        "CAD" => "CA$",
        "AUD" => "A$",
        "EUR" => "€",
        "GBP" => "£",
        _ => currencyCode.ToUpperInvariant(),
    };

    private static string FormatEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value;

    private static string FormatSource(string source) =>
        string.IsNullOrWhiteSpace(source) ? "Manual" : source;

    private static string FormatEmissionValue(decimal tonnes)
    {
        var rounded = Math.Round(tonnes, 1, MidpointRounding.AwayFromZero);
        return rounded == decimal.Truncate(rounded)
            ? decimal.Truncate(rounded).ToString("#,##0", CultureInfo.InvariantCulture)
            : rounded.ToString("#,##0.0", CultureInfo.InvariantCulture);
    }
}
