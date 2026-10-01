using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class ProjectsTests : BaseTest
{
    [Test]
    public async Task T01_Projects_DefaultView()
    {
        const string expectedPageTitle = "Projects";

        var projectsPage = new ProjectsPage(Fixture.Page);
        await projectsPage.OpenAsync();

        (await projectsPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await projectsPage.IsSynchronizeFromProcoreBtnVisibleAsync()).Should().BeTrue();
        (await projectsPage.IsSynchronizeFromProcoreBtnEnabledAsync()).Should().BeTrue();
        (await projectsPage.IsSynchDateVisibleAsync()).Should().BeTrue();
        (await projectsPage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Projects_ClickSynchFromProcoreBtn()
    {
        const string synchronizingText = "Synchronizing from Procore...";

        var projectsPage = new ProjectsPage(Fixture.Page);
        await projectsPage.OpenAsync();
        await projectsPage.ClickSynchronizeFromProcoreBtnAsync();
        await projectsPage.WaitForSynchDateTextAsync(synchronizingText);

        (await projectsPage.GetSynchDateTextAsync()).Should().Be(synchronizingText);

        var actualSynchDate = await projectsPage.WaitForSynchDateChangedFromAsync(synchronizingText);
        var completedAt = DateTime.Now;
        var acceptedSynchDates = new[]
        {
            FormatSynchDate(completedAt.AddMinutes(-1)),
            FormatSynchDate(completedAt),
            FormatSynchDate(completedAt.AddMinutes(1)),
        };
        actualSynchDate.Should().BeOneOf(acceptedSynchDates);
    }

    [Test]
    public async Task T03_Projects_GridColumns()
    {
        var expectedColumns = new[]
        {
            "ICON",
            "CODE",
            "NAME",
            "ADDRESS",
            "START DATE",
            "STATUS",
        };

        var projectsPage = new ProjectsPage(Fixture.Page);
        await projectsPage.OpenAsync();

        (await projectsPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T04_Projects_GridRecords()
    {
        var projectsPage = new ProjectsPage(Fixture.Page);
        await projectsPage.OpenAsync();

        var projects = await SqlHelper.GetProjectGridRowsAsync();
        if (projects.Count == 0)
            Assert.Pass("No Projects");

        var actualRecords = await projectsPage.GetAllGridRecordsAsync();
        var expectedRecords = projects
            .Select(project => new ProjectGridRecord(
                project.Code,
                project.Name,
                project.Address,
                project.StartDate is null ? "—" : FormatProjectStartDate(project.StartDate.Value),
                project.IsActive ? "ACTIVE" : "INACTIVE"))
            .ToList();

        actualRecords.Should().BeEquivalentTo(expectedRecords);
    }

    private static string FormatSynchDate(DateTime value) =>
        value.ToString("d MMM yyyy, HH:mm", CultureInfo.GetCultureInfo("en-GB"));

    private static string FormatProjectStartDate(DateTime value) =>
        value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
}
