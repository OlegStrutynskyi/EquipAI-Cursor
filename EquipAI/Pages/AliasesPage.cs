using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AliasesPage : BasePage
{
    public AliasesPage(IPage page) : base(page) { }

    private ILocator Message => Page.Locator("//p[@class='page-header__lead']");
    private ILocator AddAliasBtn => Page.Locator("//button[normalize-space()='Add alias']");
    private ILocator UnitsOfMeasureTab => Page.Locator("//button[normalize-space()='Units of Measure']");
    private ILocator EmissionTypesTab => Page.Locator("//button[normalize-space()='Emission Types']");
    private ILocator ProjectsTab => Page.Locator("//button[normalize-space()='Projects']");
    private ILocator Grid => Page.Locator("//table[@class='table']").Locator("visible=true");
    private ILocator DeactivateDialog => Page.Locator("//div[@class='modal']");
    private ILocator DeactivateDialogTitle => Page.Locator("//h2[@id='confirm-dialog-title']");
    private ILocator DeactivateDialogMessage => Page.Locator("//p[@id='confirm-dialog-message']");
    private ILocator DeactivateDialogCancelBtn => Page.Locator("//div[@class='modal']//button[normalize-space()='Cancel']");
    private ILocator DeactivateDialogConfirmBtn => Page.Locator("//div[@class='modal']//button[normalize-space()='Deactivate']");

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
            .Filter(new LocatorFilterOptions { HasTextString = "Factor Source" })
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }

    public async Task ClickEmissionTypesTabAsync()
    {
        await EmissionTypesTab.ClickAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Factor Source" })
            .WaitForAsync();
    }

    public async Task ClickProjectsTabAsync()
    {
        await ProjectsTab.ClickAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Resolves to" })
            .WaitForAsync();
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Factor Source" })
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Context" })
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
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
        await GoToFirstGridPageAsync();

        while (true)
        {
            var aliasTexts = await GetAliasTextCellValuesAsync();
            if (aliasTexts.Any(text => text.Equals(aliasText, StringComparison.Ordinal)))
            {
                await GoToFirstGridPageAsync();
                return true;
            }

            if (!await TryGoToNextGridPageAsync(aliasTexts.FirstOrDefault() ?? string.Empty))
                break;
        }

        await GoToFirstGridPageAsync();
        return false;
    }

    public async Task<EditAliasPage> ClickEditBtnAsync(string aliasText)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        while (true)
        {
            var row = await FindAliasRowOnCurrentPageAsync(aliasText);
            if (row is not null)
            {
                await row.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
                var editAliasPage = new EditAliasPage(Page);
                await editAliasPage.WaitForLoadedAsync();
                return editAliasPage;
            }

            var aliasTexts = await GetAliasTextCellValuesAsync();
            if (!await TryGoToNextGridPageAsync(aliasTexts.FirstOrDefault() ?? string.Empty))
                break;
        }

        throw new InvalidOperationException($"Alias '{aliasText}' was not found in the grid.");
    }

    public async Task ClickDeactivateBtnAsync(string aliasText)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        while (true)
        {
            var row = await FindAliasRowOnCurrentPageAsync(aliasText);
            if (row is not null)
            {
                await row.GetByRole(AriaRole.Button, new() { Name = "Deactivate" }).ClickAsync();
                await DeactivateDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
                return;
            }

            var aliasTexts = await GetAliasTextCellValuesAsync();
            if (!await TryGoToNextGridPageAsync(aliasTexts.FirstOrDefault() ?? string.Empty))
                break;
        }

        throw new InvalidOperationException($"Alias '{aliasText}' was not found in the grid.");
    }

    public async Task<string> GetDeactivateDialogTitleAsync()
    {
        await DeactivateDialogTitle.WaitForAsync();
        return (await DeactivateDialogTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetDeactivateDialogMessageAsync()
    {
        await DeactivateDialogMessage.WaitForAsync();
        return (await DeactivateDialogMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsDeactivateDialogVisibleAsync() => DeactivateDialog.IsVisibleAsync();
    public Task<bool> IsDeactivateDialogCancelBtnVisibleAsync() => DeactivateDialogCancelBtn.IsVisibleAsync();
    public Task<bool> IsDeactivateDialogConfirmBtnVisibleAsync() => DeactivateDialogConfirmBtn.IsVisibleAsync();
    public Task<bool> IsDeactivateDialogCancelBtnEnabledAsync() => DeactivateDialogCancelBtn.IsEnabledAsync();
    public Task<bool> IsDeactivateDialogConfirmBtnEnabledAsync() => DeactivateDialogConfirmBtn.IsEnabledAsync();

    public async Task ClickDeactivateDialogCancelBtnAsync()
    {
        await DeactivateDialogCancelBtn.ClickAsync();
        await DeactivateDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
    }

    public async Task ClickDeactivateDialogConfirmBtnAsync(string aliasText)
    {
        await DeactivateDialogConfirmBtn.ClickAsync();
        await DeactivateDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        var aliasTextColumnIndex = await GetAliasTextColumnIndexAsync();
        var row = Grid.Locator($"tbody tr:has(td:nth-child({aliasTextColumnIndex + 1}):text-is(\"{aliasText}\"))");
        await row.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }

    private async Task<ILocator?> FindAliasRowOnCurrentPageAsync(string aliasText)
    {
        var aliasTextColumnIndex = await GetAliasTextColumnIndexAsync();
        var rows = Grid.Locator("tbody tr");
        var rowCount = await rows.CountAsync();

        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            var row = rows.Nth(rowIndex);
            var currentAliasText = (await row.Locator("td").Nth(aliasTextColumnIndex).InnerTextAsync()).Trim();
            if (currentAliasText.Equals(aliasText, StringComparison.Ordinal))
                return row;
        }

        return null;
    }

    public async Task<AliasEmissionTypeGridRow?> GetEmissionTypeAliasGridRowAsync(string aliasText)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        while (true)
        {
            var row = await FindEmissionTypeAliasRowOnCurrentPageAsync(aliasText);
            if (row is not null)
            {
                await GoToFirstGridPageAsync();
                return row;
            }

            var aliasTexts = await GetAliasTextCellValuesAsync();
            if (!await TryGoToNextGridPageAsync(aliasTexts.FirstOrDefault() ?? string.Empty))
                break;
        }

        await GoToFirstGridPageAsync();
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
            var currentAliasText = (await cells.Nth(1).InnerTextAsync()).Trim();
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

    public async Task<AliasUnitGridRow?> GetProjectAliasGridRowAsync(string aliasText) =>
        await GetUnitAliasGridRowAsync(aliasText);

    public async Task<AliasUnitGridRow?> GetUnitAliasGridRowAsync(string aliasText)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await GoToFirstGridPageAsync();

        while (true)
        {
            var row = await FindUnitAliasRowOnCurrentPageAsync(aliasText);
            if (row is not null)
            {
                await GoToFirstGridPageAsync();
                return row;
            }

            var aliasTexts = await GetAliasTextCellValuesAsync();
            if (!await TryGoToNextGridPageAsync(aliasTexts.FirstOrDefault() ?? string.Empty))
                break;
        }

        await GoToFirstGridPageAsync();
        return null;
    }

    private async Task GoToFirstGridPageAsync()
    {
        var previousBtn = Page.Locator("//button[normalize-space()='Previous']").First;
        for (var i = 0; i < 50; i++)
        {
            if (await previousBtn.CountAsync() == 0 || !await previousBtn.IsVisibleAsync())
                return;
            if (!await IsPagerButtonEnabledAsync(previousBtn))
                return;

            var aliasTextCells = await GetAliasTextCellsLocatorAsync();
            if (await aliasTextCells.CountAsync() == 0)
                return;

            var firstBefore = (await aliasTextCells.First.InnerTextAsync()).Trim();
            await previousBtn.ClickAsync();
            await Assertions.Expect(aliasTextCells.First).Not.ToHaveTextAsync(firstBefore);
        }
    }

    private async Task<bool> TryGoToNextGridPageAsync(string firstAliasTextBefore)
    {
        var nextBtn = Page.Locator("//button[normalize-space()='Next']").First;
        if (await nextBtn.CountAsync() == 0
            || !await nextBtn.IsVisibleAsync()
            || !await IsPagerButtonEnabledAsync(nextBtn))
            return false;

        var aliasTextCells = await GetAliasTextCellsLocatorAsync();
        await nextBtn.ClickAsync();
        await Assertions.Expect(aliasTextCells.First).Not.ToHaveTextAsync(firstAliasTextBefore);
        return true;
    }

    private static async Task<bool> IsPagerButtonEnabledAsync(ILocator button)
    {
        return await button.EvaluateAsync<bool>(
            """
            el => !(
              el.disabled
              || el.hasAttribute('disabled')
              || el.getAttribute('aria-disabled') === 'true'
              || el.classList.contains('disabled')
            )
            """);
    }

    private async Task<AliasUnitGridRow?> FindUnitAliasRowOnCurrentPageAsync(string aliasText)
    {
        var rows = Grid.Locator("tbody tr");
        var rowCount = await rows.CountAsync();

        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            var row = rows.Nth(rowIndex);
            var cells = row.Locator("td");
            var currentAliasText = (await cells.Nth(0).InnerTextAsync()).Trim();
            if (!currentAliasText.Equals(aliasText, StringComparison.Ordinal))
                continue;

            return new AliasUnitGridRow
            {
                Context = string.Empty,
                AliasText = currentAliasText,
                ResolvesTo = (await cells.Nth(1).InnerTextAsync()).Trim(),
            };
        }

        return null;
    }

    private async Task<int> GetAliasTextColumnIndexAsync()
    {
        var headers = await GetGridColumnHeadersAsync();
        for (var i = 0; i < headers.Count; i++)
        {
            if (headers[i].Equals("ALIAS TEXT", StringComparison.OrdinalIgnoreCase)
                || headers[i].Equals("Alias Text", StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return headers.Any(header => header.Contains("CONTEXT", StringComparison.OrdinalIgnoreCase)) ? 1 : 0;
    }

    private async Task<ILocator> GetAliasTextCellsLocatorAsync()
    {
        var aliasTextColumnIndex = await GetAliasTextColumnIndexAsync();
        return Grid.Locator($"tbody tr td:nth-child({aliasTextColumnIndex + 1})");
    }

    private async Task<IReadOnlyList<string>> GetAliasTextCellValuesAsync()
    {
        var cells = await GetAliasTextCellsLocatorAsync();
        var values = await cells.AllInnerTextsAsync();
        return values.Select(value => value.Trim()).ToList();
    }

    private static string NormalizeContext(string context)
    {
        if (context.Contains("Catalog Factor Mapping", StringComparison.OrdinalIgnoreCase))
            return "Catalog Factor Mapping";

        if (context.Contains("Data Ingestion", StringComparison.OrdinalIgnoreCase))
            return "Data Ingestion";

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
