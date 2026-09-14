using EquipAI.Pages;
using FluentAssertions;

namespace EquipAI.Tests;

public class HeaderTests : BaseTest
{
    [Test]
    public async Task T01_Header_DefaultView()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var headerPage = new HeaderPage(Fixture.Page);
        (await headerPage.IsMenuIconVisibleAsync()).Should().BeTrue();
        (await headerPage.IsAppLogoVisibleAsync()).Should().BeTrue();
        (await headerPage.IsAskEquipAIBtnVisibleAsync()).Should().BeTrue();
        (await headerPage.IsNotificationIconVisibleAsync()).Should().BeTrue();
        (await headerPage.IsUserNameVisibleAsync()).Should().BeTrue();
        (await headerPage.IsUserRoleVisibleAsync()).Should().BeTrue();
        (await headerPage.IsProfilePictureVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Header_ClickMenuIcon()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        var headerPage = new HeaderPage(Fixture.Page);
        var sideMenuPage =  await headerPage.ClickMenuIconAsync();

        (await sideMenuPage.IsSideMenuVisibleAsync()).Should().BeTrue();
    }
}
