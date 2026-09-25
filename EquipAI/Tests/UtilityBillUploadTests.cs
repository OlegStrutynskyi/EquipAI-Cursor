using EquipAI.Pages;
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
}
