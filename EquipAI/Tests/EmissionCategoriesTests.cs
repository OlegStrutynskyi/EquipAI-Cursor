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
    public async Task T02_EmissionCategories_GridColumns()
    {
        var expectedColumns = new[]
        {
            "DISPLAY NAME",
            "GHG SCOPE",
            "ACTIONS",
        };

        var emissionCategoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await emissionCategoriesPage.OpenAsync();

        (await emissionCategoriesPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T03_EmissionCategories_GridRecords()
    {
        var emissionCategoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await emissionCategoriesPage.OpenAsync();

        var expectedRows = (await SqlHelper.GetEmissionCategoryGridRowsAsync())
            .Select(category => (category.DisplayName, GhgScope: $"Scope {category.GhgScope}"))
            .ToList();

        var actualRows = await emissionCategoriesPage.GetGridRowsAsync();
        actualRows.Should().HaveCount(expectedRows.Count);
        actualRows.Should().Equal(expectedRows);
        (await emissionCategoriesPage.DoesEachRowHaveEditButtonAsync()).Should().BeTrue();

        foreach (var (displayName, ghgScope) in expectedRows)
            (await emissionCategoriesPage.IsEditButtonVisibleForRowAsync(displayName, ghgScope)).Should().BeTrue();
    }

    [Test]
    public async Task T04_EmissionCategories_GridSorting()
    {
        var columns = new[] { "DISPLAY NAME", "GHG SCOPE" };

        var emissionCategoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await emissionCategoriesPage.OpenAsync();

        var categories = (await SqlHelper.GetEmissionCategoryGridRowsAsync())
            .Select(category => (category.DisplayName, GhgScope: $"Scope {category.GhgScope}"))
            .ToList();

        foreach (var column in columns)
        {
            await emissionCategoriesPage.ClickGridColumnAsync(column);
            (await emissionCategoriesPage.GetGridRowsAsync()).Should().Equal(
                ExpectedSortedCategories(categories, column, ascending: true),
                $"{column} ASC");

            await emissionCategoriesPage.ClickGridColumnAsync(column);
            (await emissionCategoriesPage.GetGridRowsAsync()).Should().Equal(
                ExpectedSortedCategories(categories, column, ascending: false),
                $"{column} DESC");

            await emissionCategoriesPage.ClickGridColumnAsync(column);
            (await emissionCategoriesPage.GetGridRowsAsync()).Should().Equal(
                ExpectedSortedCategories(categories, "DISPLAY NAME", ascending: true),
                $"{column} reset to DISPLAY NAME ASC");
        }
    }

    [Test]
    public async Task T05_EmissionCategories_ClickEditBtn()
    {
        const string expectedPageTitle = "Edit Emission Category";

        var emissionCategoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await emissionCategoriesPage.OpenAsync();
        var editEmissionCategoryPage = await emissionCategoriesPage.ClickEditBtnInFirstRowAsync();

        (await editEmissionCategoryPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T06_EmissionCategories_Edit_DefaultView()
    {
        const string expectedPageTitle = "Edit Emission Category";

        const string expectedSubtitle = "BUSINESS_TRAVEL";
        const string expectedRollupKeyText = "Rollup key: BusinessTravel (catalog-owned, read-only)";

        var emissionCategoriesPage = new EmissionCategoriesPage(Fixture.Page);
        await emissionCategoriesPage.OpenAsync();
        var (displayNameFromGrid, ghgScopeFromGrid) = await emissionCategoriesPage.GetFirstRowDisplayNameAndScopeAsync();
        var editEmissionCategoryPage = await emissionCategoriesPage.ClickEditBtnInFirstRowAsync();

        (await editEmissionCategoryPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await editEmissionCategoryPage.GetSubtitleAsync()).Should().Be(expectedSubtitle);
        (await editEmissionCategoryPage.GetDisplayNameAsync()).Should().Be(displayNameFromGrid);
        (await editEmissionCategoryPage.GetGhgScopeAsync()).Should().Be(ghgScopeFromGrid);
        (await editEmissionCategoryPage.GetRollupKeyTextAsync()).Should().Be(expectedRollupKeyText);
        (await editEmissionCategoryPage.IsSaveCategoryBtnVisibleAsync()).Should().BeTrue();
        (await editEmissionCategoryPage.IsCancelBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T07_EmissionCategories_Edit_EmptyDisplayName()
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
    public async Task T08_EmissionCategories_Edit_DisplayNameTooLong()
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
    public async Task T09_EmissionCategories_Edit_Success()
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
            (await editEmissionCategoryPage.GetSubtitleAsync()).Should().Be("BUSINESS_TRAVEL");
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

    private static List<(string DisplayName, string GhgScope)> ExpectedSortedCategories(
        IReadOnlyList<(string DisplayName, string GhgScope)> categories,
        string column,
        bool ascending)
    {
        Func<(string DisplayName, string GhgScope), string> key = column switch
        {
            "DISPLAY NAME" => row => row.DisplayName,
            "GHG SCOPE" => row => row.GhgScope,
            _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Unknown emission categories grid column."),
        };

        var ordered = ascending
            ? categories
                .OrderBy(key, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.DisplayName, StringComparer.OrdinalIgnoreCase)
            : categories
                .OrderByDescending(key, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.DisplayName, StringComparer.OrdinalIgnoreCase);

        return ordered.ToList();
    }
}
