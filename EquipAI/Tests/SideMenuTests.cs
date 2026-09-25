using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class SideMenuTests : BaseTest
{
    [Test]
    public async Task T01_SideMenu_SuperAdminView()
    {
        var expectedDefaultLinks = new[]
        {
            "Company Dashboard",
            "Projects",
            "Emission Hub",
            "Data Hub",
            "Reports",
            "Units",
            "Users",
            "Dark Mode",
            "Logout",
        };
        var expectedEmissionHubLinks = new[]
        {
            "Emission Categories",
            "Factor Import",
            "Emission Types",
            "Aliases",
        };
        var expectedDataHubLinks = new[]
        {
            "Invoice Upload",
            "Telemetry Upload",
            "Utility Bill Upload",
        };

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();

        var linkTexts = await sideMenuPage.GetLinkTextsAsync();
        linkTexts.Should().Contain(expectedDefaultLinks);
        foreach (var link in expectedEmissionHubLinks.Concat(expectedDataHubLinks))
            (await sideMenuPage.IsLinkVisibleAsync(link)).Should().BeFalse();

        await sideMenuPage.ClickEmissionHubAsync();
        foreach (var link in expectedEmissionHubLinks)
            (await sideMenuPage.IsLinkVisibleAsync(link)).Should().BeTrue();

        await sideMenuPage.ClickDataHubAsync();
        foreach (var link in expectedDataHubLinks)
            (await sideMenuPage.IsLinkVisibleAsync(link)).Should().BeTrue();
    }

    [Test]
    public async Task T02_SideMenu_DataStewardView()
    {
        try
        {
            await SetUserCapabilitiesAndLoginAsync(2);

            var sideMenuPage = new SideMenuPage(Fixture.Page);
            await sideMenuPage.OpenAsync();

            (await sideMenuPage.IsLinkVisibleAsync("Company Dashboard")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Data Hub")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Reports")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Dark Mode")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Logout")).Should().BeTrue();

            (await sideMenuPage.IsLinkVisibleAsync("Users")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Projects")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Units")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Emission Hub")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Emission Types")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Emission Categories")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Aliases")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Factor Import")).Should().BeFalse();

            await sideMenuPage.ClickDataHubAsync();
            (await sideMenuPage.IsLinkVisibleAsync("Invoice Upload")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Telemetry Upload")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Utility Bill Upload")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Factor Import")).Should().BeFalse();
        }
        finally
        {
            await SetUserCapabilitiesAndLoginAsync(1);
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

            (await sideMenuPage.IsLinkVisibleAsync("Company Dashboard")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Users")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Reports")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Dark Mode")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Logout")).Should().BeTrue();

            (await sideMenuPage.IsLinkVisibleAsync("Projects")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Units")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Emission Hub")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Emission Types")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Emission Categories")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Aliases")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Data Hub")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Invoice Upload")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Telemetry Upload")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Utility Bill Upload")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Factor Import")).Should().BeFalse();
        }
        finally
        {
            await SetUserCapabilitiesAndLoginAsync(1);
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

            (await sideMenuPage.IsLinkVisibleAsync("Company Dashboard")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Projects")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Reports")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Dark Mode")).Should().BeTrue();
            (await sideMenuPage.IsLinkVisibleAsync("Logout")).Should().BeTrue();

            (await sideMenuPage.IsLinkVisibleAsync("Users")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Units")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Emission Hub")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Emission Types")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Emission Categories")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Aliases")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Data Hub")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Invoice Upload")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Telemetry Upload")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Utility Bill Upload")).Should().BeFalse();
            (await sideMenuPage.IsLinkVisibleAsync("Factor Import")).Should().BeFalse();
        }
        finally
        {
            await SetUserCapabilitiesAndLoginAsync(1);
        }
    }

    [Test]
    public async Task T05_SideMenu_ClickCompanyDashboard()
    {
        const string expectedTitle = "Company Dashboard";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickCompanyDashboardAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T06_SideMenu_ClickProjects()
    {
        const string expectedTitle = "Projects";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickProjectsAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T07_SideMenu_ClickUsers()
    {
        const string expectedTitle = "Users";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickUsersAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T08_SideMenu_ClickUnits()
    {
        const string expectedTitle = "Units";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickUnitsAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T09_SideMenu_ClickEmissionTypes()
    {
        const string expectedTitle = "Emission Types";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickEmissionTypesAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T10_SideMenu_ClickEmissionCategories()
    {
        const string expectedTitle = "Emission Categories";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickEmissionCategoriesAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T11_SideMenu_ClickAliases()
    {
        const string expectedTitle = "Aliases";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickAliasesAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T12_SideMenu_ClickInvoiceUpload()
    {
        const string expectedTitle = "Invoice Upload";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickInvoiceUploadAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T13_SideMenu_ClickTelemetryUpload()
    {
        const string expectedTitle = "Telemetry Upload";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickTelemetryUploadAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T14_SideMenu_ClickUtilityBillUpload()
    {
        const string expectedTitle = "Utility Bill Upload";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickUtilityBillUploadAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T15_SideMenu_ClickFactorImport()
    {
        const string expectedTitle = "Factor Import";

        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickFactorImportAsync();

        (await sideMenuPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T16_SideMenu_ClickDarkMode()
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
    public async Task T17_SideMenu_ClickLogout()
    {
        var sideMenuPage = new SideMenuPage(Fixture.Page);
        await sideMenuPage.OpenAsync();
        var loginPage = await sideMenuPage.ClickLogoutAsync();

        (await loginPage.IsSignInBtnVisibleAsync()).Should().BeTrue();
    }
}
