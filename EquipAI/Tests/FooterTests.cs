using EquipAI.Pages;
using FluentAssertions;

namespace EquipAI.Tests;

public class FooterTests : BaseTest
{
    [Test]
    public async Task T01_Footer_DefaultView()
    {
        var expectedFooterMessage = $"© {DateTime.Now.Year} Haskell Construction. All rights reserved.";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var footerPage = new FooterPage(Fixture.Page);
        (await footerPage.IsFooterSectionVisibleAsync()).Should().BeTrue();
        (await footerPage.GetFooterMessageAsync()).Should().Be(expectedFooterMessage);
        (await footerPage.IsPrivacyPolicyLinkVisibleAsync()).Should().BeTrue();
        (await footerPage.IsTermsOfServiceLinkVisibleAsync()).Should().BeTrue();
    }
}
