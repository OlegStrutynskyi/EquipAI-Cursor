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
        (await dashboardPage.IsYearDropdownVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalCarbonEmissionCardVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalCarbonEmissionsByScopeCardVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsCarbonEmissionsByCategoryCardVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsTotalActiveProjectsCardVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsActiveProjectsCardVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsSupportCardVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Dashboard_YearDropdown()
    {
        var emissionYears = await SqlHelper.GetActivityYearsAsync();
        var expectedYears = new[] { "All" }.Concat(emissionYears).ToList();

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualYears = await dashboardPage.GetYearDropdownOptionsAsync();
        actualYears.Should().Equal(expectedYears);
    }

    [Test]
    public async Task T03_Dashboard_TotalEmissions_View()
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
    public async Task T04_Dashboard_TotalEmissions_TotalValue()
    {
        var expectedValue = Math.Round(
            await SqlHelper.GetTotalCarbonEmissionsTonnesAsync(),
            1,
            MidpointRounding.AwayFromZero);

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var uiValueText = await dashboardPage.GetTotalEmissionsValueAsync();
        var actualValue = ParseMetricValueToOneDecimal(uiValueText);

        actualValue.Should().Be(expectedValue);

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
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
        var expectedYearValue = Math.Round(
            await SqlHelper.GetTotalCarbonEmissionsTonnesAsync(int.Parse(selectedYear, CultureInfo.InvariantCulture)),
            1,
            MidpointRounding.AwayFromZero);

        await dashboardPage.SelectYearAsync(selectedYear);

        decimal actualYearValue = 0;
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            actualYearValue = ParseMetricValueToOneDecimal(await dashboardPage.GetTotalEmissionsValueAsync());
            if (actualYearValue == expectedYearValue)
                break;

            await Task.Delay(200);
        }

        actualYearValue.Should().Be(expectedYearValue);
    }

    [Test]
    public async Task T05_Dashboard_TotalEmissions_Scope1Value()
    {
        var expectedValue = FormatScopeEmissionsValue(
            await SqlHelper.GetCarbonEmissionsTonnesByScopeAsync(year: null, ghgScope: 1));

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualValue = await dashboardPage.GetTotalEmissionsScope1ValueAsync();
        actualValue.Should().Be(expectedValue);

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
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
        var expectedYearValue = FormatScopeEmissionsValue(
            await SqlHelper.GetCarbonEmissionsTonnesByScopeAsync(
                int.Parse(selectedYear, CultureInfo.InvariantCulture),
                ghgScope: 1));

        await dashboardPage.SelectYearAsync(selectedYear);

        var actualYearValue = string.Empty;
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            actualYearValue = await dashboardPage.GetTotalEmissionsScope1ValueAsync();
            if (actualYearValue == expectedYearValue)
                break;

            await Task.Delay(200);
        }

        actualYearValue.Should().Be(expectedYearValue);
    }

    [Test]
    public async Task T06_Dashboard_TotalEmissions_Scope2Value()
    {
        var expectedValue = FormatScopeEmissionsValue(
            await SqlHelper.GetCarbonEmissionsTonnesByScopeAsync(year: null, ghgScope: 2));

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualValue = await dashboardPage.GetTotalEmissionsScope2ValueAsync();
        actualValue.Should().Be(expectedValue);

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
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
        var expectedYearValue = FormatScopeEmissionsValue(
            await SqlHelper.GetCarbonEmissionsTonnesByScopeAsync(
                int.Parse(selectedYear, CultureInfo.InvariantCulture),
                ghgScope: 2));

        await dashboardPage.SelectYearAsync(selectedYear);

        var actualYearValue = string.Empty;
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            actualYearValue = await dashboardPage.GetTotalEmissionsScope2ValueAsync();
            if (actualYearValue == expectedYearValue)
                break;

            await Task.Delay(200);
        }

        actualYearValue.Should().Be(expectedYearValue);
    }

    [Test]
    public async Task T07_Dashboard_TotalEmissions_Scope3Value()
    {
        var expectedValue = FormatScopeEmissionsValue(
            await SqlHelper.GetCarbonEmissionsTonnesByScopeAsync(year: null, ghgScope: 3));

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualValue = await dashboardPage.GetTotalEmissionsScope3ValueAsync();
        actualValue.Should().Be(expectedValue);

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
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
        var expectedYearValue = FormatScopeEmissionsValue(
            await SqlHelper.GetCarbonEmissionsTonnesByScopeAsync(
                int.Parse(selectedYear, CultureInfo.InvariantCulture),
                ghgScope: 3));

        await dashboardPage.SelectYearAsync(selectedYear);

        var actualYearValue = string.Empty;
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            actualYearValue = await dashboardPage.GetTotalEmissionsScope3ValueAsync();
            if (actualYearValue == expectedYearValue)
                break;

            await Task.Delay(200);
        }

        actualYearValue.Should().Be(expectedYearValue);
    }

    [Test]
    public async Task T08_Dashboard_TotalEmissionsByScope_View()
    {
        const string expectedLabel = "Total Carbon Emissions by Scope";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        (await dashboardPage.GetTotalEmissionsByScopeLabelAsync()).Should().Be(expectedLabel);

        await dashboardPage.SelectYearAsync("All");
        await AssertChartSubtitleAsync(dashboardPage, "All");

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
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
        await dashboardPage.SelectYearAsync(selectedYear);
        await AssertChartSubtitleAsync(dashboardPage, selectedYear);
        await AssertChartScopeMatchesDbAsync(
            dashboardPage,
            int.Parse(selectedYear, CultureInfo.InvariantCulture),
            scope: 1,
            dashboardPage.ClickScope1BtnAsync,
            selectedYear);
    }

    [Test]
    public async Task T09_Dashboard_TotalEmissionsByScope_ChartScope1()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await dashboardPage.SelectYearAsync("All");
        await dashboardPage.ClickScope1BtnAsync();
        await AssertChartAllYearsMatchDbAsync(dashboardPage, scope: 1);

        var previousYear = DateTime.UtcNow.Year - 1;
        var previousYearText = previousYear.ToString(CultureInfo.InvariantCulture);
        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
        if (!yearOptions.Any(option => option.Equals(previousYearText, StringComparison.Ordinal)))
            return;

        await AssertChartScopeMatchesDbAsync(
            dashboardPage,
            previousYear,
            scope: 1,
            dashboardPage.ClickScope1BtnAsync,
            previousYearText);
    }

    [Test]
    public async Task T10_Dashboard_TotalEmissionsByScope_ChartScope2()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await dashboardPage.SelectYearAsync("All");
        await dashboardPage.ClickScope2BtnAsync();
        await AssertChartAllYearsMatchDbAsync(dashboardPage, scope: 2);

        var previousYear = DateTime.UtcNow.Year - 1;
        var previousYearText = previousYear.ToString(CultureInfo.InvariantCulture);
        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
        if (!yearOptions.Any(option => option.Equals(previousYearText, StringComparison.Ordinal)))
            return;

        await AssertChartScopeMatchesDbAsync(
            dashboardPage,
            previousYear,
            scope: 2,
            dashboardPage.ClickScope2BtnAsync,
            previousYearText);
    }

    [Test]
    public async Task T11_Dashboard_TotalEmissionsByScope_ChartScope3()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await dashboardPage.SelectYearAsync("All");
        await dashboardPage.ClickScope3BtnAsync();
        await AssertChartAllYearsMatchDbAsync(dashboardPage, scope: 3);

        var previousYear = DateTime.UtcNow.Year - 1;
        var previousYearText = previousYear.ToString(CultureInfo.InvariantCulture);
        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
        if (!yearOptions.Any(option => option.Equals(previousYearText, StringComparison.Ordinal)))
            return;

        await AssertChartScopeMatchesDbAsync(
            dashboardPage,
            previousYear,
            scope: 3,
            dashboardPage.ClickScope3BtnAsync,
            previousYearText);
    }

    [Test]
    public async Task T12_Dashboard_EmissionsByCategory_View()
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
    public async Task T13_Dashboard_EmissionsByCategory_Scope1Card()
    {
        const string expectedTitle = "Scope 1";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await dashboardPage.SelectYearAsync("All");
        await AssertScopeCategoryCardAsync(
            dashboardPage,
            expectedTitle,
            ghgScope: 1,
            year: null,
            dashboardPage.GetScope1TitleAsync,
            dashboardPage.GetScope1CategoryNamesAsync,
            dashboardPage.GetScope1CategoryValuesAsync);

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
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
        await dashboardPage.SelectYearAsync(selectedYear);
        await AssertScopeCategoryCardAsync(
            dashboardPage,
            expectedTitle,
            ghgScope: 1,
            int.Parse(selectedYear, CultureInfo.InvariantCulture),
            dashboardPage.GetScope1TitleAsync,
            dashboardPage.GetScope1CategoryNamesAsync,
            dashboardPage.GetScope1CategoryValuesAsync);
    }

    [Test]
    public async Task T14_Dashboard_EmissionsByCategory_Scope2Card()
    {
        const string expectedTitle = "Scope 2";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await dashboardPage.SelectYearAsync("All");
        await AssertScopeCategoryCardAsync(
            dashboardPage,
            expectedTitle,
            ghgScope: 2,
            year: null,
            dashboardPage.GetScope2TitleAsync,
            dashboardPage.GetScope2CategoryNamesAsync,
            dashboardPage.GetScope2CategoryValuesAsync);

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
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
        await dashboardPage.SelectYearAsync(selectedYear);
        await AssertScopeCategoryCardAsync(
            dashboardPage,
            expectedTitle,
            ghgScope: 2,
            int.Parse(selectedYear, CultureInfo.InvariantCulture),
            dashboardPage.GetScope2TitleAsync,
            dashboardPage.GetScope2CategoryNamesAsync,
            dashboardPage.GetScope2CategoryValuesAsync);
    }

    [Test]
    public async Task T15_Dashboard_EmissionsByCategory_Scope3Card()
    {
        const string expectedTitle = "Scope 3";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await dashboardPage.SelectYearAsync("All");
        await AssertScopeCategoryCardAsync(
            dashboardPage,
            expectedTitle,
            ghgScope: 3,
            year: null,
            dashboardPage.GetScope3TitleAsync,
            dashboardPage.GetScope3CategoryNamesAsync,
            dashboardPage.GetScope3CategoryValuesAsync);

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
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
        await dashboardPage.SelectYearAsync(selectedYear);
        await AssertScopeCategoryCardAsync(
            dashboardPage,
            expectedTitle,
            ghgScope: 3,
            int.Parse(selectedYear, CultureInfo.InvariantCulture),
            dashboardPage.GetScope3TitleAsync,
            dashboardPage.GetScope3CategoryNamesAsync,
            dashboardPage.GetScope3CategoryValuesAsync);
    }

    [Test]
    public async Task T16_Dashboard_EmissionsByCategory_Scope1EmissionTypes()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await dashboardPage.SelectYearAsync("All");
        await AssertScopeEmissionTypesAsync(
            ghgScope: 1,
            emptyCategoriesMessage: "No Emission Categories for Scope 1",
            dashboardPage.GetScope1DisclosureCategoryNamesAsync,
            dashboardPage.GetScope1CategoryEmissionTypeNamesAsync,
            year: null,
            dashboardPage.GetScope1CategoryEmissionTypeValuesAsync);

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
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
        await dashboardPage.SelectYearAsync(selectedYear);
        await AssertScopeEmissionTypesAsync(
            ghgScope: 1,
            emptyCategoriesMessage: "No Emission Categories for Scope 1",
            dashboardPage.GetScope1DisclosureCategoryNamesAsync,
            dashboardPage.GetScope1CategoryEmissionTypeNamesAsync,
            int.Parse(selectedYear, CultureInfo.InvariantCulture),
            dashboardPage.GetScope1CategoryEmissionTypeValuesAsync);
    }

    [Test]
    public async Task T17_Dashboard_EmissionsByCategory_Scope2EmissionTypes()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await dashboardPage.SelectYearAsync("All");
        await AssertScopeEmissionTypesAsync(
            ghgScope: 2,
            emptyCategoriesMessage: "No Emission Categories for Scope 2",
            dashboardPage.GetScope2DisclosureCategoryNamesAsync,
            dashboardPage.GetScope2CategoryEmissionTypeNamesAsync,
            year: null,
            dashboardPage.GetScope2CategoryEmissionTypeValuesAsync);

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
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
        await dashboardPage.SelectYearAsync(selectedYear);
        await AssertScopeEmissionTypesAsync(
            ghgScope: 2,
            emptyCategoriesMessage: "No Emission Categories for Scope 2",
            dashboardPage.GetScope2DisclosureCategoryNamesAsync,
            dashboardPage.GetScope2CategoryEmissionTypeNamesAsync,
            int.Parse(selectedYear, CultureInfo.InvariantCulture),
            dashboardPage.GetScope2CategoryEmissionTypeValuesAsync);
    }

    [Test]
    public async Task T18_Dashboard_EmissionsByCategory_Scope3EmissionTypes()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await dashboardPage.SelectYearAsync("All");
        await AssertScopeEmissionTypesAsync(
            ghgScope: 3,
            emptyCategoriesMessage: "No Emission Categories for Scope 3",
            dashboardPage.GetScope3DisclosureCategoryNamesAsync,
            dashboardPage.GetScope3CategoryEmissionTypeNamesAsync,
            year: null,
            dashboardPage.GetScope3CategoryEmissionTypeValuesAsync);

        var yearOptions = await dashboardPage.GetYearDropdownOptionsAsync();
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
        await dashboardPage.SelectYearAsync(selectedYear);
        await AssertScopeEmissionTypesAsync(
            ghgScope: 3,
            emptyCategoriesMessage: "No Emission Categories for Scope 3",
            dashboardPage.GetScope3DisclosureCategoryNamesAsync,
            dashboardPage.GetScope3CategoryEmissionTypeNamesAsync,
            int.Parse(selectedYear, CultureInfo.InvariantCulture),
            dashboardPage.GetScope3CategoryEmissionTypeValuesAsync);
    }

    [Test]
    public async Task T19_Dashboard_TotalProjects_View()
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
    public async Task T20_Dashboard_TotalProjects_TotalValue()
    {
        var expectedValue = (await SqlHelper.GetTotalActiveProjectsCountAsync())
            .ToString(CultureInfo.InvariantCulture);

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualValue = await dashboardPage.GetTotalProjectsValueAsync();
        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task T21_Dashboard_TotalProjects_ReportingValue()
    {
        var expectedValue = (await SqlHelper.GetReportingActiveProjectsCountAsync())
            .ToString(CultureInfo.InvariantCulture);

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var actualValue = await dashboardPage.GetTotalProjectsReportingValueAsync();
        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task T22_Dashboard_TotalProjects_NonReportingValue()
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
    public async Task T23_Dashboard_ActiveProjects_View()
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
    public async Task T24_Dashboard_ActiveProjects_NoActiveProjects()
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
    public async Task T25_Dashboard_ActiveProjects_List()
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
    public async Task T26_Dashboard_ActiveProjects_ClickProjectName()
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

        (await projectDashboardPage.GetPageTitleAsync()).Should().Be($"{projectName} Overview");
    }

    [Test]
    public async Task T27_Dashboard_ActiveProjects_ClickViewAllProjectsLink()
    {
        const string expectedTitle = "Projects";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var projectsPage = await dashboardPage.ClickViewAllProjectsLinkAsync();
        (await projectsPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    private static async Task AssertScopeEmissionTypesAsync(
        int ghgScope,
        string emptyCategoriesMessage,
        Func<Task<IReadOnlyList<string>>> getCategoryNamesAsync,
        Func<string, Task<IReadOnlyList<string>>> getEmissionTypeNamesAsync,
        int? year = null,
        Func<string, Task<IReadOnlyList<string>>>? getEmissionTypeValuesAsync = null)
    {
        var categories = await SqlHelper.GetEmissionCategoriesByScopeAsync(ghgScope);
        if (categories.Count == 0)
            Assert.Pass(emptyCategoriesMessage);

        IReadOnlyList<string> actualCategoryNames = [];
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            actualCategoryNames = await getCategoryNamesAsync();
            if (actualCategoryNames.Count > 0)
                break;

            await Task.Delay(200);
        }

        actualCategoryNames.Should().Contain(categories.Select(category => category.DisplayName));

        foreach (var category in categories)
        {
            var emissionTypes = await SqlHelper.GetEmissionTypeDisplayNamesByCategoryCodeAsync(category.Code);
            if (emissionTypes.Count == 0)
            {
                TestContext.Out.WriteLine($"{category.DisplayName} does not have any Emission Types.");
                continue;
            }

            var actualEmissionTypes = await getEmissionTypeNamesAsync(category.DisplayName);
            actualEmissionTypes.Should().Equal(emissionTypes);

            if (getEmissionTypeValuesAsync is null)
                continue;

            var expectedValueRows = await SqlHelper.GetEmissionTypeEmissionsByCategoryCodeAsync(year, category.Code);
            var expectedValues = expectedValueRows
                .Select(row => FormatCategoryEmissionsValue(row.Co2eTonnes))
                .ToList();
            var actualValues = await getEmissionTypeValuesAsync(category.DisplayName);
            var yearLabel = year?.ToString(CultureInfo.InvariantCulture) ?? "All";
            actualValues.Should().Equal(expectedValues, $"{category.DisplayName} emission type values for {yearLabel}");
        }
    }

    private static async Task AssertScopeCategoryCardAsync(
        DashboardPage dashboardPage,
        string expectedTitle,
        int ghgScope,
        int? year,
        Func<Task<string>> getTitleAsync,
        Func<Task<IReadOnlyList<string>>> getNamesAsync,
        Func<Task<IReadOnlyList<string>>> getValuesAsync)
    {
        var expectedRows = (await SqlHelper.GetEmissionsByCategoryAsync(year, ghgScope))
            .OrderBy(row => row.CategoryName, StringComparer.Ordinal)
            .ToList();
        var expectedNames = expectedRows.Select(row => row.CategoryName).ToList();
        var expectedValues = expectedRows.Select(row => FormatCategoryEmissionsValue(row.Co2eTonnes)).ToList();

        (await getTitleAsync()).Should().Be(expectedTitle);

        IReadOnlyList<string> actualNames = [];
        IReadOnlyList<string> actualValues = [];
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            actualNames = await getNamesAsync();
            actualValues = await getValuesAsync();
            if (actualNames.SequenceEqual(expectedNames) && actualValues.SequenceEqual(expectedValues))
                break;

            await Task.Delay(200);
        }

        actualNames.Should().Equal(expectedNames);
        actualValues.Should().Equal(expectedValues);
    }

    private static async Task AssertChartSubtitleAsync(DashboardPage dashboardPage, string yearOption)
    {
        var expected = yearOption.Equals("All", StringComparison.OrdinalIgnoreCase)
            ? "tCO2e \u00b7 Yearly, all years on record"
            : $"tCO2e \u00b7 Monthly, {yearOption}";

        var deadline = DateTime.UtcNow.AddSeconds(10);
        var actual = string.Empty;
        while (DateTime.UtcNow < deadline)
        {
            actual = await dashboardPage.GetTotalEmissionsByScopeMessageAsync();
            if (actual == expected)
                break;

            await Task.Delay(200);
        }

        actual.Should().Be(expected);
    }

    private static async Task AssertChartAllYearsMatchDbAsync(DashboardPage dashboardPage, int scope)
    {
        var expectedRows = await SqlHelper.GetChartScopeYearlyEmissionsAsync(scope);
        if (expectedRows.Count == 0)
            Assert.Pass("No data for this Scope");

        if (!await dashboardPage.HasChartLineAsync())
            Assert.Pass("No data for this Scope");

        var chartValuesByYear = await dashboardPage.GetChartYearlyTonnesAsync();
        if (chartValuesByYear.Count == 0 || chartValuesByYear.Values.All(value => value == 0m))
            Assert.Pass("No data for this Scope");

        foreach (var row in expectedRows)
        {
            chartValuesByYear.Should().ContainKey(row.Year);
            var actualValue = chartValuesByYear[row.Year];
            var expectedValue = RoundToChartDisplayPrecision(row.Co2eTonnes, actualValue);
            actualValue.Should().Be(expectedValue, $"year {row.Year}");
        }
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

    private static decimal ParseMetricValueToOneDecimal(string valueText)
    {
        var normalized = valueText
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Trim();
        if (normalized is "—" or "–" or "-" or "")
            return 0m;

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
