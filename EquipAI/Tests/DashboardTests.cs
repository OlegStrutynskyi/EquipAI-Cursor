using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class DashboardTests : BaseTest
{
    [Test]
    public async Task T01_Dashboard_DefaultView()
    {
        const string expectedTitle = "Company Dashboard";
        const string expectedSubtitle = "Global sustainability tracking across every project site.";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        (await dashboardPage.GetPageTitleAsync()).Should().Be(expectedTitle);
        (await dashboardPage.GetSubtitleAsync()).Should().Be(expectedSubtitle);
        (await dashboardPage.IsTotalCarbonEmissionCardVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalActiveProjectsCardVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalCarbonEmissionsByScopeCardVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsActiveProjectsCardVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsCarbonEmissionsByCategoryCardVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsSupportCardVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Dashboard_TotalEmissions_View()
    {
        const string expectedLabel = "Total Carbon Emissions";
        const string expectedUnit = "tCO2e";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        (await dashboardPage.GetTotalEmissionsLabelAsync()).Should().Be(expectedLabel);
        (await dashboardPage.IsTotalEmissionsValueVisibleAsync()).Should().BeTrue();
        (await dashboardPage.GetTotalEmissionsUnitAsync()).Should().Be(expectedUnit);
        (await dashboardPage.IsTotalEmissionsScope1LabelVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalEmissionsScope1ValueVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalEmissionsScope2LabelVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalEmissionsScope2ValueVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalEmissionsScope3LabelVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalEmissionsScope3ValueVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T03_Dashboard_TotalEmissions_TotalValue()
    {
        var year = DateTime.UtcNow.Year;

        var expectedValue = Math.Round(
            await SqlHelper.GetTotalCarbonEmissionsTonnesAsync(year),
            1,
            MidpointRounding.AwayFromZero);

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var uiValueText = await dashboardPage.GetTotalEmissionsValueAsync();
        var actualValue = ParseMetricValueToOneDecimal(uiValueText);

        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task T04_Dashboard_TotalEmissions_Scope1Value()
    {
        var year = DateTime.UtcNow.Year;
        var dbValue = await SqlHelper.GetCarbonEmissionsTonnesByScopeAsync(year, ghgScope: 1);
        var expectedValue = FormatScopeEmissionsValue(dbValue);

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualValue = await dashboardPage.GetTotalEmissionsScope1ValueAsync();
        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task T05_Dashboard_TotalEmissions_Scope2Value()
    {
        var year = DateTime.UtcNow.Year;
        var dbValue = await SqlHelper.GetCarbonEmissionsTonnesByScopeAsync(year, ghgScope: 2);
        var expectedValue = FormatScopeEmissionsValue(dbValue);

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualValue = await dashboardPage.GetTotalEmissionsScope2ValueAsync();
        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task T06_Dashboard_TotalEmissions_Scope3Value()
    {
        var year = DateTime.UtcNow.Year;
        var dbValue = await SqlHelper.GetCarbonEmissionsTonnesByScopeAsync(year, ghgScope: 3);
        var expectedValue = FormatScopeEmissionsValue(dbValue);

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualValue = await dashboardPage.GetTotalEmissionsScope3ValueAsync();
        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task T07_Dashboard_TotalProjects_View()
    {
        const string expectedTitle = "Total Active Projects";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        (await dashboardPage.GetTotalProjectsTitleAsync()).Should().Be(expectedTitle);
        (await dashboardPage.IsTotalProjectsValueVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalProjectsReportingLabelVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalProjectsReportingValueVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalProjectsNonReportingLabelVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalProjectsNonReportingValueVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T08_Dashboard_TotalProjects_TotalValue()
    {
        var expectedValue = (await SqlHelper.GetTotalActiveProjectsCountAsync())
            .ToString(CultureInfo.InvariantCulture);

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualValue = await dashboardPage.GetTotalProjectsValueAsync();
        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task T09_Dashboard_TotalProjects_ReportingValue()
    {
        var expectedValue = (await SqlHelper.GetReportingActiveProjectsCountAsync())
            .ToString(CultureInfo.InvariantCulture);

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualValue = await dashboardPage.GetTotalProjectsReportingValueAsync();
        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task T10_Dashboard_TotalProjects_NonReportingValue()
    {
        var totalProjects = await SqlHelper.GetTotalActiveProjectsCountAsync();
        var reportingProjects = await SqlHelper.GetReportingActiveProjectsCountAsync();
        var expectedValue = (totalProjects - reportingProjects).ToString(CultureInfo.InvariantCulture);

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualValue = await dashboardPage.GetTotalProjectsNonReportingValueAsync();
        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task T11_Dashboard_ActiveProjects_View()
    {
        const string expectedLabel = "Active Projects";
        const string expectedMessage = "Highest all-time carbon impact by tCO2e";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        (await dashboardPage.GetActiveProjectsLabelAsync()).Should().Be(expectedLabel);
        (await dashboardPage.GetActiveProjectsMessageAsync()).Should().Be(expectedMessage);
        (await dashboardPage.IsProjectsListVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsViewAllProjectsLinkVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T12_Dashboard_ActiveProjects_NoActiveProjects()
    {
        var inactiveProjectIds = await SqlHelper.GetInactiveProjectIdsAsync();

        try
        {
            await SqlHelper.DeactivateAllProjectsAsync();

            var dashboardPage = new DashboardPage(Fixture.Page);
            await dashboardPage.OpenAsync();
            await Fixture.Page.ReloadAsync();
            await dashboardPage.GetActiveProjectsLabelAsync();

            (await dashboardPage.IsActiveProjectsListEmptyAsync()).Should().BeTrue();
        }
        finally
        {
            await SqlHelper.RestoreActiveProjectsExceptAsync(inactiveProjectIds);
        }
    }

    [Test]
    public async Task T13_Dashboard_ActiveProjects_List()
    {
        var totalActiveProjects = await SqlHelper.GetTotalActiveProjectsCountAsync();
        if (totalActiveProjects == 0)
            Assert.Pass("No active projects");

        var expectedProjects = await SqlHelper.GetTopActiveProjectsByEmissionsAsync();
        var expectedNames = expectedProjects.Select(project => project.Name).ToList();
        var expectedValues = expectedProjects
            .Select(project => FormatProjectTonnesValue(project.TotalCo2eTonnes))
            .ToList();

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await Fixture.Page.ReloadAsync();
        await dashboardPage.GetActiveProjectsLabelAsync();

        var actualNames = await dashboardPage.GetActiveProjectNamesAsync();
        var actualValues = await dashboardPage.GetActiveProjectTonnesValuesAsync();

        actualNames.Should().Equal(expectedNames);
        actualValues.Should().Equal(expectedValues);
    }

    [Test]
    public async Task T14_Dashboard_ActiveProjects_ClickProjectName()
    {
        var totalActiveProjects = await SqlHelper.GetTotalActiveProjectsCountAsync();
        if (totalActiveProjects == 0)
            Assert.Pass("No active projects");

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await Fixture.Page.ReloadAsync();
        await dashboardPage.GetActiveProjectsLabelAsync();

        var projectName = await dashboardPage.GetFirstActiveProjectNameAsync();
        var projectDashboardPage = await dashboardPage.ClickFirstActiveProjectAsync();

        (await projectDashboardPage.GetPageTitleAsync()).Should().Be(projectName);
    }

    [Test]
    public async Task T15_Dashboard_ActiveProjects_ClickViewAllProjectsLink()
    {
        const string expectedTitle = "Projects";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var projectsPage = await dashboardPage.ClickViewAllProjectsLinkAsync();
        (await projectsPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T16_Dashboard_TotalEmissionsByScope_View()
    {
        const string expectedLabel = "Total Carbon Emissions by Scope";
        const string expectedMessage = "Yearly tCO2e vs. Baseline";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        (await dashboardPage.GetTotalEmissionsByScopeLabelAsync()).Should().Be(expectedLabel);
        (await dashboardPage.GetTotalEmissionsByScopeMessageAsync()).Should().Be(expectedMessage);
        (await dashboardPage.IsYearLabelVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsYearDropdownVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsScope1BtnVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsScope2BtnVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsScope3BtnVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsChartVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T17_Dashboard_TotalEmissionsByScope_YearDropdown()
    {
        var currentYear = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture);
        var dbYears = (await SqlHelper.GetActivityYearsAsync()).ToList();
        var expectedYears = dbYears.Contains(currentYear, StringComparer.Ordinal)
            ? dbYears
            : new[] { currentYear }.Concat(dbYears).ToList();

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualYears = await dashboardPage.GetYearDropdownOptionsAsync();
        actualYears.Should().Equal(expectedYears);
    }

    [Test]
    public async Task T18_Dashboard_TotalEmissionsByScope_ChartCurrentYearScope1()
    {
        var year = DateTime.UtcNow.Year;
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await AssertChartScopeMatchesDbAsync(
            dashboardPage,
            year,
            scope: 1,
            dashboardPage.ClickScope1BtnAsync);
    }

    [Test]
    public async Task T19_Dashboard_TotalEmissionsByScope_ChartCurrentYearScope2()
    {
        var year = DateTime.UtcNow.Year;
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await AssertChartScopeMatchesDbAsync(
            dashboardPage,
            year,
            scope: 2,
            dashboardPage.ClickScope2BtnAsync);
    }

    [Test]
    public async Task T20_Dashboard_TotalEmissionsByScope_ChartCurrentYearScope3()
    {
        var year = DateTime.UtcNow.Year;
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await AssertChartScopeMatchesDbAsync(
            dashboardPage,
            year,
            scope: 3,
            dashboardPage.ClickScope3BtnAsync);
    }

    [Test]
    public async Task T21_Dashboard_TotalEmissionsByScope_ChartNextYearOptionScope1()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
        var yearOption = GetNextYearOptionAfterCurrent(yearOptions);
        if (yearOption is null)
            Assert.Pass("No next year option after current year");

        var year = int.Parse(yearOption, CultureInfo.InvariantCulture);
        await AssertChartScopeMatchesDbAsync(
            dashboardPage,
            year,
            scope: 1,
            dashboardPage.ClickScope1BtnAsync,
            yearOption);
    }

    [Test]
    public async Task T22_Dashboard_TotalEmissionsByScope_ChartNextYearOptionScope2()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
        var yearOption = GetNextYearOptionAfterCurrent(yearOptions);
        if (yearOption is null)
            Assert.Pass("No next year option after current year");

        var year = int.Parse(yearOption, CultureInfo.InvariantCulture);
        await AssertChartScopeMatchesDbAsync(
            dashboardPage,
            year,
            scope: 2,
            dashboardPage.ClickScope2BtnAsync,
            yearOption);
    }

    [Test]
    public async Task T23_Dashboard_TotalEmissionsByScope_ChartNextYearOptionScope3()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
        var yearOption = GetNextYearOptionAfterCurrent(yearOptions);
        if (yearOption is null)
            Assert.Pass("No next year option after current year");

        var year = int.Parse(yearOption, CultureInfo.InvariantCulture);
        await AssertChartScopeMatchesDbAsync(
            dashboardPage,
            year,
            scope: 3,
            dashboardPage.ClickScope3BtnAsync,
            yearOption);
    }

    [Test]
    public async Task T24_Dashboard_EmissionsByCategory_View()
    {
        const string expectedLabel = "Carbon Emissions by Category";
        const string expectedMessage = "Representation of all the Scopes present on EquipAI";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        (await dashboardPage.GetEmissionsByCategoryLabelAsync()).Should().Be(expectedLabel);
        (await dashboardPage.GetEmissionsByCategoryMessageAsync()).Should().Be(expectedMessage);
        (await dashboardPage.IsScope1CardVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsScope2CardVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsScope3CardVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T25_Dashboard_EmissionsByCategory_Scope1Card()
    {
        const string expectedTitle = "Scope 1";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var selectedYearText = await dashboardPage.GetSelectedChartYearAsync();
        var year = int.Parse(selectedYearText, CultureInfo.InvariantCulture);
        var expectedRows = await SqlHelper.GetEmissionsByCategoryAsync(year, ghgScope: 1);
        var expectedNames = expectedRows.Select(row => row.CategoryName).ToList();
        var expectedValues = expectedRows.Select(row => FormatCategoryEmissionsValue(row.Co2eTonnes)).ToList();

        (await dashboardPage.GetScope1TitleAsync()).Should().Be(expectedTitle);
        (await dashboardPage.GetScope1CategoryNamesAsync()).Should().Equal(expectedNames);
        (await dashboardPage.GetScope1CategoryValuesAsync()).Should().Equal(expectedValues);
    }

    [Test]
    public async Task T26_Dashboard_EmissionsByCategory_Scope2Card()
    {
        const string expectedTitle = "Scope 2";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var selectedYearText = await dashboardPage.GetSelectedChartYearAsync();
        var year = int.Parse(selectedYearText, CultureInfo.InvariantCulture);
        var expectedRows = await SqlHelper.GetEmissionsByCategoryAsync(year, ghgScope: 2);
        var expectedNames = expectedRows.Select(row => row.CategoryName).ToList();
        var expectedValues = expectedRows.Select(row => FormatCategoryEmissionsValue(row.Co2eTonnes)).ToList();

        (await dashboardPage.GetScope2TitleAsync()).Should().Be(expectedTitle);
        (await dashboardPage.GetScope2CategoryNamesAsync()).Should().Equal(expectedNames);
        (await dashboardPage.GetScope2CategoryValuesAsync()).Should().Equal(expectedValues);
    }

    [Test]
    public async Task T27_Dashboard_EmissionsByCategory_Scope3Card()
    {
        const string expectedTitle = "Scope 3";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var selectedYearText = await dashboardPage.GetSelectedChartYearAsync();
        var year = int.Parse(selectedYearText, CultureInfo.InvariantCulture);
        var expectedRows = await SqlHelper.GetEmissionsByCategoryAsync(year, ghgScope: 3);
        var expectedNames = expectedRows.Select(row => row.CategoryName).ToList();
        var expectedValues = expectedRows.Select(row => FormatCategoryEmissionsValue(row.Co2eTonnes)).ToList();

        (await dashboardPage.GetScope3TitleAsync()).Should().Be(expectedTitle);
        (await dashboardPage.GetScope3CategoryNamesAsync()).Should().Equal(expectedNames);
        (await dashboardPage.GetScope3CategoryValuesAsync()).Should().Equal(expectedValues);
    }

    private static async Task AssertChartScopeMatchesDbAsync(
        DashboardPage dashboardPage,
        int year,
        int scope,
        Func<Task> clickScopeAsync,
        string? yearOption = null)
    {
        var expectedRows = await SqlHelper.GetChartScopeMonthlyEmissionsAsync(year, scope);
        if (expectedRows.Count == 0)
            Assert.Pass("No data for this Scope");

        await dashboardPage.SelectYearAsync(yearOption ?? year.ToString(CultureInfo.InvariantCulture));
        await clickScopeAsync();

        if (!await dashboardPage.HasChartLineAsync())
            Assert.Pass("No data for this Scope");

        var chartValuesByMonth = await dashboardPage.GetChartMonthlyTonnesAsync();
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

    private static decimal ParseMetricValueToOneDecimal(string valueText)
    {
        var normalized = valueText
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Trim();

        var parsed = decimal.Parse(normalized, CultureInfo.InvariantCulture);
        return Math.Round(parsed, 1, MidpointRounding.AwayFromZero);
    }

    private static string FormatScopeEmissionsValue(decimal tonnes)
    {
        if (tonnes < 1000m)
        {
            var rounded = Math.Round(tonnes, 1, MidpointRounding.AwayFromZero);
            return FormatOneDecimalOrInteger(rounded);
        }

        var thousands = Math.Round(tonnes / 1000m, 1, MidpointRounding.AwayFromZero);
        return FormatOneDecimalOrInteger(thousands) + "k";
    }

    private static string FormatOneDecimalOrInteger(decimal value)
    {
        return value == decimal.Truncate(value)
            ? decimal.Truncate(value).ToString("0", CultureInfo.InvariantCulture)
            : value.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static string FormatProjectTonnesValue(decimal tonnes)
    {
        var rounded = Math.Round(tonnes, 1, MidpointRounding.AwayFromZero);
        return rounded == decimal.Truncate(rounded)
            ? decimal.Truncate(rounded).ToString("#,##0", CultureInfo.InvariantCulture)
            : rounded.ToString("#,##0.0", CultureInfo.InvariantCulture);
    }

    private static string FormatCategoryEmissionsValue(decimal tonnes) =>
        FormatProjectTonnesValue(tonnes) + " tCO2e";
}
