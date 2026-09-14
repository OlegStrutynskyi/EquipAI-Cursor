using EquipAI.Pages;
using FluentAssertions;

namespace EquipAI.Tests;

public class DashboardTests : BaseTest
{
    [Test]
    public async Task T01_Dashboard_DefaultView()
    {
        const string expectedTitle = "Haskell Enterprise Overview";
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
}
