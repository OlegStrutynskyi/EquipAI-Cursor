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
        const string code = Config.SetupCode;
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
    public async Task T06_Units_AddUnit_ClickCancel()
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
    public async Task T07_Units_AddUnit_Success()
    {
        const string expectedPageTitle = "Units";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var code = $"Code{stamp}";
        var displayName = $"Unit{stamp}";

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
}
