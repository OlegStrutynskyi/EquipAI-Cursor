using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class ProjectDashboardTests : BaseTest
{
    [Test]
    public async Task T01_ProjectDashboard_DefaultView()
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
    public async Task T02_ProjectDashboard_ReportingBadge()
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
    public async Task T03_ProjectDashboard_Total_View()
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
    public Task T04_ProjectDashboard_Total_Scope1() => AssertScopeTotalAsync(scope: 1, selectScope: false);

    [Test]
    public Task T05_ProjectDashboard_Total_Scope2() => AssertScopeTotalAsync(scope: 2, selectScope: true);

    [Test]
    public Task T06_ProjectDashboard_Total_Scope3() => AssertScopeTotalAsync(scope: 3, selectScope: true);

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

    private static string FormatEmissionValue(decimal tonnes)
    {
        var rounded = Math.Round(tonnes, 1, MidpointRounding.AwayFromZero);
        return rounded == decimal.Truncate(rounded)
            ? decimal.Truncate(rounded).ToString("#,##0", CultureInfo.InvariantCulture)
            : rounded.ToString("#,##0.0", CultureInfo.InvariantCulture);
    }
}
