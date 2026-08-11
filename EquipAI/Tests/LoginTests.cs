using EquipAI.Pages;
using FluentAssertions;

namespace EquipAI.Tests;

public class LoginTestsNotAuthenticated : BaseTestNotAuthenticated
{
    [Test]
    public async Task T01_Login_DefaultView()
    {
        const string expectedTitle = "Sign in to EquipAI";
        const string expectedText = "Choose how you want to sign in.";
        const string expectedButtonText = "Sign in with SAML (SSO/OIDC)";

        var loginPage = new LoginPage(Fixture.Page);
        await loginPage.OpenAsync();

        var title = await loginPage.GetTitleAsync();
        var text = await loginPage.GetTextAsync();
        var buttonText = await loginPage.GetLoginBtnTextAsync();

        title.Should().Be(expectedTitle);
        text.Should().Be(expectedText);
        buttonText.Should().Be(expectedButtonText);
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
