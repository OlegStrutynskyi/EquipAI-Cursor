using System.Globalization;
using System.Text.Json;
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
    private ILocator SearchInput => Page.Locator("#telemetry-list-search");
    private ILocator Grid => Page.Locator("table.table.telemetry-list__table, table.table")
        .Locator("visible=true").First;

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

    public async Task<bool> IsSearchInputVisibleAsync()
    {
        try
        {
            await SearchInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10_000
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public async Task<string> GetSearchPlaceholderAsync() =>
        (await SearchInput.GetAttributeAsync("placeholder"))?.Trim() ?? string.Empty;

    public async Task SearchAsync(string text)
    {
        var responseTask = Page.WaitForResponseAsync(
            response => IsTelemetrySearchResponse(response, text),
            new PageWaitForResponseOptions { Timeout = 15_000 });
        await SearchInput.FillAsync(text);
        var response = await responseTask;
        var expectedTags = await ReadSearchEquipmentTagsAsync(response);

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            if (await Grid.Locator(".table__skeleton-bar").CountAsync() > 0)
            {
                await Task.Delay(50);
                continue;
            }

            if (expectedTags.Count == 0)
            {
                if (await IsEmptySearchMessageVisibleAsync())
                    return;
            }
            else
            {
                var actualTags = (await GetGridRowsOnCurrentPageAsync())
                    .Select(row => row.EquipmentTag)
                    .ToList();
                if (actualTags.SequenceEqual(expectedTags))
                    return;
            }

            await Task.Delay(50);
        }

        throw new TimeoutException($"Telemetry search for '{text}' did not finish.");
    }

    public async Task<string> GetGridEmptyMessageAsync()
    {
        var message = Grid.Locator("tbody").GetByText(
            "No telemetry readings match your search.",
            new LocatorGetByTextOptions { Exact = true });
        await message.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10_000
        });
        return NormalizeGridText(await message.First.InnerTextAsync());
    }

    private async Task<bool> IsEmptySearchMessageVisibleAsync()
    {
        var message = Grid.Locator("tbody").GetByText(
            "No telemetry readings match your search.",
            new LocatorGetByTextOptions { Exact = true });
        return await message.CountAsync() > 0 && await message.First.IsVisibleAsync();
    }

    private static async Task<IReadOnlyList<string>> ReadSearchEquipmentTagsAsync(IResponse response)
    {
        using var document = JsonDocument.Parse(await response.TextAsync());
        if (!document.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
            return [];

        var tags = new List<string>();
        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("equipmentTag", out var tag) || tag.ValueKind != JsonValueKind.String)
                continue;

            var value = tag.GetString();
            if (!string.IsNullOrWhiteSpace(value))
                tags.Add(value);
        }

        return tags;
    }

    private static bool IsTelemetrySearchResponse(IResponse response, string text)
    {
        if (!response.Ok || !Uri.TryCreate(response.Url, UriKind.Absolute, out var uri))
            return false;

        if (!uri.AbsolutePath.Equals("/api/telemetry", StringComparison.OrdinalIgnoreCase))
            return false;

        var query = uri.Query.TrimStart('?');
        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = part.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = Uri.UnescapeDataString(part[..separator]);
            var value = Uri.UnescapeDataString(part[(separator + 1)..].Replace("+", " "));
            if (key.Equals("Search", StringComparison.OrdinalIgnoreCase)
                && value.Equals(text, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static string NormalizeGridText(string text) =>
        System.Text.RegularExpressions.Regex.Replace(text.Replace('\u00A0', ' '), @"\s+", " ").Trim();

    public async Task SetMonthAsync(string yearMonth)
    {
        await MonthField.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var valueToFill = ToMonthInputValue(yearMonth);

        await MonthField.FillAsync(valueToFill);
        await MonthField.BlurAsync();
        if (!await MonthFieldShowsAsync(yearMonth))
        {
            await MonthField.EvaluateAsync(
                """
                (input, value) => {
                  const descriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(input), 'value');
                  descriptor?.set?.call(input, value);
                  input.dispatchEvent(new Event('input', { bubbles: true }));
                  input.dispatchEvent(new Event('change', { bubbles: true }));
                }
                """,
                valueToFill);
            await MonthField.BlurAsync();
        }

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

    public async Task ClickGridColumnAsync(string columnName)
    {
        var header = Grid.Locator("thead th").Filter(new LocatorFilterOptions { HasTextString = columnName }).First;
        await header.ScrollIntoViewIfNeededAsync();
        var ariaBefore = await header.GetAttributeAsync("aria-sort");
        var fingerprintBefore = await GetPageFingerprintAsync();
        var showingBefore = await TryGetShowingRangeAsync();
        await header.Locator("button.table__sort").ClickAsync();

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            var ariaAfter = await header.GetAttributeAsync("aria-sort");
            if (string.Equals(ariaAfter, ariaBefore, StringComparison.Ordinal))
            {
                await Task.Delay(100);
                continue;
            }

            var refreshDeadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < refreshDeadline)
            {
                var showingAfter = await TryGetShowingRangeAsync();
                var onFirstPage = showingAfter is null || showingAfter.Value.Start == 1;
                var fingerprintAfter = await GetPageFingerprintAsync();
                if (onFirstPage
                    && fingerprintAfter.Length > 0
                    && !string.Equals(fingerprintAfter, fingerprintBefore, StringComparison.Ordinal))
                {
                    await WaitForGridSettledAsync();
                    var settled = await GetPageFingerprintAsync();
                    if (!string.Equals(settled, fingerprintBefore, StringComparison.Ordinal))
                        return;
                }

                await Task.Delay(100);
            }

            if (showingBefore is null || showingBefore.Value.Start == 1)
            {
                await WaitForGridSettledAsync();
                var settled = await GetPageFingerprintAsync();
                if (!string.Equals(settled, fingerprintBefore, StringComparison.Ordinal))
                    return;
            }

            break;
        }

        await WaitForGridSettledAsync();
    }

    public async Task<IReadOnlyList<TelemetryGridRow>> GetCurrentPageGridRowsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        try
        {
            var loading = Page.Locator(".table-wrapper--loading").First;
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

        return await GetGridRowsOnCurrentPageAsync();
    }

    public async Task<IReadOnlyList<TelemetryGridRow>> GetGridRowsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var hasData = false;
        try
        {
            var loading = Page.Locator(".table-wrapper--loading").First;
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

        try
        {
            await Page.WaitForFunctionAsync(
                """
                () => [...document.querySelectorAll('table.table tbody tr')]
                  .some(tr => {
                    if (tr.closest('.table-wrapper--loading')) return false;
                    if (tr.querySelector('.table__skeleton-bar')) return false;
                    const cells = tr.querySelectorAll('td');
                    return cells.length > 1 && (cells[1].innerText || '').trim().length > 0;
                  })
                """,
                null,
                new PageWaitForFunctionOptions { Timeout = 30_000 });
            hasData = true;
        }
        catch (TimeoutException)
        {
            return Array.Empty<TelemetryGridRow>();
        }

        if (!hasData)
            return Array.Empty<TelemetryGridRow>();

        var nextBtn = Page.Locator("nav.pagination button").Filter(new LocatorFilterOptions { HasTextString = "Next" });
        var previousBtn = Page.Locator("nav.pagination button").Filter(new LocatorFilterOptions { HasTextString = "Previous" });

        while (await CanGoToAdjacentPageAsync(previousBtn))
        {
            await previousBtn.EvaluateAsync("el => el.click()");
            await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        var result = new List<TelemetryGridRow>();
        var seenTags = new HashSet<string>(StringComparer.Ordinal);
        while (true)
        {
            foreach (var row in await GetGridRowsOnCurrentPageAsync())
            {
                if (seenTags.Add(row.EquipmentTag))
                    result.Add(row);
            }

            if (!await CanGoToAdjacentPageAsync(nextBtn))
                break;

            var showingBefore = await TryGetShowingRangeAsync();
            await nextBtn.EvaluateAsync("el => el.click()");
            await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var pageChanged = false;
            var deadline = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < deadline)
            {
                var showingAfter = await TryGetShowingRangeAsync();
                if (showingBefore is not null
                    && showingAfter is not null
                    && showingAfter.Value.Start != showingBefore.Value.Start)
                {
                    pageChanged = true;
                    break;
                }

                var pageTags = await GetGridRowsOnCurrentPageAsync();
                if (pageTags.Any(r => !seenTags.Contains(r.EquipmentTag)))
                {
                    pageChanged = true;
                    break;
                }

                await Page.WaitForTimeoutAsync(200);
            }

            if (!pageChanged)
                break;
        }

        return result;
    }

    private async Task<IReadOnlyList<TelemetryGridRow>> GetGridRowsOnCurrentPageAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var rawRows = await Grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .map(tr => [...tr.querySelectorAll('td')]
                .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').trim()))
              .filter(cells => cells.length > 1 && cells[1])
            """);

        var headers = (await Grid.Locator("thead th").AllInnerTextsAsync())
            .Select(h => h.Replace('\u00A0', ' ').Trim().ToUpperInvariant())
            .ToList();

        var result = new List<TelemetryGridRow>();
        foreach (var cells in rawRows)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < cells.Length && c < headers.Count; c++)
                values[headers[c]] = cells[c];

            var equipmentTag = GetCell(values, "EQUIPMENT TAG");
            if (string.IsNullOrWhiteSpace(equipmentTag))
                continue;

            result.Add(new TelemetryGridRow
            {
                Month = GetCell(values, "MONTH"),
                EquipmentTag = equipmentTag,
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

    private async Task<bool> CanGoToAdjacentPageAsync(ILocator button)
    {
        if (await button.CountAsync() == 0 || !await button.IsVisibleAsync())
            return false;

        var isNext = (await button.InnerTextAsync()).Contains("Next", StringComparison.OrdinalIgnoreCase);
        var showing = await TryGetShowingRangeAsync();
        if (showing is not null)
        {
            return isNext
                ? showing.Value.End < showing.Value.Total
                : showing.Value.Start > 1;
        }

        var pages = await TryGetPagerPagesAsync();
        if (pages is not null)
        {
            return isNext
                ? pages.Value.Current < pages.Value.Total
                : pages.Value.Current > 1;
        }

        // Last resort: click Next if the DOM says it's enabled (ignore Playwright disabled quirks).
        return await button.EvaluateAsync<bool>(
            "el => !el.disabled && el.getAttribute('aria-disabled') !== 'true'");
    }

    private async Task<(int Start, int End, int Total)?> TryGetShowingRangeAsync()
    {
        var info = Page.Locator("nav.pagination .pagination__info").First;
        if (await info.CountAsync() == 0)
            info = Page.Locator("nav.pagination").First;
        if (await info.CountAsync() == 0)
            return null;

        var text = (await info.InnerTextAsync())
            .Replace('\u00A0', ' ')
            .Replace('–', '-')
            .Replace('—', '-')
            .Replace('−', '-')
            .Replace('‐', '-');
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        var match = System.Text.RegularExpressions.Regex.Match(
            text,
            @"Showing\s+(\d+)\s*-\s*(\d+)\s+of\s+(\d+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success)
            return null;

        return (
            int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[3].Value));
    }

    private async Task<(int Current, int Total)?> TryGetPagerPagesAsync()
    {
        var pager = Page.Locator("nav.pagination .pagination__status").First;
        if (await pager.CountAsync() == 0)
            pager = Page.Locator("nav.pagination").First;
        if (await pager.CountAsync() == 0)
            return null;

        var text = System.Text.RegularExpressions.Regex.Replace(
            (await pager.InnerTextAsync()).Replace('\u00A0', ' '),
            @"\s+",
            " ").Trim();
        var match = System.Text.RegularExpressions.Regex.Match(
            text,
            @"Page\s+(\d+)\s+of\s+(\d+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success)
            return null;

        return (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
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
        if (TryParseMonth(value, out var monthValue))
            return monthValue.ToString("MM/yyyy", CultureInfo.InvariantCulture);

        return value;
    }

    private async Task<bool> MonthFieldShowsAsync(string yearMonth)
    {
        var current = await GetMonthFieldValueAsync();
        if (!TryParseMonth(yearMonth, out var requested) || !TryParseMonth(current, out var actual))
            return string.Equals(current, ToMonthInputValue(yearMonth), StringComparison.Ordinal);

        return actual.Year == requested.Year && actual.Month == requested.Month;
    }

    private static string ToMonthInputValue(string yearMonth) =>
        TryParseMonth(yearMonth, out var month)
            ? month.ToString("MM/yyyy", CultureInfo.InvariantCulture)
            : yearMonth;

    private static bool TryParseMonth(string value, out DateTime month) =>
        DateTime.TryParseExact(
            value?.Trim(),
            ["yyyy-MM", "MM/yyyy", "M/yyyy"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out month);

    private static string GetCell(IReadOnlyDictionary<string, string> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value))
                return value;
        }

        return string.Empty;
    }

    private async Task<string> GetPageFingerprintAsync()
    {
        var rows = await GetGridRowsOnCurrentPageAsync();
        return string.Join("||", rows.Take(3).Select(row => $"{row.EquipmentTag}|{row.OperatingHours}|{row.ImportDate}"));
    }

    private async Task WaitForGridSettledAsync()
    {
        string? previous = null;
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var snapshot = await GetPageFingerprintAsync();
            if (previous is not null && snapshot.Length > 0 && snapshot == previous)
                return;

            previous = snapshot;
            await Task.Delay(200);
        }
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
