using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class ImportUtilityBillTests : BaseTest
{
    [Test]
    public async Task T01_ImportUtilityBill_DefaultView()
    {
        const string expectedTitle = "Import Utility Bill";
        const string expectedMessage =
            "Upload a single utility-bill PDF. It is read automatically and appears in the list as a draft invoice once processing finishes.";

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenUtilityBillAsync();

        (await importPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await importPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await importPage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await importPage.IsPdfFileLabelVisibleAsync()).Should().BeTrue();
        (await importPage.IsImportSectionVisibleAsync()).Should().BeTrue();
        (await importPage.IsImportBtnVisibleAsync()).Should().BeTrue();
        (await importPage.IsImportBtnEnabledAsync()).Should().BeFalse();
    }

    [Test]
    public async Task T02_ImportUtilityBill_ClickBackBtn()
    {
        const string expectedTitle = "Utility Bill Upload";

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenUtilityBillAsync();
        var utilityBillUploadPage = await importPage.ClickBackToUtilityBillUploadAsync();

        (await utilityBillUploadPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_ImportUtilityBill_IncorrectFormat()
    {
        const string expectedAlertMessage = "Only .pdf files are accepted.";

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenUtilityBillAsync();
        await importPage.UploadPdfFileAsync("scv-invoice-correct.csv");
        await importPage.ClickImportBtnAsync();

        (await importPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    [CancelAfter(660_000)]
    public async Task T04_ImportUtilityBill_Success()
    {
        const string fileName = "Madison_01_2025_2.pdf";
        const string company = "Madison Gas and Electric";
        const string expectedToasterMessage = "Madison_01_2025_2.pdf has been uploaded and is being processed";
        var expectedImportDate = DateTime.Now.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

        try
        {
            await SqlHelper.DeletePdfBlobByFileAsync(fileName);
            await SqlHelper.DeleteImportedInvoiceByPdfFileAsync(fileName);
            await SqlHelper.DeleteActivitySourceByPdfFileAsync(fileName);

            var importPage = new ImportPage(Fixture.Page);
            await importPage.OpenUtilityBillAsync();
            await importPage.ImportPdfAsync(fileName, expectedToasterMessage);

            var invoicesPage = new InvoicesPage(Fixture.Page);
            var deadline = DateTime.UtcNow.AddMinutes(10);
            UtilityBillGridRow? gridRow = null;

            while (DateTime.UtcNow < deadline)
            {
                await invoicesPage.OpenAsync();
                await invoicesPage.SelectUtilityBillsTabAsync();
                gridRow = await invoicesPage.GetUtilityBillGridRowAsync(company, expectedImportDate);
                if (gridRow is not null
                    && gridRow.Company.Equals(company, StringComparison.Ordinal)
                    && gridRow.ImportDate.Equals(expectedImportDate, StringComparison.Ordinal))
                {
                    break;
                }

                gridRow = null;
                await Task.Delay(TimeSpan.FromSeconds(10));
            }

            gridRow.Should().NotBeNull(
                $"Utility bill for '{company}' with Import Date '{expectedImportDate}' was not found within 10 minutes.");
            gridRow!.Company.Should().Be(company);
            gridRow.ImportDate.Should().Be(expectedImportDate);
        }
        finally
        {
            await SqlHelper.DeletePdfBlobByFileAsync(fileName);
            await SqlHelper.DeleteImportedInvoiceByPdfFileAsync(fileName);
            await SqlHelper.DeleteActivitySourceByPdfFileAsync(fileName);
        }
    }

    [Test]
    public async Task T05_ImportUtilityBill_DuplicatedFile()
    {
        const string fileName = "Madison_01_2025_2.pdf";
        const string expectedToasterMessage = "Madison_01_2025_2.pdf has been uploaded and is being processed";
        const string expectedAlertMessage = "A PDF with the same content has already been imported.";

        try
        {
            await SqlHelper.DeletePdfBlobByFileAsync(fileName);
            await SqlHelper.DeleteImportedInvoiceByPdfFileAsync(fileName);
            await SqlHelper.DeleteActivitySourceByPdfFileAsync(fileName);

            var importPage = new ImportPage(Fixture.Page);
            await importPage.OpenUtilityBillAsync();
            await importPage.ImportPdfAsync(fileName, expectedToasterMessage);

            var invoicesPage = new InvoicesPage(Fixture.Page);
            await invoicesPage.OpenAsync();
            var ImportPage2 = await invoicesPage.ClickImportUtilityBillBtnAsync();
            await ImportPage2.UploadPdfFileAsync(fileName);
            await ImportPage2.ClickImportBtnAsync();

            (await ImportPage2.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
        }
        finally
        {
            await SqlHelper.DeletePdfBlobByFileAsync(fileName);
            await SqlHelper.DeleteImportedInvoiceByPdfFileAsync(fileName);
            await SqlHelper.DeleteActivitySourceByPdfFileAsync(fileName);
        }
    }
}
