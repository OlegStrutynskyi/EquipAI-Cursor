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
        const string emissionCategory = "Fuel";
        const string totalCost = "1562.99";
        const string currency = "USD";
        const string description = Config.SetupInvoiceLineDescription1;
        const string quantity = "421.29";
        const string unitPrice = "3.71";
        const string emissionType = "On-site diesel combustion";
        const string unitOfMeasure = "US Gallon (US_GAL)";
        const string expectedStatus = "DRAFT";
        const string expectedSource = "Manual";

        var invoiceDateForInput = DateTime
            .ParseExact(invoiceDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
            .ToString("yyyy-MM-dd");
        var expectedInvoiceDateDisplay = DateTime
            .ParseExact(invoiceDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
            .ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

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

        var initialInvoiceCount = await invoicesPage.GetInvoiceNumberCountFromAllPagesAsync();

        var createInvoicePage = await invoicesPage.ClickCreateBtnAsync();
        await createInvoicePage.FillInvoiceFormAsync(
            invoiceNumber,
            companyName,
            address,
            invoiceDateForInput,
            emissionCategory,
            totalCost,
            currency,
            description,
            quantity,
            unitPrice,
            Config.SetupProjectName1,
            emissionType,
            unitOfMeasure);
        invoicesPage = await createInvoicePage.SaveInvoiceAsync();

        (await invoicesPage.GetInvoiceNumberCountFromAllPagesAsync()).Should().Be(initialInvoiceCount + 1);
        (await invoicesPage.IsInvoiceNumberInGridAsync(invoiceNumber)).Should().BeTrue();

        var gridRow = await invoicesPage.GetInvoiceGridRowAsync(invoiceNumber);
        gridRow.Should().NotBeNull();
        gridRow!.Project.Should().Be(Config.SetupProjectName1);
        gridRow.Company.Should().Be(companyName);
        gridRow.Date.Should().Be(expectedInvoiceDateDisplay);
        gridRow.Status.Should().Be(expectedStatus);
        gridRow.Source.Should().Be(expectedSource);
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T03_Setup_CreateDefaultUnit1()
    {
        const string code = Config.SetupDefaultUnitCode1;
        const string displayName = Config.SetupDefaultUnitName1;

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();

        if (await unitsPage.IsCodeInGridAsync(code))
            Assert.Pass("Default unit 1 already exists.");

        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.FillCodeAsync(code);
        await addUnitPage.FillDisplayNameAsync(displayName);
        unitsPage = await addUnitPage.CreateUnitAsync();

        (await unitsPage.IsCodeInGridAsync(code)).Should().BeTrue();
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T04_Setup_CreateDefaultUnit2()
    {
        const string code = Config.SetupDefaultUnitCode2;
        const string displayName = Config.SetupDefaultUnitName2;

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();

        if (await unitsPage.IsCodeInGridAsync(code))
            Assert.Pass("Default unit 2 already exists.");

        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.FillCodeAsync(code);
        await addUnitPage.FillDisplayNameAsync(displayName);
        unitsPage = await addUnitPage.CreateUnitAsync();

        (await unitsPage.IsCodeInGridAsync(code)).Should().BeTrue();
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T05_Setup_CreateUnit1()
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
    public async Task T06_Setup_CreateUnit2()
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

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T07_Setup_CreateEmissionType1()
    {
        const string code = Config.SetupCode1;
        const string displayName = Config.SetupEmissionTypeName1;

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();

        if (await emissionTypesPage.IsCodeInGridAsync(code))
            Assert.Pass("Emission type 1 already exists.");

        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
        await addEmissionTypePage.FillCodeAsync(code);
        await addEmissionTypePage.FillDisplayNameAsync(displayName);
        await addEmissionTypePage.SelectDefaultUnitByCodeAsync(Config.SetupDefaultUnitCode1);
        await addEmissionTypePage.SelectDefaultEmissionCategoryAsync(Config.SetupDefaultEmissionCategory1);
        emissionTypesPage = await addEmissionTypePage.CreateEmissionTypeAsync();

        (await emissionTypesPage.IsCodeInGridAsync(code)).Should().BeTrue();
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T08_Setup_CreateEmissionType2()
    {
        const string code = Config.SetupCode2;
        const string displayName = Config.SetupEmissionTypeName2;

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();

        if (await emissionTypesPage.IsCodeInGridAsync(code))
            Assert.Pass("Emission type 2 already exists.");

        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
        await addEmissionTypePage.FillCodeAsync(code);
        await addEmissionTypePage.FillDisplayNameAsync(displayName);
        await addEmissionTypePage.SelectDefaultUnitByCodeAsync(Config.SetupDefaultUnitCode1);
        await addEmissionTypePage.SelectDefaultEmissionCategoryAsync(Config.SetupDefaultEmissionCategory1);
        emissionTypesPage = await addEmissionTypePage.CreateEmissionTypeAsync();

        (await emissionTypesPage.IsCodeInGridAsync(code)).Should().BeTrue();
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T09_Setup_CreateEmissionType3()
    {
        const string code = Config.SetupCode3;
        const string displayName = Config.SetupEmissionTypeName3;

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();

        if (await emissionTypesPage.IsCodeInGridAsync(code))
            Assert.Pass("Emission type 3 already exists.");

        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
        await addEmissionTypePage.FillCodeAsync(code);
        await addEmissionTypePage.FillDisplayNameAsync(displayName);
        await addEmissionTypePage.SelectDefaultUnitByCodeAsync(Config.SetupDefaultUnitCode1);
        await addEmissionTypePage.SelectDefaultEmissionCategoryAsync(Config.SetupDefaultEmissionCategory1);
        emissionTypesPage = await addEmissionTypePage.CreateEmissionTypeAsync();

        (await emissionTypesPage.IsCodeInGridAsync(code)).Should().BeTrue();
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T10_Setup_CreateEmissionTypePropane()
    {
        const string code = Config.SetupCodePropane;
        const string displayName = Config.SetupEmissionNamePropane;

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();

        if (await emissionTypesPage.IsCodeInGridAsync(code))
            Assert.Pass("Emission type Propane already exists.");

        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
        await addEmissionTypePage.FillCodeAsync(code);
        await addEmissionTypePage.FillDisplayNameAsync(displayName);
        await addEmissionTypePage.SelectDefaultUnitByCodeAsync(Config.SetupDefaultUnitCode1);
        await addEmissionTypePage.SelectDefaultEmissionCategoryAsync(Config.SetupDefaultEmissionCategory1);
        emissionTypesPage = await addEmissionTypePage.CreateEmissionTypeAsync();

        (await emissionTypesPage.IsCodeInGridAsync(code)).Should().BeTrue();
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T11_Setup_CreateAliasUnit()
    {
        const string aliasText = Config.SetupAliasUnit;

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        await aliasesPage.ClickUnitsOfMeasureTabAsync();

        if (await aliasesPage.IsAliasTextInGridAsync(aliasText))
            Assert.Pass("Alias already exists.");

        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();
        await addAliasPage.FillAliasTextAsync(aliasText);
        await addAliasPage.SelectTargetKindAsync("Unit of Measure");
        await addAliasPage.SelectUnitOfMeasureAsync(Config.SetupCode1);
        await addAliasPage.FillAliasTextAsync(aliasText);
        aliasesPage = await addAliasPage.CreateAliasAsync();
        await aliasesPage.ClickUnitsOfMeasureTabAsync();

        (await aliasesPage.IsAliasTextInGridAsync(aliasText)).Should().BeTrue();
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T12_Setup_CreateAliasEmissionType()
    {
        const string aliasText = Config.SetupAliasEmissionType;

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        await aliasesPage.ClickEmissionTypesTabAsync();

        if (await aliasesPage.IsAliasTextInGridAsync(aliasText))
            Assert.Pass("Alias already exists.");

        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();
        await addAliasPage.FillAliasTextAsync(aliasText);
        await addAliasPage.SelectContextAsync("Data Ingestion");
        await addAliasPage.SelectTargetKindAsync("Emission Type");
        await addAliasPage.SelectEmissionTypeAsync(Config.SetupCode1);
        await addAliasPage.FillAliasTextAsync(aliasText);
        aliasesPage = await addAliasPage.CreateAliasAsync();
        await aliasesPage.ClickEmissionTypesTabAsync();

        (await aliasesPage.IsAliasTextInGridAsync(aliasText)).Should().BeTrue();
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T13_Setup_CreateAliasFactor1()
    {
        const string aliasText = Config.SetupAliasFactor1;

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        await aliasesPage.ClickEmissionTypesTabAsync();

        if (await aliasesPage.IsAliasTextInGridAsync(aliasText))
            Assert.Pass("Alias already exists.");

        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();
        await addAliasPage.FillAliasTextAsync(aliasText);
        await addAliasPage.SelectContextAsync("Catalog Factor Mapping");
        await addAliasPage.SelectTargetKindAsync("Emission Type");
        await addAliasPage.SelectEmissionTypeAsync(Config.SetupCode1);
        await addAliasPage.SelectEpaFactorSourceAsync();
        await addAliasPage.FillAliasTextAsync(aliasText);
        aliasesPage = await addAliasPage.CreateAliasAsync();
        await aliasesPage.ClickEmissionTypesTabAsync();

        (await aliasesPage.IsAliasTextInGridAsync(aliasText)).Should().BeTrue();
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T14_Setup_CreateAliasFactorPropane()
    {
        const string aliasText = Config.SetupAliasFactorPropane;

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        await aliasesPage.ClickEmissionTypesTabAsync();

        if (await aliasesPage.IsAliasTextInGridAsync(aliasText))
            Assert.Pass("Alias already exists.");

        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();
        await addAliasPage.FillAliasTextAsync(aliasText);
        await addAliasPage.SelectContextAsync("Catalog Factor Mapping");
        await addAliasPage.SelectTargetKindAsync("Emission Type");
        await addAliasPage.SelectEmissionTypeAsync(Config.SetupCodePropane);
        await addAliasPage.SelectEpaFactorSourceAsync();
        await addAliasPage.FillAliasTextAsync(aliasText);
        aliasesPage = await addAliasPage.CreateAliasAsync();
        await aliasesPage.ClickEmissionTypesTabAsync();

        (await aliasesPage.IsAliasTextInGridAsync(aliasText)).Should().BeTrue();
    }

    [Explicit("Manual setup test. Run before the test suite.")]
    [Test]
    public async Task T15_Setup_CreateAliasProject1()
    {
        const string aliasText = Config.SetupAliasProject1;

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        await aliasesPage.ClickProjectsTabAsync();

        if (await aliasesPage.IsAliasTextInGridAsync(aliasText))
            Assert.Pass("Alias already exists.");

        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();
        await addAliasPage.FillAliasTextAsync(aliasText);
        await addAliasPage.SelectTargetKindAsync("Project");
        await addAliasPage.SelectTelemetryProjectAsync(Config.SetupProjectName1);
        await addAliasPage.FillAliasTextAsync(aliasText);
        aliasesPage = await addAliasPage.CreateAliasAsync();
        await aliasesPage.ClickProjectsTabAsync();

        (await aliasesPage.IsAliasTextInGridAsync(aliasText)).Should().BeTrue();
    }
}
