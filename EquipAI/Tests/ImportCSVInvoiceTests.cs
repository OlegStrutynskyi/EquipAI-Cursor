using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class ImportCSVInvoiceTests : BaseTest
{
    [Test]
    public async Task T01_ImportCSVInvoice_DefaultView()
    {
        const string expectedTitle = "Import Invoice CSV";
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
    public async Task T02_ImportCSVInvoice_ClickBackBtn()
    {
        const string expectedTitle = "Invoices";

        var importInvoicePage = new ImportInvoicePage(Fixture.Page);
        await importInvoicePage.OpenAsync();
        var invoicesPage = await importInvoicePage.ClickBackBtnAsync();

        (await invoicesPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_ImportCSVInvoice_IncorrectFormat()
    {
        const string expectedAlertMessage = "Only .csv files are accepted.";

        var importInvoicePage = new ImportInvoicePage(Fixture.Page);
        await importInvoicePage.OpenAsync();
        await importInvoicePage.UploadCsvFileAsync("SC_Fuels_3.pdf");
        await importInvoicePage.ClickImportBtnAsync();

        var actualAlertMessage = (await importInvoicePage.GetAlertMessageAsync()).Trim();
        actualAlertMessage.Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T04_ImportCSVInvoice_OnlyHeaders()
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
    public async Task T05_ImportCSVInvoice_NoInvoiceNumber()
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
    public async Task T06_ImportCSVInvoice_NoProject()
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
    public async Task T07_ImportCSVInvoice_NoInvoiceDate()
    {
        await AssertImportAlertAsync(
            "scv-invoice-no-invoice-date.csv",
            "InvoiceDate is required on row 1.");
    }

    [Test]
    public async Task T08_ImportCSVInvoice_NoCurrency()
    {
        await AssertImportAlertAsync(
            "scv-invoice-no-currency.csv",
            "CurrencyCode is required on row 1.");
    }

    [Test]
    public async Task T09_ImportCSVInvoice_NoTotalCost()
    {
        await AssertImportAlertAsync(
            "scv-invoice-no-total-cost.csv",
            "TotalCost is required on row 1.");
    }

    [Test]
    public async Task T10_ImportCSVInvoice_NoQuantity()
    {
        await AssertImportAlertAsync(
            "scv-invoice-no-quantity.csv",
            "Row 1: Quantity is required.");
    }

    [Test]
    public async Task T11_ImportCSVInvoice_NoUnit()
    {
        await AssertImportAlertAsync(
            "scv-invoice-no-unit.csv",
            "Row 1: UnitOfMeasure is required.");
    }

    [Test]
    public async Task T12_ImportCSVInvoice_NoEmissionType()
    {
        await AssertImportAlertAsync(
            "scv-invoice-no-emission-type.csv",
            "Row 1: EmissionTypeCode is required.");
    }

    [Test]
    public async Task T13_ImportCSVInvoice_MultipleErrors()
    {
        await AssertImportAlertAsync(
            "scv-invoice-multiple-errors.csv",
            "ProjectCode is required on row 1. InvoiceDate is required on row 1. CurrencyCode is required on row 1. TotalCost is required on row 1. Row 2: Quantity is required. Row 3: UnitOfMeasure is required. Row 5: EmissionTypeCode is required.");
    }

    [Test]
    public async Task T14_ImportCSVInvoice_Success()
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

    [Test]
    public async Task T15_ImportCSVInvoice_DuplicatedFile()
    {
        const string invoiceNumber = "AUTOTEST-Import-1";
        const string fileName = "scv-invoice-correct.csv";
        const string expectedAlertMessage = "A CSV with the same content has already been imported.";

        try
        {
            await SqlHelper.DeleteImportedInvoiceByInvoiceNumberAsync(invoiceNumber);
            await SqlHelper.DeleteActivitySourceByCsvFileAsync(fileName);

            var importInvoicePage = new ImportInvoicePage(Fixture.Page);
            await importInvoicePage.OpenAsync();
            var invoicesPage = await importInvoicePage.ImportCsvAsync(fileName);
            var importInvoicePage2 = await invoicesPage.ClickImportCSVBtnAsync();
            await importInvoicePage2.UploadCsvFileAsync(fileName);
            await importInvoicePage2.ClickImportBtnAsync();

            (await importInvoicePage2.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
        }
        finally
        {
            await SqlHelper.DeleteImportedInvoiceByInvoiceNumberAsync(invoiceNumber);
            await SqlHelper.DeleteActivitySourceByCsvFileAsync(fileName);
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
