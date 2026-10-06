using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class EditUtilityBillTests : BaseTest
{
    [Test]
    public async Task T01_EditUtilityBill_DefaultView()
    {
        const string companyName = Config.SetupCompanyName1;
        var billDateValue = new DateTime(2022, 1, 1);
        const string expectedGridStatus = "DRAFT";
        const string expectedTitle = "Review PDF Import";
        const string expectedMessage = "Compare the scanned utility bill on the left with the recognized values on the right, then approve or reject.";

        var header = await SqlHelper.GetUtilityBillEditHeaderAsync(companyName, billDateValue);
        var lineItems = await SqlHelper.GetUtilityBillEditLineItemsAsync(companyName, billDateValue);
        var billDate = header.BillDate?.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
        var expectedBillDate = header.BillDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

        var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
        await utilityBillUploadPage.OpenAsync();
        var editPage = await utilityBillUploadPage.ClickEditBtnForRowAsync(billDate, header.Project, header.CompanyName, expectedGridStatus);

        (await editPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await editPage.GetReviewMessageAsync()).Should().Be(expectedMessage);
        (await editPage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await editPage.IsReviewApproveBtnVisibleAsync()).Should().BeTrue();
        (await editPage.IsReviewRejectBtnVisibleAsync()).Should().BeTrue();
        (await editPage.IsCancelBtnVisibleAsync()).Should().BeTrue();
        (await editPage.IsReviewSaveAsDraftBtnVisibleAsync()).Should().BeTrue();
        (await editPage.IsReviewPreviewSectionVisibleAsync()).Should().BeTrue();

        (await editPage.GetReviewCompanyNameAsync()).Should().Be(header.CompanyName);
        (await editPage.GetReviewAddressAsync()).Should().Be(header.Address);
        (await editPage.GetReviewProjectAsync()).Should().Be(header.Project);
        (await editPage.GetReviewBillDateAsync()).Should().Be(expectedBillDate);
        (await editPage.GetReviewTotalCostAsync()).Should().Be(FormatTotalCost(header.TotalCost));
        (await editPage.GetReviewCurrencyAsync()).Should().Be(header.CurrencyCode);

        foreach (var lineItem in lineItems)
        {
            (await editPage.GetReviewLineDescriptionAsync(lineItem.LinePosition)).Should().Be(lineItem.LineDescription);
            (await editPage.GetReviewLineQuantityAsync(lineItem.LinePosition)).Should().Be(FormatLineAmount(lineItem.Quantity));
            (await editPage.GetReviewLineCostAsync(lineItem.LinePosition)).Should().Be(FormatLineAmount(lineItem.Cost));
            (await editPage.GetReviewLineUnitAsync(lineItem.LinePosition)).Should().Be(lineItem.Unit);
            (await editPage.GetReviewLineEmissionTypeAsync(lineItem.LinePosition)).Should().Be(lineItem.EmissionType);
            (await editPage.GetReviewLineEmissionCategoryAsync(lineItem.LinePosition)).Should().Be(lineItem.EmissionCategory);
        }
    }

    [Test]
    public async Task T02_EditUtilityBill_ClickBackBtn()
    {
        const string expectedTitle = "Utility Bill Upload";

        var editPage = await OpenDraftReviewPageAsync();
        var utilityBillUploadPage = await editPage.ClickBackToUtilityBillUploadAsync();

        (await utilityBillUploadPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_EditUtilityBill_ClickApproveBtn()
    {
        const string project = Config.SetupProjectName1;
        const string company = Config.SetupCompanyName1;
        var billDateValue = new DateTime(2022, 1, 1);
        const string expectedStatus = "Approved";

        try
        {
            var editPage = await OpenDraftReviewPageAsync();
            var viewPage = await editPage.ClickReviewApproveBtnAsync();

            (await viewPage.GetStatusAsync()).Should().Be(expectedStatus);
        }
        finally
        {
            await SqlHelper.SetUtilityBillDraftAsync(project, company, billDateValue);
        }
    }

    [Test]
    public async Task T04_EditUtilityBill_ClickRejectBtn()
    {
        const string project = Config.SetupProjectName1;
        const string company = Config.SetupCompanyName1;
        var billDateValue = new DateTime(2022, 1, 1);
        const string expectedDialogTitle = "Confirm rejection";
        const string expectedDialogLabel = "Rejection Reason";
        const string rejectionReason = "Rejected by Autotests";
        const string expectedStatus = "Rejected";

        try
        {
            var editPage = await OpenDraftReviewPageAsync();

            await editPage.ClickReviewRejectBtnAsync();
            (await editPage.IsRejectDialogVisibleAsync()).Should().BeTrue();
            (await editPage.GetRejectDialogTitleAsync()).Should().Be(expectedDialogTitle);
            (await editPage.GetRejectDialogLabelAsync()).Should().Be(expectedDialogLabel);
            (await editPage.IsRejectionReasonInputVisibleAsync()).Should().BeTrue();
            (await editPage.IsRejectDialogCancelBtnVisibleAsync()).Should().BeTrue();
            (await editPage.IsRejectDialogConfirmBtnVisibleAsync()).Should().BeTrue();
            (await editPage.IsRejectDialogConfirmBtnDisabledAsync()).Should().BeTrue();

            await editPage.FillRejectionReasonAsync(rejectionReason);
            (await editPage.IsRejectDialogConfirmBtnEnabledAsync()).Should().BeTrue();

            await editPage.ClickRejectDialogCancelBtnAsync();
            (await editPage.IsRejectDialogVisibleAsync()).Should().BeFalse();
            (await editPage.IsReviewApproveBtnVisibleAsync()).Should().BeTrue();
            (await editPage.IsReviewRejectBtnVisibleAsync()).Should().BeTrue();

            await editPage.ClickReviewRejectBtnAsync();
            await editPage.FillRejectionReasonAsync(rejectionReason);
            var viewPage = await editPage.ClickRejectDialogConfirmBtnAsync();

            (await viewPage.GetStatusAsync()).Should().Be(expectedStatus);
        }
        finally
        {
            await SqlHelper.SetUtilityBillDraftAsync(project, company, billDateValue);
        }
    }

    [Test]
    public async Task T05_EditUtilityBill_ClickCancelBtn()
    {
        const string billDate = "Jan 1, 2022";
        const string project = Config.SetupProjectName1;
        const string cancelledCompany = "Cancel updated";
        const string expectedTitle = "Utility Bill Upload";

        var editPage = await OpenDraftReviewPageAsync();
        await editPage.FillReviewCompanyNameAsync(cancelledCompany);
        var utilityBillUploadPage = await editPage.ClickReviewCancelBtnAsync();

        (await utilityBillUploadPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await utilityBillUploadPage.FindGridRowAsync(billDate, project, cancelledCompany)).Should().BeNull();
    }

    [Test]
    public async Task T06_EditUtilityBill_ClickSaveAsDraftBtn()
    {
        var editPage = await OpenDraftReviewPageAsync();
        await editPage.ClickReviewSaveAsDraftBtnAsync();
        await editPage.WaitForReviewDraftSavedMessageAsync();

        (await editPage.IsReviewDraftSavedMessageVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T07_EditUtilityBill_EmptyFields()
    {
        const string expectedCompanyNameError = "Company name is required.";
        const string expectedTotalCostError = "Total cost is required.";
        const string expectedDescriptionError = "Description is required.";
        const string expectedQuantityError = "Quantity is required.";

        var editPage = await OpenDraftReviewPageAsync();
        await editPage.ClearReviewCompanyNameAsync();
        await editPage.ClearReviewAddressAsync();
        await editPage.ClearReviewTotalCostAsync();
        await editPage.ClearReviewLineDescriptionAsync(1);
        await editPage.ClearReviewLineQuantityAsync(1);
        await editPage.ClickReviewSaveAsDraftBtnAsync();

        (await editPage.GetReviewCompanyNameErrorAsync()).Should().Be(expectedCompanyNameError);
        (await editPage.GetReviewTotalCostErrorAsync()).Should().Be(expectedTotalCostError);
        (await editPage.GetReviewLineDescriptionErrorAsync(1)).Should().Be(expectedDescriptionError);
        (await editPage.GetReviewLineQuantityErrorAsync(1)).Should().Be(expectedQuantityError);
    }

    [Test]
    public async Task T08_EditUtilityBill_CompanyNameTooLong()
    {
        const string expectedCompanyNameError = "Company name must be at most 256 characters.";
        var randomCompanyName = GenerateRandomString(257);

        var editPage = await OpenDraftReviewPageAsync();
        await editPage.FillReviewCompanyNameAsync(randomCompanyName);
        await editPage.ClickReviewSaveAsDraftBtnAsync();

        (await editPage.GetReviewCompanyNameErrorAsync()).Should().Be(expectedCompanyNameError);
    }

    [Test]
    public async Task T09_EditUtilityBill_AddressTooLong()
    {
        const string expectedAddressError = "Address must be at most 512 characters.";
        var randomAddress = GenerateRandomString(513);

        var editPage = await OpenDraftReviewPageAsync();
        await editPage.FillReviewAddressAsync(randomAddress);
        await editPage.ClickReviewSaveAsDraftBtnAsync();

        (await editPage.GetReviewAddressErrorAsync()).Should().Be(expectedAddressError);
    }

    [Test]
    public async Task T10_EditUtilityBill_DescriptionTooLong()
    {
        const string expectedDescriptionError = "Description must be at most 512 characters.";
        var randomDescription = GenerateRandomString(513);

        var editPage = await OpenDraftReviewPageAsync();
        await editPage.FillReviewLineDescriptionAsync(1, randomDescription);
        await editPage.ClickReviewSaveAsDraftBtnAsync();

        (await editPage.GetReviewLineDescriptionErrorAsync(1)).Should().Be(expectedDescriptionError);
    }

    [Test]
    public async Task T11_EditUtilityBill_EditSuccess()
    {
        const string originalCompany = Config.SetupCompanyName1;
        var originalBillDate = new DateTime(2022, 1, 1);
        const string originalBillDateDisplay = "Jan 1, 2022";
        const string originalProject = Config.SetupProjectName1;
        const string draftStatus = "DRAFT";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var updatedCompany = "Company" + stamp;
        var updatedAddress = "Address" + stamp;
        var updatedBillDateInput = "2025-01-01";
        var updatedBillDateDisplay = "Jan 1, 2025";
        var updatedTotalCost = "111.11";
        var updatedDescription1 = "Description1" + stamp;
        var updatedQuantity1 = "500.55";
        var addedDescription2 = "Description2" + stamp;
        var addedQuantity2 = "10.11";
        const string viewStatus = "Draft";
        const string viewTitle = "Utility Bill";
        UtilityBillEditSnapshot? snapshot = null;

        try
        {
            snapshot = await SqlHelper.GetUtilityBillEditSnapshotAsync(originalCompany, originalBillDate);

            var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
            await utilityBillUploadPage.OpenAsync();
            var originalRow = await utilityBillUploadPage.FindGridRowAsync(
                originalBillDateDisplay, originalProject, originalCompany, status: draftStatus);
            originalRow.Should().NotBeNull();
            var expectedSource = originalRow!.Source;

            var editPage = await utilityBillUploadPage.ClickEditBtnForRowAsync(
                originalBillDateDisplay, originalProject, originalCompany, draftStatus);

            await editPage.FillReviewCompanyNameAsync(updatedCompany);
            await editPage.FillReviewAddressAsync(updatedAddress);
            var updatedProject = await editPage.SelectDifferentReviewProjectAsync();
            await editPage.FillReviewBillDateAsync(updatedBillDateInput);
            await editPage.FillReviewTotalCostAsync(updatedTotalCost);
            var updatedCurrency = await editPage.SelectDifferentReviewCurrencyAsync();
            var updatedTotalDisplay = $"111.11 {updatedCurrency}";

            await editPage.FillReviewLineDescriptionAsync(1, updatedDescription1);
            await editPage.FillReviewLineQuantityAsync(1, updatedQuantity1);
            var updatedEmissionType1 = await editPage.SelectDifferentReviewLineEmissionTypeAsync(1);
            var updatedUnit1 = await editPage.SelectDifferentReviewLineUnitAsync(1);
            var updatedCategory1 = await editPage.SelectDifferentReviewLineCategoryAsync(1);

            var addedLine = await editPage.ClickReviewAddRowBtnAsync();
            await editPage.FillReviewLineDescriptionAsync(addedLine, addedDescription2);
            await editPage.FillReviewLineQuantityAsync(addedLine, addedQuantity2);
            var addedEmissionType2 = await editPage.SelectDifferentReviewLineEmissionTypeAsync(addedLine);
            var addedUnit2 = await editPage.SelectDifferentReviewLineUnitAsync(addedLine);
            var addedCategory2 = await editPage.SelectDifferentReviewLineCategoryAsync(addedLine);
            var line1Cost = FormatViewCost(await editPage.GetReviewLineCostAsync(1));
            const string addedLineCost = "—";

            await editPage.ClickReviewSaveAsDraftBtnAsync();
            await editPage.WaitForReviewDraftSavedMessageAsync();
            utilityBillUploadPage = await editPage.ClickBackToUtilityBillUploadAsync();

            var gridRow = await utilityBillUploadPage.FindGridRowAsync(
                updatedBillDateDisplay, updatedProject, updatedCompany, status: draftStatus);
            gridRow.Should().NotBeNull();
            gridRow!.Project.Should().Be(updatedProject);
            gridRow.Company.Should().Be(updatedCompany);
            gridRow.Date.Should().Be(updatedBillDateDisplay);
            gridRow.Status.Should().Be(draftStatus);
            gridRow.Source.Should().Be(expectedSource);

            var viewPage = await utilityBillUploadPage.ClickViewBtnForRowAsync(
                updatedBillDateDisplay, updatedProject, updatedCompany, draftStatus);
            (await viewPage.GetTitleAsync()).Should().Be(viewTitle);
            (await viewPage.GetStatusAsync()).Should().Be(viewStatus);
            (await viewPage.GetSourceAsync()).Should().Be(expectedSource);
            (await viewPage.GetCompanyNameAsync()).Should().Be(updatedCompany);
            (await viewPage.GetAddressAsync()).Should().Be(updatedAddress);
            (await viewPage.GetProjectAsync()).Should().Be(updatedProject);
            (await viewPage.GetBillDateAsync()).Should().Be(updatedBillDateDisplay);
            (await viewPage.GetTotalAsync()).Should().Be(updatedTotalDisplay);
            (await viewPage.GetLineItemRowCountAsync()).Should().Be(snapshot.Lines.Count + 1);
            (await viewPage.GetLineItemCellsAsync(1)).Should().Equal(
            [
                "1",
                updatedDescription1,
                updatedQuantity1,
                line1Cost,
                updatedEmissionType1,
                ToViewEmissionCategory(updatedCategory1),
                updatedUnit1,
            ]);
            (await viewPage.GetLineItemCellsAsync(addedLine)).Should().Equal(
            [
                addedLine.ToString(CultureInfo.InvariantCulture),
                addedDescription2,
                addedQuantity2,
                addedLineCost,
                addedEmissionType2,
                ToViewEmissionCategory(addedCategory2),
                addedUnit2,
            ]);
        }
        finally
        {
            if (snapshot is not null)
                await SqlHelper.RestoreUtilityBillEditSnapshotAsync(snapshot);
        }
    }

    private static string ToViewEmissionCategory(string selectedOption)
    {
        const string scopeMarker = " (Scope ";
        var scopeIndex = selectedOption.IndexOf(scopeMarker, StringComparison.Ordinal);
        return scopeIndex < 0 ? selectedOption : selectedOption[..scopeIndex];
    }

    private static string FormatViewCost(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var cost))
            return cost.ToString("0.00", CultureInfo.InvariantCulture);

        return value;
    }

    private async Task<ImportPage> OpenDraftReviewPageAsync()
    {
        const string billDate = "Jan 1, 2022";
        const string project = Config.SetupProjectName1;
        const string company = Config.SetupCompanyName1;
        const string status = "DRAFT";

        var utilityBillUploadPage = new UtilityBillUploadPage(Fixture.Page);
        await utilityBillUploadPage.OpenAsync();
        return await utilityBillUploadPage.ClickEditBtnForRowAsync(billDate, project, company, status);
    }

    private static string FormatTotalCost(decimal? value) =>
        value?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string FormatLineAmount(decimal? value) =>
        value?.ToString("0.################", CultureInfo.InvariantCulture) ?? string.Empty;
}
