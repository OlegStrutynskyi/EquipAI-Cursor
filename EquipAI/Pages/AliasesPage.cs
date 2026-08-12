using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AliasesPage : BasePage
{
    public AliasesPage(IPage page) : base(page) { }

    private ILocator Message => Page.Locator("//p[@class='admin-aliases__lead']");
    private ILocator AddAliasBtn => Page.Locator("//button[normalize-space()='Add alias']");
    private ILocator UnitsOfMeasureTab => Page.Locator("//button[normalize-space()='Units of measure']");
    private ILocator EmissionTypesTab => Page.Locator("//button[normalize-space()='Emission types']");
    private ILocator Grid => Page.Locator("table.table.admin-aliases__table").Locator("visible=true");
    private ILocator AliasTextCells => Grid.Locator("tbody tr td:nth-child(2)");

    public async Task OpenAsync()
    {
        var sideMenuPage = new SideMenuPage(Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickAliasesAsync();
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await Message.WaitForAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetMessageAsync()
    {
        await Message.WaitForAsync();
        return (await Message.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsAddAliasBtnVisibleAsync() => AddAliasBtn.IsVisibleAsync();
    public Task<bool> IsAddAliasBtnEnabledAsync() => AddAliasBtn.IsEnabledAsync();
    public Task<bool> IsGridVisibleAsync() => Grid.IsVisibleAsync();

    public async Task ClickUnitsOfMeasureTabAsync()
    {
        await UnitsOfMeasureTab.ClickAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Resolves to" })
            .WaitForAsync();
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Factor source" })
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }

    public async Task ClickEmissionTypesTabAsync()
    {
        await EmissionTypesTab.ClickAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Factor source" })
            .WaitForAsync();
    }

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await Grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header))
             //                && !header.Equals("ACTIONS", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<AddAliasPage> ClickAddAliasBtnAsync()
    {
        await AddAliasBtn.ClickAsync();
        var addAliasPage = new AddAliasPage(Page);
        await addAliasPage.WaitForLoadedAsync();
        return addAliasPage;
    }

    public async Task<bool> IsAliasTextInGridAsync(string aliasText)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var nextBtn = Page.Locator("//button[normalize-space()='Next']");
        var previousBtn = Page.Locator("//button[normalize-space()='Previous']");
        var visitedNextPage = false;

        while (true)
        {
            var aliasTexts = await AliasTextCells.AllInnerTextsAsync();
            if (aliasTexts.Any(text => text.Trim().Equals(aliasText, StringComparison.Ordinal)))
                return true;

            if (await nextBtn.CountAsync() == 0
                || !await nextBtn.First.IsVisibleAsync()
                || !await nextBtn.First.IsEnabledAsync()
                || await nextBtn.First.GetAttributeAsync("disabled") is not null)
                break;

            var firstBefore = aliasTexts.FirstOrDefault()?.Trim() ?? string.Empty;
            visitedNextPage = true;
            await nextBtn.First.ClickAsync();
            await Assertions.Expect(AliasTextCells.First).Not.ToHaveTextAsync(firstBefore);
        }

        if (visitedNextPage)
        {
            while (await previousBtn.CountAsync() > 0
                   && await previousBtn.First.IsVisibleAsync()
                   && await previousBtn.First.IsEnabledAsync()
                   && await previousBtn.First.GetAttributeAsync("disabled") is null)
            {
                await previousBtn.First.ClickAsync();
                await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            }
        }

        return false;
    }

    public async Task<AliasEmissionTypeGridRow?> GetEmissionTypeAliasGridRowAsync(string aliasText)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var nextBtn = Page.Locator("//button[normalize-space()='Next']");
        var previousBtn = Page.Locator("//button[normalize-space()='Previous']");
        var visitedNextPage = false;

        while (true)
        {
            var row = await FindEmissionTypeAliasRowOnCurrentPageAsync(aliasText);
            if (row is not null)
                return row;

            var aliasTexts = await AliasTextCells.AllInnerTextsAsync();
            if (await nextBtn.CountAsync() == 0
                || !await nextBtn.First.IsVisibleAsync()
                || !await nextBtn.First.IsEnabledAsync()
                || await nextBtn.First.GetAttributeAsync("disabled") is not null)
                break;

            var firstBefore = aliasTexts.FirstOrDefault()?.Trim() ?? string.Empty;
            visitedNextPage = true;
            await nextBtn.First.ClickAsync();
            await Assertions.Expect(AliasTextCells.First).Not.ToHaveTextAsync(firstBefore);
        }

        if (visitedNextPage)
        {
            while (await previousBtn.CountAsync() > 0
                   && await previousBtn.First.IsVisibleAsync()
                   && await previousBtn.First.IsEnabledAsync()
                   && await previousBtn.First.GetAttributeAsync("disabled") is null)
            {
                await previousBtn.First.ClickAsync();
                await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            }
        }

        return null;
    }

    private async Task<AliasEmissionTypeGridRow?> FindEmissionTypeAliasRowOnCurrentPageAsync(string aliasText)
    {
        var rows = Grid.Locator("tbody tr");
        var rowCount = await rows.CountAsync();

        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            var row = rows.Nth(rowIndex);
            var cells = row.Locator("td");
            var currentAliasText = (await cells.Nth(1).TextContentAsync())?.Trim() ?? string.Empty;
            if (!currentAliasText.Equals(aliasText, StringComparison.Ordinal))
                continue;

            return new AliasEmissionTypeGridRow
            {
                Context = NormalizeContext((await cells.Nth(0).InnerTextAsync()).Trim()),
                AliasText = currentAliasText,
                FactorSource = (await cells.Nth(2).InnerTextAsync()).Trim(),
                ResolvesTo = (await cells.Nth(3).InnerTextAsync()).Trim(),
            };
        }

        return null;
    }

    public async Task<AliasUnitGridRow?> GetUnitAliasGridRowAsync(string aliasText)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var nextBtn = Page.Locator("//button[normalize-space()='Next']");
        var previousBtn = Page.Locator("//button[normalize-space()='Previous']");
        var visitedNextPage = false;

        while (true)
        {
            var row = await FindUnitAliasRowOnCurrentPageAsync(aliasText);
            if (row is not null)
                return row;

            var aliasTexts = await AliasTextCells.AllInnerTextsAsync();
            if (await nextBtn.CountAsync() == 0
                || !await nextBtn.First.IsVisibleAsync()
                || !await nextBtn.First.IsEnabledAsync()
                || await nextBtn.First.GetAttributeAsync("disabled") is not null)
                break;

            var firstBefore = aliasTexts.FirstOrDefault()?.Trim() ?? string.Empty;
            visitedNextPage = true;
            await nextBtn.First.ClickAsync();
            await Assertions.Expect(AliasTextCells.First).Not.ToHaveTextAsync(firstBefore);
        }

        if (visitedNextPage)
        {
            while (await previousBtn.CountAsync() > 0
                   && await previousBtn.First.IsVisibleAsync()
                   && await previousBtn.First.IsEnabledAsync()
                   && await previousBtn.First.GetAttributeAsync("disabled") is null)
            {
                await previousBtn.First.ClickAsync();
                await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            }
        }

        return null;
    }

    private async Task<AliasUnitGridRow?> FindUnitAliasRowOnCurrentPageAsync(string aliasText)
    {
        var rows = Grid.Locator("tbody tr");
        var rowCount = await rows.CountAsync();

        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            var row = rows.Nth(rowIndex);
            var cells = row.Locator("td");
            var currentAliasText = (await cells.Nth(1).TextContentAsync())?.Trim() ?? string.Empty;
            if (!currentAliasText.Equals(aliasText, StringComparison.Ordinal))
                continue;

            return new AliasUnitGridRow
            {
                Context = NormalizeContext((await cells.Nth(0).InnerTextAsync()).Trim()),
                AliasText = currentAliasText,
                ResolvesTo = (await cells.Nth(2).InnerTextAsync()).Trim(),
            };
        }

        return null;
    }

    private static string NormalizeContext(string context)
    {
        const string catalogFactorMapping = "Catalog factor mapping";
        if (context.Contains(catalogFactorMapping, StringComparison.Ordinal))
            return catalogFactorMapping;

        const string dataIngestion = "Data ingestion";
        if (context.Contains(dataIngestion, StringComparison.Ordinal))
            return dataIngestion;

        return context;
    }
}

public sealed class AliasEmissionTypeGridRow
{
    public required string Context { get; init; }
    public required string AliasText { get; init; }
    public required string FactorSource { get; init; }
    public required string ResolvesTo { get; init; }
}

public sealed class AliasUnitGridRow
{
    public required string Context { get; init; }
    public required string AliasText { get; init; }
    public required string ResolvesTo { get; init; }
}
