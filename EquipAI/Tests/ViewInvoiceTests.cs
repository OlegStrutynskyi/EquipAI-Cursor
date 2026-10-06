using System.Globalization;
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
        const string companyName = Config.SetupCompanyName1;
        const string expectedGridStatus = "DRAFT";
        const string expectedViewStatus = "Draft";

        var header = await SqlHelper.GetInvoiceViewHeaderAsync(companyName, invoiceNumber);
        var lineItems = await SqlHelper.GetInvoiceViewLineItemsAsync(companyName, invoiceNumber);
        var invoiceDate = header.InvoiceDate?.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) ?? string.Empty;

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();

        var gridRow = await invoicesPage.GetInvoiceGridRowAsync(invoiceNumber);
        gridRow.Should().NotBeNull();
        var expectedSource = gridRow!.Source;
        gridRow.Project.Should().Be(header.Project);
        gridRow.InvoiceNumber.Should().Be(invoiceNumber);
        gridRow.Company.Should().Be(header.CompanyName);
        gridRow.Date.Should().Be(invoiceDate);
        gridRow.Status.Should().Be(expectedGridStatus);

        var viewInvoicePage = await invoicesPage.ClickViewBtnAsync(invoiceNumber);

        (await viewInvoicePage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await viewInvoicePage.GetTitleAsync()).Should().Be("Invoice " + invoiceNumber);
        (await viewInvoicePage.GetStatusAsync()).Should().Be(expectedViewStatus);
        (await viewInvoicePage.GetSourceAsync()).Should().Be(expectedSource);
        (await viewInvoicePage.IsEditDraftBtnVisibleAsync()).Should().BeTrue();

        (await viewInvoicePage.GetCompanyNameAsync()).Should().Be(header.CompanyName);
        (await viewInvoicePage.GetAddressAsync()).Should().Be(header.Address);
        (await viewInvoicePage.GetProjectAsync()).Should().Be(header.Project);
        (await viewInvoicePage.GetInvoiceDateAsync()).Should().Be(invoiceDate);
        (await viewInvoicePage.GetCategoryAsync()).Should().Be(header.EmissionCategory);
        (await viewInvoicePage.GetTotalAsync()).Should().Be(FormatTotal(header.Total));

        (await viewInvoicePage.GetLineItemRowCountAsync()).Should().Be(lineItems.Count);
        foreach (var lineItem in lineItems)
            (await viewInvoicePage.GetLineItemCellsAsync(lineItem.LinePosition)).Should().Equal(ToLineCells(lineItem));
    }

    private static string FormatTotal(string total)
    {
        var parts = total.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var cost))
            return $"{cost.ToString("N2", CultureInfo.GetCultureInfo("en-US"))} {parts[1]}";

        return total;
    }

    private static string[] ToLineCells(InvoiceViewLineItemRow lineItem) =>
    [
        lineItem.LinePosition.ToString(CultureInfo.InvariantCulture),
        lineItem.LineDescription,
        FormatAmount(lineItem.Quantity),
        FormatAmount(lineItem.UnitPrice),
        FormatAmount(lineItem.Cost),
        lineItem.EmissionType,
        lineItem.Unit,
    ];

    private static string FormatAmount(decimal? value) =>
        value?.ToString("N2", CultureInfo.GetCultureInfo("en-US")) ?? string.Empty;

    [Test]
    public async Task T02_ViewInvoice_ClickEditDraft()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedTitle = "Edit Invoice";

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
