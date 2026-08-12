using EquipAI.Pages;
using EquipAI.Utils;
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
            "ACTIONS",
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
            "ACTIONS",
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

    [Test]
    public async Task T12_Aliases_Add_ClickCancel()
    {
        const string expectedPageTitle = "Aliases";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var aliasText = $"Alias{stamp}";

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();
        await addAliasPage.FillAliasTextAsync(aliasText);
        await addAliasPage.SelectUnitOfMeasureAsync(Config.SetupCode1);
        aliasesPage = await addAliasPage.ClickCancelBtnAsync();

        (await aliasesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await aliasesPage.IsAliasTextInGridAsync(aliasText)).Should().BeFalse();
    }

    [Test]
    public async Task T13_Aliases_Add_Success_Unit()
    {
        const string expectedPageTitle = "Aliases";
        const string expectedContext = "Data ingestion";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var aliasText = $"Alias{stamp}";

        try
        {
            var aliasesPage = new AliasesPage(Fixture.Page);
            await aliasesPage.OpenAsync();
            var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();
            await addAliasPage.SelectContextAsync(expectedContext);
            await addAliasPage.FillAliasTextAsync(aliasText);
            await addAliasPage.SelectTargetKindAsync("Unit of measure");
            var expectedResolvesTo = await addAliasPage.SelectUnitOfMeasureAsync(Config.SetupCode1);
            aliasesPage = await addAliasPage.CreateAliasAsync();

            (await aliasesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            await aliasesPage.ClickUnitsOfMeasureTabAsync();

            var gridRow = await aliasesPage.GetUnitAliasGridRowAsync(aliasText);
            gridRow.Should().NotBeNull();
            gridRow!.Context.Should().Be(expectedContext);
            gridRow.AliasText.Should().Be(aliasText);
            gridRow.ResolvesTo.Should().Be(expectedResolvesTo);
        }
        finally
        {
            await SqlHelper.DeleteReferenceAliasByAliasTextAsync(aliasText);
        }
    }

    [Test]
    public async Task T14_Aliases_Add_Success_EmissionType()
    {
        const string expectedPageTitle = "Aliases";
        const string expectedContext = "Data ingestion";
        const string expectedFactorSource = "—";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var aliasText = $"Alias{stamp}";

        try
        {
            var aliasesPage = new AliasesPage(Fixture.Page);
            await aliasesPage.OpenAsync();
            var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();
            await addAliasPage.SelectContextAsync(expectedContext);
            await addAliasPage.FillAliasTextAsync(aliasText);
            await addAliasPage.SelectTargetKindAsync("Emission type");
            var expectedResolvesTo = await addAliasPage.SelectEmissionTypeAsync(Config.SetupCode1);
            aliasesPage = await addAliasPage.CreateAliasAsync();

            (await aliasesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            await aliasesPage.ClickEmissionTypesTabAsync();

            var gridRow = await aliasesPage.GetEmissionTypeAliasGridRowAsync(aliasText);
            gridRow.Should().NotBeNull();
            gridRow!.Context.Should().Be(expectedContext);
            gridRow.AliasText.Should().Be(aliasText);
            gridRow.FactorSource.Should().Be(expectedFactorSource);
            gridRow.ResolvesTo.Should().Be(expectedResolvesTo);
        }
        finally
        {
            await SqlHelper.DeleteReferenceAliasByAliasTextAsync(aliasText);
        }
    }

    [Test]
    public async Task T15_Aliases_Add_Success_CatalogEmissionType()
    {
        const string expectedPageTitle = "Aliases";
        const string expectedContext = "Catalog factor mapping";
        const string expectedFactorSource = "EPA";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var aliasText = $"Alias{stamp}";

        try
        {
            var aliasesPage = new AliasesPage(Fixture.Page);
            await aliasesPage.OpenAsync();
            var addAliasPage = await aliasesPage.ClickAddAliasBtnAsync();
            await addAliasPage.SelectContextAsync(expectedContext);
            await addAliasPage.FillAliasTextAsync(aliasText);
            await addAliasPage.SelectTargetKindAsync("Emission type");
            var expectedResolvesTo = await addAliasPage.SelectEmissionTypeAsync(Config.SetupCode2);
            await addAliasPage.SelectEpaFactorSourceAsync();
            aliasesPage = await addAliasPage.CreateAliasAsync();

            (await aliasesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            await aliasesPage.ClickEmissionTypesTabAsync();

            var gridRow = await aliasesPage.GetEmissionTypeAliasGridRowAsync(aliasText);
            gridRow.Should().NotBeNull();
            gridRow!.Context.Should().Be(expectedContext);
            gridRow.AliasText.Should().Be(aliasText);
            gridRow.FactorSource.Should().Be(expectedFactorSource);
            gridRow.ResolvesTo.Should().Be(expectedResolvesTo);
        }
        finally
        {
            await SqlHelper.DeleteReferenceAliasByAliasTextAsync(aliasText);
        }
    }

    [Test]
    public async Task T16_Aliases_ClickEdit()
    {
        const string expectedPageTitle = "Edit alias";

        var editAliasPage = await OpenEditAliasUnitPageAsync();

        (await editAliasPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T17_Aliases_Edit_Unit_DefaultView()
    {
        const string expectedPageTitle = "Edit alias";
        const string expectedContext = "Data ingestion";
        const string expectedTargetKind = "Unit of measure";

        var editAliasPage = await OpenEditAliasUnitPageAsync();

        (await editAliasPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await editAliasPage.GetSubtitleAsync()).Should().Be(Config.SetupAliasUnit);
        (await editAliasPage.GetContextAsync()).Should().Be(expectedContext);
        (await editAliasPage.GetAliasTextAsync()).Should().Be(Config.SetupAliasUnit);
        (await editAliasPage.GetTargetKindAsync()).Should().Be(expectedTargetKind);
        (await editAliasPage.GetUnitOfMeasureAsync()).Should().Match(value =>
            value.Equals(Config.SetupCode1, StringComparison.Ordinal)
            || value.StartsWith(Config.SetupCode1 + " —", StringComparison.Ordinal)
            || value.StartsWith(Config.SetupCode1 + " -", StringComparison.Ordinal));
        (await editAliasPage.IsBackToAliasesBtnVisibleAsync()).Should().BeTrue();
        (await editAliasPage.IsSaveAliasBtnVisibleAsync()).Should().BeTrue();
        (await editAliasPage.IsCancelBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T18_Aliases_Edit_Unit_EmptyAliasText()
    {
        const string expectedAliasTextError = "Alias text is required.";

        var editAliasPage = await OpenEditAliasUnitPageAsync();
        await editAliasPage.ClearAliasTextAsync();
        await editAliasPage.ClickSaveAliasBtnAsync();

        (await editAliasPage.GetAliasTextErrorAsync()).Should().Be(expectedAliasTextError);
    }

    [Test]
    public async Task T19_Aliases_Edit_Unit_ExistingAliasText()
    {
        const string expectedAlertMessage = "Alias text should be unique.";

        var editAliasPage = await OpenEditAliasUnitPageAsync();
        await editAliasPage.ClearAliasTextAsync();
        await editAliasPage.FillAliasTextAsync(Config.SetupAliasEmissionType);
        await editAliasPage.ClickSaveAliasBtnAsync();

        (await editAliasPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T20_Aliases_Edit_Unit_AliasTextTooLong()
    {
        const string expectedAliasTextError = "Alias text must be at most 256 characters.";
        var randomAliasText = GenerateRandomString(257);

        var editAliasPage = await OpenEditAliasUnitPageAsync();
        await editAliasPage.ClearAliasTextAsync();
        await editAliasPage.FillAliasTextAsync(randomAliasText);
        await editAliasPage.ClickSaveAliasBtnAsync();

        (await editAliasPage.GetAliasTextErrorAsync()).Should().Be(expectedAliasTextError);
    }

    [Test]
    public async Task T21_Aliases_Edit_Unit_ClickCancel()
    {
        const string expectedPageTitle = "Aliases";
        const string updatedAliasText = $"{Config.SetupAliasUnit} UPDATED";
        const string expectedResolvesTo = $"{Config.SetupCode1} — {Config.SetupUnitName1}";

        var editAliasPage = await OpenEditAliasUnitPageAsync();
        await editAliasPage.FillAliasTextAsync(updatedAliasText);
        await editAliasPage.SelectUnitOfMeasureAsync(Config.SetupCode2);
        var aliasesPage = await editAliasPage.ClickCancelBtnAsync();

        (await aliasesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        await aliasesPage.ClickUnitsOfMeasureTabAsync();
        (await aliasesPage.IsAliasTextInGridAsync(updatedAliasText)).Should().BeFalse();

        var gridRow = await aliasesPage.GetUnitAliasGridRowAsync(Config.SetupAliasUnit);
        gridRow.Should().NotBeNull();
        gridRow!.AliasText.Should().Be(Config.SetupAliasUnit);
        gridRow.ResolvesTo.Should().Be(expectedResolvesTo);
    }

    [Test]
    public async Task T22_Aliases_Edit_Unit_Success()
    {
        const string expectedPageTitle = "Aliases";
        var updatedAliasText = $"{Config.SetupAliasUnit} UPDATED";
        var expectedResolvesTo = $"{Config.SetupCode2} — {Config.SetupUnitName2}";
        object? aliasId = null;

        try
        {
            aliasId = await SqlHelper.GetReferenceAliasIdByAliasTextAsync(Config.SetupAliasUnit);

            var editAliasPage = await OpenEditAliasUnitPageAsync();
            await editAliasPage.FillAliasTextAsync(updatedAliasText);
            await editAliasPage.SelectUnitOfMeasureAsync(Config.SetupCode2);
            var aliasesPage = await editAliasPage.SaveAliasAsync();

            (await aliasesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            await aliasesPage.ClickUnitsOfMeasureTabAsync();
            (await aliasesPage.IsAliasTextInGridAsync(Config.SetupAliasUnit)).Should().BeFalse();

            var gridRow = await aliasesPage.GetUnitAliasGridRowAsync(updatedAliasText);
            gridRow.Should().NotBeNull();
            gridRow!.AliasText.Should().Be(updatedAliasText);
            gridRow.ResolvesTo.Should().Be(expectedResolvesTo);
        }
        finally
        {
            if (aliasId is not null)
                await SqlHelper.RestoreReferenceAliasUnitAsync(aliasId, Config.SetupAliasUnit, Config.SetupCode1);
        }
    }

    [Test]
    public async Task T23_Aliases_Edit_EmissionType_DefaultView()
    {
        const string expectedPageTitle = "Edit alias";
        const string expectedContext = "Data ingestion";
        const string expectedTargetKind = "Emission type";

        var editAliasPage = await OpenEditAliasEmissionTypePageAsync();

        (await editAliasPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await editAliasPage.GetSubtitleAsync()).Should().Be(Config.SetupAliasEmissionType);
        (await editAliasPage.GetContextAsync()).Should().Be(expectedContext);
        (await editAliasPage.GetAliasTextAsync()).Should().Be(Config.SetupAliasEmissionType);
        (await editAliasPage.GetTargetKindAsync()).Should().Be(expectedTargetKind);
        (await editAliasPage.GetEmissionTypeAsync()).Should().Match(value =>
            value.Equals(Config.SetupCode1, StringComparison.Ordinal)
            || value.StartsWith(Config.SetupCode1 + " —", StringComparison.Ordinal)
            || value.StartsWith(Config.SetupCode1 + " -", StringComparison.Ordinal));
        (await editAliasPage.IsPreparedFactorsPreviewTextVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T24_Aliases_Edit_EmissionType_Success()
    {
        const string expectedPageTitle = "Aliases";
        var updatedAliasText = $"{Config.SetupAliasEmissionType} UPDATED";
        var expectedResolvesTo = $"{Config.SetupCode2} — {Config.SetupEmissionTypeName2}";
        object? aliasId = null;

        try
        {
            aliasId = await SqlHelper.GetReferenceAliasIdByAliasTextAsync(Config.SetupAliasEmissionType);

            var editAliasPage = await OpenEditAliasEmissionTypePageAsync();
            await editAliasPage.FillAliasTextAsync(updatedAliasText);
            await editAliasPage.SelectEmissionTypeAsync(Config.SetupCode2);
            var aliasesPage = await editAliasPage.SaveAliasAsync();

            (await aliasesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            await aliasesPage.ClickEmissionTypesTabAsync();

            var gridRow = await aliasesPage.GetEmissionTypeAliasGridRowAsync(updatedAliasText);
            gridRow.Should().NotBeNull();
            gridRow!.AliasText.Should().Be(updatedAliasText);
            gridRow.ResolvesTo.Should().Be(expectedResolvesTo);
            (await aliasesPage.IsAliasTextInGridAsync(Config.SetupAliasEmissionType)).Should().BeFalse();
        }
        finally
        {
            if (aliasId is not null)
                await SqlHelper.RestoreReferenceAliasEmissionTypeAsync(
                    aliasId,
                    Config.SetupAliasEmissionType,
                    Config.SetupCode1);
        }
    }

    [Test]
    public async Task T25_Aliases_Edit_Factor_DefaultView()
    {
        const string expectedPageTitle = "Edit alias";
        const string expectedContext = "Catalog factor mapping";
        const string expectedTargetKind = "Emission type";

        var editAliasPage = await OpenEditAliasFactorPageAsync();

        (await editAliasPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await editAliasPage.GetSubtitleAsync()).Should().Be(Config.SetupAliasFactor1);
        (await editAliasPage.GetContextAsync()).Should().Be(expectedContext);
        (await editAliasPage.GetAliasTextAsync()).Should().Be(Config.SetupAliasFactor1);
        (await editAliasPage.GetTargetKindAsync()).Should().Be(expectedTargetKind);
        (await editAliasPage.GetEmissionTypeAsync()).Should().Match(value =>
            value.Equals(Config.SetupCode1, StringComparison.Ordinal)
            || value.StartsWith(Config.SetupCode1 + " —", StringComparison.Ordinal)
            || value.StartsWith(Config.SetupCode1 + " -", StringComparison.Ordinal));
        (await editAliasPage.IsEpaFactorSourceSelectedAsync()).Should().BeTrue();
        (await editAliasPage.IsPreparedFactorsPreviewTextVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T26_Aliases_Edit_Factor_Success()
    {
        const string expectedPageTitle = "Aliases";
        const string expectedFactorSource = "DEFRA";
        var updatedAliasText = $"{Config.SetupAliasFactor1} UPDATED";
        var expectedResolvesTo = $"{Config.SetupCode2} — {Config.SetupEmissionTypeName2}";
        object? aliasId = null;

        try
        {
            aliasId = await SqlHelper.GetReferenceAliasIdByAliasTextAsync(Config.SetupAliasFactor1);

            var editAliasPage = await OpenEditAliasFactorPageAsync();
            await editAliasPage.FillAliasTextAsync(updatedAliasText);
            await editAliasPage.SelectEmissionTypeAsync(Config.SetupCode2);
            await editAliasPage.SelectDefraFactorSourceAsync();
            var aliasesPage = await editAliasPage.SaveAliasAsync();

            (await aliasesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            await aliasesPage.ClickEmissionTypesTabAsync();

            var gridRow = await aliasesPage.GetEmissionTypeAliasGridRowAsync(updatedAliasText);
            gridRow.Should().NotBeNull();
            gridRow!.AliasText.Should().Be(updatedAliasText);
            gridRow.FactorSource.Should().Be(expectedFactorSource);
            gridRow.ResolvesTo.Should().Be(expectedResolvesTo);
            (await aliasesPage.IsAliasTextInGridAsync(Config.SetupAliasFactor1)).Should().BeFalse();
        }
        finally
        {
            if (aliasId is not null)
                await SqlHelper.RestoreReferenceAliasFactorAsync(
                    aliasId,
                    Config.SetupAliasFactor1,
                    Config.SetupCode1);
        }
    }

    [Test]
    public async Task T27_Aliases_ClickDeactivate()
    {
        const string expectedTitle = "Deactivate alias";
        var expectedMessage =
            $"Deactivate alias \"{Config.SetupAliasUnit}\"? Imports will no longer resolve this text.";

        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        await aliasesPage.ClickUnitsOfMeasureTabAsync();
        await aliasesPage.ClickDeactivateBtnAsync(Config.SetupAliasUnit);

        (await aliasesPage.IsDeactivateDialogVisibleAsync()).Should().BeTrue();
        (await aliasesPage.GetDeactivateDialogTitleAsync()).Should().Be(expectedTitle);
        (await aliasesPage.GetDeactivateDialogMessageAsync()).Should().Be(expectedMessage);
        (await aliasesPage.IsDeactivateDialogCancelBtnVisibleAsync()).Should().BeTrue();
        (await aliasesPage.IsDeactivateDialogCancelBtnEnabledAsync()).Should().BeTrue();
        (await aliasesPage.IsDeactivateDialogConfirmBtnVisibleAsync()).Should().BeTrue();
        (await aliasesPage.IsDeactivateDialogConfirmBtnEnabledAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T28_Aliases_Deactivate_Cancel()
    {
        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        await aliasesPage.ClickUnitsOfMeasureTabAsync();
        await aliasesPage.ClickDeactivateBtnAsync(Config.SetupAliasUnit);
        await aliasesPage.ClickDeactivateDialogCancelBtnAsync();

        (await aliasesPage.IsDeactivateDialogVisibleAsync()).Should().BeFalse();
        (await aliasesPage.IsAliasTextInGridAsync(Config.SetupAliasUnit)).Should().BeTrue();
    }

    [Test]
    public async Task T29_Aliases_Deactivate_Success()
    {
        try
        {
            var aliasesPage = new AliasesPage(Fixture.Page);
            await aliasesPage.OpenAsync();
            await aliasesPage.ClickUnitsOfMeasureTabAsync();
            await aliasesPage.ClickDeactivateBtnAsync(Config.SetupAliasUnit);
            await aliasesPage.ClickDeactivateDialogConfirmBtnAsync(Config.SetupAliasUnit);

            (await aliasesPage.IsDeactivateDialogVisibleAsync()).Should().BeFalse();
            (await aliasesPage.IsAliasTextInGridAsync(Config.SetupAliasUnit)).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.RestoreReferenceAliasByAliasTextAsync(Config.SetupAliasUnit);
        }
    }

    private async Task<EditAliasPage> OpenEditAliasUnitPageAsync()
    {
        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        await aliasesPage.ClickUnitsOfMeasureTabAsync();
        return await aliasesPage.ClickEditBtnAsync(Config.SetupAliasUnit);
    }

    private async Task<EditAliasPage> OpenEditAliasEmissionTypePageAsync()
    {
        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        await aliasesPage.ClickEmissionTypesTabAsync();
        return await aliasesPage.ClickEditBtnAsync(Config.SetupAliasEmissionType);
    }

    private async Task<EditAliasPage> OpenEditAliasFactorPageAsync()
    {
        var aliasesPage = new AliasesPage(Fixture.Page);
        await aliasesPage.OpenAsync();
        await aliasesPage.ClickEmissionTypesTabAsync();
        return await aliasesPage.ClickEditBtnAsync(Config.SetupAliasFactor1);
    }
}
