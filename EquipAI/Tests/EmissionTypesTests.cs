using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class EmissionTypesTests : BaseTest
{
    [Test]
    public async Task T01_EmissionType_DefaultView()
    {
        const string expectedPageTitle = "Emission types";
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
        const string expectedPageTitle = "Add emission type";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();

        (await addEmissionTypePage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T03_EmissionType_Add_DefaultView()
    {
        const string expectedPageTitle = "Add emission type";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();

        (await addEmissionTypePage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await addEmissionTypePage.IsCodeInputVisibleAsync()).Should().BeTrue();
        (await addEmissionTypePage.IsDisplayNameInputVisibleAsync()).Should().BeTrue();
        (await addEmissionTypePage.IsDefaultUnitDropdownVisibleAsync()).Should().BeTrue();
        (await addEmissionTypePage.IsCreateEmissionTypeBtnVisibleAsync()).Should().BeTrue();
        (await addEmissionTypePage.IsCancelBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T04_EmissionType_Add_EmptyFields()
    {
        const string expectedCodeError = "Code is required.";
        const string expectedDisplayNameError = "Display name is required.";
        const string expectedDefaultUnitError = "Default unit is required.";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
        await addEmissionTypePage.ClickCreateEmissionTypeBtnAsync();

        (await addEmissionTypePage.GetCodeErrorAsync()).Should().Be(expectedCodeError);
        (await addEmissionTypePage.GetDisplayNameErrorAsync()).Should().Be(expectedDisplayNameError);
        (await addEmissionTypePage.GetDefaultUnitErrorAsync()).Should().Be(expectedDefaultUnitError);
    }

    [Test]
    public async Task T05_EmissionType_Add_ExistingCode()
    {
        const string code = Config.SetupCode1;
        const string expectedAlertMessage = "Code should be unique.";
        var displayName = $"Type{DateTime.Now:yyyyMMddHHmmss}";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
        await addEmissionTypePage.FillCodeAsync(code);
        await addEmissionTypePage.FillDisplayNameAsync(displayName);
        await addEmissionTypePage.SelectDefaultUnitByCodeAsync(Config.SetupCode1);
        await addEmissionTypePage.ClickCreateEmissionTypeBtnAsync();

        (await addEmissionTypePage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T06_EmissionType_Add_CodeTooLong()
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
    public async Task T07_EmissionType_Add_DisplayNameTooLong()
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
    public async Task T08_EmissionType_Add_ClickCancel()
    {
        const string expectedPageTitle = "Emission types";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var code = $"Code{stamp}";
        var displayName = $"Type{stamp}";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
        await addEmissionTypePage.FillCodeAsync(code);
        await addEmissionTypePage.FillDisplayNameAsync(displayName);
        await addEmissionTypePage.SelectDefaultUnitByCodeAsync(Config.SetupCode1);
        emissionTypesPage = await addEmissionTypePage.ClickCancelBtnAsync();

        (await emissionTypesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await emissionTypesPage.IsCodeInGridAsync(code)).Should().BeFalse();
    }

    [Test]
    public async Task T09_EmissionType_Add_Success()
    {
        const string expectedPageTitle = "Emission types";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var codePrefix = $"Code{stamp}";
        var displayNamePrefix = $"Type{stamp}";
        var code = codePrefix + GenerateRandomString(64 - codePrefix.Length);
        var displayName = displayNamePrefix + GenerateRandomString(256 - displayNamePrefix.Length);

        try
        {
            var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
            await emissionTypesPage.OpenAsync();
            var addEmissionTypePage = await emissionTypesPage.ClickAddEmissionTypeBtnAsync();
            await addEmissionTypePage.FillCodeAsync(code);
            await addEmissionTypePage.FillDisplayNameAsync(displayName);
            await addEmissionTypePage.SelectDefaultUnitByCodeAsync(Config.SetupCode1);
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
    public async Task T10_EmissionType_ClickEditBtn()
    {
        const string expectedPageTitle = "Edit emission type";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var editEmissionTypePage = await emissionTypesPage.ClickEditBtnAsync(Config.SetupCode1);

        (await editEmissionTypePage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T11_EmissionType_Edit_DefaultView()
    {
        const string expectedPageTitle = "Edit emission type";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        var (code, displayName) = await emissionTypesPage.GetCodeAndDisplayNameAsync(Config.SetupCode1);
        var editEmissionTypePage = await emissionTypesPage.ClickEditBtnAsync(Config.SetupCode1);

        (await editEmissionTypePage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await editEmissionTypePage.GetSubtitleAsync()).Should().Be(code);
        (await editEmissionTypePage.GetCodeAsync()).Should().Be(code);
        (await editEmissionTypePage.GetDisplayNameAsync()).Should().Be(displayName);
        (await editEmissionTypePage.IsSaveEmissionTypeBtnVisibleAsync()).Should().BeTrue();
        (await editEmissionTypePage.IsSaveEmissionTypeBtnEnabledAsync()).Should().BeTrue();
        (await editEmissionTypePage.IsCancelBtnVisibleAsync()).Should().BeTrue();
        (await editEmissionTypePage.IsCancelBtnEnabledAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T12_EmissionType_Edit_EmptyFields()
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
    public async Task T13_EmissionType_Edit_ExistingCode()
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
    public async Task T14_EmissionType_Edit_CodeTooLong()
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
    public async Task T15_EmissionType_Edit_DisplayNameTooLong()
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
    public async Task T16_EmissionType_Edit_ClickCancel()
    {
        const string expectedPageTitle = "Emission types";
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
    public async Task T17_EmissionType_Edit_Success()
    {
        const string expectedPageTitle = "Emission types";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var codePrefix = $"Code{stamp}";
        var displayNamePrefix = $"Type{stamp}";
        var code = codePrefix + GenerateRandomString(64 - codePrefix.Length);
        var displayName = displayNamePrefix + GenerateRandomString(256 - displayNamePrefix.Length);
        var expectedDefaultUnit = $"{Config.SetupCode2} — {Config.SetupUnitName2}";
        object? emissionTypeId = null;

        try
        {
            emissionTypeId = await SqlHelper.GetEmissionTypeIdByCodeAsync(Config.SetupCode1);

            var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
            await emissionTypesPage.OpenAsync();
            var editEmissionTypePage = await emissionTypesPage.ClickEditBtnAsync(Config.SetupCode1);
            await editEmissionTypePage.FillCodeAsync(code);
            await editEmissionTypePage.FillDisplayNameAsync(displayName);
            await editEmissionTypePage.SelectDefaultUnitByCodeAsync(Config.SetupCode2);
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
                    Config.SetupCode1);
        }
    }

    [Test]
    public async Task T18_EmissionType_Deactivate_Click()
    {
        const string expectedTitle = "Deactivate emission type";
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
    public async Task T19_EmissionType_Deactivate_Cancel()
    {
        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();
        await emissionTypesPage.ClickDeactivateBtnAsync(Config.SetupCode1);
        await emissionTypesPage.ClickDeactivateDialogCancelBtnAsync();

        (await emissionTypesPage.IsDeactivateDialogVisibleAsync()).Should().BeFalse();
        (await emissionTypesPage.IsCodeInGridAsync(Config.SetupCode1)).Should().BeTrue();
    }

    [Test]
    public async Task T20_EmissionType_Deactivate_Success()
    {
        try
        {
            var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
            await emissionTypesPage.OpenAsync();
            await emissionTypesPage.ClickDeactivateBtnAsync(Config.SetupCode1);
            await emissionTypesPage.ClickDeactivateDialogConfirmBtnAsync(Config.SetupCode1);

            (await emissionTypesPage.IsDeactivateDialogVisibleAsync()).Should().BeFalse();
            (await emissionTypesPage.IsCodeInGridAsync(Config.SetupCode1)).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.RestoreEmissionTypeByCodeAsync(Config.SetupCode1);
        }
    }
}
