using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class SideMenuTests : BaseTest
{
    [Test]
    public async Task T01_SideMenu_SuperAdminView()
    {
        var expectedLinks = new[]
        {
            "Enterprise",
            "Invoices",
            "Telemetry",
            "Users",
            "Projects",
            "Units",
            "Emission Types",
            "Aliases",
            "Factor Import",
            "Data Hub",
            "Reports",
            "Dark Mode",
            "Permissions",
            "Logout",
        };

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();

        var linkTexts = await sideMenuPage.GetLinkTextsAsync();
        linkTexts.Should().Contain(expectedLinks);
    }

    [Test]
    public async Task T02_SideMenu_DataStewardView()
    {
        try
        {
            await SetUserCapabilitiesAndLoginAsync(2);

            var sideMenuPage = new SideMenuPage(Fixture.Page);
            await sideMenuPage.OpenAsync();

            (await sideMenuPage.IsLinkVisibleAsync("Enterprise")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Invoices")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Telemetry")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Data Hub")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Reports")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Dark Mode")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Logout")).Should().BeTrue();

            (await sideMenuPage.IsLinkVisibleAsync("Users")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Projects")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Units")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Emission Types")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Aliases")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Factor Import")).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.SetUserCapabilitiesAsync(Config.MicrosoftEmail, 1);
        }
    }

    [Test]
    public async Task T03_SideMenu_UserManagerView()
    {
        try
        {
            await SetUserCapabilitiesAndLoginAsync(8);

            var sideMenuPage = new SideMenuPage(Fixture.Page);
            await sideMenuPage.OpenAsync();

            (await sideMenuPage.IsLinkVisibleAsync("Enterprise")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Users")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Data Hub")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Reports")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Dark Mode")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Logout")).Should().BeTrue();

            (await sideMenuPage.IsLinkVisibleAsync("Invoices")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Telemetry")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Projects")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Units")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Emission Types")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Aliases")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Factor Import")).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.SetUserCapabilitiesAsync(Config.MicrosoftEmail, 1);
        }
    }

    [Test]
    public async Task T04_SideMenu_ProjectManagerView()
    {
        try
        {
            await SetUserCapabilitiesAndLoginAsync(16);

            var sideMenuPage = new SideMenuPage(Fixture.Page);
            await sideMenuPage.OpenAsync();

            (await sideMenuPage.IsLinkVisibleAsync("Enterprise")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Projects")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Data Hub")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Reports")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Dark Mode")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Logout")).Should().BeTrue();

            (await sideMenuPage.IsLinkVisibleAsync("Invoices")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Telemetry")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Users")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Units")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Emission Types")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Aliases")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Factor Import")).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.SetUserCapabilitiesAsync(Config.MicrosoftEmail, 1);
        }
    }

    [Test]
    public async Task T05_SideMenu_ClickEnterprise()
    {
        const string expectedTitle = "Haskell Enterprise Overview";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickEnterpriseAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T06_SideMenu_ClickInvoices()
    {
        const string expectedTitle = "Invoices";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickInvoicesAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T07_SideMenu_ClickTelemetry()
    {
        const string expectedTitle = "Telemetry";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickTelemetryAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T08_SideMenu_ClickUsers()
    {
        const string expectedTitle = "Users";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickUsersAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T09_SideMenu_ClickProjects()
    {
        const string expectedTitle = "Projects";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickProjectsAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T10_SideMenu_ClickUnits()
    {
        const string expectedTitle = "Units";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickUnitsAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T11_SideMenu_ClickEmissionTypes()
    {
        const string expectedTitle = "Emission Types";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickEmissionTypesAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T12_SideMenu_ClickAliases()
    {
        const string expectedTitle = "Aliases";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickAliasesAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T13_SideMenu_ClickFactorImport()
    {
        const string expectedTitle = "Factor Import";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickFactorImportAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T14_SideMenu_ClickDarkMode()
    {
        const string expectedDarkBackground = "rgb(0, 0, 0)";
        const string expectedLightBackground = "rgb(255, 255, 255)";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();

        if (await sideMenuPage.IsDarkModeEnabledAsync())
            await sideMenuPage.ClickDarkModeAsync();

        await sideMenuPage.ClickDarkModeAsync();
        (await sideMenuPage.GetBodyBackgroundColorAsync()).Should().Be(expectedDarkBackground);

        await sideMenuPage.ClickDarkModeAsync();
        (await sideMenuPage.GetBodyBackgroundColorAsync()).Should().Be(expectedLightBackground);
    }

    [Test]
    public async Task T15_SideMenu_ClickLogout()
    {
        const string expectedLoginTitle = "Sign in to EquipAI";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        var loginPage = await sideMenuPage.ClickLogoutAsync();

        (await loginPage.GetTitleAsync()).Should().Be(expectedLoginTitle);
    }
}
