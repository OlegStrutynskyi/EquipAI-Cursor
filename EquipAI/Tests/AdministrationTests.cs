using EquipAI.Pages;
using FluentAssertions;

namespace EquipAI.Tests;

public class AdministrationTests : BaseTest
{
    [Test]
    public async Task T01_Admin_DefaultView()
    {
        const string expectedTitle = "Administration";
        const string expectedMessage = "Full platform administration";

        var administrationPage = new AdministrationPage(Fixture.Page);
        await administrationPage.OpenAsync();

        (await administrationPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await administrationPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await administrationPage.IsUsersTabVisibleAsync()).Should().BeTrue();
        (await administrationPage.IsProjectsTabVisibleAsync()).Should().BeTrue();
        (await administrationPage.IsUnitsTabVisibleAsync()).Should().BeTrue();
        (await administrationPage.IsEmissionTypesTabVisibleAsync()).Should().BeTrue();
        (await administrationPage.IsAliasesTabVisibleAsync()).Should().BeTrue();
        (await administrationPage.IsFactorImportTabVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Admin_ClickUsersTab()
    {
        const string expectedPageTitle = "Users";

        var administrationPage = new AdministrationPage(Fixture.Page);
        await administrationPage.OpenAsync();
        await administrationPage.ClickUsersTabAsync();

        (await administrationPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T03_Admin_ClickProjectsTab()
    {
        const string expectedPageTitle = "Projects";

        var administrationPage = new AdministrationPage(Fixture.Page);
        await administrationPage.OpenAsync();
        await administrationPage.ClickProjectsTabAsync();

        (await administrationPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T04_Admin_ClickUnitsTab()
    {
        const string expectedPageTitle = "Units";

        var administrationPage = new AdministrationPage(Fixture.Page);
        await administrationPage.OpenAsync();
        await administrationPage.ClickUnitsTabAsync();

        (await administrationPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T05_Admin_ClickEmissionTypesTab()
    {
        const string expectedPageTitle = "Emission types";

        var administrationPage = new AdministrationPage(Fixture.Page);
        await administrationPage.OpenAsync();
        await administrationPage.ClickEmissionTypesTabAsync();

        (await administrationPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T06_Admin_ClickAliasesTab()
    {
        const string expectedPageTitle = "Aliases";

        var administrationPage = new AdministrationPage(Fixture.Page);
        await administrationPage.OpenAsync();
        await administrationPage.ClickAliasesTabAsync();

        (await administrationPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T07_Admin_ClickFactorImportTab()
    {
        const string expectedPageTitle = "Factor import";

        var administrationPage = new AdministrationPage(Fixture.Page);
        await administrationPage.OpenAsync();
        await administrationPage.ClickFactorImportTabAsync();

        (await administrationPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }
}
