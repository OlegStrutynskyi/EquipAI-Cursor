using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class FactorImportTests : BaseTest
{
    [Test]
    public async Task T01_FactorImport_DefaultView()
    {
        const string expectedTitle = "Factor Import";
        const string expectedSubtitle =
            "Upload EPA, DEFRA, or Custom emission-factor workbooks into raw staging. Prepared factors are materialized using current catalog mappings.";
        const string expectedMessage1 =
            "Custom files must start with title Custom Emission Factors and the fixed header row (Activity, Fuel, Unit, kgCO2e, gas columns). Pick the library year on upload — it is not read from the sheet.";
        const string expectedMessage2 =
            "Fuel-to-emission-type mappings for factor materialization are edited under Aliases (emission types, CatalogFactorMapping context, FactorSource EPA/DEFRA/Custom). Unit spellings use the global unit aliases. If materialization fails after upload, the raw batch is still kept — fix aliases, then re-upload the same file (or save an alias change) to rematerialize.";

        var factorImportPage = new FactorImportPage(Fixture.Page);
        await factorImportPage.OpenAsync();

        (await factorImportPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await factorImportPage.GetSubtitleAsync()).Should().Be(expectedSubtitle);
        (await factorImportPage.GetMessage1Async()).Should().Be(expectedMessage1);
        (await factorImportPage.GetMessage2Async()).Should().Be(expectedMessage2);
        (await factorImportPage.IsUploadWorkbookLabelVisibleAsync()).Should().BeTrue();
        (await factorImportPage.IsSourceLabelVisibleAsync()).Should().BeTrue();
        (await factorImportPage.IsSourceDropdownVisibleAsync()).Should().BeTrue();
        (await factorImportPage.IsWorkbookLabelVisibleAsync()).Should().BeTrue();
        (await factorImportPage.IsSelectFileAreaVisibleAsync()).Should().BeTrue();
        (await factorImportPage.IsUploadBtnVisibleAsync()).Should().BeTrue();
        (await factorImportPage.IsUploadBtnEnabledAsync()).Should().BeFalse();
        (await factorImportPage.IsRecentBatchesLabelVisibleAsync()).Should().BeTrue();
        (await factorImportPage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_FactorImport_ClickAliasesLink()
    {
        const string expectedTitle = "Aliases";

        var factorImportPage = new FactorImportPage(Fixture.Page);
        await factorImportPage.OpenAsync();
        var aliasesPage = await factorImportPage.ClickAliasesLinkAsync();

        (await aliasesPage.GetPageTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_FactorImport_SourceOptions()
    {
        var expectedOptions = new[] { "EPA", "DEFRA", "CUSTOM" };

        var factorImportPage = new FactorImportPage(Fixture.Page);
        await factorImportPage.OpenAsync();

        (await factorImportPage.GetSourceOptionsAsync()).Should().Equal(expectedOptions);
    }

    [Test]
    public async Task T04_FactorImport_GridColumns()
    {
        var expectedColumns = new[]
        {
            "BATCH ID",
            "SOURCE",
            "YEAR",
            "LIBRARY",
            "LINES",
            "CREATED",
        };

        var factorImportPage = new FactorImportPage(Fixture.Page);
        await factorImportPage.OpenAsync();

        (await factorImportPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T05_FactorImport_YearField()
    {
        var factorImportPage = new FactorImportPage(Fixture.Page);
        await factorImportPage.OpenAsync();

        await factorImportPage.SelectSourceAsync("CUSTOM");
        (await factorImportPage.IsYearLabelVisibleAsync()).Should().BeTrue();
        (await factorImportPage.IsYearInputVisibleAsync()).Should().BeTrue();

        await factorImportPage.SelectSourceAsync("EPA");
        (await factorImportPage.IsYearLabelVisibleAsync()).Should().BeFalse();
        (await factorImportPage.IsYearInputVisibleAsync()).Should().BeFalse();

        await factorImportPage.SelectSourceAsync("DEFRA");
        (await factorImportPage.IsYearLabelVisibleAsync()).Should().BeFalse();
        (await factorImportPage.IsYearInputVisibleAsync()).Should().BeFalse();
    }

    [Test]
    public async Task T06_FactorImport_Import_IncorrectFormat()
    {
        const string expectedAlert = "Only .xlsx files are accepted.";

        var factorImportPage = new FactorImportPage(Fixture.Page);
        await factorImportPage.OpenAsync();
        await factorImportPage.UploadFileAsync("scv-invoice-correct.csv");
        await factorImportPage.ClickUploadBtnAsync();

        (await factorImportPage.GetAlertBodyTextAsync()).Should().Be(expectedAlert);
    }

    [Test]
    public async Task T07_FactorImport_Custom_No1stRow()
    {
        const string expectedAlert = "Custom workbook row 1 must be 'Custom Emission Factors'.";

        var factorImportPage = new FactorImportPage(Fixture.Page);
        await factorImportPage.OpenAsync();
        await factorImportPage.SelectSourceAsync("CUSTOM");
        await factorImportPage.SetYearAsync("2001");
        await factorImportPage.UploadFileAsync("Factor_Custom_2001_No_1stRow.xlsx");
        await factorImportPage.ClickUploadBtnAsync();

        (await factorImportPage.GetAlertBodyTextAsync()).Should().Be(expectedAlert);
    }

    [Test]
    public async Task T08_FactorImport_Custom_NoActivity()
    {
        await AssertCustomHeaderErrorAsync("Factor_Custom_2001_No_Activity.xlsx");
    }

    [Test]
    public async Task T09_FactorImport_Custom_NoFuel()
    {
        await AssertCustomHeaderErrorAsync("Factor_Custom_2001_No_Fuel.xlsx");
    }

    [Test]
    public async Task T10_FactorImport_Custom_NoUnit()
    {
        await AssertCustomHeaderErrorAsync("Factor_Custom_2001_No_Unit.xlsx");
    }

    [Test]
    public async Task T11_FactorImport_Custom_NoKGCO2()
    {
        await AssertCustomHeaderErrorAsync("Factor_Custom_2001_No_KGCO2.xlsx");
    }

    [Test]
    public async Task T12_FactorImport_Custom_NoCO2PerUnit()
    {
        await AssertCustomHeaderErrorAsync("Factor_Custom_2001_No_CO2PerUnit.xlsx");
    }

    [Test]
    public async Task T13_FactorImport_Custom_NoCH4PerUnit()
    {
        await AssertCustomHeaderErrorAsync("Factor_Custom_2001_No_CH4PerUnit.xlsx");
    }

    [Test]
    public async Task T14_FactorImport_Custom_NoN2OPerUnit()
    {
        await AssertCustomHeaderErrorAsync("Factor_Custom_2001_No_N2OPerUnit.xlsx");
    }

    [Test]
    public async Task T15_FactorImport_Custom_OnlyHeaders()
    {
        const string expectedAlert = "The workbook did not contain any eligible factor rows.";

        var factorImportPage = new FactorImportPage(Fixture.Page);
        await factorImportPage.OpenAsync();
        await factorImportPage.SelectSourceAsync("CUSTOM");
        await factorImportPage.SetYearAsync("2001");
        await factorImportPage.UploadFileAsync("Factor_Custom_2001_OnlyHeaders.xlsx");
        await factorImportPage.ClickUploadBtnAsync();

        (await factorImportPage.GetAlertBodyTextAsync()).Should().Be(expectedAlert);
    }

    [Test]
    public async Task T16_FactorImport_Custom_Success()
    {
        const string fileName = "Factor_Custom_2001_Correct.xlsx";
        const string expectedOutcome = "Created";
        const string expectedSource = "CUSTOM";
        const string expectedYear = "2001";
        const string expectedLineCount = "22";
        const string expectedLibrary = "factors-2001";
        var expectedCreatedDate = DateTime.Now.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
        string? batchId = null;

        try
        {
            await SqlHelper.DeleteCustomFactorImportByYearAsync(2001);

            var factorImportPage = new FactorImportPage(Fixture.Page);
            await factorImportPage.OpenAsync();
            await factorImportPage.SelectSourceAsync("CUSTOM");
            await factorImportPage.SetYearAsync("2001");
            await factorImportPage.UploadFileAsync(fileName);
            await factorImportPage.ClickUploadAndWaitForResultAsync();

            (await factorImportPage.IsImportResultLabelVisibleAsync()).Should().BeTrue();
            (await factorImportPage.GetResultOutcomeAsync()).Should().Be(expectedOutcome);
            (await factorImportPage.GetResultSourceAsync()).Should().Be(expectedSource);
            (await factorImportPage.GetResultYearAsync()).Should().Be(expectedYear);
            (await factorImportPage.GetResultLineCountAsync()).Should().Be(expectedLineCount);
            (await factorImportPage.GetResultLibraryAsync()).Should().Be(expectedLibrary);

            batchId = await factorImportPage.GetResultBatchIdAsync();
            batchId.Should().NotBeNullOrWhiteSpace();

            var batchRow = await factorImportPage.GetBatchGridRowByIdAsync(batchId!);
            batchRow.Should().NotBeNull();
            batchRow!.Source.Should().Be(expectedSource);
            batchRow.Year.Should().Be(expectedYear);
            batchRow.Library.Should().Be(expectedLibrary);
            batchRow.Lines.Should().Be(expectedLineCount);
            batchRow.Created.Should().StartWith(expectedCreatedDate);

            var aliasesPage = new AliasesPage(Fixture.Page);
            await aliasesPage.OpenAsync();
            await aliasesPage.ClickEmissionTypesTabAsync();

            var propaneAlias = await aliasesPage.GetEmissionTypeAliasGridRowAsync("Propane");
            propaneAlias.Should().NotBeNull();
            propaneAlias!.Context.Should().Be("Catalog Factor Mapping");

            var editAliasPage = await aliasesPage.ClickEditBtnAsync("Catalog Factor Mapping", "Propane");

            IReadOnlyList<PreparedFactorGridRow> factorRows;
            if (await editAliasPage.IsCustomFactorSourceSelectedAsync())
            {
                factorRows = await editAliasPage.GetPreparedFactorsRowsAsync();
            }
            else
            {
                await editAliasPage.SelectCustomFactorSourceAsync();
                factorRows = await editAliasPage.GetSelectedTargetFactorRowsAsync();
            }

            var propaneFactor = factorRows.FirstOrDefault(r =>
                r.Year == expectedYear
                && r.Unit.Contains("US Gallon", StringComparison.OrdinalIgnoreCase)
                && r.SourceFileLine == "21");
            propaneFactor.Should().NotBeNull();
            propaneFactor!.TCo2ePerActivityUnit.Should().Be("0.00649");
            propaneFactor.TCo2.Should().Be("0.00617");
            propaneFactor.TCh4.Should().Be("0.00027");
            propaneFactor.TN2o.Should().Be("0.00005");
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(batchId))
                await SqlHelper.DeleteCustomFactorImportAsync(batchId);
        }
    }

    private async Task AssertCustomHeaderErrorAsync(string fileName)
    {
        const string expectedAlert =
            "Custom workbook row 2 headers must be: Activity, Fuel, Unit, kgCO2e, kg CO2e of CO2 per unit, kg CO2e of CH4 per unit, kg CO2e of N2O per unit.";

        var factorImportPage = new FactorImportPage(Fixture.Page);
        await factorImportPage.OpenAsync();
        await factorImportPage.SelectSourceAsync("CUSTOM");
        await factorImportPage.SetYearAsync("2001");
        await factorImportPage.UploadFileAsync(fileName);
        await factorImportPage.ClickUploadBtnAsync();

        (await factorImportPage.GetAlertBodyTextAsync()).Should().Be(expectedAlert);
    }
}
