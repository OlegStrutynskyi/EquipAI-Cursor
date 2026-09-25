using System.Globalization;
using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class TelemetryPage : BasePage
{
    public TelemetryPage(IPage page) : base(page) { }

    private ILocator PageTitle => Page.Locator("//h1[contains(@class,'page-title')]")
        .Filter(new LocatorFilterOptions { HasTextString = "Telemetry" });
    private ILocator Message => Page.Locator("//p[@class='page-header__lead']");
    private ILocator ImportBtn => Page.Locator("//a[normalize-space()='Import'] | //button[normalize-space()='Import']");
    private ILocator MonthLabel => Page.Locator("label[for='telemetry-month'] span.form-label");
    private ILocator MonthField => Page.Locator("#telemetry-month");
    private ILocator Grid => Page.Locator("//table[contains(@class,'table')]");

    public async Task OpenAsync()
    {
        await Page.GotoAsync(Config.BaseUrl + "telemetry");
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await PageTitle.WaitForAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetTitleAsync()
    {
        await PageTitle.WaitForAsync();
        return (await PageTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessageAsync()
    {
        await Message.WaitForAsync();
        return (await Message.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsImportBtnVisibleAsync() => ImportBtn.IsVisibleAsync();
    public Task<bool> IsMonthLabelVisibleAsync() => MonthLabel.IsVisibleAsync();
    public Task<bool> IsMonthFieldVisibleAsync() => MonthField.IsVisibleAsync();
    public Task<bool> IsGridVisibleAsync() => Grid.IsVisibleAsync();

    public async Task SetMonthAsync(string yearMonth)
    {
        await MonthField.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await MonthField.FillAsync(yearMonth);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await Grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header))
            .ToList();
    }

    public async Task<IReadOnlyList<TelemetryGridRow>> GetGridRowsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = (await Grid.Locator("thead th").AllInnerTextsAsync())
            .Select(h => h.Replace('\u00A0', ' ').Trim().ToUpperInvariant())
            .ToList();
        var rows = Grid.Locator("tbody tr");
        var count = await rows.CountAsync();
        var result = new List<TelemetryGridRow>();

        for (var i = 0; i < count; i++)
        {
            var cells = rows.Nth(i).Locator("td");
            var cellCount = await cells.CountAsync();
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < cellCount && c < headers.Count; c++)
            {
                values[headers[c]] = (await cells.Nth(c).InnerTextAsync()).Replace('\u00A0', ' ').Trim();
            }

            result.Add(new TelemetryGridRow
            {
                Month = GetCell(values, "MONTH"),
                EquipmentTag = GetCell(values, "EQUIPMENT TAG"),
                EquipmentType = GetCell(values, "EQUIPMENT TYPE"),
                Location = GetCell(values, "LOCATION", "LOCATION TEXT"),
                OperatingHours = GetCell(values, "OPERATING HOURS"),
                FuelType = GetCell(values, "FUEL TYPE"),
                ImportDate = GetCell(values, "IMPORT DATE"),
                Source = GetCell(values, "SOURCE"),
            });
        }

        return result;
    }

    public async Task<TelemetryGridRow?> GetGridRowByEquipmentTagAsync(string equipmentTag)
    {
        var rows = await GetGridRowsAsync();
        return rows.FirstOrDefault(row =>
            row.EquipmentTag.Equals(equipmentTag, StringComparison.Ordinal));
    }

    public async Task<ImportPage> ClickImportBtnAsync()
    {
        await ImportBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/telemetry", StringComparison.OrdinalIgnoreCase)
                   && url.Contains("import", StringComparison.OrdinalIgnoreCase));
        var importPage = new ImportPage(Page);
        await importPage.WaitForLoadedAsync();
        return importPage;
    }

    public async Task<string> GetMonthFieldValueAsync()
    {
        await MonthField.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var value = await MonthField.InputValueAsync();
        if (!string.IsNullOrWhiteSpace(value))
            return value.Trim();

        return (await MonthField.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMonthFieldDisplayValueAsync()
    {
        var value = await GetMonthFieldValueAsync();
        if (DateTime.TryParseExact(
                value,
                "yyyy-MM",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var monthValue))
        {
            return monthValue.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        }

        return value;
    }

    private static string GetCell(IReadOnlyDictionary<string, string> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value))
                return value;
        }

        return string.Empty;
    }
}

public sealed class TelemetryGridRow
{
    public required string Month { get; init; }
    public required string EquipmentTag { get; init; }
    public required string EquipmentType { get; init; }
    public required string Location { get; init; }
    public required string OperatingHours { get; init; }
    public required string FuelType { get; init; }
    public required string ImportDate { get; init; }
    public required string Source { get; init; }
}
