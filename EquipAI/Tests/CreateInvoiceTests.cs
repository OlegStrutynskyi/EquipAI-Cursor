using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;
using System.Globalization;

namespace EquipAI.Tests;

public class CreateInvoiceTests : BaseTest
{
    [Test]
    public async Task T01_CreateInvoice_DefaultView()
    {
        const string expectedTitle = "Create Invoice";
        const string expectedMessage = "Enter invoice header details and line items, then save as a draft.";

        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();

        (await createInvoicePage.GetTitleAsync()).Should().Be(expectedTitle);
        (await createInvoicePage.GetMessageAsync()).Should().Be(expectedMessage);
        (await createInvoicePage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await createInvoicePage.IsHeaderSectionVisibleAsync()).Should().BeTrue();
        (await createInvoicePage.IsLineItemsSectionVisibleAsync()).Should().BeTrue();
        (await createInvoicePage.IsCancelBtnVisibleAsync()).Should().BeTrue();
        (await createInvoicePage.IsCreateInvoiceBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_CreateInvoice_ClickBack()
    {
        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();
        var invoicesPage = await createInvoicePage.ClickBackBtnAsync();

        (await invoicesPage.IsInvoicesTitleVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T03_CreateInvoice_ClickCancel()
    {
        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();

        var initialInvoiceCount = await invoicesPage.GetInvoiceNumberCountFromAllPagesAsync();

        var createInvoicePage = await invoicesPage.ClickCreateBtnAsync();
        await createInvoicePage.FillAllFieldsAsync();
        invoicesPage = await createInvoicePage.ClickCancelAsync();

        (await invoicesPage.IsInvoicesTitleVisibleAsync()).Should().BeTrue();
        (await invoicesPage.GetInvoiceNumberCountFromAllPagesAsync()).Should().Be(initialInvoiceCount);
    }

    [Test]
    public async Task T04_CreateInvoice_EmptyFields()
    {
        const string expectedInvoiceNumberError = "Invoice number is required.";
        const string expectedCompanyNameError = "Company name is required.";
        const string expectedInvoiceDateError = "Invoice date is required.";
        const string expectedEmissionCategoryError = "Emission category is required.";
        const string expectedTotalCostError = "Total cost is required.";
        const string expectedCurrencyError = "Currency is required.";
        const string expectedDescriptionError = "Description is required.";
        const string expectedQuantityError = "Quantity is required.";
        const string expectedUnitPriceError = "Unit price is required.";
        const string expectedUnitOfMeasureError = "Unit of measure is required.";
        const string expectedEmissionTypeError = "Emission type is required.";

        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();
        await createInvoicePage.ClickCreateInvoiceBtnAsync();

        (await createInvoicePage.GetInvoiceNumberErrorAsync()).Should().Be(expectedInvoiceNumberError);
        (await createInvoicePage.GetCompanyNameErrorAsync()).Should().Be(expectedCompanyNameError);
        (await createInvoicePage.IsAddressErrorVisibleAsync()).Should().BeFalse();
        (await createInvoicePage.GetInvoiceDateErrorAsync()).Should().Be(expectedInvoiceDateError);
        (await createInvoicePage.GetEmissionCategoryErrorAsync()).Should().Be(expectedEmissionCategoryError);
        (await createInvoicePage.GetTotalCostErrorAsync()).Should().Be(expectedTotalCostError);
        (await createInvoicePage.GetCurrencyErrorAsync()).Should().Be(expectedCurrencyError);
        (await createInvoicePage.GetDescription1ErrorAsync()).Should().Be(expectedDescriptionError);
        (await createInvoicePage.GetQuantity1ErrorAsync()).Should().Be(expectedQuantityError);
        (await createInvoicePage.GetUnitPrice1ErrorAsync()).Should().Be(expectedUnitPriceError);
        (await createInvoicePage.GetUnit1ErrorAsync()).Should().Be(expectedUnitOfMeasureError);
        (await createInvoicePage.GetEmissionType1ErrorAsync()).Should().Be(expectedEmissionTypeError);
    }

    [Test]
    public async Task T05_CreateInvoice_InvoiceNumberTooLong()
    {
        const string expectedInvoiceNumberError = "Invoice number must be at most 64 characters.";
        var randomInvoiceNumber = GenerateRandomString(65);

        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();
        await createInvoicePage.FillInvoiceNumberAsync(randomInvoiceNumber);
        await createInvoicePage.ClickCreateInvoiceBtnAsync();

        (await createInvoicePage.GetInvoiceNumberErrorAsync()).Should().Be(expectedInvoiceNumberError);
    }

    [Test]
    public async Task T06_CreateInvoice_CompanyNameTooLong()
    {
        const string expectedCompanyNameError = "Company name must be at most 256 characters.";
        var randomCompanyName = GenerateRandomString(257);

        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();
        await createInvoicePage.FillCompanyNameAsync(randomCompanyName);
        await createInvoicePage.ClickCreateInvoiceBtnAsync();

        (await createInvoicePage.GetCompanyNameErrorAsync()).Should().Be(expectedCompanyNameError);
    }

    [Test]
    public async Task T07_CreateInvoice_AddressTooLong()
    {
        const string expectedAddressError = "Address must be at most 512 characters.";
        var randomAddress = GenerateRandomString(513);

        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();
        await createInvoicePage.FillAddressAsync(randomAddress);
        await createInvoicePage.ClickCreateInvoiceBtnAsync();

        (await createInvoicePage.GetAddressErrorAsync()).Should().Be(expectedAddressError);
    }

    [Test]
    public async Task T08_CreateInvoice_DescriptionTooLong()
    {
        const string expectedDescriptionError = "Description must be at most 512 characters.";
        var randomDescription = GenerateRandomString(513);

        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();
        await createInvoicePage.FillDescriptionAsync(randomDescription);
        await createInvoicePage.ClickCreateInvoiceBtnAsync();

        (await createInvoicePage.GetDescription1ErrorAsync()).Should().Be(expectedDescriptionError);
    }

    [Test]
    public async Task T09_CreateInvoice_EmissionCategoryOptions()
    {
        var categoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await categoriesPage.OpenAsync();
        var expectedOptions = await categoriesPage.GetAllDisplayNamesWithGhgScopesAsync();

        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();
        await createInvoicePage.ClickEmissionCategoryDropdownAsync();

        var categoryOptions = await createInvoicePage.GetEmissionCategoryOptionsAsync();
        categoryOptions.Should().Contain(expectedOptions);
    }

    [Test]
    public async Task T10_CreateInvoice_CurrencyOptions()
    {
        var expectedCurrencyOptions = new[] { "USD", "CAD", "EUR" };

        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();
        await createInvoicePage.ClickCurrencyDropdownAsync();

        var currencyOptions = await createInvoicePage.GetCurrencyOptionsAsync();

        foreach (var expectedCurrencyOption in expectedCurrencyOptions)
        {
            currencyOptions.Should().Contain(
                expectedCurrencyOption,
                because: $"currency dropdown is missing option: {expectedCurrencyOption}");
        }
    }

    [Test]
    public async Task T11_CreateInvoice_EmissionTypeOptions()
    {
        var expectedEmissionTypeOptions = new[] { "On-site diesel combustion" };

        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();
        await createInvoicePage.ClickEmissionType1DropdownAsync();

        var emissionTypeOptions = await createInvoicePage.GetEmissionType1OptionsAsync();

        foreach (var expectedEmissionTypeOption in expectedEmissionTypeOptions)
        {
            emissionTypeOptions.Should().Contain(
                expectedEmissionTypeOption,
                because: $"emission type dropdown is missing option: {expectedEmissionTypeOption}");
        }
    }

    [Test]
    public async Task T12_CreateInvoice_UnitOfMeasureOptions()
    {
        var expectedUnitOfMeasureOptions = new[] { "Litre (L)", "US Gallon (US_GAL)" };

        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();
        await createInvoicePage.ClickUnit1DropdownAsync();

        var unitOfMeasureOptions = await createInvoicePage.GetUnit1OptionsAsync();

        foreach (var expectedUnitOfMeasureOption in expectedUnitOfMeasureOptions)
        {
            unitOfMeasureOptions.Should().Contain(
                expectedUnitOfMeasureOption,
                because: $"unit of measure dropdown is missing option: {expectedUnitOfMeasureOption}");
        }
    }

    [Test]
    public async Task T13_CreateInvoice_ProjectOptions()
    {
        var expectedProjectOptions = await SqlHelper.GetProjectNamesAsync();

        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();
        await createInvoicePage.ClickProjectDropdownAsync();

        var projectOptions = await createInvoicePage.GetProjectOptionsAsync();

        foreach (var expectedProjectOption in expectedProjectOptions)
        {
            projectOptions.Should().Contain(
                expectedProjectOption,
                because: $"project dropdown is missing option: {expectedProjectOption}");
        }
    }

    [Test]
    public async Task T14_CreateInvoice_AddRemoveRow()
    {
        var createInvoicePage = new CreateInvoicePage(Fixture.Page);
        await createInvoicePage.OpenAsync();

        (await createInvoicePage.GetLineNumberCountAsync()).Should().Be(1);
        (await createInvoicePage.IsRemoveRowBtnDisabledAsync(1)).Should().BeTrue();

        await createInvoicePage.ClickAddRowBtnAsync();

        (await createInvoicePage.GetLineNumberCountAsync()).Should().Be(2);
        (await createInvoicePage.IsRemoveRowBtnDisabledAsync(1)).Should().BeFalse();
        (await createInvoicePage.IsRemoveRowBtnDisabledAsync(2)).Should().BeFalse();

        await createInvoicePage.ClickRemoveRowBtnAsync(2);

        (await createInvoicePage.GetLineNumberCountAsync()).Should().Be(1);
        (await createInvoicePage.IsRemoveRowBtnDisabledAsync(1)).Should().BeTrue();
    }

    [Test]
    public async Task T15_CreateInvoice_CorrectData()
    {
        var invoiceNumber = string.Empty;

        try
        {
            const string expectedStatus = "DRAFT";
            const string expectedSource = "Manual";

            var invoicesPage = new InvoicesPage(Fixture.Page);
            await invoicesPage.OpenAsync();
            var initialInvoiceCount = await invoicesPage.GetInvoiceNumberCountFromAllPagesAsync();

            var createInvoicePage = await invoicesPage.ClickCreateBtnAsync();
            var filledInvoice = await createInvoicePage.FillAllFieldsAsync(
                invoiceNumber: GenerateAlphanumericString(64),
                companyName: GenerateAlphanumericString(256),
                address: GenerateAlphanumericString(512),
                description: GenerateAlphanumericString(512));
            invoiceNumber = filledInvoice.InvoiceNumber;
            invoicesPage = await createInvoicePage.SaveInvoiceAsync();

            (await invoicesPage.GetInvoiceNumberCountFromAllPagesAsync()).Should().Be(initialInvoiceCount + 1);
            (await invoicesPage.IsInvoiceNumberInGridAsync(invoiceNumber)).Should().BeTrue();

            var gridRow = await invoicesPage.GetInvoiceGridRowAsync(invoiceNumber);
            gridRow.Should().NotBeNull();
            gridRow!.Project.Should().Be(filledInvoice.Project);
            gridRow.Company.Should().Be(filledInvoice.CompanyName);
            gridRow.Date.Should().Be(
                DateTime.Parse(filledInvoice.InvoiceDate, CultureInfo.InvariantCulture)
                    .ToString("MMM d, yyyy", CultureInfo.InvariantCulture));
            gridRow.Status.Should().Be(expectedStatus);
            gridRow.Source.Should().Be(expectedSource);
        }
        finally
        {
            if (!string.IsNullOrEmpty(invoiceNumber))
                await SqlHelper.DeleteInvoiceByInvoiceNumberAsync(invoiceNumber);
        }
    }

    private static string GenerateAlphanumericString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(0, length).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }
}
