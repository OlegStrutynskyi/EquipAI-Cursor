using System.Globalization;
using System.Text.Json;
using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class WasteReportPage : BasePage
{
    public WasteReportPage(IPage page) : base(page) { }

    private const string EmptySearchMessage = "No waste-report readings match your search.";

    private ILocator PageTitle => Page.Locator("//h1[@id='waste-report-list-title']");
    private ILocator Message => Page.Locator("//p[@class='page-header__lead']");
    private ILocator ImportBtn => Page.Locator("//a[contains(@href,'/waste-reports/import')]");
    private ILocator MonthLabel => Page.Locator("label[for='waste-report-month'] span.form-label");
    private ILocator MonthField => Page.Locator("#waste-report-month");
    private ILocator SearchInput => Page.Locator("#waste-report-list-search");
    private ILocator Grid => Page.Locator("table.waste-report-list__table");

    public async Task OpenAsync()
    {
        await Page.GotoAsync(Config.BaseUrl + "waste-reports");
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

    public async Task<string> GetMonthFieldValueAsync()
    {
        await MonthField.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await MonthField.InputValueAsync()).Trim();
    }

    public async Task<string> GetMonthFieldDisplayValueAsync()
    {
        var value = await GetMonthFieldValueAsync();
        if (DateTime.TryParseExact(
                value,
                ["yyyy-MM", "yyyy-MM-dd"],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var month))
            return month.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

        return value;
    }

    public async Task SetMonthAsync(string yearMonth)
    {
        var value = DateTime.ParseExact(yearMonth, ["yyyy-MM", "yyyy-MM-dd"], CultureInfo.InvariantCulture)
            .ToString("yyyy-MM", CultureInfo.InvariantCulture);
        await MonthField.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await MonthField.EvaluateAsync(
            """
            (input, value) => {
              const descriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(input), 'value');
              descriptor?.set?.call(input, value);
              input.dispatchEvent(new Event('input', { bubbles: true }));
              input.dispatchEvent(new Event('change', { bubbles: true }));
            }
            """,
            value);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task SearchAsync(string text)
    {
        var responseTask = Page.WaitForResponseAsync(
            response => IsWasteSearchResponse(response, text),
            new PageWaitForResponseOptions { Timeout = 15_000 });
        await SearchInput.FillAsync(text);
        var response = await responseTask;
        var expectedTickets = await ReadSearchTicketNumbersAsync(response);

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            if (await Grid.Locator(".table__skeleton-bar").CountAsync() > 0)
            {
                await Task.Delay(50);
                continue;
            }

            if (expectedTickets.Count == 0)
            {
                if (await IsEmptySearchMessageVisibleAsync())
                    return;
            }
            else
            {
                var actualTickets = (await GetGridRowsOnCurrentPageAsync())
                    .Select(row => row.TicketNumber)
                    .ToList();
                if (actualTickets.SequenceEqual(expectedTickets))
                    return;
            }

            await Task.Delay(50);
        }

        throw new TimeoutException($"Waste report search for '{text}' did not finish.");
    }

    public async Task<string> GetGridEmptyMessageAsync()
    {
        var message = Grid.Locator("tbody").GetByText(
            EmptySearchMessage,
            new LocatorGetByTextOptions { Exact = true });
        await message.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10_000
        });
        return NormalizeGridText(await message.First.InnerTextAsync());
    }

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await Grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(NormalizeGridText)
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

    public async Task<IReadOnlyList<WasteReportGridRow>> GetCurrentPageGridRowsAsync()
    {
        await WaitForGridSettledAsync();
        return await GetGridRowsOnCurrentPageAsync();
    }

    public async Task<IReadOnlyList<WasteReportGridRow>> GetGridRowsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        try
        {
            await Page.WaitForFunctionAsync(
                """
                () => [...document.querySelectorAll('table.waste-report-list__table tbody tr')]
                  .some(tr => {
                    if (tr.closest('.table-wrapper--loading')) return false;
                    if (tr.querySelector('.table__skeleton-bar')) return false;
                    const cells = tr.querySelectorAll('td');
                    return cells.length > 1 && [...cells].some(td => (td.innerText || '').trim().length > 0);
                  })
                """,
                null,
                new PageWaitForFunctionOptions { Timeout = 30_000 });
        }
        catch (TimeoutException)
        {
            return Array.Empty<WasteReportGridRow>();
        }

        var nextBtn = Page.Locator("nav.pagination button").Filter(new LocatorFilterOptions { HasTextString = "Next" });
        var previousBtn = Page.Locator("nav.pagination button").Filter(new LocatorFilterOptions { HasTextString = "Previous" });

        while (await CanGoToAdjacentPageAsync(previousBtn))
        {
            await previousBtn.EvaluateAsync("el => el.click()");
            await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        var result = new List<WasteReportGridRow>();
        while (true)
        {
            result.AddRange(await GetGridRowsOnCurrentPageAsync());
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

                await Page.WaitForTimeoutAsync(200);
            }

            if (!pageChanged)
                break;
        }

        return result;
    }

    public async Task<ImportPage> ClickImportBtnAsync()
    {
        await ImportBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/waste-reports/import", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 30_000 });
        var importPage = new ImportPage(Page);
        await importPage.WaitForLoadedAsync();
        return importPage;
    }

    private async Task<IReadOnlyList<WasteReportGridRow>> GetGridRowsOnCurrentPageAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var rawRows = await Grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .map(tr => [...tr.querySelectorAll('td')]
                .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim()))
              .filter(cells => cells.length > 1 && cells.some(cell => cell.length > 0))
            """);

        var headers = (await Grid.Locator("thead th").AllInnerTextsAsync())
            .Select(header => NormalizeGridText(header).ToUpperInvariant())
            .ToList();

        var result = new List<WasteReportGridRow>();
        foreach (var cells in rawRows)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < cells.Length && c < headers.Count; c++)
                values[headers[c]] = cells[c];

            result.Add(new WasteReportGridRow
            {
                TicketNumber = GetCell(values, "TICKET NUMBER"),
                ContainerId = GetCell(values, "CONTAINER ID"),
                SiteName = GetCell(values, "SITE NAME"),
                Location = GetCell(values, "LOCATION"),
                Project = GetCell(values, "PROJECT"),
                HaulDate = GetCell(values, "HAUL DATE"),
                Tonnage = GetCellByPrefix(values, "TONNAGE"),
                EmissionType = GetCell(values, "EMISSION TYPE"),
                Company = GetCell(values, "COMPANY"),
            });
        }

        return result;
    }

    private async Task<bool> IsEmptySearchMessageVisibleAsync()
    {
        var message = Grid.Locator("tbody").GetByText(
            EmptySearchMessage,
            new LocatorGetByTextOptions { Exact = true });
        return await message.CountAsync() > 0 && await message.First.IsVisibleAsync();
    }

    private static async Task<IReadOnlyList<string>> ReadSearchTicketNumbersAsync(IResponse response)
    {
        using var document = JsonDocument.Parse(await response.TextAsync());
        if (!document.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
            return [];

        var tickets = new List<string>();
        foreach (var item in items.EnumerateArray())
        {
            tickets.Add(item.TryGetProperty("ticketNumber", out var ticket) && ticket.ValueKind == JsonValueKind.String
                ? ticket.GetString()?.Trim() ?? string.Empty
                : string.Empty);
        }

        return tickets;
    }

    private static bool IsWasteSearchResponse(IResponse response, string text)
    {
        if (!response.Ok || !Uri.TryCreate(response.Url, UriKind.Absolute, out var uri))
            return false;

        if (!uri.AbsolutePath.Equals("/api/waste-reports", StringComparison.OrdinalIgnoreCase))
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

        return await button.EvaluateAsync<bool>(
            "el => !el.disabled && el.getAttribute('aria-disabled') !== 'true'");
    }

    private async Task<(int Start, int End, int Total)?> TryGetShowingRangeAsync()
    {
        var info = Page.Locator("nav.pagination .pagination__info").First;
        if (await info.CountAsync() == 0)
            return null;

        var text = NormalizeGridText((await info.InnerTextAsync())
            .Replace('–', '-')
            .Replace('—', '-')
            .Replace('−', '-')
            .Replace('‐', '-'));
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
            return null;

        var text = NormalizeGridText(await pager.InnerTextAsync());
        var match = System.Text.RegularExpressions.Regex.Match(
            text,
            @"Page\s+(\d+)\s+of\s+(\d+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success)
            return null;

        return (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
    }

    private async Task<string> GetPageFingerprintAsync()
    {
        var rows = await GetGridRowsOnCurrentPageAsync();
        return string.Join("||", rows.Take(3).Select(row => $"{row.TicketNumber}|{row.ContainerId}|{row.HaulDate}|{row.Tonnage}"));
    }

    private async Task WaitForGridSettledAsync()
    {
        string? previous = null;
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            if (await Grid.Locator(".table__skeleton-bar").CountAsync() > 0)
            {
                await Task.Delay(100);
                continue;
            }

            var snapshot = await GetPageFingerprintAsync();
            if (previous is not null && snapshot == previous)
                return;

            previous = snapshot;
            await Task.Delay(200);
        }
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

    private static string GetCellByPrefix(IReadOnlyDictionary<string, string> values, string prefix)
    {
        foreach (var pair in values)
        {
            if (pair.Key.Equals(prefix, StringComparison.OrdinalIgnoreCase)
                || pair.Key.StartsWith(prefix + " ", StringComparison.OrdinalIgnoreCase)
                || pair.Key.StartsWith(prefix + "(", StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        }

        return string.Empty;
    }

    private static string NormalizeGridText(string text) =>
        System.Text.RegularExpressions.Regex.Replace(text.Replace('\u00A0', ' '), @"\s+", " ").Trim();
}

public sealed class WasteReportGridRow
{
    public required string TicketNumber { get; init; }
    public required string ContainerId { get; init; }
    public required string SiteName { get; init; }
    public required string Location { get; init; }
    public required string Project { get; init; }
    public required string HaulDate { get; init; }
    public required string Tonnage { get; init; }
    public required string EmissionType { get; init; }
    public required string Company { get; init; }
}
