using EquipAI.Pages;
using FluentAssertions;

namespace EquipAI.Tests;

public class AliasesTests : BaseTest
{
    [Test]
    public async Task T01_Aliases_DefaultView()
    {
        const string expectedPageTitle = "Aliases";
        const string expectedMessage =
            "Manage import-time synonyms for units and emission types during data ingestion and factor catalog import.";

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();

        (await aliasesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await aliasesPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await aliasesPage.IsAddAliasBtnVisibleAsync()).Should().BeTrue();
        (await aliasesPage.IsAddAliasBtnEnabledAsync()).Should().BeTrue();
        (await aliasesPage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Aliases_ClickAddAliasBtn()
    {
        const string expectedPageTitle = "Add alias";

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();

        (await addAliasPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T03_Aliases_GridColumns_Units()
    {
        var expectedColumns = new[]
        {
            "CONTEXT",
            "ALIAS TEXT",
            "RESOLVES TO",
        };

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        await aliasesPage.ClickUnitsOfMeasureTabAsync();

        (await aliasesPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T04_Aliases_GridColumns_EmissionTypes()
    {
        var expectedColumns = new[]
        {
            "CONTEXT",
            "ALIAS TEXT",
            "FACTOR SOURCE",
            "RESOLVES TO",
        };

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        await aliasesPage.ClickEmissionTypesTabAsync();

        (await aliasesPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T05_Aliases_Add_DefaultView()
    {
        const string expectedPageTitle = "Add alias";

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();

        (await addAliasPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await addAliasPage.IsContextDropdownVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsAliasTextInputVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsTargetKindDropdownVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsUnitOfMeasureDropdownVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsCreateAliasBtnVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsCancelBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T06_Aliases_Add_ContextDropdown()
    {
        const string dataIngestionOption = "Data ingestion";
        const string catalogFactorMappingOption = "Catalog factor mapping";
        const string expectedDataIngestionMessage =
            "Maps strings from invoice CSV upload and AI ingestion to units or emission types.";
        const string expectedCatalogFactorMappingMessage =
            "Maps strings from external factor catalogs to emission types (gov fuel labels) or units of measure (gov activity UOM spellings).";

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();

        var contextOptions = await addAliasPage.GetContextOptionsAsync();
        contextOptions.Should().Contain(dataIngestionOption);
        contextOptions.Should().Contain(catalogFactorMappingOption);

        await addAliasPage.SelectContextAsync(dataIngestionOption);
        (await addAliasPage.GetContextHelpMessageAsync()).Should().Be(expectedDataIngestionMessage);

        await addAliasPage.SelectContextAsync(catalogFactorMappingOption);
        (await addAliasPage.GetContextHelpMessageAsync()).Should().Be(expectedCatalogFactorMappingMessage);
    }

    [Test]
    public async Task T07_Aliases_Add_FieldsList()
    {
        const string dataIngestionOption = "Data ingestion";
        const string catalogFactorMappingOption = "Catalog factor mapping";

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();

        await addAliasPage.SelectContextAsync(dataIngestionOption);
        (await addAliasPage.IsAliasTextInputVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsTargetKindDropdownVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsUnitOfMeasureDropdownVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsEmissionTypeDropdownVisibleAsync()).Should().BeFalse();
        (await addAliasPage.IsFactorSourceTitleVisibleAsync()).Should().BeFalse();
        (await addAliasPage.IsFactorSourceMessageVisibleAsync()).Should().BeFalse();
        (await addAliasPage.IsEpaBtnVisibleAsync()).Should().BeFalse();
        (await addAliasPage.IsDefraBtnVisibleAsync()).Should().BeFalse();

        await addAliasPage.SelectContextAsync(catalogFactorMappingOption);
        (await addAliasPage.IsAliasTextInputVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsTargetKindDropdownVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsEmissionTypeDropdownVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsFactorSourceTitleVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsFactorSourceMessageVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsEpaBtnVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsDefraBtnVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsUnitOfMeasureDropdownVisibleAsync()).Should().BeFalse();
    }

    [Test]
    public async Task T08_Aliases_Add_AliasTextTooLong()
    {
        const string expectedAliasTextError = "Alias text must be at most 256 characters.";
        var randomAliasText = GenerateRandomString(257);

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();
        await addAliasPage.FillAliasTextAsync(randomAliasText);
        await addAliasPage.ClickCreateAliasBtnAsync();

        (await addAliasPage.GetAliasTextErrorAsync()).Should().Be(expectedAliasTextError);
    }

    [Test]
    public async Task T09_Aliases_Add_TargetKind()
    {
        const string unitOfMeasureOption = "Unit of measure";
        const string emissionTypeOption = "Emission type";

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();

        var targetKindOptions = await addAliasPage.GetTargetKindOptionsAsync();
        targetKindOptions.Should().Contain(unitOfMeasureOption);
        targetKindOptions.Should().Contain(emissionTypeOption);

        await addAliasPage.SelectTargetKindAsync(unitOfMeasureOption);
        (await addAliasPage.IsUnitOfMeasureDropdownVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsEmissionTypeDropdownVisibleAsync()).Should().BeFalse();

        await addAliasPage.SelectTargetKindAsync(emissionTypeOption);
        (await addAliasPage.IsEmissionTypeDropdownVisibleAsync()).Should().BeTrue();
        (await addAliasPage.IsUnitOfMeasureDropdownVisibleAsync()).Should().BeFalse();
    }

    [Test]
    public async Task T10_Aliases_Add_UnitOfMeasure()
    {
        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var units = await unitsPage.GetAllCodesAndDisplayNamesAsync();
        var expectedOptions = units
            .Select(unit => $"{unit.Code} — {unit.DisplayName}")
            .ToList();

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();
        await addAliasPage.SelectContextAsync("Data ingestion");
        await addAliasPage.SelectTargetKindAsync("Unit of measure");

        var unitOfMeasureOptions = await addAliasPage.GetUnitOfMeasureOptionsAsync();
        unitOfMeasureOptions.Should().Contain(expectedOptions);
    }

    [Test]
    public async Task T11_Aliases_Add_EmptyFields()
    {
        const string dataIngestionOption = "Data ingestion";
        const string catalogFactorMappingOption = "Catalog factor mapping";
        const string expectedAliasTextError = "Alias text is required.";
        const string expectedUnitError = "Unit is required.";
        const string expectedEmissionTypeError = "Emission type is required.";
        const string expectedFactorSourceError = "Factor source is required.";

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();

        await addAliasPage.SelectContextAsync(dataIngestionOption);
        await addAliasPage.ClickCreateAliasBtnAsync();
        (await addAliasPage.GetAliasTextErrorAsync()).Should().Be(expectedAliasTextError);
        (await addAliasPage.GetUnitOfMeasureErrorAsync()).Should().Be(expectedUnitError);

        await addAliasPage.SelectContextAsync(catalogFactorMappingOption);
        await addAliasPage.ClickCreateAliasBtnAsync();
        (await addAliasPage.GetAliasTextErrorAsync()).Should().Be(expectedAliasTextError);
        (await addAliasPage.GetEmissionTypeErrorAsync()).Should().Be(expectedEmissionTypeError);
        (await addAliasPage.GetFactorSourceErrorAsync()).Should().Be(expectedFactorSourceError);
    }
}
