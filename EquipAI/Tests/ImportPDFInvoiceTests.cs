using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class ImportPDFInvoiceTests : BaseTest
{
    [Test]
    public async Task T01_ImportPDFInvoice_DefaultView()
    {
        const string expectedTitle = "Import Invoice PDF";
        const string expectedMessage =
            "Upload a single PDF invoice. It is read automatically and appears in the list as a draft invoice once processing finishes.";

        var importPDFInvoicePage = new ImportPDFInvoicePage(Fixture.Page);
        await importPDFInvoicePage.OpenAsync();

        (await importPDFInvoicePage.GetTitleAsync()).Should().Be(expectedTitle);
        (await importPDFInvoicePage.GetMessageAsync()).Should().Be(expectedMessage);
        (await importPDFInvoicePage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await importPDFInvoicePage.IsPdfFileLabelVisibleAsync()).Should().BeTrue();
        (await importPDFInvoicePage.IsImportSectionVisibleAsync()).Should().BeTrue();
        (await importPDFInvoicePage.IsImportBtnVisibleAsync()).Should().BeTrue();
        (await importPDFInvoicePage.IsImportBtnEnabledAsync()).Should().BeFalse();
    }

    [Test]
    public async Task T02_ImportPDFInvoice_ClickBackBtn()
    {
        const string expectedTitle = "Invoices";

        var importPDFInvoicePage = new ImportPDFInvoicePage(Fixture.Page);
        await importPDFInvoicePage.OpenAsync();
        var invoicesPage = await importPDFInvoicePage.ClickBackBtnAsync();

        (await invoicesPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_ImportPDFInvoice_IncorrectFormat()
    {
        const string expectedAlertMessage = "Only .pdf files are accepted.";

        var importPDFInvoicePage = new ImportPDFInvoicePage(Fixture.Page);
        await importPDFInvoicePage.OpenAsync();
        await importPDFInvoicePage.UploadFileAsync("scv-invoice-correct.csv");
        await importPDFInvoicePage.ClickImportBtnAsync();

        (await importPDFInvoicePage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    [CancelAfter(660_000)]
    public async Task T04_ImportPDFInvoice_Success()
    {
        const string fileName = "SC_Fuels_3.pdf";
        const string invoiceNumber = "IN-0000103090";
        const string expectedToasterMessage = "SC_Fuels_3.pdf has been uploaded and is being processed";
        var expectedImportDate = DateTime.Now.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

        try
        {
            await SqlHelper.DeleteImportedInvoiceByInvoiceNumberAsync(invoiceNumber);
            await SqlHelper.DeletePdfBlobByFileAsync(fileName);
            await SqlHelper.DeleteActivitySourceByPdfFileAsync(fileName);

            var importPDFInvoicePage = new ImportPDFInvoicePage(Fixture.Page);
            await importPDFInvoicePage.OpenAsync();
            await importPDFInvoicePage.ImportPdfAsync(fileName, expectedToasterMessage);

            var invoicesPage = new InvoicesPage(Fixture.Page);
            var deadline = DateTime.UtcNow.AddMinutes(10);
            InvoiceGridRow? gridRow = null;

            while (DateTime.UtcNow < deadline)
            {
                await invoicesPage.OpenAsync();
                gridRow = await invoicesPage.GetInvoiceGridRowAsync(invoiceNumber);
                if (gridRow is not null
                    && gridRow.InvoiceNumber.Equals(invoiceNumber, StringComparison.Ordinal)
                    && gridRow.ImportDate.Equals(expectedImportDate, StringComparison.Ordinal))
                {
                    break;
                }

                gridRow = null;
                await Task.Delay(TimeSpan.FromSeconds(10));
            }

            gridRow.Should().NotBeNull(
                $"Invoice '{invoiceNumber}' with Import Date '{expectedImportDate}' was not found within 10 minutes.");
            gridRow!.InvoiceNumber.Should().Be(invoiceNumber);
            gridRow.ImportDate.Should().Be(expectedImportDate);
        }
        finally
        {
            await SqlHelper.DeleteImportedInvoiceByInvoiceNumberAsync(invoiceNumber);
            await SqlHelper.DeletePdfBlobByFileAsync(fileName);
            await SqlHelper.DeleteActivitySourceByPdfFileAsync(fileName);
        }
    }

    [Test]
    public async Task T05_ImportPDFInvoice_DuplicatedFile()
    {
        const string invoiceNumber = "IN-0000103090";
        const string fileName = "SC_Fuels_3.pdf";
        const string expectedToasterMessage = "SC_Fuels_3.pdf has been uploaded and is being processed";
        const string expectedAlertMessage = "A PDF with the same content has already been imported.";

        try
        {
            await SqlHelper.DeleteImportedInvoiceByInvoiceNumberAsync(invoiceNumber);
            await SqlHelper.DeletePdfBlobByFileAsync(fileName);
            await SqlHelper.DeleteActivitySourceByPdfFileAsync(fileName);

            var importPDFInvoicePage = new ImportPDFInvoicePage(Fixture.Page);
            await importPDFInvoicePage.OpenAsync();
            await importPDFInvoicePage.ImportPdfAsync(fileName, expectedToasterMessage);

            var invoicesPage = new InvoicesPage(Fixture.Page);
            await invoicesPage.OpenAsync();
            var importPDFInvoicePage2 = await invoicesPage.ClickImportPDFBtnAsync();
            await importPDFInvoicePage2.UploadFileAsync(fileName);
            await importPDFInvoicePage2.ClickImportBtnAsync();

            (await importPDFInvoicePage2.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
        }
        finally
        {
            await SqlHelper.DeleteImportedInvoiceByInvoiceNumberAsync(invoiceNumber);
            await SqlHelper.DeletePdfBlobByFileAsync(fileName);
            await SqlHelper.DeleteActivitySourceByPdfFileAsync(fileName);
        }
    }
}
