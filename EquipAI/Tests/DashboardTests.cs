using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class DashboardTests : BaseTest
{
    [Test]
    public async Task T01_Dashboard_SuperAdminView()
    {
        const string expectedTitle = "EquipAI";
        var expectedSignedAsText = $"Signed in as {Config.TestUser3FirstName} {Config.TestUser3LastName}";
        const string expectedWelcomeText = "Welcome. Protected routes are available after Entra sign-in.";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();

        (await dashboardPage.IsTitleVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsSignedAsTextVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsWelcomeTextVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsInvoicesBtnVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsAdministrationBtnVisibleAsync()).Should().BeTrue();
        (await dashboardPage.IsSignOutButtonVisibleAsync()).Should().BeTrue();

        (await dashboardPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await dashboardPage.GetSignedAsTextAsync()).Should().Be(expectedSignedAsText);
        (await dashboardPage.GetWelcomeTextAsync()).Should().Be(expectedWelcomeText);
    }

    [Test]
    public async Task T02_Dashboard_DataStewardView()
    {
        try
        {
            await SetUserCapabilitiesAndLoginAsync(2);

            var dashboardPage = new DashboardPage(Fixture.Page);
            await dashboardPage.OpenAsync();

            (await dashboardPage.IsInvoicesBtnVisibleAsync()).Should().BeTrue();
            (await dashboardPage.IsAdministrationBtnVisibleAsync()).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.SetUserCapabilitiesAsync(Config.MicrosoftEmail, 1);
        }
    }

    [Test]
    public async Task T03_Dashboard_UserManagerView()
    {
        try
        {
            await SetUserCapabilitiesAndLoginAsync(8);

            var dashboardPage = new DashboardPage(Fixture.Page);
            await dashboardPage.OpenAsync();

            (await dashboardPage.IsInvoicesBtnVisibleAsync()).Should().BeFalse();
            (await dashboardPage.IsAdministrationBtnVisibleAsync()).Should().BeTrue();
        }
        finally
        {
            await SqlHelper.SetUserCapabilitiesAsync(Config.MicrosoftEmail, 1);
        }
    }

    [Test]
    public async Task T04_Dashboard_ProjectManagerView()
    {
        try
        {
            await SetUserCapabilitiesAndLoginAsync(16);

            var dashboardPage = new DashboardPage(Fixture.Page);
            await dashboardPage.OpenAsync();

            (await dashboardPage.IsInvoicesBtnVisibleAsync()).Should().BeFalse();
            (await dashboardPage.IsAdministrationBtnVisibleAsync()).Should().BeTrue();
        }
        finally
        {
            await SqlHelper.SetUserCapabilitiesAsync(Config.MicrosoftEmail, 1);
        }
    }

    [Test]
    public async Task T05_Dashboard_ClickInvoicesBtn()
    {
        await SetUserCapabilitiesAndLoginAsync(1);

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        var invoicesPage = await dashboardPage.ClickInvoicesBtnAsync();

        (await invoicesPage.IsInvoicesTitleVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T06_Dashboard_ClickAdministrationBtn()
    {
        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        var administrationPage = await dashboardPage.ClickAdministrationBtnAsync();

        (await administrationPage.IsAdministrationTitleVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T07_Dashboard_ClickSignOut()
    {
        const string expectedLoginTitle = "Sign in to EquipAI";

        var dashboardPage = new DashboardPage(Fixture.Page);
        await dashboardPage.OpenAsync();
        await dashboardPage.ClickSignOutAsync();

        var loginPage = new LoginPage(Fixture.Page);

        (await loginPage.GetTitleAsync()).Should().Be(expectedLoginTitle);
    }
}
