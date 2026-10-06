using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class UtilityBillUploadTests : BaseTest
{
    [Test]
    public async Task T01_UtilityBillUpload_DefaultView()
    {
        const string expectedTitle = "Utility Bill Upload";
        const string expectedMessage = "Browse utility bills and edit drafts before approval.";

        var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
        await utilityBillUploadPage.OpenAsync();

        (await utilityBillUploadPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await utilityBillUploadPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await utilityBillUploadPage.IsImportPDFBtnVisibleAsync()).Should().BeTrue();
        (await utilityBillUploadPage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_UtilityBillUpload_ClickImportPdfBtn()
    {
        const string expectedTitle = "Import Utility Bill";

        var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
        await utilityBillUploadPage.OpenAsync();
        var importPage = await utilityBillUploadPage.ClickImportPDFBtnAsync();

        (await importPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_UtilityBillUpload_GridColumns()
    {
        var expectedColumns = new[]
        {
            "PROJECT",
            "COMPANY",
            "BILL DATE",
            "STATUS",
            "IMPORT DATE",
            "APPROVE/REJECT DATE",
            "SOURCE",
            "ACTIONS",
        };

        var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
        await utilityBillUploadPage.OpenAsync();

        (await utilityBillUploadPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T04_UtilityBillUpload_GridRecords()
    {
        var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
        await utilityBillUploadPage.OpenAsync();

        var expected = (await SqlHelper.GetUtilityBillGridRowsAsync())
            .Select(ToGridRow)
            .ToList();
        var actual = (await utilityBillUploadPage.GetAllGridRowsAsync())
            .Select(ToActualRow)
            .ToList();

        if (actual.Count != expected.Count)
        {
            static string Key((string Project, string Company, string Date, string Status, string ImportDate, string ApproveRejectDate, string Source) row) =>
                $"{row.Company}|{row.Date}|{row.Status}|{row.ImportDate}|{row.Source}";

            var actualKeys = actual.Select(Key).ToList();
            var expectedKeys = expected.Select(Key).ToList();
            var extra = actualKeys.Except(expectedKeys).Take(15).ToList();
            var missing = expectedKeys.Except(actualKeys).Take(15).ToList();
            throw new AssertionException(
                $"Grid has {actual.Count} rows, database has {expected.Count}. Missing: {string.Join(" || ", missing)}. Extra: {string.Join(" || ", extra)}.");
        }

        actual.Should().Equal(expected);
    }

    [Test]
    public async Task T05_UtilityBillUpload_GridSorting()
    {
        var columns = new[]
        {
            "PROJECT",
            "COMPANY",
            "BILL DATE",
            "STATUS",
            "IMPORT DATE",
            "APPROVE/REJECT DATE",
            "SOURCE",
        };

        var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
        await utilityBillUploadPage.OpenAsync();

        var bills = await SqlHelper.GetUtilityBillGridRowsAsync();

        foreach (var column in columns)
        {
            await utilityBillUploadPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(
                utilityBillUploadPage,
                ExpectedSortedBills(bills, column, ascending: true),
                $"{column} ASC");

            await utilityBillUploadPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(
                utilityBillUploadPage,
                ExpectedSortedBills(bills, column, ascending: false),
                $"{column} DESC");

            await utilityBillUploadPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(
                utilityBillUploadPage,
                ExpectedSortedBills(bills, "BILL DATE", ascending: false),
                $"{column} reset to BILL DATE DESC");
        }
    }

    [Test]
    public async Task T06_UtilityBillUpload_ClickViewBtn()
    {
        const string expectedTitle = "Utility Bill";

        var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
        await utilityBillUploadPage.OpenAsync();
        var viewPage = await utilityBillUploadPage.ClickViewBtnAsync(Config.SetupProjectName1);

        (await viewPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T07_UtilityBillUpload_ClickEditBtn()
    {
        const string expectedTitle = "Review PDF Import";

        var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
        await utilityBillUploadPage.OpenAsync();
        var editPage = await utilityBillUploadPage.ClickEditBtnAsync(Config.SetupProjectName1);

        (await editPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T08_UtilityBillUpload_Rejected_View()
    {
        const string billDate = "Jan 1, 2022";
        const string project = Config.SetupProjectName1;
        const string company = Config.SetupCompanyName1;
        var billDateValue = new DateTime(2022, 1, 1);
        const string expectedStatus = "REJECTED";

        try
        {
            var updatedAt = await SqlHelper.SetUtilityBillRejectedAsync(project, company, billDateValue, "123");
            var expectedApproveRejectDates = BuildApproveRejectDateCandidates(updatedAt);

            var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
            await utilityBillUploadPage.OpenAsync();

            (await utilityBillUploadPage.IsEditBtnVisibleForBillAsync(billDate, project, company, expectedStatus)).Should().BeFalse();
            (await utilityBillUploadPage.IsViewBtnVisibleForBillAsync(billDate, project, company, expectedStatus)).Should().BeTrue();

            var gridRow = await utilityBillUploadPage.FindGridRowAsync(billDate, project, company, status: expectedStatus);
            gridRow.Should().NotBeNull();
            gridRow!.Status.Should().Be(expectedStatus);
            expectedApproveRejectDates.Should().Contain(gridRow.ApproveRejectDate);
        }
        finally
        {
            await SqlHelper.SetUtilityBillDraftAsync(project, company, billDateValue);
        }
    }

    [Test]
    public async Task T09_UtilityBillUpload_Approved_View()
    {
        const string billDate = "Jan 1, 2022";
        const string project = Config.SetupProjectName1;
        const string company = Config.SetupCompanyName1;
        var billDateValue = new DateTime(2022, 1, 1);
        const string expectedStatus = "APPROVED";

        try
        {
            var approvedAt = await SqlHelper.SetUtilityBillApprovedAsync(project, company, billDateValue);
            var expectedApproveRejectDates = BuildApproveRejectDateCandidates(approvedAt);

            var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
            await utilityBillUploadPage.OpenAsync();

            (await utilityBillUploadPage.IsEditBtnVisibleForBillAsync(billDate, project, company, expectedStatus)).Should().BeFalse();
            (await utilityBillUploadPage.IsViewBtnVisibleForBillAsync(billDate, project, company, expectedStatus)).Should().BeTrue();

            var gridRow = await utilityBillUploadPage.FindGridRowAsync(billDate, project, company, status: expectedStatus);
            gridRow.Should().NotBeNull();
            gridRow!.Status.Should().Be(expectedStatus);
            expectedApproveRejectDates.Should().Contain(gridRow.ApproveRejectDate);
        }
        finally
        {
            await SqlHelper.SetUtilityBillDraftAsync(project, company, billDateValue);
        }
    }

    private static async Task AssertCurrentPageSortedAsync(
        UtilityBillUploadPage utilityBillUploadPage,
        IReadOnlyList<(string Project, string Company, string Date, string Status, string ImportDate, string ApproveRejectDate, string Source)> expected,
        string because)
    {
        var actual = (await utilityBillUploadPage.GetCurrentPageGridRowsAsync())
            .Select(ToActualRow)
            .ToList();
        actual.Should().Equal(expected.Take(actual.Count), because);
    }

    private static List<(string Project, string Company, string Date, string Status, string ImportDate, string ApproveRejectDate, string Source)> ExpectedSortedBills(
        IReadOnlyList<UtilityBillGridDbRow> bills,
        string column,
        bool ascending)
    {
        IOrderedEnumerable<UtilityBillGridDbRow> ordered = column switch
        {
            "PROJECT" => OrderByText(bills, row => row.Project, ascending),
            "COMPANY" => OrderByText(bills, row => row.Company, ascending),
            "BILL DATE" => OrderByDate(bills, row => row.BillDate, ascending),
            "STATUS" => OrderByText(bills, row => row.Status, ascending),
            "IMPORT DATE" => OrderByDate(bills, row => row.ImportDate, ascending),
            "APPROVE/REJECT DATE" => OrderByDate(bills, row => row.ApproveRejectDate, ascending),
            "SOURCE" => OrderByText(bills, row => FormatSource(row.Source), ascending),
            _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Unknown utility bill grid column."),
        };

        return ordered.Select(ToGridRow).ToList();
    }

    private static IOrderedEnumerable<UtilityBillGridDbRow> OrderByText(
        IReadOnlyList<UtilityBillGridDbRow> bills,
        Func<UtilityBillGridDbRow, string> key,
        bool ascending) =>
        ascending
            ? bills.OrderBy(key, StringComparer.OrdinalIgnoreCase).ThenByDescending(row => row.BillDate ?? DateTime.MinValue).ThenByDescending(row => row.ImportDate ?? DateTime.MinValue).ThenByDescending(row => row.Id)
            : bills.OrderByDescending(key, StringComparer.OrdinalIgnoreCase).ThenByDescending(row => row.BillDate ?? DateTime.MinValue).ThenByDescending(row => row.ImportDate ?? DateTime.MinValue).ThenByDescending(row => row.Id);

    private static IOrderedEnumerable<UtilityBillGridDbRow> OrderByDate(
        IReadOnlyList<UtilityBillGridDbRow> bills,
        Func<UtilityBillGridDbRow, DateTime?> key,
        bool ascending) =>
        ascending
            ? bills.OrderBy(row => key(row) ?? DateTime.MinValue).ThenByDescending(row => row.BillDate ?? DateTime.MinValue).ThenByDescending(row => row.ImportDate ?? DateTime.MinValue).ThenByDescending(row => row.Id)
            : bills.OrderByDescending(row => key(row) ?? DateTime.MinValue).ThenByDescending(row => row.BillDate ?? DateTime.MinValue).ThenByDescending(row => row.ImportDate ?? DateTime.MinValue).ThenByDescending(row => row.Id);

    private static (string Project, string Company, string Date, string Status, string ImportDate, string ApproveRejectDate, string Source) ToGridRow(
        UtilityBillGridDbRow row) =>
        (
            FormatEmpty(row.Project),
            FormatEmpty(row.Company),
            FormatDate(row.BillDate),
            row.Status.ToUpperInvariant(),
            FormatDate(row.ImportDate),
            FormatDate(row.ApproveRejectDate),
            FormatSource(row.Source)
        );

    private static (string Project, string Company, string Date, string Status, string ImportDate, string ApproveRejectDate, string Source) ToActualRow(
        UtilityBillGridRow row) =>
        (
            row.Project,
            row.Company,
            row.Date,
            row.Status.ToUpperInvariant(),
            row.ImportDate,
            row.ApproveRejectDate,
            row.Source
        );

    private static string FormatDate(DateTime? value) =>
        value?.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) ?? "—";

    private static string FormatEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value;

    private static string FormatSource(string source) =>
        string.IsNullOrWhiteSpace(source) ? "Manual" : source;

    private static string[] BuildApproveRejectDateCandidates(DateTime source)
    {
        return new[]
            {
                source,
                source.ToUniversalTime(),
                source.AddDays(1),
                source.ToUniversalTime().AddDays(1),
                DateTime.Now,
                DateTime.UtcNow,
                DateTime.Now.AddDays(1),
                DateTime.UtcNow.AddDays(1),
                DateTime.Now.AddDays(-1),
                DateTime.UtcNow.AddDays(-1),
            }
            .Select(d => d.ToString("MMM d, yyyy", CultureInfo.InvariantCulture))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}
