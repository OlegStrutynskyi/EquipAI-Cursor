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
        await GoToFirstGridPageAsync();

        while (true)
        {
            var aliasTexts = await AliasTextCells.AllInnerTextsAsync();
            if (aliasTexts.Any(text => text.Trim().Equals(aliasText, StringComparison.Ordinal)))
            {
                await GoToFirstGridPageAsync();
                return true;
            }

            if (!await TryGoToNextGridPageAsync(aliasTexts.FirstOrDefault()?.Trim() ?? string.Empty))
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

            var aliasTexts = await AliasTextCells.AllInnerTextsAsync();
            if (!await TryGoToNextGridPageAsync(aliasTexts.FirstOrDefault()?.Trim() ?? string.Empty))
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

            var aliasTexts = await AliasTextCells.AllInnerTextsAsync();
            if (!await TryGoToNextGridPageAsync(aliasTexts.FirstOrDefault()?.Trim() ?? string.Empty))
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
        var row = Page.Locator(
            $"//table[contains(@class,'admin-aliases__table')]//tr[td[2][normalize-space()='{aliasText}']]");
        await row.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }

    private async Task<ILocator?> FindAliasRowOnCurrentPageAsync(string aliasText)
    {
        var rows = Grid.Locator("tbody tr");
        var rowCount = await rows.CountAsync();

        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            var row = rows.Nth(rowIndex);
            var currentAliasText = (await row.Locator("td").Nth(1).TextContentAsync())?.Trim() ?? string.Empty;
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

            var aliasTexts = await AliasTextCells.AllInnerTextsAsync();
            if (!await TryGoToNextGridPageAsync(aliasTexts.FirstOrDefault()?.Trim() ?? string.Empty))
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
        await GoToFirstGridPageAsync();

        while (true)
        {
            var row = await FindUnitAliasRowOnCurrentPageAsync(aliasText);
            if (row is not null)
            {
                await GoToFirstGridPageAsync();
                return row;
            }

            var aliasTexts = await AliasTextCells.AllInnerTextsAsync();
            if (!await TryGoToNextGridPageAsync(aliasTexts.FirstOrDefault()?.Trim() ?? string.Empty))
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

            var firstBefore = (await AliasTextCells.First.InnerTextAsync()).Trim();
            await previousBtn.ClickAsync();
            await Assertions.Expect(AliasTextCells.First).Not.ToHaveTextAsync(firstBefore);
        }
    }

    private async Task<bool> TryGoToNextGridPageAsync(string firstAliasTextBefore)
    {
        var nextBtn = Page.Locator("//button[normalize-space()='Next']").First;
        if (await nextBtn.CountAsync() == 0
            || !await nextBtn.IsVisibleAsync()
            || !await IsPagerButtonEnabledAsync(nextBtn))
            return false;

        await nextBtn.ClickAsync();
        await Assertions.Expect(AliasTextCells.First).Not.ToHaveTextAsync(firstAliasTextBefore);
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
