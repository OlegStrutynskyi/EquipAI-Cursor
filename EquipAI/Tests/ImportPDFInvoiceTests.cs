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

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenPdfAsync();

        (await importPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await importPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await importPage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await importPage.IsPdfFileLabelVisibleAsync()).Should().BeTrue();
        (await importPage.IsImportSectionVisibleAsync()).Should().BeTrue();
        (await importPage.IsImportBtnVisibleAsync()).Should().BeTrue();
        (await importPage.IsImportBtnEnabledAsync()).Should().BeFalse();
    }

    [Test]
    public async Task T02_ImportPDFInvoice_ClickBackBtn()
    {
        const string expectedTitle = "Invoice Upload";

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenPdfAsync();
        var invoicesPage = await importPage.ClickBackToInvoicesAsync();

        (await invoicesPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_ImportPDFInvoice_IncorrectFormat()
    {
        const string expectedAlertMessage = "Only .pdf files are accepted.";

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenPdfAsync();
        await importPage.UploadPdfFileAsync("scv-invoice-correct.csv");
        await importPage.ClickImportBtnAsync();

        (await importPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
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

            var importPage = new ImportPage(Fixture.Page);
            await importPage.OpenPdfAsync();
            await importPage.ImportPdfAsync(fileName, expectedToasterMessage);

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

            var importPage = new ImportPage(Fixture.Page);
            await importPage.OpenPdfAsync();
            await importPage.ImportPdfAsync(fileName, expectedToasterMessage);

            var invoicesPage = new InvoicesPage(Fixture.Page);
            await invoicesPage.OpenAsync();
            var ImportPage2 = await invoicesPage.ClickImportPDFBtnAsync();
            await ImportPage2.UploadPdfFileAsync(fileName);
            await ImportPage2.ClickImportBtnAsync();

            (await ImportPage2.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
        }
        finally
        {
            await SqlHelper.DeleteImportedInvoiceByInvoiceNumberAsync(invoiceNumber);
            await SqlHelper.DeletePdfBlobByFileAsync(fileName);
            await SqlHelper.DeleteActivitySourceByPdfFileAsync(fileName);
        }
    }
}
