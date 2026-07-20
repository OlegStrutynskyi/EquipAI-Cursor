using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class ViewInvoiceTests : BaseTest
{
    [Test]
    public async Task T01_ViewInvoice_Manual_DefaultView()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedAddress = Config.SetupInvoiceAddress1;
        const string expectedCategory = "Fuel";
        const string expectedTotal = "1,562.99 USD";
        const string expectedDescription = Config.SetupInvoiceLineDescription1;
        const string expectedQty = "421.29";
        const string expectedUnitPrice = "3.71";
        const string expectedCost = "1562.99";
        const string expectedEmissionType = "On-site diesel combustion";
        const string expectedUnit = "US Gallon (US_GAL)";

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();

        var gridRow = await invoicesPage.GetInvoiceGridRowAsync(invoiceNumber);
        gridRow.Should().NotBeNull();
        var expectedProject = gridRow!.Project;
        var expectedInvoiceNumber = gridRow.InvoiceNumber;
        var expectedCompany = gridRow.Company;
        var expectedInvoiceDate = gridRow.Date;
        var expectedStatus = gridRow.Status;
        var expectedType = gridRow.Source;

        var viewInvoicePage = await invoicesPage.ClickViewBtnAsync(invoiceNumber);

        // Top section
        (await viewInvoicePage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await viewInvoicePage.GetTitleAsync()).Should().Be("Invoice " + expectedInvoiceNumber);
        (await viewInvoicePage.GetStatusAsync()).Should().Be(expectedStatus);
        (await viewInvoicePage.GetSourceAsync()).Should().Be(expectedType);
        (await viewInvoicePage.IsEditDraftBtnVisibleAsync()).Should().BeTrue();

        // Header section
        (await viewInvoicePage.GetCompanyNameAsync()).Should().Be(expectedCompany);
        (await viewInvoicePage.GetAddressAsync()).Should().Be(expectedAddress);
        (await viewInvoicePage.GetProjectAsync()).Should().Be(expectedProject);
        (await viewInvoicePage.GetInvoiceDateAsync()).Should().Be(expectedInvoiceDate);
        (await viewInvoicePage.GetCategoryAsync()).Should().Be(expectedCategory);
        (await viewInvoicePage.GetTotalAsync()).Should().Be(expectedTotal);

        // Line items
        (await viewInvoicePage.GetLineItemRowCountAsync()).Should().Be(1);
        (await viewInvoicePage.GetDescription1Async()).Should().Be(expectedDescription);
        (await viewInvoicePage.GetQuantity1Async()).Should().Be(expectedQty);
        (await viewInvoicePage.GetUnitPrice1Async()).Should().Be(expectedUnitPrice);
        (await viewInvoicePage.GetCost1Async()).Should().Be(expectedCost);
        (await viewInvoicePage.GetEmissionType1Async()).Should().Be(expectedEmissionType);
        (await viewInvoicePage.GetUnit1Async()).Should().Be(expectedUnit);
    }

    [Test]
    public async Task T02_ViewInvoice_ClickEditDraft()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedTitle = "Edit invoice";

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var viewInvoicePage = await invoicesPage.ClickViewBtnAsync(invoiceNumber);
        var editInvoicePage = await viewInvoicePage.ClickEditDraftBtnAsync();

        (await editInvoicePage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_ViewInvoice_Rejected_View()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedStatus = "Rejected";

        try
        {
            await SqlHelper.SetInvoiceRejectedAsync(invoiceNumber, "123");

            var invoicesPage = new InvoicesPage(Fixture.Page);
            await invoicesPage.OpenAsync();
            var viewInvoicePage = await invoicesPage.ClickViewBtnAsync(invoiceNumber);

            (await viewInvoicePage.GetStatusAsync()).Should().Be(expectedStatus);
            (await viewInvoicePage.IsEditDraftBtnVisibleAsync()).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.SetInvoiceDraftAsync(invoiceNumber);
        }
    }

    [Test]
    public async Task T04_ViewInvoice_Approved_View()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedStatus = "Approved";

        try
        {
            await SqlHelper.SetInvoiceApprovedAsync(invoiceNumber);

            var invoicesPage = new InvoicesPage(Fixture.Page);
            await invoicesPage.OpenAsync();
            var viewInvoicePage = await invoicesPage.ClickViewBtnAsync(invoiceNumber);

            (await viewInvoicePage.GetStatusAsync()).Should().Be(expectedStatus);
            (await viewInvoicePage.IsEditDraftBtnVisibleAsync()).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.SetInvoiceDraftAsync(invoiceNumber);
        }
    }
}
