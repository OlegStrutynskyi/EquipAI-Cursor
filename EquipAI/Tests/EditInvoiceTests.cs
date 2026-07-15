using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class EditInvoiceTests : BaseTest
{
    [Test]
    public async Task T01_EditInvoice_DefaultView()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedTitle = "Edit invoice";
        const string expectedMessage = "Review and edit this draft invoice, then approve or reject.";
        const string expectedCompany = Config.SetupCompanyName1;
        const string expectedAddress = Config.SetupInvoiceAddress1;
        const string expectedProject = Config.SetupProjectName1;
        const string expectedInvoiceDate = "2026-06-02";
        const string expectedCategory = "Fuel";
        const string expectedTotalCost = "1562.99";
        const string expectedCurrency = "USD";
        const string expectedDescription = Config.SetupInvoiceLineDescription1;
        const string expectedQuantity = "421.29";
        const string expectedUnitPrice = "3.71";
        const string expectedCost = "1562.99";
        const string expectedEmissionType = "On-site diesel combustion";
        const string expectedUnit = "US Gallon (US_GAL)";

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);

        (await editInvoicePage.GetTitleAsync()).Should().Be(expectedTitle);
        (await editInvoicePage.GetMessageAsync()).Should().Be(expectedMessage);
        (await editInvoicePage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await editInvoicePage.IsApproveBtnVisibleAsync()).Should().BeTrue();
        (await editInvoicePage.IsRejectBtnVisibleAsync()).Should().BeTrue();
        (await editInvoicePage.IsCancelBtnVisibleAsync()).Should().BeTrue();
        (await editInvoicePage.IsSaveAsDraftBtnVisibleAsync()).Should().BeTrue();

        // Header section
        (await editInvoicePage.GetInvoiceNumberAsync()).Should().Be(invoiceNumber);
        (await editInvoicePage.GetCompanyNameAsync()).Should().Be(expectedCompany);
        (await editInvoicePage.GetAddressAsync()).Should().Be(expectedAddress);
        (await editInvoicePage.GetProjectAsync()).Should().Be(expectedProject);
        (await editInvoicePage.GetInvoiceDateAsync()).Should().Be(expectedInvoiceDate);
        (await editInvoicePage.GetInvoiceCategoryAsync()).Should().Be(expectedCategory);
        (await editInvoicePage.GetTotalCostAsync()).Should().Be(expectedTotalCost);
        (await editInvoicePage.GetCurrencyAsync()).Should().Be(expectedCurrency);

        // Line items section
        (await editInvoicePage.IsAddRowBtnVisibleAsync()).Should().BeTrue();
        (await editInvoicePage.IsAddRowBtnEnabledAsync()).Should().BeTrue();
        (await editInvoicePage.GetDescription1Async()).Should().Be(expectedDescription);
        (await editInvoicePage.GetQuantity1Async()).Should().Be(expectedQuantity);
        (await editInvoicePage.GetUnitPrice1Async()).Should().Be(expectedUnitPrice);
        (await editInvoicePage.GetCost1Async()).Should().Be(expectedCost);
        (await editInvoicePage.GetEmissionType1Async()).Should().Be(expectedEmissionType);
        (await editInvoicePage.GetUnit1Async()).Should().Be(expectedUnit);
        (await editInvoicePage.IsRemoveRowBtnVisibleAsync(1)).Should().BeTrue();
        (await editInvoicePage.IsRemoveRowBtnDisabledAsync(1)).Should().BeTrue();
    }

    [Test]
    public async Task T02_EditInvoice_ClickBackBtn()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedTitle = "Invoices";

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);
        invoicesPage = await editInvoicePage.ClickBackBtnAsync();

        (await invoicesPage.GetTitleAsync()).Should().Be(expectedTitle);
    }
}
