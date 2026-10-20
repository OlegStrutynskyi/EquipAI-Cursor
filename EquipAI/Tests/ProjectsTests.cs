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
    public async Task T03_Projects_Grid_Columns()
    {
        var expectedColumns = new[]
        {
            "ICON",
            "CODE",
            "NAME",
            "ADDRESS",
            "START DATE",
            "STATUS",
            "ACTIONS",
        };

        var projectsPage = new ProjectsPage(Fixture.Page);
        await projectsPage.OpenAsync();

        (await projectsPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T04_Projects_Grid_Records()
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

    [Test]
    public async Task T05_Projects_Grid_Sorting()
    {
        var columns = new[] { "CODE", "NAME", "ADDRESS", "START DATE", "STATUS" };

        var projectsPage = new ProjectsPage(Fixture.Page);
        await projectsPage.OpenAsync();

        var projects = await SqlHelper.GetProjectGridRowsAsync();
        if (projects.Count == 0)
            Assert.Pass("No Projects");

        foreach (var column in columns)
        {
            await projectsPage.ClickGridColumnAsync(column);
            await AssertFirstPageSortedAsync(projectsPage, projects, column, ascending: true);

            await projectsPage.ClickGridColumnAsync(column);
            await AssertFirstPageSortedAsync(projectsPage, projects, column, ascending: false);

            await projectsPage.ClickGridColumnAsync(column);
            await AssertFirstPageSortedAsync(projectsPage, projects, "CODE", ascending: true);
        }
    }

    [Test]
    public async Task T06_Projects_Grid_Search()
    {
        var projectsPage = new ProjectsPage(Fixture.Page);
        await projectsPage.OpenAsync();

        (await projectsPage.IsSearchInputVisibleAsync()).Should().BeTrue();
        (await projectsPage.GetSearchPlaceholderAsync()).Should().Be("Search projects or data...");

        await AssertProjectSearchAsync(projectsPage, RandomProjectSearch());
        await AssertProjectSearchAsync(projectsPage, "test");
        await AssertProjectSearchAsync(projectsPage, "123456");
    }

    [Test]
    public async Task T07_Projects_ClickProjectName()
    {
        var projects = await SqlHelper.GetProjectGridRowsAsync();
        if (projects.Count == 0)
            Assert.Pass("No Projects");

        var projectsPage = new ProjectsPage(Fixture.Page);
        await projectsPage.OpenAsync();

        var projectName = await projectsPage.GetFirstRowNameAsync();
        var projectDashboardPage = await projectsPage.ClickProjectNameAsync(projectName);

        (await projectDashboardPage.GetPageTitleAsync()).Should().Be($"{projectName} Overview");
    }

    [Test]
    public async Task T08_Projects_ClickOfficesBtn()
    {
        var projects = await SqlHelper.GetProjectsWithOfficesAsync();
        if (projects.Count == 0)
            Assert.Pass("There are no offices.");

        var projectsPage = new ProjectsPage(Fixture.Page);
        await projectsPage.OpenAsync();

        var rows = await projectsPage.GetOfficesButtonRowsAsync();
        var expectedNames = projects.Select(project => project.Name).Distinct(StringComparer.Ordinal).ToList();
        foreach (var row in rows)
            row.HasOfficesButton.Should().Be(expectedNames.Contains(row.Name), $"project '{row.Name}'");

        rows.Select(row => row.Name).Should().Contain(expectedNames);

        foreach (var project in projects)
        {
            await projectsPage.OpenAsync();
            var officesPage = await projectsPage.ClickOfficesBtnAsync(project.Code, project.Name);

            (await officesPage.GetPageTitleAsync()).Should().Be("Offices");
            (await officesPage.GetSubtitleAsync()).Should().Be($"{project.Code} — {project.Name}");
        }
    }

    private static async Task AssertFirstPageSortedAsync(
        ProjectsPage projectsPage,
        IReadOnlyList<ProjectGridRow> projects,
        string column,
        bool ascending)
    {
        var actual = await projectsPage.GetCurrentPageGridRecordsAsync();
        var expected = ExpectedSortedRecords(projects, column, ascending).Take(actual.Count).ToList();
        actual.Should().Equal(expected, $"{column} {(ascending ? "ASC" : "DESC")}");
    }

    private static string FormatSynchDate(DateTime value) => ProjectsPage.FormatSynchDate(value);

    private static string FormatProjectStartDate(DateTime value) =>
        value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    private static string RandomProjectSearch()
    {
        var chars = GenerateRandomAlphanumericString(10).ToCharArray();
        chars[0] = (char)('a' + Random.Shared.Next(26));
        chars[1] = (char)('0' + Random.Shared.Next(10));
        return new string(chars);
    }

    private static async Task AssertProjectSearchAsync(ProjectsPage projectsPage, string term)
    {
        await projectsPage.SearchAsync(term);

        var expected = (await SqlHelper.GetProjectGridRowsAsync())
            .Where(row => ProjectMatchesSearch(row, term))
            .Select(project => new ProjectGridRecord(
                project.Code,
                project.Name,
                project.Address,
                project.StartDate is null ? "—" : FormatProjectStartDate(project.StartDate.Value),
                project.IsActive ? "ACTIVE" : "INACTIVE"))
            .ToList();

        if (expected.Count == 0)
        {
            (await projectsPage.GetGridEmptyMessageAsync()).Should().Be("No projects match your search.");
            return;
        }

        var actual = await projectsPage.GetAllGridRecordsAsync();
        actual.Should().BeEquivalentTo(expected);
        actual.Should().OnlyContain(row => RowContains(row, term));
    }

    private static bool RowContains(ProjectGridRecord row, string term) =>
        string.Join(' ', row.Code, row.Name, row.Address)
            .Contains(term, StringComparison.OrdinalIgnoreCase);

    private static bool ProjectMatchesSearch(ProjectGridRow row, string term) =>
        string.Join(' ', row.Code, row.Name, row.Address)
            .Contains(term, StringComparison.OrdinalIgnoreCase);

    private static List<ProjectGridRecord> ExpectedSortedRecords(
        IReadOnlyList<ProjectGridRow> projects,
        string column,
        bool ascending)
    {
        var ordered = column switch
        {
            "CODE" => OrderByText(projects, project => project.Code, ascending, StringComparer.OrdinalIgnoreCase),
            "NAME" => OrderByText(projects, project => project.Name, ascending, HyphenInsensitiveComparer.Instance),
            "ADDRESS" => OrderByText(projects, project => project.Address, ascending, HyphenInsensitiveComparer.Instance),
            "STATUS" => OrderByText(projects, project => project.IsActive ? "ACTIVE" : "INACTIVE", ascending, StringComparer.OrdinalIgnoreCase),
            "START DATE" => ascending
                ? projects
                    .OrderBy(project => project.StartDate ?? DateTime.MinValue)
                    .ThenBy(project => project.Code, StringComparer.OrdinalIgnoreCase)
                : projects
                    .OrderByDescending(project => project.StartDate ?? DateTime.MinValue)
                    .ThenBy(project => project.Code, StringComparer.OrdinalIgnoreCase),
            _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Unknown projects grid column."),
        };

        return ordered
            .Select(project => new ProjectGridRecord(
                project.Code,
                project.Name,
                project.Address,
                project.StartDate is null ? "—" : FormatProjectStartDate(project.StartDate.Value),
                project.IsActive ? "ACTIVE" : "INACTIVE"))
            .ToList();
    }

    private static IEnumerable<ProjectGridRow> OrderByText(
        IReadOnlyList<ProjectGridRow> projects,
        Func<ProjectGridRow, string> key,
        bool ascending,
        StringComparer comparer)
    {
        return ascending
            ? projects
                .OrderBy(key, comparer)
                .ThenBy(project => project.Code, StringComparer.OrdinalIgnoreCase)
            : projects
                .OrderByDescending(key, comparer)
                .ThenBy(project => project.Code, StringComparer.OrdinalIgnoreCase);
    }

    private sealed class HyphenInsensitiveComparer : StringComparer
    {
        public static readonly HyphenInsensitiveComparer Instance = new();

        public override int Compare(string? x, string? y) =>
            string.Compare(StripHyphens(x), StripHyphens(y), StringComparison.OrdinalIgnoreCase);

        public override bool Equals(string? x, string? y) => Compare(x, y) == 0;

        public override int GetHashCode(string obj) =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(StripHyphens(obj));

        private static string StripHyphens(string? value) =>
            (value ?? string.Empty)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("‐", string.Empty, StringComparison.Ordinal)
                .Replace("‑", string.Empty, StringComparison.Ordinal)
                .Replace("–", string.Empty, StringComparison.Ordinal)
                .Replace("—", string.Empty, StringComparison.Ordinal);
    }
}
