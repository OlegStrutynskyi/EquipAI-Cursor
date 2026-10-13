using System.Globalization;
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
        const string expectedPageTitle = "Add Unit";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();

        (await addUnitPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T03_Units_Grid_Columns()
    {
        var expectedColumns = new[]
        {
            "CODE",
            "DISPLAY NAME",
            "SYMBOL",
            "DIMENSION",
            "SCALE",
            "ACTIONS",
        };

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();

        (await unitsPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T04_Units_Grid_Records()
    {
        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();

        var expected = (await SqlHelper.GetUnitGridRowsAsync())
            .Select(ToGridRecord)
            .ToList();
        if (expected.Count == 0)
            Assert.Pass("No Units");

        var actual = await unitsPage.GetAllGridRecordsAsync();
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public async Task T05_Units_Grid_Sorting()
    {
        var columns = new[] { "CODE", "DISPLAY NAME", "DIMENSION", "SCALE", "SYMBOL" };

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();

        var units = (await SqlHelper.GetUnitGridRowsAsync())
            .Select(ToGridRecord)
            .ToList();
        if (units.Count == 0)
            Assert.Pass("No Units");

        foreach (var column in columns)
        {
            await unitsPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(unitsPage, units, column, ascending: true);

            await unitsPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(unitsPage, units, column, ascending: false);

            await unitsPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(unitsPage, units, "CODE", ascending: true);
        }
    }

    [Test]
    public async Task T06_Units_Grid_Search()
    {
        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();

        (await unitsPage.IsSearchInputVisibleAsync()).Should().BeTrue();
        (await unitsPage.GetSearchPlaceholderAsync()).Should().Be("Search units or data...");

        await AssertUnitSearchAsync(unitsPage, RandomUnitSearch());
        await AssertUnitSearchAsync(unitsPage, "us_gal");
        await AssertUnitSearchAsync(unitsPage, "WATT");
        await AssertUnitSearchAsync(unitsPage, "volume");
        await AssertUnitSearchAsync(unitsPage, "SYM");
    }

    [Test]
    public async Task T07_Units_AddUnit_DefaultView()
    {
        const string expectedPageTitle = "Add Unit";
        const string expectedSymbolMessage = "Short label shown after a quantity, e.g. Gal in \"50 Gal\".";
        const string expectedScaleHelpMessage =
            "Multiply CO₂e intensity per this unit by Scale to get intensity per the same-dimension canonical (Volume→US_GAL, Mass→T, Energy→KWH). Leave blank when not convertible. Other requires blank scale.";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();

        (await addUnitPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await addUnitPage.IsCodeInputVisibleAsync()).Should().BeTrue();
        (await addUnitPage.IsDisplayNameInputVisibleAsync()).Should().BeTrue();
        (await addUnitPage.IsSymbolInputVisibleAsync()).Should().BeTrue();
        (await addUnitPage.GetSymbolMessageAsync()).Should().Be(expectedSymbolMessage);
        (await addUnitPage.IsDimensionDropdownVisibleAsync()).Should().BeTrue();
        (await addUnitPage.IsScaleInputVisibleAsync()).Should().BeTrue();
        (await addUnitPage.GetScaleHelpMessageAsync()).Should().Be(expectedScaleHelpMessage);
        (await addUnitPage.IsCreateUnitBtnVisibleAsync()).Should().BeTrue();
        (await addUnitPage.IsCancelBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T08_Units_AddUnit_DimensionDropdown()
    {
        var expectedOptions = new[] { "Volume", "Mass", "Energy", "Other", "Area" };

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();

        var dimensionOptions = await addUnitPage.GetDimensionOptionsAsync();
        dimensionOptions.Should().Equal(expectedOptions);
    }

    [Test]
    public async Task T09_Units_AddUnit_EmptyFields()
    {
        const string expectedCodeError = "Code is required.";
        const string expectedDisplayNameError = "Display name is required.";
        const string expectedSymbolError = "Symbol is required.";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.ClickCreateUnitBtnAsync();

        (await addUnitPage.GetCodeErrorAsync()).Should().Be(expectedCodeError);
        (await addUnitPage.GetDisplayNameErrorAsync()).Should().Be(expectedDisplayNameError);
        (await addUnitPage.GetSymbolErrorAsync()).Should().Be(expectedSymbolError);
    }

    [Test]
    public async Task T10_Units_AddUnit_ExistingCode()
    {
        const string code = Config.SetupCode1;
        const string expectedAlertMessage = "Code should be unique.";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var displayName = $"Unit{stamp}";
        var symbol = $"Sy{stamp}";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.FillCodeAsync(code);
        await addUnitPage.FillDisplayNameAsync(displayName);
        await addUnitPage.FillSymbolAsync(symbol);
        await addUnitPage.ClickCreateUnitBtnAsync();

        (await addUnitPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T11_Units_AddUnit_CodeTooLong()
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
    public async Task T12_Units_AddUnit_DisplayNameTooLong()
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
    public async Task T13_Units_AddUnit_SymbolTooLong()
    {
        const string expectedSymbolError = "Symbol must be at most 16 characters.";
        var randomSymbol = GenerateRandomString(17);

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.FillSymbolAsync(randomSymbol);
        await addUnitPage.ClickCreateUnitBtnAsync();

        (await addUnitPage.GetSymbolErrorAsync()).Should().Be(expectedSymbolError);
    }

    [Test]
    public async Task T14_Units_AddUnit_ClickCancel()
    {
        const string expectedPageTitle = "Units";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var code = $"Code{stamp}";
        var displayName = $"Unit{stamp}";
        var symbol = $"Sy{stamp}";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
        await addUnitPage.FillCodeAsync(code);
        await addUnitPage.FillDisplayNameAsync(displayName);
        await addUnitPage.FillSymbolAsync(symbol);
        unitsPage = await addUnitPage.ClickCancelBtnAsync();

        (await unitsPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await unitsPage.IsCodeInGridAsync(code)).Should().BeFalse();
    }

    [Test]
    public async Task T15_Units_AddUnit_Success()
    {
        const string expectedPageTitle = "Units";
        const string expectedDimension = "Volume";
        const string expectedScale = "1.15";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var codePrefix = $"Code{stamp}";
        var displayNamePrefix = $"Unit{stamp}";
        var symbol = $"Sy{stamp}";
        var code = codePrefix + GenerateRandomAlphanumericString(32 - codePrefix.Length);
        var displayName = displayNamePrefix + GenerateRandomAlphanumericString(128 - displayNamePrefix.Length);

        try
        {
            var unitsPage = new UnitsPage(Fixture.Page);
            await unitsPage.OpenAsync();
            var addUnitPage = await unitsPage.ClickAddUnitBtnAsync();
            await addUnitPage.FillCodeAsync(code);
            await addUnitPage.FillDisplayNameAsync(displayName);
            await addUnitPage.FillSymbolAsync(symbol);
            await addUnitPage.SelectDimensionAsync(expectedDimension);
            await addUnitPage.FillScaleAsync(expectedScale);
            unitsPage = await addUnitPage.CreateUnitAsync();

            (await unitsPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            (await unitsPage.IsCodeInGridAsync(code)).Should().BeTrue();
            (await unitsPage.IsDisplayNameInGridAsync(displayName)).Should().BeTrue();
            (await unitsPage.GetSymbolByCodeAsync(code)).Should().Be(symbol);
            var (dimension, scale) = await unitsPage.GetDimensionAndScaleByCodeAsync(code);
            dimension.Should().Be(expectedDimension);
            scale.Should().Be(expectedScale);
        }
        finally
        {
            await SqlHelper.DeleteUnitOfMeasureByCodeAsync(code);
        }
    }

    [Test]
    public async Task T16_Units_ClickEditBtn()
    {
        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var editUnitPage = await unitsPage.ClickEditBtnAsync(Config.SetupCode1);

        (await editUnitPage.GetPageTitleAsync()).Should().Match(title =>
            title.Equals("Edit Unit", StringComparison.Ordinal)
            || title.StartsWith("Edit Unit ", StringComparison.Ordinal));
    }

    [Test]
    public async Task T17_Units_Edit_DefaultView()
    {
        const string expectedPageTitle = "Edit Unit";

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var (code, displayName) = await unitsPage.GetCodeAndDisplayNameAsync(Config.SetupCode1);
        var editUnitPage = await unitsPage.ClickEditBtnAsync(Config.SetupCode1);

        (await editUnitPage.GetPageTitleAsync()).Should().Match(title =>
            title.Equals(expectedPageTitle, StringComparison.Ordinal)
            || title.StartsWith(expectedPageTitle + " ", StringComparison.Ordinal));
        (await editUnitPage.GetSubtitleAsync()).Should().Be(code);
        (await editUnitPage.GetCodeAsync()).Should().Be(code);
        (await editUnitPage.GetDisplayNameAsync()).Should().Be(displayName);
        (await editUnitPage.IsSaveUnitBtnVisibleAsync()).Should().BeTrue();
        (await editUnitPage.IsSaveUnitBtnEnabledAsync()).Should().BeTrue();
        (await editUnitPage.IsCancelBtnVisibleAsync()).Should().BeTrue();
        (await editUnitPage.IsCancelBtnEnabledAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T18_Units_Edit_EmptyFields()
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
    public async Task T19_Units_Edit_ExistingCode()
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
    public async Task T20_Units_Edit_CodeTooLong()
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
    public async Task T21_Units_Edit_DisplayNameTooLong()
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
    public async Task T22_Units_Edit_SymbolTooLong()
    {
        const string expectedSymbolError = "Symbol must be at most 16 characters.";
        var randomSymbol = GenerateRandomString(17);

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        var editUnitPage = await unitsPage.ClickEditBtnAsync(Config.SetupCode1);
        await editUnitPage.FillSymbolAsync(randomSymbol);
        await editUnitPage.ClickSaveUnitBtnAsync();

        (await editUnitPage.GetSymbolErrorAsync()).Should().Be(expectedSymbolError);
    }

    [Test]
    public async Task T23_Units_Edit_ClickCancel()
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
    public async Task T24_Units_Edit_Success()
    {
        const string expectedPageTitle = "Units";
        const string expectedDimension = "Mass";
        const string expectedScale = "2";
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var codePrefix = $"Code{stamp}";
        var displayNamePrefix = $"Unit{stamp}";
        var symbol = $"Sy{stamp}";
        var code = codePrefix + GenerateRandomString(32 - codePrefix.Length);
        var displayName = displayNamePrefix + GenerateRandomString(128 - displayNamePrefix.Length);
        object? unitId = null;
        int? originalDimension = null;
        decimal? originalScale = null;

        try
        {
            unitId = await SqlHelper.GetUnitOfMeasureIdByCodeAsync(Config.SetupCode1);
            (originalDimension, originalScale) = await SqlHelper.GetUnitOfMeasureDimensionAndScaleAsync(unitId);

            var unitsPage = new UnitsPage(Fixture.Page);
            await unitsPage.OpenAsync();
            var editUnitPage = await unitsPage.ClickEditBtnAsync(Config.SetupCode1);
            await editUnitPage.FillCodeAsync(code);
            await editUnitPage.FillDisplayNameAsync(displayName);
            await editUnitPage.FillSymbolAsync(symbol);
            await editUnitPage.SelectDimensionAsync(expectedDimension);
            await editUnitPage.FillScaleAsync(expectedScale);
            unitsPage = await editUnitPage.SaveUnitAsync();

            (await unitsPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
            (await unitsPage.IsCodeInGridAsync(code)).Should().BeTrue();
            (await unitsPage.IsDisplayNameInGridAsync(displayName)).Should().BeTrue();
            (await unitsPage.GetSymbolByCodeAsync(code)).Should().Be(symbol);
            var (dimension, scale) = await unitsPage.GetDimensionAndScaleByCodeAsync(code);
            dimension.Should().Be(expectedDimension);
            scale.Should().Be(expectedScale);
        }
        finally
        {
            if (unitId is not null)
                await SqlHelper.RestoreUnitOfMeasureAsync(
                    unitId,
                    Config.SetupCode1,
                    Config.SetupUnitName1,
                    Config.SetupUnitSymbol1,
                    originalDimension,
                    originalScale);
        }
    }

    [Test]
    public async Task T25_Units_Deactivate_Click()
    {
        const string expectedTitle = "Deactivate Unit";
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
    public async Task T26_Units_Deactivate_Cancel()
    {
        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        await unitsPage.ClickDeactivateBtnAsync(Config.SetupCode1);
        await unitsPage.ClickDeactivateDialogCancelBtnAsync();

        (await unitsPage.IsDeactivateDialogVisibleAsync()).Should().BeFalse();
        (await unitsPage.IsCodeInGridAsync(Config.SetupCode1)).Should().BeTrue();
    }

    [Test]
    public async Task T27_Units_Deactivate_Success()
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
    public async Task T28_Units_Deactivate_UsedInEmissionType()
    {
        const string expectedAlertMessage =
            "Cannot deactivate a unit of measure that is an emission type's default unit.";
        var expectedDefaultUnit = $"{Config.SetupDefaultUnitCode1} — {Config.SetupDefaultUnitName1}";

        var emissionTypesPage = new EmissionTypesPage(Fixture.Page);
        await emissionTypesPage.OpenAsync();

        if (!await emissionTypesPage.IsDefaultUnitInGridAsync(expectedDefaultUnit))
            Assert.Fail("Setup is not complete. Run test 'T06_Setup_CreateEmissionType1'.");

        var unitsPage = new UnitsPage(Fixture.Page);
        await unitsPage.OpenAsync();
        await unitsPage.ClickDeactivateBtnAsync(Config.SetupDefaultUnitCode1);
        await unitsPage.ClickDeactivateDialogConfirmBtnExpectingErrorAsync();

        (await unitsPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    private static async Task AssertCurrentPageSortedAsync(
        UnitsPage unitsPage,
        IReadOnlyList<UnitGridRecord> units,
        string column,
        bool ascending)
    {
        var actual = await unitsPage.GetCurrentPageGridRecordsAsync();
        var expected = ExpectedSortedUnits(units, column, ascending).Take(actual.Count).ToList();
        actual.Should().Equal(expected, $"{column} {(ascending ? "ASC" : "DESC")}");
    }

    private static async Task AssertUnitSearchAsync(UnitsPage unitsPage, string term)
    {
        await unitsPage.SearchAsync(term);

        var expected = (await SqlHelper.GetUnitGridRowsAsync())
            .Select(ToGridRecord)
            .Where(row => UnitMatchesSearch(row, term))
            .ToList();

        if (expected.Count == 0)
        {
            (await unitsPage.GetGridEmptyMessageAsync()).Should().Be("No units match your search.");
            return;
        }

        var actual = await unitsPage.GetAllGridRecordsAsync();
        actual.Should().BeEquivalentTo(expected);
        actual.Should().OnlyContain(row => UnitMatchesSearch(row, term));
    }

    private static string RandomUnitSearch()
    {
        var chars = GenerateRandomAlphanumericString(10).ToCharArray();
        chars[0] = (char)('a' + Random.Shared.Next(26));
        chars[1] = (char)('0' + Random.Shared.Next(10));
        return new string(chars);
    }

    private static bool UnitMatchesSearch(UnitGridRecord row, string term) =>
        string.Join(' ', row.Code, row.DisplayName, row.Symbol, row.Dimension)
            .Contains(term, StringComparison.OrdinalIgnoreCase);

    private static UnitGridRecord ToGridRecord(UnitGridRow row) =>
        new(
            row.Code,
            row.DisplayName,
            row.Symbol,
            FormatDimension(row.Dimension),
            FormatScale(row.Scale));

    private static string FormatDimension(int? dimension) => dimension switch
    {
        0 => "Volume",
        1 => "Mass",
        2 => "Energy",
        3 => "Other",
        4 => "Area",
        null => string.Empty,
        _ => dimension.Value.ToString(CultureInfo.InvariantCulture),
    };

    private static string FormatScale(decimal? scale) =>
        scale is null
            ? "—"
            : scale.Value.ToString("0.################", CultureInfo.InvariantCulture);

    private static List<UnitGridRecord> ExpectedSortedUnits(
        IReadOnlyList<UnitGridRecord> units,
        string column,
        bool ascending)
    {
        IEnumerable<UnitGridRecord> ordered = column switch
        {
            "CODE" => OrderByText(units, row => row.Code, ascending),
            "DISPLAY NAME" => OrderByText(units, row => row.DisplayName, ascending, HyphenInsensitiveComparer.Instance),
            "SYMBOL" => OrderByText(units, row => row.Symbol, ascending),
            "DIMENSION" => OrderByText(units, row => row.Dimension, ascending),
            "SCALE" => ascending
                ? units.OrderBy(row => ParseScale(row.Scale) ?? decimal.MinValue).ThenBy(row => row.Code, StringComparer.OrdinalIgnoreCase)
                : units.OrderByDescending(row => ParseScale(row.Scale) ?? decimal.MinValue).ThenBy(row => row.Code, StringComparer.OrdinalIgnoreCase),
            _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Unknown units grid column."),
        };

        return ordered.ToList();
    }

    private static IEnumerable<UnitGridRecord> OrderByText(
        IReadOnlyList<UnitGridRecord> units,
        Func<UnitGridRecord, string> key,
        bool ascending,
        StringComparer? comparer = null)
    {
        comparer ??= StringComparer.OrdinalIgnoreCase;
        return ascending
            ? units.OrderBy(key, comparer).ThenBy(row => row.Code, StringComparer.OrdinalIgnoreCase)
            : units.OrderByDescending(key, comparer).ThenBy(row => row.Code, StringComparer.OrdinalIgnoreCase);
    }

    private sealed class HyphenInsensitiveComparer : StringComparer
    {
        public static readonly HyphenInsensitiveComparer Instance = new();

        public override int Compare(string? x, string? y) =>
            string.Compare(StripHyphens(x), StripHyphens(y), StringComparison.OrdinalIgnoreCase);

        public override bool Equals(string? x, string? y) => Compare(x, y) == 0;

        public override int GetHashCode(string obj) =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(StripHyphens(obj));

        private static string StripHyphens(string? value) =>
            (value ?? string.Empty)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("‐", string.Empty, StringComparison.Ordinal)
                .Replace("‑", string.Empty, StringComparison.Ordinal)
                .Replace("–", string.Empty, StringComparison.Ordinal)
                .Replace("—", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal);
    }

    private static decimal? ParseScale(string scale) =>
        decimal.TryParse(scale, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
}
