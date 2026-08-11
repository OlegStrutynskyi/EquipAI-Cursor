using EquipAI.Pages;
using FluentAssertions;

namespace EquipAI.Tests;

public class DashboardTests : BaseTest
{
    [Test]
    public async Task T01_Dashboard_DefaultView()
    {
        const string expectedTitle = "Haskell Enterprise Overview";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        (await dashboardPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }
}
