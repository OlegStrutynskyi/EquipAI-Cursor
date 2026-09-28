using System.Globalization;
using System.Linq;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class InvoicesTests : BaseTest
{
    [Test]
    public async Task T01_Invoices_DefaultView()
    {
        const string expectedTitle = "Invoice Upload";
        const string expectedMessage = "Browse invoices and edit drafts before approval.";

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();

        (await invoicesPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await invoicesPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await invoicesPage.IsImportCSVBtnVisibleAsync()).Should().BeTrue();
        (await invoicesPage.IsImportPDFBtnVisibleAsync()).Should().BeTrue();
        (await invoicesPage.IsCreateBtnVisibleAsync()).Should().BeTrue();
        (await invoicesPage.IsInvoicesGridVisibleAsync()).Should().BeTrue();
    }
        
    [Test]
    public async Task T02_Invoices_ClickImportCSVBtn()
    {
        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var importPage = await invoicesPage.ClickImportCSVBtnAsync();

        (await importPage.IsImportTitleVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T03_Invoices_ClickImportPDFBtn()
    {
        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var importPage = await invoicesPage.ClickImportPDFBtnAsync();

        (await importPage.IsImportTitleVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T04_Invoices_ClickCreateBtn()
    {
        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var createInvoicePage = await invoicesPage.ClickCreateBtnAsync();

        (await createInvoicePage.IsCreateInvoiceTitleVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T05_Invoices_GridColumns()
    {
        var expectedColumns = new[]
        {
            "PROJECT",
            "INVOICE NUMBER",
            "COMPANY",
            "INVOICE DATE",
            "STATUS",
            "IMPORT DATE",
            "APPROVE/REJECT DATE",
            "SOURCE",
            "ACTIONS",
        };

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();

        (await invoicesPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T06_Invoices_Manual_Data()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedProject = Config.SetupProjectName1;
        const string expectedCompany = Config.SetupCompanyName1;
        const string expectedInvoiceDate = "Jun 2, 2026";
        const string expectedStatus = "DRAFT";
        const string expectedSource = "Manual";

        await SqlHelper.SetInvoiceDraftAsync(invoiceNumber);

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();

        var gridRow = await invoicesPage.GetInvoiceGridRowAsync(invoiceNumber);
        gridRow.Should().NotBeNull();
        gridRow!.Project.Should().Be(expectedProject);
        gridRow.InvoiceNumber.Should().Be(invoiceNumber);
        gridRow.Company.Should().Be(expectedCompany);
        gridRow.Date.Should().Be(expectedInvoiceDate);
        gridRow.Status.Should().Be(expectedStatus);
        gridRow.Source.Should().Be(expectedSource);
    }

    [Test]
    public async Task T07_Invoices_ClickViewBtn()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var viewInvoicePage = await invoicesPage.ClickViewBtnAsync(invoiceNumber);

        (await viewInvoicePage.GetTitleAsync()).Should().Contain(invoiceNumber);
    }

    [Test]
    public async Task T08_Invoices_ClickEditBtn()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedTitle = "Edit Invoice";

        await SqlHelper.SetInvoiceDraftAsync(invoiceNumber);

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);

        (await editInvoicePage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T09_Invoices_Rejected_View()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedStatus = "REJECTED";

        try
        {
            var updatedAt = await SqlHelper.SetInvoiceRejectedAsync(invoiceNumber, "123");
            var expectedApproveRejectDates = BuildApproveRejectDateCandidates(updatedAt);

            var invoicesPage = new InvoicesPage(Fixture.Page);
            await invoicesPage.OpenAsync();

            (await invoicesPage.IsEditBtnVisibleForInvoiceAsync(invoiceNumber)).Should().BeFalse();
            (await invoicesPage.IsViewBtnVisibleForInvoiceAsync(invoiceNumber)).Should().BeTrue();

            var gridRow = await invoicesPage.GetInvoiceGridRowAsync(invoiceNumber);
            gridRow.Should().NotBeNull();
            gridRow!.Status.Should().Be(expectedStatus);
            expectedApproveRejectDates.Should().Contain(gridRow.ApproveRejectDate);
        }
        finally
        {
            await SqlHelper.SetInvoiceDraftAsync(invoiceNumber);
        }
    }

    [Test]
    public async Task T10_Invoices_Approved_View()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedStatus = "APPROVED";

        try
        {
            var approvedAt = await SqlHelper.SetInvoiceApprovedAsync(invoiceNumber);
            var expectedApproveRejectDates = BuildApproveRejectDateCandidates(approvedAt);

            var invoicesPage = new InvoicesPage(Fixture.Page);
            await invoicesPage.OpenAsync();

            (await invoicesPage.IsEditBtnVisibleForInvoiceAsync(invoiceNumber)).Should().BeFalse();
            (await invoicesPage.IsViewBtnVisibleForInvoiceAsync(invoiceNumber)).Should().BeTrue();

            var gridRow = await invoicesPage.GetInvoiceGridRowAsync(invoiceNumber);
            gridRow.Should().NotBeNull();
            gridRow!.Status.Should().Be(expectedStatus);
            expectedApproveRejectDates.Should().Contain(gridRow.ApproveRejectDate);
        }
        finally
        {
            await SqlHelper.SetInvoiceDraftAsync(invoiceNumber);
        }
    }

    [Test]
    public async Task T11_Invoices_InvoicesCount()
    {
        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();

        var totalFromPaginationSummary = await invoicesPage.GetTotalInvoiceCountFromPaginationSummaryAsync();
        var totalFromGrid = await invoicesPage.GetInvoiceNumberCountFromAllPagesAsync();

        totalFromGrid.Should().Be(totalFromPaginationSummary);
    }

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
