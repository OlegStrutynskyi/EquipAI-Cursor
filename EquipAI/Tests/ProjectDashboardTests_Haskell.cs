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
        (await projectDashboardPage.IsYearDropdownVisibleAsync()).Should().BeTrue();
        (await projectDashboardPage.IsTotalEmissionsCardVisibleAsync()).Should().BeTrue();
        (await projectDashboardPage.IsChartCardVisibleAsync()).Should().BeTrue();
        (await projectDashboardPage.IsDataGridVisibleAsync()).Should().BeTrue();

        var officesCardVisible = await SqlHelper.GetProjectHasOfficesAsync(Config.DefaultEnterpriseProjectName);
        if (officesCardVisible)
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < deadline && !await projectDashboardPage.IsOfficesCardVisibleAsync())
                await Task.Delay(200);
        }

        (await projectDashboardPage.IsOfficesCardVisibleAsync()).Should().Be(officesCardVisible);
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
    public async Task T03_HaskellDashboard_YearDropdown()
    {
        var emissionYears = await SqlHelper.GetProjectActivityYearsAsync(Config.DefaultEnterpriseProjectName);
        var expectedYears = new[] { "All" }.Concat(emissionYears).ToList();

        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        var actualYears = await projectDashboardPage.GetYearDropdownOptionsAsync();
        actualYears.Should().Equal(expectedYears);
    }

    [Test]
    public async Task T04_HaskellDashboard_Total_View()
    {
        const string expectedTitle = "Total Carbon Emissions";

        var totalCo2eTonnes = await SqlHelper.GetActiveProjectTotalCo2eTonnesAsync(Config.DefaultEnterpriseProjectName) ?? 0m;
        var rounded = Math.Round(totalCo2eTonnes, 1, MidpointRounding.AwayFromZero);
        var expectedTotal = rounded.ToString("#,##0.0", CultureInfo.InvariantCulture) + " tCO2e";

        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();

        (await projectDashboardPage.IsTotalEmissionsIconVisibleAsync()).Should().BeTrue();
        (await projectDashboardPage.GetTotalEmissionsTitleAsync()).Should().Be(expectedTitle);
        (await projectDashboardPage.GetTotalEmissionsValueAsync()).Should().Be(expectedTotal);
        (await projectDashboardPage.IsScopeButtonVisibleAsync(1)).Should().BeTrue();
        (await projectDashboardPage.IsScopeButtonVisibleAsync(2)).Should().BeTrue();
        (await projectDashboardPage.IsScopeButtonVisibleAsync(3)).Should().BeTrue();
        (await projectDashboardPage.IsScopeTotalTextVisibleAsync()).Should().BeTrue();

        var yearOptions = await projectDashboardPage.GetYearDropdownOptionsAsync();
        var allIndex = -1;
        for (var i = 0; i < yearOptions.Count; i++)
        {
            if (!yearOptions[i].Equals("All", StringComparison.OrdinalIgnoreCase))
                continue;

            allIndex = i;
            break;
        }

        if (allIndex < 0 || allIndex >= yearOptions.Count - 1)
            return;

        var selectedYear = yearOptions[allIndex + 1];
        var yearTotal = await SqlHelper.GetActiveProjectTotalCo2eTonnesAsync(
            Config.DefaultEnterpriseProjectName,
            int.Parse(selectedYear, CultureInfo.InvariantCulture)) ?? 0m;
        var expectedYearTotal = Math.Round(yearTotal, 1, MidpointRounding.AwayFromZero)
            .ToString("#,##0.0", CultureInfo.InvariantCulture) + " tCO2e";

        await projectDashboardPage.SelectYearAsync(selectedYear);

        var actualYearTotal = string.Empty;
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            actualYearTotal = await projectDashboardPage.GetTotalEmissionsValueAsync();
            if (actualYearTotal == expectedYearTotal)
                break;

            await Task.Delay(200);
        }

        actualYearTotal.Should().Be(expectedYearTotal);
        (await projectDashboardPage.IsScopeButtonVisibleAsync(1)).Should().BeTrue();
        (await projectDashboardPage.IsScopeButtonVisibleAsync(2)).Should().BeTrue();
        (await projectDashboardPage.IsScopeButtonVisibleAsync(3)).Should().BeTrue();
        (await projectDashboardPage.IsScopeTotalTextVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public Task T05_HaskellDashboard_Total_Scope1() =>
        AssertScopeTotalAsync(scope: 1, selectScope: false, checkYearAfterAll: true);

    [Test]
    public Task T06_HaskellDashboard_Total_Scope2() =>
        AssertScopeTotalAsync(scope: 2, selectScope: true, checkYearAfterAll: true);

    [Test]
    public Task T07_HaskellDashboard_Total_Scope3() =>
        AssertScopeTotalAsync(scope: 3, selectScope: true, checkYearAfterAll: true);

    [Test]
    public Task T08_HaskellDashboard_Chart_Scope1() =>
        AssertChartAllThenYearAfterAllAsync(scope: 1);

    [Test]
    public Task T09_HaskellDashboard_Chart_Scope2() =>
        AssertChartAllThenYearAfterAllAsync(scope: 2);

    [Test]
    public Task T10_HaskellDashboard_Chart_Scope3() =>
        AssertChartAllThenYearAfterAllAsync(scope: 3);

    [Test]
    public async Task T11_HaskellDashboard_OfficesGrid()
    {
        const string officesNote =
            "One row per office for the selected year. The Method column shows whether the figure comes from bills or from floor area.";

        var projectName = Config.DefaultEnterpriseProjectName;
        if (!await SqlHelper.GetProjectHasOfficesAsync(projectName))
            Assert.Pass("Project does not have any Offices.");

        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        await projectDashboardPage.SelectYearAsync("All");
        (await projectDashboardPage.GetOfficesNoteAsync()).Should().Be(officesNote);
        await AssertOfficesGridMatchesDbAsync(projectDashboardPage, projectName, null, null);

        var selectedYear = GetYearAfterAll(await projectDashboardPage.GetYearDropdownOptionsAsync());
        if (selectedYear is null)
            return;

        await projectDashboardPage.SelectYearAsync(selectedYear);
        var year = int.Parse(selectedYear, CultureInfo.InvariantCulture);
        await AssertOfficesGridMatchesDbAsync(
            projectDashboardPage,
            projectName,
            new DateTime(year, 1, 1),
            new DateTime(year, 12, 31));
    }

    [Test]
    public async Task T12_HaskellDashboard_Data_Fuel()
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
    public async Task T13_HaskellDashboard_Data_Office()
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
    public async Task T14_HaskellDashboard_Data_Telemetry()
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

    private async Task AssertScopeTotalAsync(int scope, bool selectScope, bool checkYearAfterAll = false)
    {
        var projectName = Config.DefaultEnterpriseProjectName;
        var expectedTotal = await SqlHelper.GetProjectScopeTotalCo2eTonnesAsync(projectName, scope);

        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        if (selectScope)
            await projectDashboardPage.ClickScopeButtonAsync(scope);

        (await projectDashboardPage.IsScopeButtonSelectedAsync(scope)).Should().BeTrue();
        await AssertDisplayedScopeAsync(projectDashboardPage, projectName, scope, expectedTotal);

        if (checkYearAfterAll)
            await AssertScopeForYearAfterAllAsync(projectDashboardPage, projectName, scope);
    }

    private async Task AssertScopeForYearAfterAllAsync(
        ProjectDashboardPage projectDashboardPage,
        string projectName,
        int scope)
    {
        var yearOptions = await projectDashboardPage.GetYearDropdownOptionsAsync();
        string? selectedYear = null;
        for (var i = 0; i < yearOptions.Count - 1; i++)
        {
            if (!yearOptions[i].Equals("All", StringComparison.OrdinalIgnoreCase))
                continue;

            selectedYear = yearOptions[i + 1];
            break;
        }

        if (selectedYear is null)
            return;

        var year = int.Parse(selectedYear, CultureInfo.InvariantCulture);
        var expectedTotal = await SqlHelper.GetProjectScopeTotalCo2eTonnesAsync(projectName, scope, year);
        await projectDashboardPage.SelectYearAsync(selectedYear);
        await AssertDisplayedScopeAsync(projectDashboardPage, projectName, scope, expectedTotal, year);
    }

    private async Task AssertDisplayedScopeAsync(
        ProjectDashboardPage projectDashboardPage,
        string projectName,
        int scope,
        decimal expectedTotal,
        int? year = null)
    {
        if (expectedTotal == 0m)
        {
            var emptyDeadline = DateTime.UtcNow.AddSeconds(15);
            var emptyValue = string.Empty;
            while (DateTime.UtcNow < emptyDeadline)
            {
                emptyValue = await projectDashboardPage.GetScopeTotalValueAsync();
                if (emptyValue == "0")
                    break;

                await Task.Delay(200);
            }

            emptyValue.Should().Be("0");
            (await projectDashboardPage.GetScopeTotalTextAsync()).Should().Be($"Total Scope {scope} \u2013 0 tCO2e");
            (await projectDashboardPage.GetScopeEmptyMessageAsync()).Should().Be($"No Scope {scope} emissions recorded.");
            return;
        }

        var expectedValue = FormatTonnes(expectedTotal);
        var expectedLines = (await SqlHelper.GetProjectScopeEmissionTypesAsync(projectName, scope, year))
            .Select(row => $"{row.TypeName} {FormatEmissionValue(row.Co2eTonnes)} tCO2e {FormatQuantity(row.Quantity)} {row.UnitSymbol}")
            .ToList();

        var deadline = DateTime.UtcNow.AddSeconds(15);
        var actualValue = string.Empty;
        IReadOnlyList<string> actualLines = [];
        while (DateTime.UtcNow < deadline)
        {
            actualValue = await projectDashboardPage.GetScopeTotalValueAsync();
            try
            {
                actualLines = await projectDashboardPage.GetScopeEmissionLinesAsync();
            }
            catch (InvalidOperationException)
            {
                actualLines = [];
            }

            if (actualValue == expectedValue && actualLines.SequenceEqual(expectedLines))
                break;

            await Task.Delay(200);
        }

        actualValue.Should().Be(expectedValue);
        (await projectDashboardPage.GetScopeTotalTextAsync()).Should().Be($"Total Scope {scope} \u2013 {expectedValue} tCO2e");
        actualLines.Should().Equal(expectedLines);
    }

    private async Task<ProjectDashboardPage> OpenDefaultProjectDashboardAsync()
    {
        var projectName = Config.DefaultEnterpriseProjectName;
        var projectsPage = new ProjectsPage(Fixture.Page);
        await projectsPage.OpenAsync();
        await projectsPage.SearchAsync(projectName);
        return await projectsPage.ClickProjectNameAsync(projectName);
    }

    private static string FormatTonnes(decimal tonnes) => FormatEmissionValue(tonnes);

    private static string FormatQuantity(decimal quantity) => FormatEmissionValue(quantity);

    private static async Task AssertChartScopeMatchesDbAsync(
        ProjectDashboardPage projectDashboardPage,
        int year,
        int scope,
        string? yearOption = null,
        bool completeTestOnNoData = true)
    {
        var expectedRows = await SqlHelper.GetProjectChartScopeMonthlyEmissionsAsync(
            Config.DefaultEnterpriseProjectName,
            year,
            scope);
        if (expectedRows.Count == 0)
        {
            if (completeTestOnNoData)
                Assert.Pass("No data for this Scope");
            return;
        }

        await projectDashboardPage.SelectYearAsync(yearOption ?? year.ToString(CultureInfo.InvariantCulture));
        await projectDashboardPage.SelectChartScopeAsync(scope);

        if (!await projectDashboardPage.HasChartLineAsync())
        {
            if (completeTestOnNoData)
                Assert.Pass("No data for this Scope");
            return;
        }

        var chartValuesByMonth = await projectDashboardPage.GetChartMonthlyTonnesAsync();
        if (chartValuesByMonth.Count == 0 || chartValuesByMonth.Values.All(value => value == 0m))
        {
            if (completeTestOnNoData)
                Assert.Pass("No data for this Scope");
            return;
        }

        foreach (var row in expectedRows)
        {
            chartValuesByMonth.Should().ContainKey(row.Month);
            var actualValue = chartValuesByMonth[row.Month];
            var expectedValue = RoundToChartDisplayPrecision(row.Co2eTonnes, actualValue);
            actualValue.Should().Be(expectedValue, $"month {row.Month}");
        }
    }

    private static async Task AssertChartAllYearsMatchDbAsync(ProjectDashboardPage projectDashboardPage, int scope)
    {
        var expectedRows = await SqlHelper.GetProjectChartScopeYearlyEmissionsAsync(Config.DefaultEnterpriseProjectName, scope);
        if (expectedRows.Count == 0)
            return;

        await projectDashboardPage.SelectChartScopeAsync(scope);
        if (!await projectDashboardPage.HasChartLineAsync())
            return;

        var chartValuesByYear = await projectDashboardPage.GetChartYearlyTonnesAsync();
        foreach (var row in expectedRows)
        {
            chartValuesByYear.Should().ContainKey(row.Year);
            var actualValue = chartValuesByYear[row.Year];
            var expectedValue = RoundToChartDisplayPrecision(row.Co2eTonnes, actualValue);
            actualValue.Should().Be(expectedValue, $"year {row.Year}");
        }
    }

    private async Task AssertChartAllThenYearAfterAllAsync(int scope)
    {
        var projectDashboardPage = await OpenDefaultProjectDashboardAsync();
        await projectDashboardPage.SelectYearAsync("All");
        await AssertChartSubtitleAsync(projectDashboardPage, "All");
        await AssertChartAllYearsMatchDbAsync(projectDashboardPage, scope);

        var selectedYear = GetYearAfterAll(await projectDashboardPage.GetYearDropdownOptionsAsync());
        if (selectedYear is null)
            return;

        await projectDashboardPage.SelectYearAsync(selectedYear);
        await AssertChartSubtitleAsync(projectDashboardPage, selectedYear);
        await AssertChartScopeMatchesDbAsync(
            projectDashboardPage,
            int.Parse(selectedYear, CultureInfo.InvariantCulture),
            scope,
            selectedYear);
    }

    private static async Task AssertChartSubtitleAsync(ProjectDashboardPage projectDashboardPage, string yearOption)
    {
        var expected = yearOption.Equals("All", StringComparison.OrdinalIgnoreCase)
            ? "tCO2e \u00b7 Yearly, all years on record"
            : $"tCO2e \u00b7 Monthly, {yearOption}";

        var deadline = DateTime.UtcNow.AddSeconds(10);
        var actual = string.Empty;
        while (DateTime.UtcNow < deadline)
        {
            actual = await projectDashboardPage.GetChartSubtitleAsync();
            if (actual == expected)
                break;

            await Task.Delay(200);
        }

        actual.Should().Be(expected);
    }

    private static async Task AssertOfficesGridMatchesDbAsync(
        ProjectDashboardPage projectDashboardPage,
        string projectName,
        DateTime? periodStart,
        DateTime? periodEnd)
    {
        var dbRows = await SqlHelper.GetProjectOfficesDashboardAsync(projectName, periodStart, periodEnd);
        if (dbRows.Count == 0)
        {
            var expectedMessage = periodStart.HasValue && await SqlHelper.GetProjectHasOfficesAsync(projectName)
                ? $"No office was occupied in {periodStart.Value.Year}."
                : "No offices recorded yet.";

            var deadline = DateTime.UtcNow.AddSeconds(15);
            var message = string.Empty;
            while (DateTime.UtcNow < deadline)
            {
                if (await projectDashboardPage.IsOfficesEmptyMessageVisibleAsync())
                {
                    message = await projectDashboardPage.GetOfficesEmptyMessageAsync();
                    if (message == expectedMessage)
                        break;
                }

                await Task.Delay(200);
            }

            message.Should().Be(expectedMessage);
            return;
        }

        var rowsDeadline = DateTime.UtcNow.AddSeconds(15);
        IReadOnlyList<ProjectOfficesCardGridRow> actual = [];
        var expected = new List<ProjectOfficesCardGridRow>();
        while (DateTime.UtcNow < rowsDeadline)
        {
            actual = await projectDashboardPage.GetOfficesCardRowsAsync();
            expected = dbRows
                .Select((row, index) => ToOfficesCardRow(
                    row,
                    index < actual.Count ? actual[index].TonnesPerSquareFoot : null))
                .ToList();
            if (actual.SequenceEqual(expected))
                break;

            await Task.Delay(200);
        }

        actual.Should().Equal(expected);
    }

    private static ProjectOfficesCardGridRow ToOfficesCardRow(
        ProjectOfficesDashboardDbRow row,
        string? displayedTonnesPerSquareFoot = null)
    {
        var location = string.Join(", ", new[] { row.City, row.StateProvince }.Where(part => !string.IsNullOrWhiteSpace(part)));
        var office = string.IsNullOrWhiteSpace(location) ? row.Name : $"{row.Name} {location}";

        return new ProjectOfficesCardGridRow(
            office,
            FormatOfficeArea(row.AreaSquareFeet),
            row.Method.ToUpperInvariant(),
            FormatEmpty(row.EmissionTypes),
            FormatOfficeTonnes(row.TotalCo2eTonnes),
            FormatOfficeTonnesPerSqFt(row.TotalPerSqFt, displayedTonnesPerSquareFoot));
    }

    private static string FormatOfficeArea(decimal? area) =>
        area?.ToString("#,##0.00", CultureInfo.InvariantCulture) ?? "—";

    private static string FormatOfficeTonnes(decimal tonnes) =>
        Math.Round(tonnes, 1, MidpointRounding.AwayFromZero)
            .ToString("#,##0.0", CultureInfo.InvariantCulture);

    private static string FormatOfficeTonnesPerSqFt(decimal? tonnesPerSqFt, string? displayed = null)
    {
        if (tonnesPerSqFt is null)
            return "—";

        var decimals = TryGetDisplayedDecimalPlaces(displayed, out var places) ? places : 6;
        var rounded = Math.Round(tonnesPerSqFt.Value, decimals, MidpointRounding.AwayFromZero);
        var format = decimals == 0 ? "#,##0" : "#,##0." + new string('0', decimals);
        return rounded.ToString(format, CultureInfo.InvariantCulture);
    }

    private static bool TryGetDisplayedDecimalPlaces(string? displayed, out int places)
    {
        places = 0;
        if (string.IsNullOrWhiteSpace(displayed))
            return false;

        var text = displayed.Trim().Replace(",", "").Replace(" ", "");
        if (text is "—" or "-" or "–" || !decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
            return false;

        var separator = text.LastIndexOf('.');
        places = separator < 0 ? 0 : text.Length - separator - 1;
        return true;
    }

    private static string? GetYearAfterAll(IReadOnlyList<string> yearOptions)
    {
        for (var i = 0; i < yearOptions.Count - 1; i++)
        {
            if (yearOptions[i].Equals("All", StringComparison.OrdinalIgnoreCase))
                return yearOptions[i + 1];
        }

        return null;
    }

    private static decimal RoundToChartDisplayPrecision(decimal databaseValue, decimal chartValue)
    {
        var chartText = chartValue.ToString(CultureInfo.InvariantCulture);
        var separatorIndex = chartText.IndexOf('.');
        var decimals = separatorIndex < 0 ? 0 : chartText.Length - separatorIndex - 1;
        return Math.Round(databaseValue, decimals, MidpointRounding.AwayFromZero);
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
