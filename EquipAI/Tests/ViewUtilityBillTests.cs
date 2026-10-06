using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class ViewUtilityBillTests : BaseTest
{
    [Test]
    public async Task T01_ViewUtilityBill_DefaultView()
    {
        const string companyName = Config.SetupCompanyName1;
        var billDateValue = new DateTime(2022, 1, 1);
        const string expectedGridStatus = "DRAFT";
        const string expectedViewStatus = "Draft";
        const string expectedTitle = "Utility Bill";

        var header = await SqlHelper.GetUtilityBillViewHeaderAsync(companyName, billDateValue);
        var lineItems = await SqlHelper.GetUtilityBillViewLineItemsAsync(companyName, billDateValue);
        var billDate = header.BillDate?.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) ?? string.Empty;

        var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
        await utilityBillUploadPage.OpenAsync();

        var gridRow = await utilityBillUploadPage.FindGridRowAsync(billDate, header.Project, header.CompanyName, status: expectedGridStatus);
        gridRow.Should().NotBeNull();
        var expectedSource = gridRow!.Source;
        gridRow.Date.Should().Be(billDate);
        gridRow.Project.Should().Be(header.Project);
        gridRow.Company.Should().Be(header.CompanyName);
        gridRow.Status.ToUpperInvariant().Should().Be(expectedGridStatus);

        var viewPage = await utilityBillUploadPage.ClickViewBtnForRowAsync(billDate, header.Project, header.CompanyName, expectedGridStatus);

        (await viewPage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await viewPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await viewPage.GetStatusAsync()).Should().Be(expectedViewStatus);
        (await viewPage.GetSourceAsync()).Should().Be(expectedSource);
        (await viewPage.IsEditDraftBtnVisibleAsync()).Should().BeFalse();

        (await viewPage.GetCompanyNameAsync()).Should().Be(header.CompanyName);
        (await viewPage.GetAddressAsync()).Should().Be(header.Address);
        (await viewPage.GetProjectAsync()).Should().Be(header.Project);
        (await viewPage.GetBillDateAsync()).Should().Be(billDate);
        (await viewPage.GetTotalAsync()).Should().Be(FormatTotal(header.Total));

        (await viewPage.GetLineItemRowCountAsync()).Should().Be(lineItems.Count);
        foreach (var lineItem in lineItems)
            (await viewPage.GetLineItemCellsAsync(lineItem.LinePosition)).Should().Equal(ToLineCells(lineItem));
    }

    private static string FormatTotal(string total)
    {
        var parts = total.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var cost))
            return $"{cost.ToString("0.00", CultureInfo.InvariantCulture)} {parts[1]}";

        return total;
    }

    private static string[] ToLineCells(UtilityBillViewLineItemRow lineItem) =>
    [
        lineItem.LinePosition.ToString(CultureInfo.InvariantCulture),
        lineItem.LineDescription,
        lineItem.Quantity?.ToString("0.################", CultureInfo.InvariantCulture) ?? string.Empty,
        lineItem.Cost?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty,
        lineItem.EmissionType,
        lineItem.Category,
        lineItem.Unit,
    ];

    [Test]
    public async Task T02_ViewUtilityBill_ClickReviewImport()
    {
        const string billDate = "Jan 1, 2022";
        const string project = Config.SetupProjectName1;
        const string company = Config.SetupCompanyName1;
        const string expectedGridStatus = "DRAFT";
        const string expectedTitle = "Review PDF Import";

        var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
        await utilityBillUploadPage.OpenAsync();
        var viewPage = await utilityBillUploadPage.ClickViewBtnForRowAsync(billDate, project, company, expectedGridStatus);
        var reviewPage = await viewPage.ClickReviewImportBtnAsync();

        (await reviewPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_ViewUtilityBill_Rejected_View()
    {
        const string billDate = "Jan 1, 2022";
        const string project = Config.SetupProjectName1;
        const string company = Config.SetupCompanyName1;
        var billDateValue = new DateTime(2022, 1, 1);
        const string expectedStatus = "Rejected";

        try
        {
            await SqlHelper.SetUtilityBillRejectedAsync(project, company, billDateValue, "123");

            var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
            await utilityBillUploadPage.OpenAsync();
            var viewPage = await utilityBillUploadPage.ClickViewBtnForRowAsync(billDate, project, company, expectedStatus);

            (await viewPage.GetStatusAsync()).Should().Be(expectedStatus);
            (await viewPage.IsReviewImportBtnVisibleAsync()).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.SetUtilityBillDraftAsync(project, company, billDateValue);
        }
    }

    [Test]
    public async Task T04_ViewUtilityBill_Approved_View()
    {
        const string billDate = "Jan 1, 2022";
        const string project = Config.SetupProjectName1;
        const string company = Config.SetupCompanyName1;
        var billDateValue = new DateTime(2022, 1, 1);
        const string expectedStatus = "Approved";

        try
        {
            await SqlHelper.SetUtilityBillApprovedAsync(project, company, billDateValue);

            var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
            await utilityBillUploadPage.OpenAsync();
            var viewPage = await utilityBillUploadPage.ClickViewBtnForRowAsync(billDate, project, company, expectedStatus);

            (await viewPage.GetStatusAsync()).Should().Be(expectedStatus);
            (await viewPage.IsReviewImportBtnVisibleAsync()).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.SetUtilityBillDraftAsync(project, company, billDateValue);
        }
    }
}
