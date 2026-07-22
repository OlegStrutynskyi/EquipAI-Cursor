using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class _01_Setup_Data : BaseTest
{
    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T01_Setup_AddProject()
    {
        var isDeleted = await SqlHelper.TryGetProjectIsDeletedByNameAsync(Config.SetupProjectName1);

        if (isDeleted is not null)
        {
            if (isDeleted == false)
                Assert.Pass("Test Project exists.");

            await SqlHelper.RestoreProjectByNameAsync(Config.SetupProjectName1);
            (await SqlHelper.ProjectExistsByNameAsync(Config.SetupProjectName1)).Should().BeTrue();
            Assert.Pass("Test Project updated.");
        }

        await SqlHelper.CreateSetupTestProjectAsync();

        (await SqlHelper.ProjectExistsByNameAsync(Config.SetupProjectName1)).Should().BeTrue();
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T02_Setup_CreateManualInvoice()
    {
        const string invoiceNumber = Config.SetupInvoiceNumber1;
        const string companyName = Config.SetupCompanyName1;
        const string address = Config.SetupInvoiceAddress1;
        const string invoiceDate = "02/06/2026";
        const string invoiceCategory = "Fuel";
        const string totalCost = "1562.99";
        const string currency = "USD";
        const string description = Config.SetupInvoiceLineDescription1;
        const string quantity = "421.29";
        const string unitPrice = "3.71";
        const string emissionType = "On-site diesel combustion";
        const string unitOfMeasure = "US Gallon (US_GAL)";

        var invoiceDateForInput = DateTime
            .ParseExact(invoiceDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
            .ToString("yyyy-MM-dd");

        var invoicesPage = new InvoicesPage(Fixture.Page);
        await invoicesPage.OpenAsync();

        if (await invoicesPage.IsInvoiceNumberInGridAsync(invoiceNumber))
            Assert.Pass("Invoice already exists.");

        if (await SqlHelper.InvoiceExistsByNumberAsync(invoiceNumber))
        {
            await SqlHelper.RestoreInvoiceByNumberAsync(invoiceNumber);
            await invoicesPage.OpenAsync();
            (await invoicesPage.IsInvoiceNumberInGridAsync(invoiceNumber)).Should().BeTrue();
            return;
        }

        var createInvoicePage = await invoicesPage.ClickCreateBtnAsync();
        await createInvoicePage.FillInvoiceFormAsync(
            invoiceNumber,
            companyName,
            address,
            invoiceDateForInput,
            invoiceCategory,
            totalCost,
            currency,
            description,
            quantity,
            unitPrice,
            Config.SetupProjectName1,
            emissionType,
            unitOfMeasure);
        invoicesPage = await createInvoicePage.SaveInvoiceAsync();

        (await invoicesPage.IsInvoiceNumberInGridAsync(invoiceNumber)).Should().BeTrue();
        Console.WriteLine("Invoice created.");
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T03_Setup_CreateUnit1()
    {
        const string code = Config.SetupCode1;
        const string displayName = Config.SetupUnitName1;

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();

        if (await unitsPage.IsCodeInGridAsync(code))
            Assert.Pass("Unit 1 already exists.");

        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.FillCodeAsync(code);
        await addUnitPage.FillDisplayNameAsync(displayName);
        unitsPage = await addUnitPage.CreateUnitAsync();

        (await unitsPage.IsCodeInGridAsync(code)).Should().BeTrue();
    }


    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T04_Setup_CreateUnit2()
    {
        const string code = Config.SetupCode2;
        const string displayName = Config.SetupUnitName2;

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();

        if (await unitsPage.IsCodeInGridAsync(code))
            Assert.Pass("Unit 2 already exists.");

        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.FillCodeAsync(code);
        await addUnitPage.FillDisplayNameAsync(displayName);
        unitsPage = await addUnitPage.CreateUnitAsync();

        (await unitsPage.IsCodeInGridAsync(code)).Should().BeTrue();
    }
}
