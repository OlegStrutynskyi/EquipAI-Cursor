using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class ProjectDashboardPage : BasePage
{
    public ProjectDashboardPage(IPage page) : base(page) { }

    private ILocator ProjectDashboardTitle => Page.Locator("#project-dashboard-title");
    private ILocator ReportingBadge => Page.Locator(
        "//span[contains(@class,'pill') and contains(translate(normalize-space(.), 'abcdefghijklmnopqrstuvwxyz', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'), 'REPORTING')]");
    private ILocator SynchedProcore => Page.Locator("//p[@class='page-meta__item']");
    private ILocator YearDropdown => Page.Locator("//button[@id='project-dashboard-year']");
    private ILocator TotalEmissionsCard => Page.Locator("//article[@class='card card--subtle stat-card']");
    private ILocator TotalEmissionsIcon => TotalEmissionsCard.Locator("xpath=.//span[@class='icon-badge']");
    private ILocator TotalEmissionsTitle => TotalEmissionsCard.Locator("xpath=.//*[normalize-space()='Total Carbon Emissions']");
    private ILocator TotalEmissionsSubtitle => TotalEmissionsCard.Locator("xpath=.//p[@class='section-header__lead']");
    private ILocator TotalEmissionsMetricValue => TotalEmissionsCard.Locator("xpath=.//span[contains(@class,'metric__value')]");
    private ILocator TotalEmissionsMetricUnit => TotalEmissionsCard.Locator("xpath=.//span[contains(@class,'metric__unit')]");
    private ILocator ScopeTotalText => Page.Locator("//p[@class='breakdown__total']");
    private ILocator ScopeTotalValue => Page.Locator("//span[@class='breakdown__total-value']");
    private ILocator ScopeEmptyMessage => Page.Locator("//p[@class='project-dashboard__breakdown-empty']");
    private ILocator ChartCard => Page.Locator("//div[@class='card card--subtle project-dashboard__chart-card']");
    private ILocator ChartSubtitle => ChartCard.Locator("xpath=.//p[@class='section-header__lead']");
    private ILocator Chart => ChartCard.Locator("div.chart__plot");
    private ILocator SearchableSelectList => Page.Locator("//div[@role='listbox' and contains(@class,'select__list')]");
    private ILocator DataGrid => Page.Locator("//section[@class='project-dashboard__data']");
    private ILocator OfficesCard => Page.Locator("//*[@class='project-dashboard__offices']");
    private ILocator OfficesNote => Page.Locator("//p[@class='project-offices__note']");
    private ILocator OfficesEmptyMessage => Page.Locator("//p[@class='project-offices__empty']");
    private ILocator OfficesTable => Page.Locator("//table[contains(@class,'project-offices__table')]");
    private ILocator FuelTab => Page.Locator("//button[normalize-space()='Fuel']");
    private ILocator OfficeTab => Page.Locator("//button[normalize-space()='Office' and not(contains(@class,'table__sort'))]");
    private ILocator TelemetryTab => Page.Locator("//button[normalize-space()='Telemetry']");
    private ILocator FuelGrid => DataGrid.Locator("table");
    private ILocator FuelPagination => Page.Locator("nav[aria-label='Project data pagination']");
    private ILocator FuelNextPageBtn => FuelPagination.Locator("button").Filter(new LocatorFilterOptions { HasTextString = "Next" });
    private ILocator FuelPreviousPageBtn => FuelPagination.Locator("button").Filter(new LocatorFilterOptions { HasTextString = "Previous" });
    private const int FuelDataPageSize = 5;

    public async Task WaitForLoadedAsync()
    {
        await ProjectDashboardTitle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task WaitForDefaultViewAsync()
    {
        await WaitForLoadedAsync();
        await ReportingBadge.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await SynchedProcore.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await TotalEmissionsCard.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await ChartCard.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await DataGrid.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public new async Task<string> GetPageTitleAsync()
    {
        await ProjectDashboardTitle.WaitForAsync();
        return (await ProjectDashboardTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetReportingBadgeTextAsync()
    {
        await ReportingBadge.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await ReportingBadge.First.InnerTextAsync()).Trim();
    }

    public Task<bool> IsReportingBadgeVisibleAsync() => ReportingBadge.First.IsVisibleAsync();
    public Task<bool> IsSynchedProcoreVisibleAsync() => SynchedProcore.First.IsVisibleAsync();
    public Task<bool> IsYearDropdownVisibleAsync() => YearDropdown.IsVisibleAsync();
    public Task<bool> IsTotalEmissionsCardVisibleAsync() => TotalEmissionsCard.First.IsVisibleAsync();

    public Task<bool> IsTotalEmissionsIconVisibleAsync() => TotalEmissionsIcon.First.IsVisibleAsync();

    public async Task<string> GetTotalEmissionsTitleAsync()
    {
        await TotalEmissionsTitle.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await TotalEmissionsTitle.First.InnerTextAsync()).Trim();
    }

    public async Task<string> GetTotalEmissionsSubtitleAsync()
    {
        await TotalEmissionsSubtitle.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await TotalEmissionsSubtitle.First.InnerTextAsync()).Trim();
    }

    public async Task<string> GetTotalEmissionsValueAsync()
    {
        await TotalEmissionsMetricValue.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var deadline = DateTime.UtcNow.AddSeconds(15);
        var value = string.Empty;
        while (DateTime.UtcNow < deadline)
        {
            value = (await TotalEmissionsMetricValue.First.InnerTextAsync()).Trim();
            if (value.Any(char.IsDigit))
                break;

            await Task.Delay(200);
        }

        if (await TotalEmissionsMetricUnit.CountAsync() == 0)
            return value;

        var unit = (await TotalEmissionsMetricUnit.First.InnerTextAsync()).Trim();
        return value.Contains(unit, StringComparison.OrdinalIgnoreCase) ? value : $"{value} {unit}";
    }

    public ILocator ScopeButton(int scope) =>
        Page.Locator($"//div[@aria-label='Scope']//button[@type='button'][normalize-space()='Scope {scope}']");

    public Task<bool> IsScopeButtonVisibleAsync(int scope) => ScopeButton(scope).IsVisibleAsync();

    public async Task ClickScopeButtonAsync(int scope)
    {
        var button = ScopeButton(scope);
        await button.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await button.ClickAsync();

        var expectedPrefix = $"Total Scope {scope}";
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            var text = NormalizeText(await ScopeTotalText.First.InnerTextAsync());
            if (text.StartsWith(expectedPrefix, StringComparison.Ordinal))
                return;

            await Task.Delay(200);
        }

        throw new TimeoutException(
            $"Scope {scope} breakdown did not load. Actual: '{NormalizeText(await ScopeTotalText.First.InnerTextAsync())}'.");
    }

    public async Task<bool> IsScopeButtonSelectedAsync(int scope)
    {
        var button = ScopeButton(scope);
        await button.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var ariaPressed = await button.GetAttributeAsync("aria-pressed");
        if (string.Equals(ariaPressed, "true", StringComparison.OrdinalIgnoreCase))
            return true;

        var ariaSelected = await button.GetAttributeAsync("aria-selected");
        if (string.Equals(ariaSelected, "true", StringComparison.OrdinalIgnoreCase))
            return true;

        var className = await button.GetAttributeAsync("class") ?? string.Empty;
        return className.Contains("active", StringComparison.OrdinalIgnoreCase)
            || className.Contains("selected", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> GetScopeTotalValueAsync()
    {
        await ScopeTotalValue.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var deadline = DateTime.UtcNow.AddSeconds(15);
        var value = string.Empty;
        while (DateTime.UtcNow < deadline)
        {
            value = NormalizeText(await ScopeTotalValue.First.InnerTextAsync());
            if (value.Any(char.IsDigit))
                break;

            await Task.Delay(200);
        }

        return value;
    }

    public async Task<string> GetScopeTotalTextAsync()
    {
        await ScopeTotalText.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return NormalizeText(await ScopeTotalText.First.InnerTextAsync());
    }

    public async Task<string> GetScopeEmptyMessageAsync()
    {
        await ScopeEmptyMessage.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return NormalizeText(await ScopeEmptyMessage.First.InnerTextAsync());
    }

    public async Task<IReadOnlyList<string>> GetScopeEmissionLinesAsync()
    {
        var rows = Page.Locator(
            "//p[@class='breakdown__total']/following-sibling::*[self::ul or self::ol or self::div][1]//li");
        if (await rows.CountAsync() == 0)
        {
            var html = await ScopeTotalText.First.EvaluateAsync<string>(
                "el => (el.parentElement?.innerHTML || '').slice(0, 4000)");
            throw new InvalidOperationException(html);
        }

        var texts = await rows.AllInnerTextsAsync();
        return texts.Select(NormalizeText).Where(text => text.Length > 0).ToList();
    }

    public Task<bool> IsScopeTotalTextVisibleAsync() => ScopeTotalText.First.IsVisibleAsync();
    public Task<bool> IsChartCardVisibleAsync() => ChartCard.First.IsVisibleAsync();

    public async Task<string> GetChartSubtitleAsync()
    {
        await ChartSubtitle.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return NormalizeText(await ChartSubtitle.First.InnerTextAsync());
    }
    public Task<bool> IsDataGridVisibleAsync() => DataGrid.First.IsVisibleAsync();

    public async Task<bool> IsOfficesCardVisibleAsync()
    {
        if (await OfficesCard.CountAsync() == 0)
            return false;

        return await OfficesCard.First.IsVisibleAsync();
    }

    public async Task<string> GetOfficesNoteAsync()
    {
        await OfficesNote.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return NormalizeText(await OfficesNote.First.InnerTextAsync());
    }

    public async Task<bool> IsOfficesEmptyMessageVisibleAsync()
    {
        if (await OfficesEmptyMessage.CountAsync() == 0)
            return false;

        return await OfficesEmptyMessage.First.IsVisibleAsync();
    }

    public async Task<string> GetOfficesEmptyMessageAsync()
    {
        await OfficesEmptyMessage.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return NormalizeText(await OfficesEmptyMessage.First.InnerTextAsync());
    }

    public async Task<IReadOnlyList<ProjectOfficesCardGridRow>> GetOfficesCardRowsAsync()
    {
        if (await OfficesTable.CountAsync() == 0 || !await OfficesTable.First.IsVisibleAsync())
            return [];

        var rawRows = await OfficesTable.First.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => !tr.querySelector('.table__skeleton-bar'))
              .map(tr => [...tr.querySelectorAll(':scope > td')]
                .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim()))
              .filter(cells => cells.some(cell => cell.length > 0))
            """);

        var headers = (await OfficesTable.First.Locator("thead th").AllInnerTextsAsync())
            .Select(NormalizeOfficesHeader)
            .ToList();

        var result = new List<ProjectOfficesCardGridRow>();
        foreach (var cells in rawRows)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < cells.Length && i < headers.Count; i++)
            {
                if (headers[i].Length == 0)
                    continue;

                values[headers[i]] = cells[i];
            }

            var office = OfficesCell(values, "OFFICE");
            if (string.IsNullOrWhiteSpace(office))
                continue;

            result.Add(new ProjectOfficesCardGridRow(
                office,
                OfficesCell(values, "AREA"),
                OfficesCell(values, "METHOD"),
                OfficesCell(values, "EMISSION"),
                OfficesCell(values, "TONNES"),
                OfficesCell(values, "PER")));
        }

        return result;
    }

    private static string NormalizeOfficesHeader(string header)
    {
        var text = NormalizeText(header)
            .Replace("₂", "2", StringComparison.Ordinal)
            .Replace("²", "2", StringComparison.Ordinal)
            .ToUpperInvariant();

        if (text.Contains("PER", StringComparison.Ordinal))
            return "PER";
        if (text.Contains("TCO2", StringComparison.Ordinal) || text.Contains("CO2", StringComparison.Ordinal))
            return "TONNES";
        if (text.Contains("EMISSION", StringComparison.Ordinal))
            return "EMISSION";
        if (text.Contains("METHOD", StringComparison.Ordinal))
            return "METHOD";
        if (text.Contains("AREA", StringComparison.Ordinal))
            return "AREA";
        if (text.Contains("OFFICE", StringComparison.Ordinal))
            return "OFFICE";

        return text;
    }

    private static string OfficesCell(IReadOnlyDictionary<string, string> values, string name) =>
        values.TryGetValue(name, out var value) ? value : string.Empty;

    public async Task<bool> IsFuelTabVisibleAsync()
    {
        if (await FuelTab.CountAsync() == 0)
            return false;

        return await FuelTab.First.IsVisibleAsync();
    }

    public async Task<bool> IsFuelTabSelectedAsync()
    {
        await FuelTab.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return await FuelTab.First.EvaluateAsync<bool>(
            """
            el => el.getAttribute('aria-selected') === 'true'
              || el.getAttribute('aria-pressed') === 'true'
              || el.getAttribute('aria-current') === 'page'
              || el.getAttribute('aria-current') === 'true'
              || el.classList.contains('active')
              || el.classList.contains('is-active')
              || el.classList.contains('selected')
              || el.getAttribute('data-state') === 'active'
            """);
    }

    public async Task<IReadOnlyList<string>> GetFuelGridColumnHeadersAsync()
    {
        var grid = await WaitForFuelGridAsync();
        var headers = await grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(NormalizeText)
            .Where(header => header.Length > 0 && !header.Equals("Actions", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<bool> IsOfficeTabVisibleAsync()
    {
        if (await OfficeTab.CountAsync() == 0)
            return false;

        return await OfficeTab.First.IsVisibleAsync();
    }

    public async Task<bool> IsOfficeTabSelectedAsync()
    {
        await OfficeTab.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return await IsDataTabSelectedAsync(OfficeTab.First);
    }

    public async Task ClickOfficeTabAsync()
    {
        var tab = OfficeTab.First;
        await tab.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        if (await IsDataTabSelectedAsync(tab))
        {
            await WaitForFuelGridAsync();
            return;
        }

        var before = await GetFirstDataRowFingerprintAsync();
        await tab.ClickAsync();

        var selectedDeadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < selectedDeadline && !await IsDataTabSelectedAsync(tab))
            await Task.Delay(100);

        if (!await IsDataTabSelectedAsync(tab))
            throw new TimeoutException("Office tab did not become selected.");

        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var after = await GetFirstDataRowFingerprintAsync();
            if (!string.IsNullOrEmpty(after) && !string.Equals(after, before, StringComparison.Ordinal))
                break;

            await Task.Delay(150);
        }

        await WaitForFuelGridAsync();
    }

    public async Task<IReadOnlyList<ProjectOfficeGridRow>> GetOfficeGridRowsAsync()
    {
        await WaitForFuelGridAsync();
        try
        {
            await FuelPagination.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10_000,
            });
        }
        catch (TimeoutException)
        {
        }

        await GoToFirstFuelGridPageAsync();

        var result = new List<ProjectOfficeGridRow>();
        while (true)
        {
            await WaitForDisplayedFuelPageAsync();
            result.AddRange(await ReadOfficeGridRowsOnCurrentPageAsync());
            if (!await TryGoToNextFuelGridPageAsync())
                break;
        }

        return result;
    }

    public async Task<bool> IsTelemetryTabVisibleAsync()
    {
        if (await TelemetryTab.CountAsync() == 0)
            return false;

        return await TelemetryTab.First.IsVisibleAsync();
    }

    public async Task<bool> IsTelemetryTabSelectedAsync()
    {
        await TelemetryTab.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return await IsDataTabSelectedAsync(TelemetryTab.First);
    }

    public async Task ClickTelemetryTabAsync()
    {
        await TelemetryTab.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var pressed = await TelemetryTab.First.GetAttributeAsync("aria-pressed");
        if (!string.Equals(pressed, "true", StringComparison.OrdinalIgnoreCase))
        {
            var before = await GetFirstDataRowFingerprintAsync();
            await TelemetryTab.First.ClickAsync();
            await Assertions.Expect(TelemetryTab.First).ToHaveAttributeAsync(
                "aria-pressed",
                "true",
                new LocatorAssertionsToHaveAttributeOptions { Timeout = 5_000 });

            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < deadline)
            {
                var after = await GetFirstDataRowFingerprintAsync();
                if (!string.IsNullOrEmpty(after) && !string.Equals(after, before, StringComparison.Ordinal))
                    break;

                await Task.Delay(150);
            }
        }

        await WaitForFuelGridAsync();
    }

    public async Task<IReadOnlyList<TelemetryGridRow>> GetTelemetryDataGridRowsAsync()
    {
        await WaitForFuelGridAsync();
        try
        {
            await FuelPagination.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10_000,
            });
        }
        catch (TimeoutException)
        {
        }

        await GoToFirstFuelGridPageAsync();

        var result = new List<TelemetryGridRow>();
        while (true)
        {
            await WaitForDisplayedFuelPageAsync();
            result.AddRange(await ReadTelemetryDataGridRowsOnCurrentPageAsync());
            if (!await TryGoToNextFuelGridPageAsync())
                break;
        }

        return result;
    }

    public async Task WaitForFuelTabAsync()
    {
        await FuelTab.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000,
        });
    }

    public async Task<IReadOnlyList<ProjectFuelGridRow>> GetFuelGridRowsAsync()
    {
        await WaitForFuelGridAsync();
        try
        {
            await FuelPagination.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10_000,
            });
        }
        catch (TimeoutException)
        {
        }

        await GoToFirstFuelGridPageAsync();

        var result = new List<ProjectFuelGridRow>();
        while (true)
        {
            await WaitForDisplayedFuelPageAsync();
            result.AddRange(await ReadFuelGridRowsOnCurrentPageAsync());
            if (!await TryGoToNextFuelGridPageAsync())
                break;
        }

        return result;
    }

    public async Task<IReadOnlyList<string>> GetYearDropdownOptionsAsync()
    {
        await OpenSearchableSelectAsync(YearDropdown);
        var options = await SearchableSelectList.Locator("[role='option']").AllInnerTextsAsync();
        var result = options
            .Select(option => option.Replace('\u00A0', ' ').Trim())
            .Where(option => !string.IsNullOrEmpty(option)
                             && !option.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            .ToList();

        await Page.Keyboard.PressAsync("Escape");
        try
        {
            await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 2_000,
            });
        }
        catch (TimeoutException)
        {
        }

        return result;
    }

    public async Task SelectYearAsync(string year)
    {
        var selectedValue = YearDropdown.Locator(".select__value");
        var selection = await selectedValue.CountAsync() > 0 ? selectedValue.First : YearDropdown;
        var currentSelected = (await selection.InnerTextAsync())
            .Replace('\u00A0', ' ')
            .Trim();
        if (currentSelected.Equals(year, StringComparison.OrdinalIgnoreCase))
            return;

        var chartLine = Chart.Locator("path.chart__line");
        var previousPath = await chartLine.CountAsync() > 0
            ? await chartLine.GetAttributeAsync("d")
            : null;

        await OpenSearchableSelectAsync(YearDropdown);
        var options = SearchableSelectList.Locator("[role='option']");
        var count = await options.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var option = options.Nth(i);
            var text = (await option.TextContentAsync())?.Replace('\u00A0', ' ').Trim() ?? string.Empty;
            if (!text.Equals(year, StringComparison.OrdinalIgnoreCase))
                continue;

            await option.ClickAsync();
            try
            {
                await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = 5_000,
                });
            }
            catch (TimeoutException)
            {
            }

            await selection
                .Filter(new LocatorFilterOptions { HasTextString = text })
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });

            try
            {
                await chartLine.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 5_000,
                });
            }
            catch (TimeoutException)
            {
                return;
            }

            if (!string.IsNullOrEmpty(previousPath))
            {
                try
                {
                    await Assertions.Expect(chartLine)
                        .Not.ToHaveAttributeAsync("d", previousPath, new LocatorAssertionsToHaveAttributeOptions
                        {
                            Timeout = 15_000,
                        });
                }
                catch (PlaywrightException)
                {
                }
            }

            return;
        }

        throw new InvalidOperationException($"Year option '{year}' was not found.");
    }

    public async Task<bool> HasChartLineAsync()
    {
        var chartLine = Chart.Locator("path.chart__line");
        return await chartLine.CountAsync() > 0 && await chartLine.First.IsVisibleAsync();
    }

    public async Task SelectChartScopeAsync(int scope)
    {
        var chartLine = Chart.Locator("path.chart__line");
        var previousPath = await chartLine.CountAsync() > 0
            ? await chartLine.First.GetAttributeAsync("d")
            : null;

        var target = ChartCard.Locator("button.legend__item")
            .Filter(new LocatorFilterOptions { HasText = $"Scope {scope}" });
        await target.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var pressed = await target.First.GetAttributeAsync("aria-pressed");
        if (string.Equals(pressed, "true", StringComparison.OrdinalIgnoreCase))
            return;

        await target.First.ClickAsync();
        await Assertions.Expect(target.First)
            .ToHaveAttributeAsync("aria-pressed", "true", new LocatorAssertionsToHaveAttributeOptions
            {
                Timeout = 5_000,
            });

        if (string.IsNullOrEmpty(previousPath))
            return;

        try
        {
            await Assertions.Expect(chartLine.First)
                .Not.ToHaveAttributeAsync("d", previousPath, new LocatorAssertionsToHaveAttributeOptions
                {
                    Timeout = 5_000,
                });
        }
        catch (PlaywrightException)
        {
        }
    }

    public async Task<IReadOnlyDictionary<int, decimal>> GetChartMonthlyTonnesAsync()
    {
        await Chart.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Chart.Locator("path.chart__line")
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var tooltipValues = await TryGetChartTonnesFromTooltipsAsync(yearly: false);
        if (tooltipValues.Count > 0)
            return tooltipValues;

        var (topY, bottomY, topValue) = await GetChartYAxisScaleAsync();
        var points = await GetChartLinePointsAsync();
        if (points.Count == 0)
            return new Dictionary<int, decimal>();

        var result = new Dictionary<int, decimal>();
        var yRange = bottomY - topY;
        for (var i = 0; i < points.Count; i++)
        {
            var tonnes = yRange == 0
                ? 0m
                : (decimal)((bottomY - points[i].Y) / yRange) * topValue;
            result[i + 1] = tonnes;
        }

        return result;
    }

    public async Task<IReadOnlyDictionary<int, decimal>> GetChartYearlyTonnesAsync()
    {
        await Chart.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Chart.Locator("path.chart__line")
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        return await TryGetChartTonnesFromTooltipsAsync(yearly: true);
    }

    private async Task OpenSearchableSelectAsync(ILocator dropdown)
    {
        await dropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var trigger = dropdown.Locator("button.select__trigger");
        var clickTarget = await trigger.CountAsync() > 0 ? trigger.First : dropdown;

        var expanded = await clickTarget.GetAttributeAsync("aria-expanded");
        if (string.Equals(expanded, "true", StringComparison.OrdinalIgnoreCase)
            && await SearchableSelectList.IsVisibleAsync())
            return;

        if (await SearchableSelectList.IsVisibleAsync()
            || await Page.Locator("button.overlay[aria-label='Close dropdown']").IsVisibleAsync())
        {
            await Page.Keyboard.PressAsync("Escape");
            try
            {
                await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = 2_000,
                });
            }
            catch (TimeoutException)
            {
            }
        }

        await clickTarget.ClickAsync();
        await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    private async Task<IReadOnlyDictionary<int, decimal>> TryGetChartTonnesFromTooltipsAsync(bool yearly)
    {
        var result = new Dictionary<int, decimal>();
        var axisLabels = Chart.Locator("g.chart__x-axis text.chart__axis-label");
        var count = await axisLabels.CountAsync();
        if (count == 0)
            return result;

        var tooltip = Page.Locator(
            "//div[contains(@class,'chart__tooltip') or contains(@class,'tooltip') or @role='tooltip']");

        for (var i = 0; i < count; i++)
        {
            var label = axisLabels.Nth(i);
            if (!await label.IsVisibleAsync())
                continue;

            var labelText = NormalizeText(await label.TextContentAsync());
            int year = 0;
            if (yearly && !int.TryParse(labelText, NumberStyles.None, CultureInfo.InvariantCulture, out year))
                continue;

            try
            {
                await label.HoverAsync(new LocatorHoverOptions { Timeout = 1_500 });
            }
            catch (TimeoutException)
            {
                continue;
            }
            try
            {
                await tooltip.First.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 1_500,
                });
            }
            catch (TimeoutException)
            {
                continue;
            }

            var tooltipText = (await tooltip.First.InnerTextAsync())?.Replace('\u00A0', ' ').Trim() ?? string.Empty;
            if (!TryParseChartTooltipTonnes(tooltipText, yearly ? labelText : null, out var tonnes))
                continue;

            result[yearly ? year : i + 1] = tonnes;
        }

        await Chart.HoverAsync(new LocatorHoverOptions { Position = new Position { X = 1, Y = 1 } });
        return result;
    }

    private static bool TryParseChartTooltipTonnes(string tooltipText, string? axisLabel, out decimal tonnes)
    {
        tonnes = 0m;
        if (string.IsNullOrWhiteSpace(tooltipText))
            return false;

        var lines = tooltipText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith("Scope", StringComparison.OrdinalIgnoreCase))
                continue;

            var normalized = line.Replace(",", string.Empty, StringComparison.Ordinal)
                .Replace("tCO2e", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Trim();
            if (axisLabel is not null
                && normalized.Equals(axisLabel, StringComparison.OrdinalIgnoreCase))
                continue;

            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out tonnes))
                return true;
        }

        return false;
    }

    private async Task<(double TopY, double BottomY, decimal TopValue)> GetChartYAxisScaleAsync()
    {
        var labels = Chart.Locator("g.chart__grid text.chart__axis-label");
        var count = await labels.CountAsync();
        if (count == 0)
            throw new InvalidOperationException("Chart Y-axis labels were not found.");

        double? topY = null;
        double? bottomY = null;
        decimal topValue = 0m;

        for (var i = 0; i < count; i++)
        {
            var label = labels.Nth(i);
            var yAttr = await label.GetAttributeAsync("y");
            var text = (await label.TextContentAsync())?.Replace('\u00A0', ' ').Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(yAttr) || string.IsNullOrWhiteSpace(text))
                continue;

            var y = double.Parse(yAttr, CultureInfo.InvariantCulture);
            var value = ParseChartAxisValue(text);

            if (topY is null || y < topY)
            {
                topY = y;
                topValue = value;
            }

            if (bottomY is null || y > bottomY)
                bottomY = y;
        }

        if (topY is null || bottomY is null)
            throw new InvalidOperationException("Unable to resolve chart Y-axis scale.");

        return (topY.Value, bottomY.Value, topValue);
    }

    private async Task<IReadOnlyList<(double X, double Y)>> GetChartLinePointsAsync()
    {
        var pathD = await Chart.Locator("path.chart__line").GetAttributeAsync("d");
        if (string.IsNullOrWhiteSpace(pathD))
            return Array.Empty<(double X, double Y)>();

        var matches = Regex.Matches(pathD, @"[ML]\s*([0-9]+(?:\.[0-9]+)?)\s+([0-9]+(?:\.[0-9]+)?)", RegexOptions.IgnoreCase);
        var points = new List<(double X, double Y)>();
        foreach (Match match in matches)
        {
            var x = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var y = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            points.Add((x, y));
        }

        return points;
    }

    private static decimal ParseChartAxisValue(string label)
    {
        var normalized = label.Replace(",", string.Empty, StringComparison.Ordinal).Trim();
        if (normalized is "—" or "–" or "-" or "")
            return 0m;

        var hasK = normalized.EndsWith("k", StringComparison.OrdinalIgnoreCase);
        if (hasK)
            normalized = normalized[..^1].Trim();

        var value = decimal.Parse(normalized, CultureInfo.InvariantCulture);
        return hasK ? value * 1000m : value;
    }

    private async Task<ILocator> WaitForFuelGridAsync()
    {
        await DataGrid.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await FuelGrid.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        try
        {
            var loading = DataGrid.Locator(".table-wrapper--loading").First;
            if (await loading.CountAsync() > 0)
            {
                await loading.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Detached,
                    Timeout = 30_000,
                });
            }
        }
        catch (TimeoutException)
        {
        }

        return FuelGrid.First;
    }

    private async Task<IReadOnlyList<ProjectFuelGridRow>> ReadFuelGridRowsOnCurrentPageAsync()
    {
        var grid = FuelGrid.First;
        var rawRows = await grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => !tr.querySelector('.table__skeleton-bar'))
              .map(tr => [...tr.querySelectorAll(':scope > td')]
                .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim()))
              .filter(cells => cells.some(cell => cell.length > 0))
            """);

        var headers = (await grid.Locator("thead th").AllInnerTextsAsync())
            .Select(header => NormalizeText(header).ToUpperInvariant())
            .ToList();

        var result = new List<ProjectFuelGridRow>();
        foreach (var cells in rawRows)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < cells.Length && i < headers.Count; i++)
            {
                if (headers[i].Length == 0)
                    continue;

                values[headers[i]] = cells[i];
            }

            var invoiceNumber = Cell(values, "INVOICE NUMBER");
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                continue;

            result.Add(new ProjectFuelGridRow(
                invoiceNumber,
                Cell(values, "COMPANY"),
                Cell(values, "INVOICE DATE"),
                Cell(values, "STATUS"),
                Cell(values, "IMPORT DATE"),
                Cell(values, "TOTAL"),
                Cell(values, "SOURCE"),
                Cell(values, "CATEGORY")));
        }

        return result;
    }

    private async Task<IReadOnlyList<ProjectOfficeGridRow>> ReadOfficeGridRowsOnCurrentPageAsync()
    {
        var pages = await ReadDataGridPagesAsync();
        var result = new List<ProjectOfficeGridRow>();
        foreach (var values in pages)
        {
            var company = Cell(values, "COMPANY");
            var billDate = Cell(values, "UTILITY BILL DATE");
            if (string.IsNullOrWhiteSpace(company) && string.IsNullOrWhiteSpace(billDate))
                continue;

            result.Add(new ProjectOfficeGridRow(
                company,
                billDate,
                Cell(values, "STATUS"),
                Cell(values, "IMPORT DATE"),
                Cell(values, "TOTAL"),
                Cell(values, "SOURCE"),
                Cell(values, "CATEGORY")));
        }

        return result;
    }

    private async Task<IReadOnlyList<TelemetryGridRow>> ReadTelemetryDataGridRowsOnCurrentPageAsync()
    {
        var pages = await ReadDataGridPagesAsync();
        var result = new List<TelemetryGridRow>();
        foreach (var values in pages)
        {
            var equipmentTag = Cell(values, "EQUIPMENT TAG");
            if (string.IsNullOrWhiteSpace(equipmentTag))
                continue;

            result.Add(new TelemetryGridRow
            {
                Month = Cell(values, "MONTH"),
                EquipmentTag = equipmentTag,
                EquipmentType = Cell(values, "EQUIPMENT TYPE"),
                Location = Cell(values, "LOCATION"),
                OperatingHours = Cell(values, "OPERATING HOURS"),
                FuelType = Cell(values, "FUEL TYPE"),
                ImportDate = Cell(values, "IMPORT DATE"),
                Source = Cell(values, "SOURCE"),
            });
        }

        return result;
    }

    private async Task<IReadOnlyList<Dictionary<string, string>>> ReadDataGridPagesAsync()
    {
        var grid = FuelGrid.First;
        var rawRows = await grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => !tr.querySelector('.table__skeleton-bar'))
              .map(tr => [...tr.querySelectorAll(':scope > td')]
                .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim()))
              .filter(cells => cells.some(cell => cell.length > 0))
            """);

        var headers = (await grid.Locator("thead th").AllInnerTextsAsync())
            .Select(header => NormalizeText(header).ToUpperInvariant())
            .ToList();

        var result = new List<Dictionary<string, string>>();
        foreach (var cells in rawRows)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < cells.Length && i < headers.Count; i++)
            {
                if (headers[i].Length == 0)
                    continue;

                values[headers[i]] = cells[i];
            }

            if (values.Count > 0)
                result.Add(values);
        }

        return result;
    }

    private async Task ClickFuelNextPageAsync()
    {
        if (await FuelNextPageBtn.CountAsync() == 0)
            return;

        var button = FuelNextPageBtn.First;
        await button.ScrollIntoViewIfNeededAsync();
        // Next stays disabled while rowsLoading is true, including after a superseded
        // request leaves that flag set. The click handler still pages when the total says so.
        await button.EvaluateAsync(
            """
            el => {
              el.disabled = false;
              el.removeAttribute('disabled');
              el.removeAttribute('aria-disabled');
              el.click();
            }
            """);
    }

    private async Task<string> GetDataPageFingerprintAsync()
    {
        if (await FuelGrid.CountAsync() == 0)
            return string.Empty;

        return await FuelGrid.First.EvaluateAsync<string>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => !tr.querySelector('.table__skeleton-bar'))
              .map(tr => [...tr.querySelectorAll(':scope > td')]
                .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim())
                .filter(value => value.length > 0)
                .join('|'))
              .filter(row => row.length > 0)
              .join('||')
            """);
    }

    private async Task<string> GetFirstDataRowFingerprintAsync()
    {
        if (await FuelGrid.CountAsync() == 0)
            return string.Empty;

        return await FuelGrid.First.EvaluateAsync<string>(
            """
            table => {
              const rows = [...table.querySelectorAll('tbody tr')]
                .filter(tr => !tr.querySelector('.table__skeleton-bar'));
              for (const tr of rows) {
                const text = [...tr.querySelectorAll(':scope > td')]
                  .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim())
                  .filter(value => value.length > 0)
                  .join('|');
                if (text.length > 0)
                  return text;
              }
              return '';
            }
            """);
    }

    private static async Task<bool> IsDataTabSelectedAsync(ILocator tab) =>
        await tab.EvaluateAsync<bool>(
            """
            el => el.getAttribute('aria-selected') === 'true'
              || el.getAttribute('aria-pressed') === 'true'
              || el.getAttribute('aria-current') === 'page'
              || el.getAttribute('aria-current') === 'true'
              || el.classList.contains('active')
              || el.classList.contains('is-active')
              || el.classList.contains('selected')
              || el.className.includes('--active')
              || el.getAttribute('data-state') === 'active'
            """);

    private async Task GoToFirstFuelGridPageAsync()
    {
        await WaitForFuelDataPageAsync();

        while (await CanGoToFuelPageAsync(FuelPreviousPageBtn))
        {
            var showingBefore = await TryGetFuelShowingRangeAsync();
            await FuelPreviousPageBtn.First.ClickAsync(new LocatorClickOptions { Timeout = 5_000, Force = true });
            await WaitForFuelPageChangeAsync(showingBefore);
        }
    }

    private async Task WaitForFuelDataPageAsync()
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            var showing = await TryGetFuelShowingRangeAsync();
            if (showing is not null && !await IsFuelDataLoadingAsync())
                return;

            // Rows and the real total can already be on screen while Next stays disabled.
            if (showing is not null && showing.Value.End < showing.Value.Total)
                return;

            await Task.Delay(150);
        }
    }

    private async Task<bool> TryGoToNextFuelGridPageAsync()
    {
        var decisionDeadline = DateTime.UtcNow.AddSeconds(12);
        var fullPageSince = (DateTime?)null;
        while (DateTime.UtcNow < decisionDeadline)
        {
            var showing = await TryGetFuelShowingRangeAsync();
            if (showing is null)
            {
                fullPageSince = null;
                await Task.Delay(200);
                continue;
            }

            if (showing.Value.End < showing.Value.Total)
                return await AdvanceFuelPageAsync(showing);

            var pageLength = showing.Value.End - showing.Value.Start + 1;
            var loading = await IsFuelDataLoadingAsync();
            if (!loading && pageLength < FuelDataPageSize)
                return false;

            if (!loading)
            {
                fullPageSince ??= DateTime.UtcNow;
                if (DateTime.UtcNow - fullPageSince.Value >= TimeSpan.FromSeconds(2))
                    return false;
            }
            else
            {
                fullPageSince = null;
            }

            await Task.Delay(200);
        }

        var finalShowing = await TryGetFuelShowingRangeAsync();
        if (finalShowing is not null && finalShowing.Value.End < finalShowing.Value.Total)
            return await AdvanceFuelPageAsync(finalShowing);

        return false;
    }

    private async Task<bool> AdvanceFuelPageAsync((int Start, int End, int Total)? showingBefore)
    {
        var pageBefore = await GetDataPageFingerprintAsync();
        await ClickFuelNextPageAsync();

        var deadline = DateTime.UtcNow.AddSeconds(10);
        var nextClickAt = DateTime.UtcNow.AddSeconds(2);
        while (DateTime.UtcNow < deadline)
        {
            var showingAfter = await TryGetFuelShowingRangeAsync();
            var pageAfter = await GetDataPageFingerprintAsync();
            var showingAdvanced = showingBefore is not null
                && showingAfter is not null
                && showingAfter.Value.Start != showingBefore.Value.Start;
            var rowsAdvanced = !string.IsNullOrEmpty(pageAfter)
                && !string.Equals(pageAfter, pageBefore, StringComparison.Ordinal);
            if (rowsAdvanced && (showingAdvanced || showingBefore is null))
                return true;

            if (DateTime.UtcNow >= nextClickAt)
            {
                await ClickFuelNextPageAsync();
                nextClickAt = DateTime.UtcNow.AddSeconds(2);
            }

            await Task.Delay(150);
        }

        var stayedOn = showingBefore is null
            ? "an unknown page"
            : $"rows {showingBefore.Value.Start}-{showingBefore.Value.End} of {showingBefore.Value.Total}";
        throw new TimeoutException($"Fuel grid stayed on {stayedOn}.");
    }

    private async Task<bool> CanGoToFuelPageAsync(ILocator button)
    {
        if (await button.CountAsync() == 0 || !await button.First.IsVisibleAsync())
            return false;

        var isNext = (await button.First.InnerTextAsync()).Contains("Next", StringComparison.OrdinalIgnoreCase);
        var showing = await TryGetFuelShowingRangeAsync();
        if (showing is not null)
        {
            return isNext
                ? showing.Value.End < showing.Value.Total
                : showing.Value.Start > 1;
        }

        return await button.First.EvaluateAsync<bool>(
            """
            el => !(
              el.disabled
              || el.hasAttribute('disabled')
              || el.getAttribute('aria-disabled') === 'true'
              || el.classList.contains('disabled')
            )
            """);
    }

    private async Task WaitForFuelPageChangeAsync((int Start, int End, int Total)? showingBefore)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var showingAfter = await TryGetFuelShowingRangeAsync();
            if (showingBefore is null
                || (showingAfter is not null && showingAfter.Value.Start != showingBefore.Value.Start))
                return;

            await Task.Delay(150);
        }
    }

    private async Task WaitForDisplayedFuelPageAsync()
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var showing = await TryGetFuelShowingRangeAsync();
            if (showing is null || await IsFuelDataLoadingAsync())
            {
                await Task.Delay(100);
                continue;
            }

            var expected = showing.Value.End - showing.Value.Start + 1;
            if (expected > 0 && await CountDisplayedDataRowsAsync() == expected)
                return;

            await Task.Delay(100);
        }
    }

    private async Task<int> CountDisplayedDataRowsAsync()
    {
        if (await FuelGrid.CountAsync() == 0)
            return 0;

        return await FuelGrid.First.EvaluateAsync<int>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => !tr.querySelector('.table__skeleton-bar'))
              .filter(tr => [...tr.querySelectorAll(':scope > td')]
                .some(td => (td.innerText || '').replace(/\s+/g, ' ').trim().length > 0))
              .length
            """);
    }

    private async Task<bool> IsFuelDataLoadingAsync() =>
        await DataGrid.Locator(".table-wrapper--loading").CountAsync() > 0;

    private async Task<(int Start, int End, int Total)?> TryGetFuelShowingRangeAsync()
    {
        var showing = FuelPagination.Locator(".pagination__info").First;
        if (await showing.CountAsync() == 0)
            return null;

        var text = (await showing.InnerTextAsync()).Replace('\u00A0', ' ');
        var match = Regex.Match(
            text,
            @"Showing\s+(\d+)\D+(\d+)\s+of\s+(\d+)",
            RegexOptions.IgnoreCase);
        if (!match.Success)
            return null;

        return (
            int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture));
    }

    private static string Cell(IReadOnlyDictionary<string, string> values, string name) =>
        values.TryGetValue(name, out var value) ? value : string.Empty;

    private static string NormalizeText(string? value) =>
        System.Text.RegularExpressions.Regex.Replace(value ?? string.Empty, @"\s+", " ").Trim();
}

public sealed record ProjectOfficeGridRow(
    string Company,
    string BillDate,
    string Status,
    string ImportDate,
    string Total,
    string Source,
    string Category);

public sealed record ProjectFuelGridRow(
    string InvoiceNumber,
    string Company,
    string InvoiceDate,
    string Status,
    string ImportDate,
    string Total,
    string Source,
    string Category);

public sealed record ProjectOfficesCardGridRow(
    string Office,
    string Area,
    string Method,
    string EmissionType,
    string Tonnes,
    string TonnesPerSquareFoot);
