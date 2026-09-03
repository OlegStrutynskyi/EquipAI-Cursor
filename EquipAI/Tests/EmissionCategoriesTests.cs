using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class EmissionCategoriesTests : BaseTest
{
    [Test]
    public async Task T01_EmissionCategories_DefaultView()
    {
        const string expectedPageTitle = "Emission Categories";
        const string expectedMessage = "Edit display names and GHG scopes for the stable emission category catalog.";

        var emissionCategoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await emissionCategoriesPage.OpenAsync();

        (await emissionCategoriesPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await emissionCategoriesPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await emissionCategoriesPage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_EmissionCategories_Grid()
    {
        var expectedColumns = new[]
        {
            "DISPLAY NAME",
            "GHG SCOPE",
            "ACTIONS",
        };
        var expectedRows = new[]
        {
            ("Equipment", "Scope 2"),
            ("External Fuel", "Scope 3"),
            ("Internal Fuel", "Scope 1"),
            ("Material", "Scope 3"),
            ("Waste", "Scope 3"),
        };

        var emissionCategoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await emissionCategoriesPage.OpenAsync();

        (await emissionCategoriesPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
        (await emissionCategoriesPage.GetGridRowCountAsync()).Should().Be(5);
        (await emissionCategoriesPage.DoesEachRowHaveEditButtonAsync()).Should().BeTrue();

        foreach (var (displayName, ghgScope) in expectedRows)
        {
            (await emissionCategoriesPage.IsEditButtonVisibleForRowAsync(displayName, ghgScope)).Should().BeTrue();
        }
    }

    [Test]
    public async Task T03_EmissionCategories_ClickEditBtn()
    {
        const string expectedPageTitle = "Edit Emission Category";

        var emissionCategoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await emissionCategoriesPage.OpenAsync();
        var editEmissionCategoryPage = await emissionCategoriesPage.ClickEditBtnInFirstRowAsync();

        (await editEmissionCategoryPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T04_EmissionCategories_Edit_DefaultView()
    {
        const string expectedPageTitle = "Edit Emission Category";

        var emissionCategoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await emissionCategoriesPage.OpenAsync();
        var (displayNameFromGrid, ghgScopeFromGrid) = await emissionCategoriesPage.GetFirstRowDisplayNameAndScopeAsync();
        var editEmissionCategoryPage = await emissionCategoriesPage.ClickEditBtnInFirstRowAsync();
        var lastWordFromDisplayName = displayNameFromGrid.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();
        var expectedRollupKeyText = $"Rollup key: {lastWordFromDisplayName} (catalog-owned, read-only)";

        (await editEmissionCategoryPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await editEmissionCategoryPage.GetSubtitleAsync()).Should().BeEquivalentTo(displayNameFromGrid);
        (await editEmissionCategoryPage.GetDisplayNameAsync()).Should().Be(displayNameFromGrid);
        (await editEmissionCategoryPage.GetGhgScopeAsync()).Should().Be(ghgScopeFromGrid);
        (await editEmissionCategoryPage.GetRollupKeyTextAsync()).Should().BeEquivalentTo(expectedRollupKeyText);
        (await editEmissionCategoryPage.IsSaveCategoryBtnVisibleAsync()).Should().BeTrue();
        (await editEmissionCategoryPage.IsCancelBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T05_EmissionCategories_Edit_EmptyDisplayName()
    {
        const string expectedDisplayNameError = "Display name is required.";

        var emissionCategoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await emissionCategoriesPage.OpenAsync();
        var editEmissionCategoryPage = await emissionCategoriesPage.ClickEditBtnInFirstRowAsync();
        await editEmissionCategoryPage.ClearDisplayNameAsync();
        await editEmissionCategoryPage.ClickSaveCategoryBtnAsync();

        (await editEmissionCategoryPage.GetDisplayNameErrorAsync()).Should().Be(expectedDisplayNameError);
    }

    [Test]
    public async Task T06_EmissionCategories_Edit_DisplayNameTooLong()
    {
        const string expectedDisplayNameError = "Display name must be at most 256 characters.";
        var tooLongDisplayName = GenerateRandomString(257);

        var emissionCategoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await emissionCategoriesPage.OpenAsync();
        var editEmissionCategoryPage = await emissionCategoriesPage.ClickEditBtnInFirstRowAsync();
        await editEmissionCategoryPage.ClearDisplayNameAsync();
        await editEmissionCategoryPage.FillDisplayNameAsync(tooLongDisplayName);
        await editEmissionCategoryPage.ClickSaveCategoryBtnAsync();

        (await editEmissionCategoryPage.GetDisplayNameErrorAsync()).Should().Be(expectedDisplayNameError);
    }

    [Test]
    public async Task T07_EmissionCategories_Edit_Success()
    {
        var updatedDisplayName = $"Name{DateTime.Now:yyyyMMddHHmmss}";
        object? emissionCategoryId = null;

        var emissionCategoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await emissionCategoriesPage.OpenAsync();
        var (originalDisplayName, originalGhgScope) = await emissionCategoriesPage.GetFirstRowDisplayNameAndScopeAsync();
        var originalScopeDigit = int.Parse(originalGhgScope.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last());

        try
        {
            emissionCategoryId = await SqlHelper.GetEmissionCategoryIdByDisplayNameAsync(originalDisplayName);

            var editEmissionCategoryPage = await emissionCategoriesPage.ClickEditBtnInFirstRowAsync();
            await editEmissionCategoryPage.ClearDisplayNameAsync();
            await editEmissionCategoryPage.FillDisplayNameAsync(updatedDisplayName);
            var updatedGhgScope = await editEmissionCategoryPage.SelectAnyOtherGhgScopeAsync(originalGhgScope);
            emissionCategoriesPage = await editEmissionCategoryPage.SaveCategoryAsync();

            (await emissionCategoriesPage.IsEditButtonVisibleForRowAsync(updatedDisplayName, updatedGhgScope)).Should().BeTrue();

            editEmissionCategoryPage = await emissionCategoriesPage.ClickEditBtnAsync(updatedDisplayName, updatedGhgScope);
            (await editEmissionCategoryPage.GetSubtitleAsync()).Should().BeEquivalentTo(originalDisplayName);
            (await editEmissionCategoryPage.GetDisplayNameAsync()).Should().Be(updatedDisplayName);
            (await editEmissionCategoryPage.GetGhgScopeAsync()).Should().Be(updatedGhgScope);
        }
        finally
        {
            if (emissionCategoryId is not null)
            {
                await SqlHelper.RestoreEmissionCategoryAsync(
                    emissionCategoryId,
                    originalDisplayName,
                    originalScopeDigit);
            }
        }
    }
}
