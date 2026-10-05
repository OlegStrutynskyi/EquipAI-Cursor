using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class EmissionTypesTests : BaseTest
{
    [Test]
    public async Task T01_EmissionType_DefaultView()
    {
        const string expectedPageTitle = "Emission Types";
        const string expectedMessage = "Manage fuel and emission type reference data used by imports and factors.";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();

        (await emissionTypesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await emissionTypesPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await emissionTypesPage.IsAddEmissionTypeBtnVisibleAsync()).Should().BeTrue();
        (await emissionTypesPage.IsAddEmissionTypeBtnEnabledAsync()).Should().BeTrue();
        (await emissionTypesPage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_EmissionType_ClickAddEmissionTypeBtn()
    {
        const string expectedPageTitle = "Add Emission Type";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();

        (await addEmissionTypePage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T03_EmissionType_GridColumns()
    {
        var expectedColumns = new[]
        {
            "CODE",
            "DISPLAY NAME",
            "DEFAULT UNIT",
            "ACTIONS",
        };

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();

        (await emissionTypesPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T04_EmissionType_GridRecords()
    {
        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();

        var expectedRows = (await SqlHelper.GetEmissionTypeGridRowsAsync())
            .Select(row => (row.Code, row.DisplayName, row.DefaultUnit))
            .ToList();
        var actualRows = await emissionTypesPage.GetAllGridRecordsAsync();

        actualRows.Should().HaveCount(expectedRows.Count);
        actualRows.Should().Equal(expectedRows);
    }

    [Test]
    public async Task T05_EmissionType_GridSorting()
    {
        var columns = new[] { "CODE", "DISPLAY NAME", "DEFAULT UNIT" };

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();

        var emissionTypes = (await SqlHelper.GetEmissionTypeGridRowsAsync())
            .Select(row => (row.Code, row.DisplayName, row.DefaultUnit))
            .ToList();

        foreach (var column in columns)
        {
            await emissionTypesPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(
                emissionTypesPage,
                ExpectedSortedEmissionTypes(emissionTypes, column, ascending: true),
                $"{column} ASC");

            await emissionTypesPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(
                emissionTypesPage,
                ExpectedSortedEmissionTypes(emissionTypes, column, ascending: false),
                $"{column} DESC");

            await emissionTypesPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(
                emissionTypesPage,
                ExpectedSortedEmissionTypes(emissionTypes, "CODE", ascending: true),
                $"{column} reset to CODE ASC");
        }
    }

    [Test]
    public async Task T06_EmissionType_Add_DefaultView()
    {
        const string expectedPageTitle = "Add Emission Type";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();

        (await addEmissionTypePage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await addEmissionTypePage.IsCodeInputVisibleAsync()).Should().BeTrue();
        (await addEmissionTypePage.IsDisplayNameInputVisibleAsync()).Should().BeTrue();
        (await addEmissionTypePage.IsDefaultUnitDropdownVisibleAsync()).Should().BeTrue();
        (await addEmissionTypePage.IsDefaultEmissionCategoryDropdownVisibleAsync()).Should().BeTrue();
        (await addEmissionTypePage.IsCreateEmissionTypeBtnVisibleAsync()).Should().BeTrue();
        (await addEmissionTypePage.IsCancelBtnVisibleAsync()).Should().BeTrue();
    }
    
    [Test]
    public async Task T07_EmissionType_Add_UnitDropdown()
    {
        var expectedOptions = new[]
        {
            "KWH — Kilowatt-hour",
            "SQFT — Square foot",
            "T — Tonne",
            "US_GAL — US Gallon",
        };

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();

        var defaultUnitOptions = await addEmissionTypePage.GetDefaultUnitOptionsAsync();
        defaultUnitOptions.Should().BeEquivalentTo(expectedOptions);
    }

    [Test]
    public async Task T08_EmissionType_Add_CategoryDropdown()
    {
        var categoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await categoriesPage.OpenAsync();
        var expectedOptions = await categoriesPage.GetAllDisplayNamesWithGhgScopesAsync();

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();

        var categoryOptions = await addEmissionTypePage.GetDefaultEmissionCategoryOptionsAsync();
        categoryOptions.Should().Contain(expectedOptions);
    }

    [Test]
    public async Task T09_EmissionType_Add_EmptyFields()
    {
        const string expectedCodeError = "Code is required.";
        const string expectedDisplayNameError = "Display name is required.";
        const string expectedDefaultUnitError = "Default unit is required.";
        const string expectedDefaultCategoryError = "Default category is required.";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
        await addEmissionTypePage.ClickCreateEmissionTypeBtnAsync();

        (await addEmissionTypePage.GetCodeErrorAsync()).Should().Be(expectedCodeError);
        (await addEmissionTypePage.GetDisplayNameErrorAsync()).Should().Be(expectedDisplayNameError);
        (await addEmissionTypePage.GetDefaultUnitErrorAsync()).Should().Be(expectedDefaultUnitError);
        (await addEmissionTypePage.GetDefaultCategoryErrorAsync()).Should().Be(expectedDefaultCategoryError);
    }

    [Test]
    public async Task T10_EmissionType_Add_ExistingCode()
    {
        const string code = Config.SetupCode1;
        const string expectedAlertMessage = "Code should be unique.";
        var displayName = $"Type{DateTime.Now:yyyyMMddHHmmss}";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
        await addEmissionTypePage.FillCodeAsync(code);
        await addEmissionTypePage.FillDisplayNameAsync(displayName);
        await addEmissionTypePage.SelectDefaultUnitByCodeAsync(Config.SetupDefaultUnitCode1);
        await addEmissionTypePage.SelectRandomDefaultEmissionCategoryAsync();
        await addEmissionTypePage.ClickCreateEmissionTypeBtnAsync();

        (await addEmissionTypePage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T11_EmissionType_Add_CodeTooLong()
    {
        const string expectedCodeError = "Code must be at most 64 characters.";
        var randomCode = GenerateRandomString(65);

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
        await addEmissionTypePage.FillCodeAsync(randomCode);
        await addEmissionTypePage.ClickCreateEmissionTypeBtnAsync();

        (await addEmissionTypePage.GetCodeErrorAsync()).Should().Be(expectedCodeError);
    }

    [Test]
    public async Task T12_EmissionType_Add_DisplayNameTooLong()
    {
        const string expectedDisplayNameError = "Display name must be at most 256 characters.";
        var randomDisplayName = GenerateRandomString(257);

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
        await addEmissionTypePage.FillDisplayNameAsync(randomDisplayName);
        await addEmissionTypePage.ClickCreateEmissionTypeBtnAsync();

        (await addEmissionTypePage.GetDisplayNameErrorAsync()).Should().Be(expectedDisplayNameError);
    }

    [Test]
    public async Task T13_EmissionType_Add_ClickCancel()
    {
        const string expectedPageTitle = "Emission Types";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var code = $"Code{stamp}";
        var displayName = $"Type{stamp}";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
        await addEmissionTypePage.FillCodeAsync(code);
        await addEmissionTypePage.FillDisplayNameAsync(displayName);
        await addEmissionTypePage.SelectDefaultUnitByCodeAsync(Config.SetupDefaultUnitCode1);
        await addEmissionTypePage.SelectRandomDefaultEmissionCategoryAsync();
        emissionTypesPage = await addEmissionTypePage.ClickCancelBtnAsync();

        (await emissionTypesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await emissionTypesPage.IsCodeInGridAsync(code)).Should().BeFalse();
    }

    [Test]
    public async Task T14_EmissionType_Add_Success()
    {
        const string expectedPageTitle = "Emission Types";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var codePrefix = $"Code{stamp}";
        var displayNamePrefix = $"Type{stamp}";
        var code = codePrefix + GenerateRandomAlphanumericString(64 - codePrefix.Length);
        var displayName = displayNamePrefix + GenerateRandomAlphanumericString(256 - displayNamePrefix.Length);

        try
        {
            var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
            await emissionTypesPage.OpenAsync();
            var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
            await addEmissionTypePage.FillCodeAsync(code);
            await addEmissionTypePage.FillDisplayNameAsync(displayName);
            await addEmissionTypePage.SelectDefaultUnitByCodeAsync(Config.SetupDefaultUnitCode1);
            await addEmissionTypePage.SelectRandomDefaultEmissionCategoryAsync();
            emissionTypesPage = await addEmissionTypePage.CreateEmissionTypeAsync();

            (await emissionTypesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            (await emissionTypesPage.IsCodeInGridAsync(code)).Should().BeTrue();
            (await emissionTypesPage.IsDisplayNameInGridAsync(displayName)).Should().BeTrue();
        }
        finally
        {
            await SqlHelper.DeleteEmissionTypeByCodeAsync(code);
        }
    }

    [Test]
    public async Task T15_EmissionType_ClickEditBtn()
    {
        const string expectedPageTitle = "Edit Emission Type";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var editEmissionTypePage = await emissionTypesPage.ClickEditBtnAsync(Config.SetupCode1);

        (await editEmissionTypePage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T16_EmissionType_Edit_DefaultView()
    {
        const string expectedPageTitle = "Edit Emission Type";
        var expectedDefaultUnit = $"{Config.SetupDefaultUnitCode1} — {Config.SetupDefaultUnitName1}";
        var expectedDefaultEmissionCategory = Config.SetupDefaultEmissionCategory1;

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var (code, displayName) = await emissionTypesPage.GetCodeAndDisplayNameAsync(Config.SetupCode1);
        var editEmissionTypePage = await emissionTypesPage.ClickEditBtnAsync(Config.SetupCode1);

        (await editEmissionTypePage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await editEmissionTypePage.GetSubtitleAsync()).Should().Be(code);
        (await editEmissionTypePage.GetCodeAsync()).Should().Be(code);
        (await editEmissionTypePage.GetDisplayNameAsync()).Should().Be(displayName);
        (await editEmissionTypePage.GetDefaultUnitAsync()).Should().Be(expectedDefaultUnit);
        (await editEmissionTypePage.GetDefaultEmissionCategoryAsync()).Should().Be(expectedDefaultEmissionCategory);
        (await editEmissionTypePage.IsSaveEmissionTypeBtnVisibleAsync()).Should().BeTrue();
        (await editEmissionTypePage.IsSaveEmissionTypeBtnEnabledAsync()).Should().BeTrue();
        (await editEmissionTypePage.IsCancelBtnVisibleAsync()).Should().BeTrue();
        (await editEmissionTypePage.IsCancelBtnEnabledAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T17_EmissionType_Edit_EmptyFields()
    {
        const string expectedCodeError = "Code is required.";
        const string expectedDisplayNameError = "Display name is required.";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var editEmissionTypePage = await emissionTypesPage.ClickEditBtnAsync(Config.SetupCode1);
        await editEmissionTypePage.ClearCodeAsync();
        await editEmissionTypePage.ClearDisplayNameAsync();
        await editEmissionTypePage.ClickSaveEmissionTypeBtnAsync();

        (await editEmissionTypePage.GetCodeErrorAsync()).Should().Be(expectedCodeError);
        (await editEmissionTypePage.GetDisplayNameErrorAsync()).Should().Be(expectedDisplayNameError);
    }

    [Test]
    public async Task T18_EmissionType_Edit_ExistingCode()
    {
        const string expectedAlertMessage = "Code should be unique.";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var editEmissionTypePage = await emissionTypesPage.ClickEditBtnAsync(Config.SetupCode1);
        await editEmissionTypePage.FillCodeAsync(Config.SetupCode2);
        await editEmissionTypePage.ClickSaveEmissionTypeBtnAsync();

        (await editEmissionTypePage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T19_EmissionType_Edit_CodeTooLong()
    {
        const string expectedCodeError = "Code must be at most 64 characters.";
        var randomCode = GenerateRandomString(65);

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var editEmissionTypePage = await emissionTypesPage.ClickEditBtnAsync(Config.SetupCode1);
        await editEmissionTypePage.FillCodeAsync(randomCode);
        await editEmissionTypePage.ClickSaveEmissionTypeBtnAsync();

        (await editEmissionTypePage.GetCodeErrorAsync()).Should().Be(expectedCodeError);
    }

    [Test]
    public async Task T20_EmissionType_Edit_DisplayNameTooLong()
    {
        const string expectedDisplayNameError = "Display name must be at most 256 characters.";
        var randomDisplayName = GenerateRandomString(257);

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var editEmissionTypePage = await emissionTypesPage.ClickEditBtnAsync(Config.SetupCode1);
        await editEmissionTypePage.FillDisplayNameAsync(randomDisplayName);
        await editEmissionTypePage.ClickSaveEmissionTypeBtnAsync();

        (await editEmissionTypePage.GetDisplayNameErrorAsync()).Should().Be(expectedDisplayNameError);
    }

    [Test]
    public async Task T21_EmissionType_Edit_ClickCancel()
    {
        const string expectedPageTitle = "Emission Types";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var code = $"Code{stamp}";
        var displayName = $"Type{stamp}";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var editEmissionTypePage = await emissionTypesPage.ClickEditBtnAsync(Config.SetupCode1);
        await editEmissionTypePage.FillCodeAsync(code);
        await editEmissionTypePage.FillDisplayNameAsync(displayName);
        emissionTypesPage = await editEmissionTypePage.ClickCancelBtnAsync();

        (await emissionTypesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await emissionTypesPage.IsCodeInGridAsync(Config.SetupCode1)).Should().BeTrue();
        (await emissionTypesPage.IsDisplayNameInGridAsync(Config.SetupEmissionTypeName1)).Should().BeTrue();
    }

    [Test]
    public async Task T22_EmissionType_Edit_Success()
    {
        const string expectedPageTitle = "Emission Types";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var codePrefix = $"Code{stamp}";
        var displayNamePrefix = $"Type{stamp}";
        var code = codePrefix + GenerateRandomAlphanumericString(64 - codePrefix.Length);
        var displayName = displayNamePrefix + GenerateRandomAlphanumericString(256 - displayNamePrefix.Length);
        var expectedDefaultUnit = $"{Config.SetupDefaultUnitCode2} — {Config.SetupDefaultUnitName2}";
        object? emissionTypeId = null;

        try
        {
            await SqlHelper.RestoreEmissionTypeByCodeAsync(Config.SetupCode1);
            emissionTypeId = await SqlHelper.GetEmissionTypeIdByCodeAsync(Config.SetupCode1);

            var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
            await emissionTypesPage.OpenAsync();
            var editEmissionTypePage = await emissionTypesPage.ClickEditBtnAsync(Config.SetupCode1);
            await editEmissionTypePage.FillCodeAsync(code);
            await editEmissionTypePage.FillDisplayNameAsync(displayName);
            await editEmissionTypePage.SelectDefaultUnitByCodeAsync(Config.SetupDefaultUnitCode2);
            await editEmissionTypePage.SelectDefaultEmissionCategoryAsync(Config.SetupDefaultEmissionCategory2);
            emissionTypesPage = await editEmissionTypePage.SaveEmissionTypeAsync();

            (await emissionTypesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            (await emissionTypesPage.IsCodeInGridAsync(code)).Should().BeTrue();
            (await emissionTypesPage.IsDisplayNameInGridAsync(displayName)).Should().BeTrue();
            (await emissionTypesPage.GetDefaultUnitByCodeAsync(code)).Should().Be(expectedDefaultUnit);
        }
        finally
        {
            if (emissionTypeId is not null)
                await SqlHelper.RestoreEmissionTypeAsync(
                    emissionTypeId,
                    Config.SetupCode1,
                    Config.SetupEmissionTypeName1,
                    Config.SetupDefaultUnitCode1,
                    Config.SetupDefaultEmissionCategory1);
        }
    }

    [Test]
    public async Task T23_EmissionType_Deactivate_Click()
    {
        const string expectedTitle = "Deactivate Emission Type";
        var expectedMessage = $"Deactivate emission type {Config.SetupCode1}? It will no longer be available for new imports.";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        await emissionTypesPage.ClickDeactivateBtnAsync(Config.SetupCode1);

        (await emissionTypesPage.IsDeactivateDialogVisibleAsync()).Should().BeTrue();
        (await emissionTypesPage.GetDeactivateDialogTitleAsync()).Should().Be(expectedTitle);
        (await emissionTypesPage.GetDeactivateDialogMessageAsync()).Should().Be(expectedMessage);
        (await emissionTypesPage.IsDeactivateDialogCancelBtnVisibleAsync()).Should().BeTrue();
        (await emissionTypesPage.IsDeactivateDialogConfirmBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T24_EmissionType_Deactivate_Cancel()
    {
        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        await emissionTypesPage.ClickDeactivateBtnAsync(Config.SetupCode1);
        await emissionTypesPage.ClickDeactivateDialogCancelBtnAsync();

        (await emissionTypesPage.IsDeactivateDialogVisibleAsync()).Should().BeFalse();
        (await emissionTypesPage.IsCodeInGridAsync(Config.SetupCode1)).Should().BeTrue();
    }

    [Test]
    public async Task T25_EmissionType_Deactivate_Success()
    {
        try
        {
            await SqlHelper.RestoreEmissionTypeByCodeAsync(Config.SetupCode3);

            var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
            await emissionTypesPage.OpenAsync();
            await emissionTypesPage.ClickDeactivateBtnAsync(Config.SetupCode3);
            await emissionTypesPage.ClickDeactivateDialogConfirmBtnAsync(Config.SetupCode3);

            (await emissionTypesPage.IsDeactivateDialogVisibleAsync()).Should().BeFalse();
            (await emissionTypesPage.IsCodeInGridAsync(Config.SetupCode3)).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.RestoreEmissionTypeByCodeAsync(Config.SetupCode3);
        }
    }

    private static async Task AssertCurrentPageSortedAsync(
        EmissionTypesPage emissionTypesPage,
        IReadOnlyList<(string Code, string DisplayName, string DefaultUnit)> expected,
        string because)
    {
        var actual = await emissionTypesPage.GetCurrentPageGridRecordsAsync();
        actual.Should().Equal(expected.Take(actual.Count), because);
    }

    private static List<(string Code, string DisplayName, string DefaultUnit)> ExpectedSortedEmissionTypes(
        IReadOnlyList<(string Code, string DisplayName, string DefaultUnit)> emissionTypes,
        string column,
        bool ascending)
    {
        Func<(string Code, string DisplayName, string DefaultUnit), string> key = column switch
        {
            "CODE" => row => row.Code,
            "DISPLAY NAME" => row => row.DisplayName,
            "DEFAULT UNIT" => row => row.DefaultUnit,
            _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Unknown emission types grid column."),
        };

        var ordered = ascending
            ? emissionTypes
                .OrderBy(key, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.Code, StringComparer.OrdinalIgnoreCase)
            : emissionTypes
                .OrderByDescending(key, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.Code, StringComparer.OrdinalIgnoreCase);

        return ordered.ToList();
    }
}
