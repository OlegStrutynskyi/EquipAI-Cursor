using System.Text.Json;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class OfficesPage : BasePage
{
    public OfficesPage(IPage page) : base(page) { }

    private ILocator Title => Page.Locator("//h1[@id='admin-offices-title']");
    private ILocator Subtitle => Page.Locator("//span[@class='page-header__subtitle']");
    private ILocator Message => Page.Locator("//p[@class='page-header__lead']");
    private ILocator AddOfficeBtn => Page.Locator("//button[normalize-space()='Add office']");
    private ILocator SearchInput => Page.Locator("#admin-offices-search");
    private ILocator Grid => Page.Locator("//table[contains(@class,'table')]");
    private ILocator Pager => Page.Locator("nav[aria-label='Offices pagination']");
    private ILocator NextPageBtn => Pager.GetByRole(AriaRole.Button, new() { Name = "Next", Exact = true });
    private ILocator PreviousPageBtn => Pager.GetByRole(AriaRole.Button, new() { Name = "Previous", Exact = true });

    public async Task OpenAsync()
    {
        var sideMenuPage = new SideMenuPage(Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickOfficesAsync();
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await Title.WaitForAsync();
        await Message.WaitForAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public new async Task<string> GetPageTitleAsync()
    {
        await Title.WaitForAsync();
        return await Title.EvaluateAsync<string>(
            """
            el => {
              const clone = el.cloneNode(true);
              clone.querySelectorAll('.page-header__subtitle').forEach(node => node.remove());
              return (clone.textContent || '').replace(/\s+/g, ' ').trim();
            }
            """);
    }

    public async Task<string> GetSubtitleAsync()
    {
        await Subtitle.WaitForAsync();
        return ((await Subtitle.InnerTextAsync()) ?? string.Empty).Trim();
    }

    public async Task<string> GetMessageAsync()
    {
        await Message.WaitForAsync();
        return (await Message.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsAddOfficeBtnVisibleAsync() => AddOfficeBtn.IsVisibleAsync();
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
            response => IsOfficesSearchResponse(response, text),
            new PageWaitForResponseOptions { Timeout = 15_000 });
        await SearchInput.FillAsync(text);
        var response = await responseTask;
        var body = await response.TextAsync();
        var expectedNames = ReadSearchNames(body);

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            if (await Grid.Locator("app-table-skeleton, .table__skeleton-bar").CountAsync() > 0)
            {
                await Task.Delay(50);
                continue;
            }

            if (expectedNames.Count == 0)
            {
                if (await IsEmptySearchMessageVisibleAsync())
                    return;
            }
            else
            {
                var actualNames = (await GetGridRecordsOnCurrentPageAsync())
                    .Select(row => row.Name)
                    .ToList();
                if (actualNames.SequenceEqual(expectedNames))
                    return;
            }

            await Task.Delay(50);
        }

        throw new TimeoutException($"Offices search for '{text}' did not finish.");
    }

    public async Task<bool> IsNameInGridAsync(string name)
    {
        await SearchAsync(name);
        return (await GetAllGridRecordsAsync()).Any(row => row.Name.Equals(name, StringComparison.Ordinal));
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
        return ((await message.First.InnerTextAsync()) ?? string.Empty).Trim();
    }

    private async Task<bool> IsEmptySearchMessageVisibleAsync()
    {
        var message = Grid.Locator("tbody").GetByText(
            EmptySearchMessage,
            new LocatorGetByTextOptions { Exact = true });
        return await message.CountAsync() > 0 && await message.First.IsVisibleAsync();
    }

    private static bool IsOfficesListResponse(IResponse response) =>
        response.Ok
        && Uri.TryCreate(response.Url, UriKind.Absolute, out var uri)
        && uri.AbsolutePath.Equals("/api/admin/offices", StringComparison.OrdinalIgnoreCase);

    private static bool IsOfficesSearchResponse(IResponse response, string text)
    {
        if (!IsOfficesListResponse(response) || !Uri.TryCreate(response.Url, UriKind.Absolute, out var uri))
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

    private static IReadOnlyList<string> ReadSearchNames(string body)
    {
        using var document = JsonDocument.Parse(body);
        if (!TryGetItems(document.RootElement, out var items))
            return [];

        var names = new List<string>();
        foreach (var item in items.EnumerateArray())
            names.Add(ReadJsonString(item, "name") ?? ReadJsonString(item, "Name") ?? string.Empty);

        return names;
    }

    private static bool TryGetItems(JsonElement root, out JsonElement items)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            items = root;
            return true;
        }

        foreach (var name in new[] { "offices", "Offices", "items", "Items" })
        {
            if (root.TryGetProperty(name, out items) && items.ValueKind == JsonValueKind.Array)
                return true;
        }

        items = default;
        return false;
    }

    private static string? ReadJsonString(JsonElement item, string name) =>
        item.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private const string EmptySearchMessage = "No offices match your search.";

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await Grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Replace('\u00A0', ' ').Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header))
            .ToList();
    }

    public async Task ClickGridColumnAsync(string columnName)
    {
        var header = Grid.Locator("thead th").Filter(new LocatorFilterOptions { HasTextString = columnName }).First;
        var ariaBefore = await header.GetAttributeAsync("aria-sort");
        var responseTask = Page.WaitForResponseAsync(
            IsOfficesListResponse,
            new PageWaitForResponseOptions { Timeout = 15_000 });
        await header.Locator("button.table__sort").ClickAsync();
        await responseTask;

        var deadline = DateTime.UtcNow.AddSeconds(15);
        string? previous = null;
        while (DateTime.UtcNow < deadline)
        {
            if (await Grid.Locator("app-table-skeleton, .table__skeleton-bar").CountAsync() > 0)
            {
                previous = null;
                await Task.Delay(50);
                continue;
            }

            var ariaAfter = await header.GetAttributeAsync("aria-sort");
            if (string.Equals(ariaAfter, ariaBefore, StringComparison.Ordinal))
            {
                await Task.Delay(50);
                continue;
            }

            var fingerprint = await CurrentPageFingerprintAsync();
            if (previous is not null && fingerprint == previous)
                return;

            previous = fingerprint;
            await Task.Delay(100);
        }

        throw new TimeoutException($"Column '{columnName}' sort did not change from '{ariaBefore ?? "none"}'.");
    }

    public async Task<IReadOnlyList<OfficeGridRecord>> GetCurrentPageGridRecordsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        return await GetGridRecordsOnCurrentPageAsync();
    }

    public async Task<AddOfficePage> ClickAddOfficeBtnAsync()
    {
        await AddOfficeBtn.ClickAsync();
        await Page.WaitForURLAsync("**/admin/offices/new**");
        var addOfficePage = new AddOfficePage(Page);
        await addOfficePage.WaitForLoadedAsync();
        return addOfficePage;
    }

    public async Task<IReadOnlyList<OfficeGridRecord>> GetAllGridRecordsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        await GoToFirstGridPageAsync();

        var results = new List<OfficeGridRecord>();
        var seenPages = new HashSet<string>(StringComparer.Ordinal);
        while (true)
        {
            var pageRows = await GetGridRecordsOnCurrentPageAsync();
            var fingerprint = string.Join("||", pageRows.Select(row => $"{row.Project}|{row.Name}|{row.StartDate}|{row.EndDate}"));
            if (!seenPages.Add(fingerprint))
                break;

            results.AddRange(pageRows);
            if (!await TryGoToNextGridPageAsync())
                break;
        }

        return results;
    }

    private async Task<IReadOnlyList<OfficeGridRecord>> GetGridRecordsOnCurrentPageAsync()
    {
        var rows = await Grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => !tr.querySelector('app-table-skeleton, .table__skeleton-bar'))
              .map(tr => [...tr.querySelectorAll(':scope > td')].slice(0, 6).map(td => {
                const rendered = (td.innerText || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim();
                const title = (td.getAttribute('title') || td.querySelector('[title]')?.getAttribute('title') || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim();
                const full = (td.textContent || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim();
                return [title, full, rendered].sort((a, b) => b.length - a.length)[0];
              }))
              .filter(cells => cells.length > 1 && cells[1])
            """);

        return rows
            .Select(cells => new OfficeGridRecord(
                Project: Cell(cells, 0),
                Name: Cell(cells, 1),
                Address: Cell(cells, 2),
                StartDate: Cell(cells, 3),
                EndDate: Cell(cells, 4),
                Area: Cell(cells, 5)))
            .ToList();
    }

    private static string Cell(string[] cells, int index) =>
        index < cells.Length ? cells[index] : string.Empty;

    private async Task WaitForGridDataAsync()
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            if (await Grid.Locator("app-table-skeleton, .table__skeleton-bar").CountAsync() == 0)
                return;

            await Task.Delay(50);
        }
    }

    private async Task GoToFirstGridPageAsync()
    {
        while (await IsPagerButtonEnabledAsync(PreviousPageBtn))
        {
            var before = await CurrentPageFingerprintAsync();
            await PreviousPageBtn.ClickAsync();
            await WaitForPageChangeAsync(before);
        }
    }

    private async Task<bool> TryGoToNextGridPageAsync()
    {
        if (!await IsPagerButtonEnabledAsync(NextPageBtn))
            return false;

        var before = await CurrentPageFingerprintAsync();
        await NextPageBtn.ClickAsync();
        return await WaitForPageChangeAsync(before);
    }

    private async Task<string> CurrentPageFingerprintAsync()
    {
        var rows = await GetGridRecordsOnCurrentPageAsync();
        return string.Join("||", rows.Select(row => $"{row.Project}|{row.Name}|{row.StartDate}|{row.EndDate}"));
    }

    private async Task<bool> WaitForPageChangeAsync(string before)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            await WaitForGridDataAsync();
            var current = await CurrentPageFingerprintAsync();
            if (!string.Equals(current, before, StringComparison.Ordinal))
                return true;

            await Task.Delay(50);
        }

        return false;
    }

    private static async Task<bool> IsPagerButtonEnabledAsync(ILocator button)
    {
        if (await button.CountAsync() == 0 || !await button.IsVisibleAsync())
            return false;

        return !await button.IsDisabledAsync();
    }
}

public sealed record OfficeGridRecord(
    string Project,
    string Name,
    string Address,
    string StartDate,
    string EndDate,
    string Area);
