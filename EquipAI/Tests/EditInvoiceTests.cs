using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class EditInvoiceTests : BaseTest
{
    [Test]
    public async Task T01_EditInvoice_DefaultView()
    {
        const string companyName = Config.SetupCompanyName1;
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedTitle = "Edit Invoice";
        const string expectedMessage = "Review and edit this draft invoice, then approve or reject.";

        var header = await SqlHelper.GetInvoiceEditHeaderAsync(companyName, invoiceNumber);
        var lineItems = await SqlHelper.GetInvoiceViewLineItemsAsync(companyName, invoiceNumber);
        var invoiceDate = header.InvoiceDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

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

        (await editInvoicePage.GetInvoiceNumberAsync()).Should().Be(header.InvoiceNumber);
        (await editInvoicePage.GetCompanyNameAsync()).Should().Be(header.CompanyName);
        (await editInvoicePage.GetAddressAsync()).Should().Be(header.Address);
        (await editInvoicePage.GetProjectAsync()).Should().Be(header.Project);
        (await editInvoicePage.GetInvoiceDateAsync()).Should().Be(invoiceDate);
        (await editInvoicePage.GetEmissionCategoryAsync()).Should().Be(header.EmissionCategory);
        (await editInvoicePage.GetTotalCostAsync()).Should().Be(FormatInputAmount(header.TotalCost));
        (await editInvoicePage.GetCurrencyAsync()).Should().Be(header.CurrencyCode);

        (await editInvoicePage.IsAddRowBtnVisibleAsync()).Should().BeTrue();
        (await editInvoicePage.IsAddRowBtnEnabledAsync()).Should().BeTrue();
        (await editInvoicePage.GetLineNumberCountAsync()).Should().Be(lineItems.Count);
        foreach (var lineItem in lineItems)
        {
            (await editInvoicePage.GetLineDescriptionAsync(lineItem.LinePosition)).Should().Be(lineItem.LineDescription);
            (await editInvoicePage.GetLineQuantityAsync(lineItem.LinePosition)).Should().Be(FormatInputAmount(lineItem.Quantity));
            (await editInvoicePage.GetLineUnitPriceAsync(lineItem.LinePosition)).Should().Be(FormatInputAmount(lineItem.UnitPrice));
            (await editInvoicePage.GetLineCostAsync(lineItem.LinePosition)).Should().Be(FormatInputAmount(lineItem.Cost));
            (await editInvoicePage.GetLineEmissionTypeAsync(lineItem.LinePosition)).Should().Be(lineItem.EmissionType);
            (await editInvoicePage.GetLineUnitAsync(lineItem.LinePosition)).Should().Be(lineItem.Unit);
        }

        (await editInvoicePage.IsRemoveRowBtnVisibleAsync(1)).Should().BeTrue();
        (await editInvoicePage.IsRemoveRowBtnDisabledAsync(1)).Should().BeTrue();
    }

    private static string FormatInputAmount(decimal? value) =>
        value?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty;

    [Test]
    public async Task T02_EditInvoice_ClickBackBtn()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedTitle = "Invoice Upload";

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);
        invoicesPage = await editInvoicePage.ClickBackBtnAsync();

        (await invoicesPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_EditInvoice_ClickApproveBtn()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedStatus = "Approved";

        try
        {
            var invoicesPage = new InvoicesPage(Fixture.Page);
            await invoicesPage.OpenAsync();
            var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);
            var viewInvoicePage = await editInvoicePage.ClickApproveBtnAsync();

            (await viewInvoicePage.GetStatusAsync()).Should().Be(expectedStatus);
        }
        finally
        {
            await SqlHelper.SetInvoiceDraftAsync(invoiceNumber);
        }
    }

    [Test]
    public async Task T04_EditInvoice_ClickRejectBtn()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedDialogTitle = "Confirm rejection";
        const string expectedDialogLabel = "Rejection Reason";
        const string rejectionReason = "Rejected by Autotests";
        const string expectedStatus = "Rejected";

        try
        {
            var invoicesPage = new InvoicesPage(Fixture.Page);
            await invoicesPage.OpenAsync();
            var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);

            await editInvoicePage.ClickRejectBtnAsync();
            (await editInvoicePage.IsRejectDialogVisibleAsync()).Should().BeTrue();
            (await editInvoicePage.GetRejectDialogTitleAsync()).Should().Be(expectedDialogTitle);
            (await editInvoicePage.GetRejectDialogLabelAsync()).Should().Be(expectedDialogLabel);
            (await editInvoicePage.IsRejectionReasonInputVisibleAsync()).Should().BeTrue();
            (await editInvoicePage.IsRejectDialogCancelBtnVisibleAsync()).Should().BeTrue();
            (await editInvoicePage.IsRejectDialogConfirmBtnVisibleAsync()).Should().BeTrue();
            (await editInvoicePage.IsRejectDialogConfirmBtnDisabledAsync()).Should().BeTrue();

            await editInvoicePage.FillRejectionReasonAsync(rejectionReason);
            (await editInvoicePage.IsRejectDialogConfirmBtnEnabledAsync()).Should().BeTrue();

            await editInvoicePage.ClickRejectDialogCancelBtnAsync();
            (await editInvoicePage.IsRejectDialogVisibleAsync()).Should().BeFalse();
            (await editInvoicePage.IsApproveBtnVisibleAsync()).Should().BeTrue();
            (await editInvoicePage.IsRejectBtnVisibleAsync()).Should().BeTrue();

            await editInvoicePage.ClickRejectBtnAsync();
            await editInvoicePage.FillRejectionReasonAsync(rejectionReason);
            var viewInvoicePage = await editInvoicePage.ClickRejectDialogConfirmBtnAsync();

            (await viewInvoicePage.GetStatusAsync()).Should().Be(expectedStatus);
        }
        finally
        {
            await SqlHelper.SetInvoiceDraftAsync(invoiceNumber);
        }
    }

    [Test]
    public async Task T05_EditInvoice_ClickCancelBtn()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string cancelledInvoiceNumber = "Cancel updated";
        const string expectedTitle = "Invoice Upload";

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);

        var originalInvoiceNumber = await editInvoicePage.GetInvoiceNumberAsync();
        await editInvoicePage.FillInvoiceNumberAsync(cancelledInvoiceNumber);
        invoicesPage = await editInvoicePage.ClickCancelBtnAsync();

        (await invoicesPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await invoicesPage.IsInvoiceNumberInGridAsync(cancelledInvoiceNumber)).Should().BeFalse();
    }

    [Test]
    public async Task T06_EditInvoice_ClickSaveAsDraftBtn()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);
        await editInvoicePage.ClickSaveAsDraftBtnAsync();
        await editInvoicePage.WaitForDraftSavedMessageAsync();

        (await editInvoicePage.IsDraftSavedMessageVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T07_EditInvoice_EmptyFields()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedInvoiceNumberError = "Invoice number is required.";
        const string expectedCompanyNameError = "Company name is required.";
        const string expectedTotalCostError = "Total cost is required.";
        const string expectedDescriptionError = "Description is required.";
        const string expectedQuantityError = "Quantity is required.";
        const string expectedUnitPriceError = "Unit price is required.";

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);

        await editInvoicePage.ClearInvoiceNumberAsync();
        await editInvoicePage.ClearCompanyNameAsync();
        await editInvoicePage.ClearAddressAsync();
        await editInvoicePage.ClearTotalCostAsync();
        await editInvoicePage.ClearDescription1Async();
        await editInvoicePage.ClearQuantity1Async();
        await editInvoicePage.ClearUnitPrice1Async();
        await editInvoicePage.ClickSaveAsDraftBtnAsync();

        (await editInvoicePage.GetInvoiceNumberErrorAsync()).Should().Be(expectedInvoiceNumberError);
        (await editInvoicePage.GetCompanyNameErrorAsync()).Should().Be(expectedCompanyNameError);
        (await editInvoicePage.GetTotalCostErrorAsync()).Should().Be(expectedTotalCostError);
        (await editInvoicePage.GetDescription1ErrorAsync()).Should().Be(expectedDescriptionError);
        (await editInvoicePage.GetQuantity1ErrorAsync()).Should().Be(expectedQuantityError);
        (await editInvoicePage.GetUnitPrice1ErrorAsync()).Should().Be(expectedUnitPriceError);
    }

    [Test]
    public async Task T08_EditInvoice_InvoiceNumberTooLong()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedInvoiceNumberError = "Invoice number must be at most 64 characters.";
        var randomInvoiceNumber = GenerateRandomString(65);

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);
        await editInvoicePage.FillInvoiceNumberAsync(randomInvoiceNumber);
        await editInvoicePage.ClickSaveAsDraftBtnAsync();

        (await editInvoicePage.GetInvoiceNumberErrorAsync()).Should().Be(expectedInvoiceNumberError);
    }

    [Test]
    public async Task T09_EditInvoice_CompanyNameTooLong()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedCompanyNameError = "Company name must be at most 256 characters.";
        var randomCompanyName = GenerateRandomString(257);

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);
        await editInvoicePage.FillCompanyNameAsync(randomCompanyName);
        await editInvoicePage.ClickSaveAsDraftBtnAsync();

        (await editInvoicePage.GetCompanyNameErrorAsync()).Should().Be(expectedCompanyNameError);
    }

    [Test]
    public async Task T10_EditInvoice_AddressTooLong()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedAddressError = "Address must be at most 512 characters.";
        var randomAddress = GenerateRandomString(513);

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);
        await editInvoicePage.FillAddressAsync(randomAddress);
        await editInvoicePage.ClickSaveAsDraftBtnAsync();

        (await editInvoicePage.GetAddressErrorAsync()).Should().Be(expectedAddressError);
    }

    [Test]
    public async Task T11_EditInvoice_DescriptionTooLong()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string expectedDescriptionError = "Description must be at most 512 characters.";
        var randomDescription = GenerateRandomString(513);

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();
        var editInvoicePage = await invoicesPage.ClickEditBtnAsync(invoiceNumber);
        await editInvoicePage.FillDescriptionAsync(randomDescription);
        await editInvoicePage.ClickSaveAsDraftBtnAsync();

        (await editInvoicePage.GetDescription1ErrorAsync()).Should().Be(expectedDescriptionError);
    }

    [Test]
    public async Task T12_EditInvoice_ManualInvoiceEdit()
    {
        const string originalInvoiceNumber = Config.SetupInvoiceNumber1;
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var updatedInvoiceNumber = "Invoice" + stamp;
        var updatedCompany = "Company" + stamp;
        var updatedAddress = "Address" + stamp;
        var updatedInvoiceDateInput = "2025-01-01";
        var updatedInvoiceDateDisplay = "Jan 1, 2025";
        var updatedTotalCost = "111.11";
        var updatedGridStatus = "DRAFT";
        var updatedViewStatus = "Draft";
        var updatedSource = "Manual";
        var updatedDescription1 = "Description1" + stamp;
        var updatedQuantity1 = "500.55";
        var updatedUnitPrice1 = "2.22";
        var addedDescription2 = "Description2" + stamp;
        var addedQuantity2 = "10.11";
        var addedUnitPrice2 = "1.11";
        const int expectedLineItemRowCount = 2;
        object? invoiceId = null;

        try
        {
            invoiceId = await SqlHelper.GetInvoiceIdByInvoiceNumberAsync(originalInvoiceNumber);

            var invoicesPage = new InvoicesPage(Fixture.Page);
            await invoicesPage.OpenAsync();
            (await invoicesPage.IsInvoiceNumberInGridAsync(originalInvoiceNumber)).Should().BeTrue();

            var editInvoicePage = await invoicesPage.ClickEditBtnAsync(originalInvoiceNumber);

            await editInvoicePage.FillInvoiceNumberAsync(updatedInvoiceNumber);
            await editInvoicePage.FillCompanyNameAsync(updatedCompany);
            await editInvoicePage.FillAddressAsync(updatedAddress);
            await editInvoicePage.FillDescriptionAsync(updatedDescription1);
            var updatedProject = await editInvoicePage.SelectDifferentProjectAsync();
            await editInvoicePage.FillInvoiceDateAsync(updatedInvoiceDateInput);
            await editInvoicePage.FillTotalCostAsync(updatedTotalCost);
            var updatedCurrency = await editInvoicePage.SelectDifferentCurrencyAsync();
            var updatedTotalDisplay = $"111.11 {updatedCurrency}";
            var updatedCategory = await editInvoicePage.SelectDifferentEmissionCategoryAsync();
            await editInvoicePage.FillQuantity1Async(updatedQuantity1);
            await editInvoicePage.FillUnitPrice1Async(updatedUnitPrice1);
            var updatedEmissionType1 = await editInvoicePage.SelectDifferentEmissionType1Async();
            var updatedUnit1 = await editInvoicePage.SelectDifferentUnit1Async();

            await editInvoicePage.ClickAddRowBtnAsync();
            await editInvoicePage.FillDescription2Async(addedDescription2);
            await editInvoicePage.FillQuantity2Async(addedQuantity2);
            await editInvoicePage.FillUnitPrice2Async(addedUnitPrice2);
            var addedEmissionType2 = await editInvoicePage.SelectDifferentEmissionType2Async();
            var addedUnit2 = await editInvoicePage.SelectDifferentUnit2Async();

            await editInvoicePage.ClickSaveAsDraftBtnAsync();
            await editInvoicePage.WaitForDraftSavedMessageAsync();
            invoicesPage = await editInvoicePage.ClickBackBtnAsync();
            var gridRow = await invoicesPage.GetInvoiceGridRowAsync(updatedInvoiceNumber);
            gridRow.Should().NotBeNull();
            gridRow!.Project.Should().Be(updatedProject);
            gridRow.InvoiceNumber.Should().Be(updatedInvoiceNumber);
            gridRow.Company.Should().Be(updatedCompany);
            gridRow.Status.Should().Be(updatedGridStatus);
            gridRow.Date.Should().Be(updatedInvoiceDateDisplay);
            gridRow.Source.Should().Be(updatedSource);

            var viewInvoicePage = await invoicesPage.ClickViewBtnAsync(updatedInvoiceNumber);
            (await viewInvoicePage.GetTitleAsync()).Should().Be("Invoice " + updatedInvoiceNumber);
            (await viewInvoicePage.GetStatusAsync()).Should().Be(updatedViewStatus);
            (await viewInvoicePage.GetSourceAsync()).Should().Be(updatedSource);
            (await viewInvoicePage.GetCompanyNameAsync()).Should().Be(updatedCompany);
            (await viewInvoicePage.GetAddressAsync()).Should().Be(updatedAddress);
            (await viewInvoicePage.GetProjectAsync()).Should().Be(updatedProject);
            (await viewInvoicePage.GetInvoiceDateAsync()).Should().Be(updatedInvoiceDateDisplay);
            (await viewInvoicePage.GetCategoryAsync()).Should().Be(ToViewEmissionCategory(updatedCategory));
            (await viewInvoicePage.GetTotalAsync()).Should().Be(updatedTotalDisplay);
            (await viewInvoicePage.GetLineItemRowCountAsync()).Should().Be(expectedLineItemRowCount);
            (await viewInvoicePage.GetDescription1Async()).Should().Be(updatedDescription1);
            (await viewInvoicePage.GetQuantity1Async()).Should().Be(updatedQuantity1);
            (await viewInvoicePage.GetUnitPrice1Async()).Should().Be(updatedUnitPrice1);
            (await viewInvoicePage.GetEmissionType1Async()).Should().Be(updatedEmissionType1);
            (await viewInvoicePage.GetUnit1Async()).Should().Be(updatedUnit1);
            (await viewInvoicePage.GetDescription2Async()).Should().Be(addedDescription2);
            (await viewInvoicePage.GetQuantity2Async()).Should().Be(addedQuantity2);
            (await viewInvoicePage.GetUnitPrice2Async()).Should().Be(addedUnitPrice2);
            (await viewInvoicePage.GetEmissionType2Async()).Should().Be(addedEmissionType2);
            (await viewInvoicePage.GetUnit2Async()).Should().Be(addedUnit2);
        }
        finally
        {
            if (invoiceId is not null)
                await SqlHelper.RestoreSetupManualInvoiceAsync(invoiceId, addedDescription2);
        }
    }

    private static string ToViewEmissionCategory(string selectedOption)
    {
        const string scopeMarker = " (Scope ";
        var scopeIndex = selectedOption.IndexOf(scopeMarker, StringComparison.Ordinal);
        return scopeIndex < 0 ? selectedOption : selectedOption[..scopeIndex];
    }
}
