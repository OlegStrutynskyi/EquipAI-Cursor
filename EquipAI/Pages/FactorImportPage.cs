using Microsoft.Playwright;

namespace EquipAI.Pages;

public class FactorImportPage : BasePage
{
    public FactorImportPage(IPage page) : base(page) { }

    private ILocator PageTitle => Page.Locator("//h1[contains(@class,'page-title')]")
        .Filter(new LocatorFilterOptions { HasTextString = "Factor Import" });
    private ILocator Subtitle => Page.Locator("//p[@class='page-header__lead']");
    private ILocator Message1 => Page.Locator("//p[@class='admin-factor-import__note'][1]");
    private ILocator Message2 => Page.Locator("//p[@class='admin-factor-import__note'][2]");
    private ILocator UploadWorkbookLabel => Page.Locator("//h2[@id='admin-factor-import-upload-title']");
    private ILocator SourceLabel => Page.Locator("//label[normalize-space()='Source']");
    private ILocator SourceDropdown => Page.Locator("//app-searchable-select[@controlid='factor-import-source']");
    private ILocator YearLabel => Page.Locator("//label[normalize-space()='Year']");
    private ILocator YearInput => Page.Locator("#factor-import-year");
    private ILocator WorkbookLabel => Page.Locator("#factor-import-file-label");
    private ILocator SelectFileArea => Page.Locator("//div[@class='form-file-picker']");
    private ILocator FileInput => Page.Locator("input[type='file']");
    private ILocator UploadBtn => Page.Locator("//button[normalize-space()='Upload and import']");
    private ILocator AlertBody => Page.Locator("//div[@class='alert__body']");
    private ILocator ImportResultLabel => Page.Locator("#admin-factor-import-result-title");
    private ILocator RecentBatchesLabel => Page.Locator("//h2[@id='admin-factor-import-recent-title']");
    private ILocator Grid => Page.Locator("//table[@class='table']");
    private ILocator AliasesLink => Page.Locator("//a[@routerlink='/admin/aliases']");
    private ILocator SearchableSelectList => Page.Locator("//div[@role='listbox' and contains(@class,'select__list')]");

    private ILocator ResultDd(string label) =>
        Page.Locator($"//dt[normalize-space()='{label}']/following-sibling::dd[1]");

    public async Task OpenAsync()
    {
        var sideMenuPage = new SideMenuPage(Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickFactorImportAsync();
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await PageTitle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<IReadOnlyList<string>> GetSourceOptionsAsync()
    {
        await OpenSearchableSelectAsync(SourceDropdown);
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
            // Dropdown may already be closed.
        }

        return result;
    }

    public async Task SelectSourceAsync(string optionText)
    {
        await OpenSearchableSelectAsync(SourceDropdown);
        var options = SearchableSelectList.Locator("[role='option']");
        var count = await options.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var option = options.Nth(i);
            var text = (await option.TextContentAsync())?.Replace('\u00A0', ' ').Trim() ?? string.Empty;
            if (!text.Equals(optionText, StringComparison.OrdinalIgnoreCase))
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
                // List may close without animation.
            }

            await SourceDropdown.Locator(".select__value")
                .Filter(new LocatorFilterOptions { HasTextString = text })
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
            return;
        }

        throw new InvalidOperationException($"Option '{optionText}' was not found.");
    }

    public async Task SetYearAsync(string year)
    {
        await YearInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await YearInput.FillAsync(year);
        await YearInput.BlurAsync();
    }

    public async Task<AliasesPage> ClickAliasesLinkAsync()
    {
        await AliasesLink.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await AliasesLink.ClickAsync();
        await Page.WaitForURLAsync(
            url => url.Contains("/admin/aliases", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 30_000 });

        var aliasesPage = new AliasesPage(Page);
        await aliasesPage.WaitForLoadedAsync();
        return aliasesPage;
    }

    public async Task<string> GetTitleAsync()
    {
        await PageTitle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await PageTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetSubtitleAsync()
    {
        await Subtitle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await Subtitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessage1Async()
    {
        await Message1.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await Message1.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessage2Async()
    {
        await Message2.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await Message2.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task UploadFileAsync(string fileName)
    {
        var filePath = ResolveTestDataPath(fileName);
        await FileInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
        await FileInput.SetInputFilesAsync(filePath);

        if (fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            await UploadBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await Assertions.Expect(UploadBtn).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions
            {
                Timeout = 30_000,
            });
        }
    }

    public async Task ClickUploadBtnAsync()
    {
        await UploadBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await UploadBtn.ClickAsync(new LocatorClickOptions { Force = true });
    }

    public async Task ClickUploadAndWaitForResultAsync()
    {
        await ClickUploadBtnAsync();
        await ImportResultLabel.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60_000,
        });
    }

    public async Task<string> GetAlertBodyTextAsync()
    {
        await AlertBody.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertBody.InnerTextAsync()).Replace('\u00A0', ' ').Trim();
    }

    public Task<bool> IsImportResultLabelVisibleAsync() => ImportResultLabel.IsVisibleAsync();

    public async Task<string> GetResultDetailAsync(string label)
    {
        var dd = ResultDd(label);
        await dd.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await dd.InnerTextAsync()).Replace('\u00A0', ' ').Trim();
    }

    public Task<string> GetResultOutcomeAsync() => GetResultDetailAsync("Outcome");
    public Task<string> GetResultSourceAsync() => GetResultDetailAsync("Source");
    public Task<string> GetResultYearAsync() => GetResultDetailAsync("Year");
    public Task<string> GetResultLineCountAsync() => GetResultDetailAsync("Line count");
    public Task<string> GetResultLibraryAsync() => GetResultDetailAsync("Library");
    public Task<string> GetResultBatchIdAsync() => GetResultDetailAsync("Batch ID");

    public async Task<IReadOnlyList<FactorImportBatchGridRow>> GetBatchGridRowsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = (await Grid.Locator("thead th").AllInnerTextsAsync())
            .Select(h => h.Replace('\u00A0', ' ').Trim().ToUpperInvariant())
            .ToList();
        var rows = Grid.Locator("tbody tr");
        var count = await rows.CountAsync();
        var result = new List<FactorImportBatchGridRow>();

        for (var i = 0; i < count; i++)
        {
            var cells = rows.Nth(i).Locator("td");
            var cellCount = await cells.CountAsync();
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < cellCount && c < headers.Count; c++)
                values[headers[c]] = (await cells.Nth(c).InnerTextAsync()).Replace('\u00A0', ' ').Trim();

            result.Add(new FactorImportBatchGridRow
            {
                BatchId = GetCell(values, "BATCH ID"),
                Source = GetCell(values, "SOURCE"),
                Year = GetCell(values, "YEAR"),
                Library = GetCell(values, "LIBRARY"),
                Lines = GetCell(values, "LINES"),
                Created = GetCell(values, "CREATED"),
            });
        }

        return result.Where(r => !string.IsNullOrWhiteSpace(r.BatchId)).ToList();
    }

    public async Task<FactorImportBatchGridRow?> GetBatchGridRowByIdAsync(string batchId)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var rowLocator = Grid.Locator($"tbody tr:has(td:nth-child(1):text-is(\"{batchId}\"))");
        await rowLocator.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000,
        });

        var rows = await GetBatchGridRowsAsync();
        return rows.FirstOrDefault(r => r.BatchId.Equals(batchId, StringComparison.OrdinalIgnoreCase));
    }

    public Task<bool> IsUploadWorkbookLabelVisibleAsync() => UploadWorkbookLabel.IsVisibleAsync();
    public Task<bool> IsSourceLabelVisibleAsync() => SourceLabel.IsVisibleAsync();
    public Task<bool> IsSourceDropdownVisibleAsync() => SourceDropdown.IsVisibleAsync();
    public Task<bool> IsYearLabelVisibleAsync() => YearLabel.IsVisibleAsync();
    public Task<bool> IsYearInputVisibleAsync() => YearInput.IsVisibleAsync();
    public Task<bool> IsWorkbookLabelVisibleAsync() => WorkbookLabel.IsVisibleAsync();
    public Task<bool> IsSelectFileAreaVisibleAsync() => SelectFileArea.IsVisibleAsync();
    public Task<bool> IsUploadBtnVisibleAsync() => UploadBtn.IsVisibleAsync();
    public Task<bool> IsUploadBtnEnabledAsync() => UploadBtn.IsEnabledAsync();
    public Task<bool> IsRecentBatchesLabelVisibleAsync() => RecentBatchesLabel.IsVisibleAsync();
    public Task<bool> IsGridVisibleAsync() => Grid.IsVisibleAsync();

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await Grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Replace('\u00A0', ' ').Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header))
            .ToList();
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
                // Dropdown may already be closed.
            }
        }

        await clickTarget.ClickAsync();
        await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    private static string ResolveTestDataPath(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "TestData", fileName),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", fileName)),
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
                return candidate;
        }

        throw new FileNotFoundException($"Test data file was not found: {fileName}");
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

public sealed class FactorImportBatchGridRow
{
    public required string BatchId { get; init; }
    public required string Source { get; init; }
    public required string Year { get; init; }
    public required string Library { get; init; }
    public required string Lines { get; init; }
    public required string Created { get; init; }
}
