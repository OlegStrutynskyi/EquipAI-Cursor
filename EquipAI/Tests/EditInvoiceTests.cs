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
        const string expectedTitle = "Edit Invoice";
        const string expectedMessage = "Review and edit this draft invoice, then approve or reject.";
        const string expectedCompany = Config.SetupCompanyName1;
        const string expectedAddress = Config.SetupInvoiceAddress1;
        const string expectedProject = Config.SetupProjectName1;
        const string expectedInvoiceDate = "2026-06-02";
        const string expectedCategory = "Internal Fuel (Scope 1)";
        const string expectedTotalCost = "1562.99";
        const string expectedCurrency = "USD";
        const string expectedDescription = Config.SetupInvoiceLineDescription1;
        const string expectedQuantity = "421.29";
        const string expectedUnitPrice = "3.71";
        const string expectedCost = "1562.99";
        const string expectedEmissionType = "Diesel (100% mineral diesel)";
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
        (await editInvoicePage.GetEmissionCategoryAsync()).Should().Be(expectedCategory);
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
        const string expectedTitle = "Invoices";

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
        var updatedProject = "Haskell";
        var updatedInvoiceDateInput = "2025-01-01";
        var updatedInvoiceDateDisplay = "Jan 1, 2025";
        var updatedTotalCost = "111.11";
        var updatedCurrency = "EUR";
        var updatedTotalDisplay = "111.11 EUR";
        var updatedCategory = "External Fuel";
        var updatedGridStatus = "DRAFT";
        var updatedViewStatus = "Draft";
        var updatedSource = "Manual";
        var updatedDescription1 = "Description1" + stamp;
        var updatedQuantity1 = "500.55";
        var updatedUnitPrice1 = "2.22";
        var updatedEmissionType1 = "Gas oil";
        var updatedUnit1 = "Litre (L)";
        var addedDescription2 = "Description2" + stamp;
        var addedQuantity2 = "10.11";
        var addedUnitPrice2 = "1.11";
        var addedEmissionType2 = "On-site diesel combustion";
        var addedUnit2 = "US Gallon (US_GAL)";
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
            await editInvoicePage.SelectProjectAsync(updatedProject);
            await editInvoicePage.FillInvoiceDateAsync(updatedInvoiceDateInput);
            await editInvoicePage.FillTotalCostAsync(updatedTotalCost);
            await editInvoicePage.SelectCurrencyAsync(updatedCurrency);
            await editInvoicePage.SelectEmissionCategoryAsync(Config.SetupDefaultEmissionCategory2);
            await editInvoicePage.FillQuantity1Async(updatedQuantity1);
            await editInvoicePage.FillUnitPrice1Async(updatedUnitPrice1);
            await editInvoicePage.SelectEmissionType1Async(updatedEmissionType1);
            await editInvoicePage.SelectUnit1Async(updatedUnit1);

            await editInvoicePage.ClickAddRowBtnAsync();
            await editInvoicePage.FillDescription2Async(addedDescription2);
            await editInvoicePage.FillQuantity2Async(addedQuantity2);
            await editInvoicePage.FillUnitPrice2Async(addedUnitPrice2);
            await editInvoicePage.SelectEmissionType2Async(addedEmissionType2);
            await editInvoicePage.SelectUnit2Async(addedUnit2);

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
            (await viewInvoicePage.GetCategoryAsync()).Should().Be(updatedCategory);
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
}
