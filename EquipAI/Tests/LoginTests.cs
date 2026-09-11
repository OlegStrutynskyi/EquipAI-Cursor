using EquipAI.Pages;
using FluentAssertions;

namespace EquipAI.Tests;

public class LoginTestsNotAuthenticated : BaseTestNotAuthenticated
{
    [Test]
    public async Task T01_Login_DefaultView()
    {
        const string expectedButtonText = "Sign in";

        var loginPage = new LoginPage(Fixture.Page);
        await loginPage.OpenAsync();

        (await loginPage.GetSignInBtnTextAsync()).Should().Be(expectedButtonText);
        (await loginPage.IsSignInBtnVisibleAsync()).Should().BeTrue();
        (await loginPage.IsSignInBtnEnabledAsync()).Should().BeTrue();
    }
}

public class LoginTests : BaseTest
{
    [Test]
    public async Task T02_Login_SuccessLogin()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        (await dashboardPage.IsLogoVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsOpenMenuBtnVisibleAsync()).Should().BeTrue();
    }
}
