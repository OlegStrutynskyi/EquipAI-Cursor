using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class UnitsTests : BaseTest
{
    [Test]
    public async Task T01_Units_DefaultView()
    {
        const string expectedPageTitle = "Units";
        const string expectedMessage = "Manage canonical units of measure for imports and emission factors.";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();

        (await unitsPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await unitsPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await unitsPage.IsAddUnitBtnVisibleAsync()).Should().BeTrue();
        (await unitsPage.IsAddUnitBtnEnabledAsync()).Should().BeTrue();
        (await unitsPage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Units_ClickAddUnitBtn()
    {
        const string expectedPageTitle = "Add unit";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();

        (await addUnitPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T03_Units_AddUnit_DefaultView()
    {
        const string expectedPageTitle = "Add unit";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();

        (await addUnitPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await addUnitPage.IsCodeInputVisibleAsync()).Should().BeTrue();
        (await addUnitPage.IsDisplayNameInputVisibleAsync()).Should().BeTrue();
        (await addUnitPage.IsCreateUnitBtnVisibleAsync()).Should().BeTrue();
        (await addUnitPage.IsCancelBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T04_Units_AddUnit_EmptyFields()
    {
        const string expectedCodeError = "Code is required.";
        const string expectedDisplayNameError = "Display name is required.";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.ClickCreateUnitBtnAsync();

        (await addUnitPage.GetCodeErrorAsync()).Should().Be(expectedCodeError);
        (await addUnitPage.GetDisplayNameErrorAsync()).Should().Be(expectedDisplayNameError);
    }

    [Test]
    public async Task T05_Units_AddUnit_ExistingCode()
    {
        const string code = Config.SetupCode1;
        const string expectedAlertMessage = "Code should be unique.";
        var displayName = $"Unit{DateTime.Now:yyyyMMddHHmmss}";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.FillCodeAsync(code);
        await addUnitPage.FillDisplayNameAsync(displayName);
        await addUnitPage.ClickCreateUnitBtnAsync();

        (await addUnitPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T06_Units_AddUnit_CodeTooLong()
    {
        const string expectedCodeError = "Code must be at most 32 characters.";
        var randomCode = GenerateRandomString(33);

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.FillCodeAsync(randomCode);
        await addUnitPage.ClickCreateUnitBtnAsync();

        (await addUnitPage.GetCodeErrorAsync()).Should().Be(expectedCodeError);
    }

    [Test]
    public async Task T07_Units_AddUnit_DisplayNameTooLong()
    {
        const string expectedDisplayNameError = "Display name must be at most 128 characters.";
        var randomDisplayName = GenerateRandomString(129);

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.FillDisplayNameAsync(randomDisplayName);
        await addUnitPage.ClickCreateUnitBtnAsync();

        (await addUnitPage.GetDisplayNameErrorAsync()).Should().Be(expectedDisplayNameError);
    }

    [Test]
    public async Task T08_Units_AddUnit_ClickCancel()
    {
        const string expectedPageTitle = "Units";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var code = $"Code{stamp}";
        var displayName = $"Unit{stamp}";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.FillCodeAsync(code);
        await addUnitPage.FillDisplayNameAsync(displayName);
        unitsPage = await addUnitPage.ClickCancelBtnAsync();

        (await unitsPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await unitsPage.IsCodeInGridAsync(code)).Should().BeFalse();
    }

    [Test]
    public async Task T09_Units_AddUnit_Success()
    {
        const string expectedPageTitle = "Units";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var codePrefix = $"Code{stamp}";
        var displayNamePrefix = $"Unit{stamp}";
        var code = codePrefix + GenerateRandomString(32 - codePrefix.Length);
        var displayName = displayNamePrefix + GenerateRandomString(128 - displayNamePrefix.Length);

        try
        {
            var unitsPage = new UnitsPage(Fixture.Page);
            await unitsPage.OpenAsync();
            var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
            await addUnitPage.FillCodeAsync(code);
            await addUnitPage.FillDisplayNameAsync(displayName);
            unitsPage = await addUnitPage.CreateUnitAsync();

            (await unitsPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            (await unitsPage.IsCodeInGridAsync(code)).Should().BeTrue();
            (await unitsPage.IsDisplayNameInGridAsync(displayName)).Should().BeTrue();
        }
        finally
        {
            await SqlHelper.DeleteUnitOfMeasureByCodeAsync(code);
        }
    }

    [Test]
    public async Task T10_Units_ClickEditBtn()
    {
        const string expectedPageTitle = "Edit unit";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var editUnitPage = await unitsPage.ClickEditBtnAsync(Config.SetupCode1);

        (await editUnitPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T11_Units_Edit_DefaultView()
    {
        const string expectedPageTitle = "Edit unit";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var (code, displayName) = await unitsPage.GetCodeAndDisplayNameAsync(Config.SetupCode1);
        var editUnitPage = await unitsPage.ClickEditBtnAsync(Config.SetupCode1);

        (await editUnitPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await editUnitPage.GetSubtitleAsync()).Should().Be(code);
        (await editUnitPage.GetCodeAsync()).Should().Be(code);
        (await editUnitPage.GetDisplayNameAsync()).Should().Be(displayName);
        (await editUnitPage.IsSaveUnitBtnVisibleAsync()).Should().BeTrue();
        (await editUnitPage.IsSaveUnitBtnEnabledAsync()).Should().BeTrue();
        (await editUnitPage.IsCancelBtnVisibleAsync()).Should().BeTrue();
        (await editUnitPage.IsCancelBtnEnabledAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T12_Units_Edit_EmptyFields()
    {
        const string expectedCodeError = "Code is required.";
        const string expectedDisplayNameError = "Display name is required.";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var editUnitPage = await unitsPage.ClickEditBtnAsync(Config.SetupCode1);
        await editUnitPage.ClearCodeAsync();
        await editUnitPage.ClearDisplayNameAsync();
        await editUnitPage.ClickSaveUnitBtnAsync();

        (await editUnitPage.GetCodeErrorAsync()).Should().Be(expectedCodeError);
        (await editUnitPage.GetDisplayNameErrorAsync()).Should().Be(expectedDisplayNameError);
    }

    [Test]
    public async Task T13_Units_Edit_ExistingCode()
    {
        const string expectedAlertMessage = "Code should be unique.";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var editUnitPage = await unitsPage.ClickEditBtnAsync(Config.SetupCode1);
        await editUnitPage.FillCodeAsync(Config.SetupCode2);
        await editUnitPage.ClickSaveUnitBtnAsync();

        (await editUnitPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T14_Units_Edit_CodeTooLong()
    {
        const string expectedCodeError = "Code must be at most 32 characters.";
        var randomCode = GenerateRandomString(33);

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var editUnitPage = await unitsPage.ClickEditBtnAsync(Config.SetupCode1);
        await editUnitPage.FillCodeAsync(randomCode);
        await editUnitPage.ClickSaveUnitBtnAsync();

        (await editUnitPage.GetCodeErrorAsync()).Should().Be(expectedCodeError);
    }

    [Test]
    public async Task T15_Units_Edit_DisplayNameTooLong()
    {
        const string expectedDisplayNameError = "Display name must be at most 128 characters.";
        var randomDisplayName = GenerateRandomString(129);

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var editUnitPage = await unitsPage.ClickEditBtnAsync(Config.SetupCode1);
        await editUnitPage.FillDisplayNameAsync(randomDisplayName);
        await editUnitPage.ClickSaveUnitBtnAsync();

        (await editUnitPage.GetDisplayNameErrorAsync()).Should().Be(expectedDisplayNameError);
    }

    [Test]
    public async Task T16_Units_Edit_ClickCancel()
    {
        const string expectedPageTitle = "Units";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var code = $"Code{stamp}";
        var displayName = $"Unit{stamp}";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var editUnitPage = await unitsPage.ClickEditBtnAsync(Config.SetupCode1);
        await editUnitPage.FillCodeAsync(code);
        await editUnitPage.FillDisplayNameAsync(displayName);
        unitsPage = await editUnitPage.ClickCancelBtnAsync();

        (await unitsPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await unitsPage.IsCodeInGridAsync(Config.SetupCode1)).Should().BeTrue();
        (await unitsPage.IsDisplayNameInGridAsync(Config.SetupUnitName1)).Should().BeTrue();
    }

    [Test]
    public async Task T17_Units_Edit_Success()
    {
        const string expectedPageTitle = "Units";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var codePrefix = $"Code{stamp}";
        var displayNamePrefix = $"Unit{stamp}";
        var code = codePrefix + GenerateRandomString(32 - codePrefix.Length);
        var displayName = displayNamePrefix + GenerateRandomString(128 - displayNamePrefix.Length);
        object? unitId = null;

        try
        {
            unitId = await SqlHelper.GetUnitOfMeasureIdByCodeAsync(Config.SetupCode1);

            var unitsPage = new UnitsPage(Fixture.Page);
            await unitsPage.OpenAsync();
            var editUnitPage = await unitsPage.ClickEditBtnAsync(Config.SetupCode1);
            await editUnitPage.FillCodeAsync(code);
            await editUnitPage.FillDisplayNameAsync(displayName);
            unitsPage = await editUnitPage.SaveUnitAsync();

            (await unitsPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            (await unitsPage.IsCodeInGridAsync(code)).Should().BeTrue();
            (await unitsPage.IsDisplayNameInGridAsync(displayName)).Should().BeTrue();
        }
        finally
        {
            if (unitId is not null)
                await SqlHelper.RestoreUnitOfMeasureAsync(unitId, Config.SetupCode1, Config.SetupUnitName1);
        }
    }

    [Test]
    public async Task T18_Units_Deactivate_Click()
    {
        const string expectedTitle = "Deactivate unit";
        var expectedMessage = $"Deactivate unit {Config.SetupCode1}? It will no longer be available for new imports.";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        await unitsPage.ClickDeactivateBtnAsync(Config.SetupCode1);

        (await unitsPage.IsDeactivateDialogVisibleAsync()).Should().BeTrue();
        (await unitsPage.GetDeactivateDialogTitleAsync()).Should().Be(expectedTitle);
        (await unitsPage.GetDeactivateDialogMessageAsync()).Should().Be(expectedMessage);
        (await unitsPage.IsDeactivateDialogCancelBtnVisibleAsync()).Should().BeTrue();
        (await unitsPage.IsDeactivateDialogConfirmBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T19_Units_Deactivate_Cancel()
    {
        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        await unitsPage.ClickDeactivateBtnAsync(Config.SetupCode1);
        await unitsPage.ClickDeactivateDialogCancelBtnAsync();

        (await unitsPage.IsDeactivateDialogVisibleAsync()).Should().BeFalse();
        (await unitsPage.IsCodeInGridAsync(Config.SetupCode1)).Should().BeTrue();
    }

    [Test]
    public async Task T20_Units_Deactivate_Success()
    {
        try
        {
            var unitsPage = new UnitsPage(Fixture.Page);
            await unitsPage.OpenAsync();
            await unitsPage.ClickDeactivateBtnAsync(Config.SetupCode2);
            await unitsPage.ClickDeactivateDialogConfirmBtnAsync(Config.SetupCode2);

            (await unitsPage.IsDeactivateDialogVisibleAsync()).Should().BeFalse();
            (await unitsPage.IsCodeInGridAsync(Config.SetupCode2)).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.RestoreUnitOfMeasureByCodeAsync(Config.SetupCode2);
        }
    }

    [Test]
    public async Task T21_Units_Deactivate_UsedInEmissionType()
    {
        const string expectedAlertMessage =
            "Cannot deactivate a unit of measure that is an emission type's default unit.";
        var expectedDefaultUnit = $"{Config.SetupCode1} — {Config.SetupUnitName1}";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();

        if (!await emissionTypesPage.IsDefaultUnitInGridAsync(expectedDefaultUnit))
            Assert.Fail("Setup is not complete. Run test 'T05_Setup_CreateEmissionType1'.");

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        await unitsPage.ClickDeactivateBtnAsync(Config.SetupCode1);
        await unitsPage.ClickDeactivateDialogConfirmBtnExpectingErrorAsync();

        (await unitsPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }
}
