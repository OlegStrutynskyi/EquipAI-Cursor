using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class ImportInvoiceTests : BaseTest
{
    [Test]
    public async Task T01_ImportInvoice_DefaultView()
    {
        const string expectedTitle = "Import Fuel Invoice";
        const string expectedMessage =
            "Upload a CSV file. After import you can edit the draft invoice before approval.";

        var importInvoicePage = new ImportInvoicePage(Fixture.Page);
        await importInvoicePage.OpenAsync();

        (await importInvoicePage.GetTitleAsync()).Should().Be(expectedTitle);
        (await importInvoicePage.GetMessageAsync()).Should().Be(expectedMessage);
        (await importInvoicePage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await importInvoicePage.IsCsvFileLabelVisibleAsync()).Should().BeTrue();
        (await importInvoicePage.IsImportSectionVisibleAsync()).Should().BeTrue();
        (await importInvoicePage.IsImportBtnVisibleAsync()).Should().BeTrue();
        (await importInvoicePage.IsImportBtnEnabledAsync()).Should().BeFalse();
    }

    [Test]
    public async Task T02_ImportInvoice_ClickBackBtn()
    {
        const string expectedTitle = "Invoices";

        var importInvoicePage = new ImportInvoicePage(Fixture.Page);
        await importInvoicePage.OpenAsync();
        var invoicesPage = await importInvoicePage.ClickBackBtnAsync();

        (await invoicesPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_ImportInvoice_OnlyHeaders()
    {
        var expectedAlertMessage = "CSV file must contain at least one line item row.";

        var importInvoicePage = new ImportInvoicePage(Fixture.Page);
        await importInvoicePage.OpenAsync();
        await importInvoicePage.UploadCsvFileAsync("scv-invoice-only-headers.csv");
        await importInvoicePage.ClickImportBtnAsync();

        var actualAlertMessage = (await importInvoicePage.GetAlertMessageAsync()).Trim();
        actualAlertMessage.Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T04_ImportInvoice_NoInvoiceNumber()
    {
        var expectedAlertMessage = "InvoiceNumber is required on row 1.";

        var importInvoicePage = new ImportInvoicePage(Fixture.Page);
        await importInvoicePage.OpenAsync();
        await importInvoicePage.UploadCsvFileAsync("scv-invoice-no-invoice-number.csv");
        await importInvoicePage.ClickImportBtnAsync();

        var actualAlertMessage = (await importInvoicePage.GetAlertMessageAsync()).Trim();
        actualAlertMessage.Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T05_ImportInvoice_NoProject()
    {
        var expectedAlertMessage = "ProjectCode is required on row 1.";

        var importInvoicePage = new ImportInvoicePage(Fixture.Page);
        await importInvoicePage.OpenAsync();
        await importInvoicePage.UploadCsvFileAsync("scv-invoice-no-project.csv");
        await importInvoicePage.ClickImportBtnAsync();

        var actualAlertMessage = (await importInvoicePage.GetAlertMessageAsync()).Trim();
        actualAlertMessage.Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T06_ImportInvoice_NoInvoiceDate()
    {
        await AssertImportAlertAsync(
            "scv-invoice-no-invoice-date.csv",
            "InvoiceDate is required on row 1.");
    }

    [Test]
    public async Task T07_ImportInvoice_NoCurrency()
    {
        await AssertImportAlertAsync(
            "scv-invoice-no-currency.csv",
            "CurrencyCode is required on row 1.");
    }

    [Test]
    public async Task T08_ImportInvoice_NoTotalCost()
    {
        await AssertImportAlertAsync(
            "scv-invoice-no-total-cost.csv",
            "TotalCost is required on row 1.");
    }

    [Test]
    public async Task T09_ImportInvoice_NoQuantity()
    {
        await AssertImportAlertAsync(
            "scv-invoice-no-quantity.csv",
            "Row 1: Quantity is required.");
    }

    [Test]
    public async Task T10_ImportInvoice_NoUnit()
    {
        await AssertImportAlertAsync(
            "scv-invoice-no-unit.csv",
            "Row 1: UnitOfMeasure is required.");
    }

    [Test]
    public async Task T11_ImportInvoice_NoEmissionType()
    {
        await AssertImportAlertAsync(
            "scv-invoice-no-emission-type.csv",
            "Row 1: EmissionTypeCode is required.");
    }

    [Test]
    public async Task T12_ImportInvoice_MultipleErrors()
    {
        await AssertImportAlertAsync(
            "scv-invoice-multiple-errors.csv",
            "ProjectCode is required on row 1. InvoiceDate is required on row 1. CurrencyCode is required on row 1. TotalCost is required on row 1. Row 2: Quantity is required. Row 3: UnitOfMeasure is required. Row 5: EmissionTypeCode is required.");
    }

    [Test]
    public async Task T13_ImportInvoice_Success()
    {
        const string invoiceNumber = "AUTOTEST-Import-1";
        const string expectedPageTitle = "Invoices";
        const string expectedProject = Config.SetupProjectName1;
        const string expectedCompany = "TestCompany1";
        const string expectedInvoiceDate = "Jan 7, 2026";
        const string expectedStatus = "DRAFT";
        const string expectedSource = "BulkCsv";
        var expectedImportDate = DateTime.Now.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

        try
        {
            await SqlHelper.DeleteImportedInvoiceByInvoiceNumberAsync(invoiceNumber);
            await SqlHelper.DeleteActivitySourceByCsvFileAsync("scv-invoice-correct.csv");

            var importInvoicePage = new ImportInvoicePage(Fixture.Page);
            await importInvoicePage.OpenAsync();
            var invoicesPage = await importInvoicePage.ImportCsvAsync("scv-invoice-correct.csv");

            (await invoicesPage.GetTitleAsync()).Should().Be(expectedPageTitle);

            var gridRow = await invoicesPage.GetInvoiceGridRowAsync(invoiceNumber);
            gridRow.Should().NotBeNull();
            gridRow!.Project.Should().Be(expectedProject);
            gridRow.InvoiceNumber.Should().Be(invoiceNumber);
            gridRow.Company.Should().Be(expectedCompany);
            gridRow.Date.Should().Be(expectedInvoiceDate);
            gridRow.Status.Should().Be(expectedStatus);
            gridRow.ImportDate.Should().Be(expectedImportDate);
            gridRow.Source.Should().Be(expectedSource);
        }
        finally
        {
            await SqlHelper.DeleteImportedInvoiceByInvoiceNumberAsync(invoiceNumber);
            await SqlHelper.DeleteActivitySourceByCsvFileAsync("scv-invoice-correct.csv");
        }
    }

    private async Task AssertImportAlertAsync(string fileName, string expectedAlertMessage)
    {
        var importInvoicePage = new ImportInvoicePage(Fixture.Page);
        await importInvoicePage.OpenAsync();
        await importInvoicePage.UploadCsvFileAsync(fileName);
        await importInvoicePage.ClickImportBtnAsync();

        var actualAlertMessage = (await importInvoicePage.GetAlertMessageAsync())
            .Replace("\r\n", "\n")
            .Trim();
        actualAlertMessage.Should().Be(expectedAlertMessage.Replace("\r\n", "\n").Trim());
    }
}
