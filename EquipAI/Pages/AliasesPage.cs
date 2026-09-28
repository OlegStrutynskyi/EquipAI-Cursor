using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AliasesPage : BasePage
{
    public AliasesPage(IPage page) : base(page) { }

    private Func<Task>? _refreshActiveTabAsync;

    private ILocator Message => Page.Locator("//p[@class='page-header__lead']");
    private ILocator AddAliasBtn => Page.Locator("//button[normalize-space()='Add alias']");
    private ILocator UnitsOfMeasureTab => Page.Locator("//button[normalize-space()='Units of Measure']");
    private ILocator EmissionTypesTab => Page.Locator("//button[normalize-space()='Emission Types']");
    private ILocator ProjectsTab => Page.Locator("//button[normalize-space()='Projects']");
    private ILocator Grid => Page.Locator("table.table").Locator("visible=true").First;
    private ILocator PreviousBtn => Page.Locator("//button[normalize-space()='Previous']").Last;
    private ILocator NextBtn => Page.Locator("//button[normalize-space()='Next']").Last;
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
        await WaitForGridDataAsync();
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
        _refreshActiveTabAsync = ClickUnitsOfMeasureTabAsync;
        await UnitsOfMeasureTab.ClickAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Resolves to" })
            .WaitForAsync();
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Factor Source" })
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
        await WaitForGridDataAsync();
        // Tab switches reset to page 1; don't treat prior tab's Showing range as settled.
        await WaitForPaginationReadyAsync(expectFirstPage: true);
    }

    public async Task ClickEmissionTypesTabAsync()
    {
        _refreshActiveTabAsync = ClickEmissionTypesTabAsync;
        await EmissionTypesTab.ClickAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Grid.Locator("thead th")
            .Filter(new LocatorFilterOptions { HasTextString = "Factor Source" })
            .WaitForAsync();
        await WaitForGridDataAsync();
        await WaitForPaginationReadyAsync(expectFirstPage: true);
    }

    public async Task ClickProjectsTabAsync()
    {
        _refreshActiveTabAsync = ClickProjectsTabAsync;
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
        await WaitForGridDataAsync();
        await WaitForPaginationReadyAsync(expectFirstPage: true);
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
        var row = await FindAliasRowAcrossPagesAsync(aliasText, context: null);
        return row is not null;
    }

    public async Task<EditAliasPage> ClickEditBtnAsync(string aliasText)
    {
        return await ClickEditBtnAsync(context: null, aliasText);
    }

    public async Task<EditAliasPage> ClickEditBtnAsync(string? context, string aliasText)
    {
        var deadline = DateTime.UtcNow.AddSeconds(60);
        Exception? lastError = null;

        while (DateTime.UtcNow < deadline)
        {
            var row = await FindAliasRowAcrossPagesOnceAsync(aliasText, context);
            if (row is not null)
            {
                try
                {
                    var editBtn = row.GetByRole(AriaRole.Button, new() { Name = "Edit" });
                    await editBtn.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 5_000,
                    });
                    await editBtn.ScrollIntoViewIfNeededAsync();
                    await editBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
                    var editAliasPage = new EditAliasPage(Page);
                    await editAliasPage.WaitForLoadedAsync();
                    return editAliasPage;
                }
                catch (Exception ex) when (ex is TimeoutException or PlaywrightException)
                {
                    lastError = ex;
                }
            }

            await Task.Delay(500);
            await RefreshActiveAliasesTabAsync();
        }

        var contextSuffix = string.IsNullOrWhiteSpace(context) ? string.Empty : $" with context '{context}'";
        throw new InvalidOperationException(
            $"Alias '{aliasText}'{contextSuffix} was not found in the grid.",
            lastError);
    }

    public async Task ClickDeactivateBtnAsync(string aliasText)
    {
        var row = await FindAliasRowAcrossPagesAsync(aliasText, context: null)
            ?? throw new InvalidOperationException($"Alias '{aliasText}' was not found in the grid.");

        await row.GetByRole(AriaRole.Button, new() { Name = "Deactivate" }).ClickAsync();
        await DeactivateDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
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

    private async Task<ILocator?> FindAliasRowOnCurrentPageAsync(string aliasText, string? context)
    {
        var aliasTextColumnIndex = await GetAliasTextColumnIndexAsync();
        var hasContext = (await GetGridColumnHeadersAsync())
            .Any(header => header.Contains("CONTEXT", StringComparison.OrdinalIgnoreCase));
        var matchContext = string.IsNullOrWhiteSpace(context) ? null : context;

        var matchIndex = await Grid.EvaluateAsync<int?>(
            """
            (table, args) => {
              const target = (args.aliasText || '').trim().toLowerCase();
              const aliasTextColumnIndex = args.aliasTextColumnIndex;
              const matchContext = args.matchContext;
              const hasContext = args.hasContext;
              const rows = [...table.querySelectorAll('tbody tr')];
              for (let i = 0; i < rows.length; i++) {
                if (rows[i].querySelector('.table__skeleton-bar')) continue;
                const tds = [...rows[i].querySelectorAll('td')];
                if (tds.length <= aliasTextColumnIndex) continue;
                const cell = tds[aliasTextColumnIndex];
                const text = (cell.innerText || '').replace(/\u00a0/g, ' ').trim();
                const title = (cell.getAttribute('title') || '').trim();
                if (!text && !title) continue;
                // Exact match only — substring would treat "...PROJECT 1 UPDATED" as "...PROJECT 1".
                if (text.toLowerCase() !== target && title.toLowerCase() !== target) continue;
                if (matchContext && hasContext) {
                  const currentContext = (tds[0].innerText || '').replace(/\u00a0/g, ' ').trim();
                  const normalized = currentContext.replace(/\s+/g, ' ').trim();
                  const compact = normalized.replace(/[\s-]/g, '');
                  let ctx = normalized;
                  if (compact.toLowerCase().includes('catalogfactormapping')
                      || normalized.toLowerCase().includes('catalog factor mapping'))
                    ctx = 'Catalog Factor Mapping';
                  else if (compact.toLowerCase().includes('dataingestion')
                      || normalized.toLowerCase().includes('data ingestion'))
                    ctx = 'Data Ingestion';
                  if (ctx.toLowerCase() !== matchContext.toLowerCase()) continue;
                }
                const hasEdit = [...rows[i].querySelectorAll('button')]
                  .some(b => ((b.innerText || b.getAttribute('aria-label') || '')).trim().toLowerCase() === 'edit');
                if (!hasEdit) continue;
                return i;
              }
              return null;
            }
            """,
            new
            {
                aliasText,
                aliasTextColumnIndex,
                matchContext,
                hasContext,
            });

        return matchIndex is null ? null : Grid.Locator("tbody tr").Nth(matchIndex.Value);
    }

    public async Task<AliasEmissionTypeGridRow?> GetEmissionTypeAliasGridRowAsync(string aliasText) =>
        await GetEmissionTypeAliasGridRowAsync(aliasText, context: null);

    public async Task<AliasEmissionTypeGridRow?> GetEmissionTypeAliasGridRowAsync(string aliasText, string? context)
    {
        var deadline = DateTime.UtcNow.AddSeconds(45);
        while (true)
        {
            await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await WaitForGridDataAsync();
            await WaitForPaginationReadyAsync();
            await GoToFirstGridPageAsync();

            while (true)
            {
                var row = await FindEmissionTypeAliasRowOnCurrentPageAsync(aliasText, context);
                if (row is not null)
                {
                    await GoToFirstGridPageAsync();
                    return row;
                }

                if (!await TryGoToNextGridPageAsync())
                    break;
            }

            if (DateTime.UtcNow >= deadline)
                break;

            await Task.Delay(500);
            await ClickEmissionTypesTabAsync();
        }

        await GoToFirstGridPageAsync();
        return null;
    }

    private async Task<AliasEmissionTypeGridRow?> FindEmissionTypeAliasRowOnCurrentPageAsync(string aliasText) =>
        await FindEmissionTypeAliasRowOnCurrentPageAsync(aliasText, context: null);

    private async Task<AliasEmissionTypeGridRow?> FindEmissionTypeAliasRowOnCurrentPageAsync(string aliasText, string? context)
    {
        var raw = await Grid.EvaluateAsync<string[]?>(
            """
            (table, args) => {
              const target = (args.aliasText || '').trim().toLowerCase();
              const matchContext = (args.context || '').trim().toLowerCase();
              const rows = [...table.querySelectorAll('tbody tr')];
              for (const tr of rows) {
                if (tr.querySelector('.table__skeleton-bar')) continue;
                const tds = [...tr.querySelectorAll('td')];
                const cells = tds.map(td => (td.innerText || '').replace(/\u00a0/g, ' ').trim());
                if (cells.length < 4 || !cells[1]) continue;
                const title = (tds[1]?.getAttribute('title') || '').trim();
                if (cells[1].toLowerCase() !== target && title.toLowerCase() !== target) continue;

                let ctx = (cells[0] || '').replace(/\s+/g, ' ').trim();
                const compact = ctx.replace(/[\s-]/g, '').toLowerCase();
                if (compact.includes('catalogfactormapping') || ctx.toLowerCase().includes('catalog factor mapping'))
                  ctx = 'Catalog Factor Mapping';
                else if (compact.includes('dataingestion') || ctx.toLowerCase().includes('data ingestion'))
                  ctx = 'Data Ingestion';

                if (matchContext && ctx.toLowerCase() !== matchContext) continue;
                return [ctx, cells[1] || title, cells[2], cells[3]];
              }
              return null;
            }
            """,
            new { aliasText, context });

        if (raw is null)
            return null;

        return new AliasEmissionTypeGridRow
        {
            Context = NormalizeContext(raw[0]),
            AliasText = raw[1],
            FactorSource = raw[2],
            ResolvesTo = raw[3],
        };
    }

    public async Task<AliasUnitGridRow?> GetProjectAliasGridRowAsync(string aliasText) =>
        await GetUnitAliasGridRowAsync(aliasText, refreshTab: ClickProjectsTabAsync);

    public async Task<AliasUnitGridRow?> GetUnitAliasGridRowAsync(string aliasText) =>
        await GetUnitAliasGridRowAsync(aliasText, refreshTab: ClickUnitsOfMeasureTabAsync);

    private async Task<AliasUnitGridRow?> GetUnitAliasGridRowAsync(string aliasText, Func<Task>? refreshTab)
    {
        var deadline = DateTime.UtcNow.AddSeconds(45);
        while (true)
        {
            await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await WaitForGridDataAsync();
            await WaitForPaginationReadyAsync();
            await GoToFirstGridPageAsync();

            while (true)
            {
                var row = await FindUnitAliasRowOnCurrentPageAsync(aliasText);
                if (row is not null)
                {
                    await GoToFirstGridPageAsync();
                    return row;
                }

                if (!await TryGoToNextGridPageAsync())
                    break;
            }

            if (DateTime.UtcNow >= deadline)
                break;

            await Task.Delay(500);
            if (refreshTab is not null)
                await refreshTab();
        }

        await GoToFirstGridPageAsync();
        return null;
    }

    private async Task<ILocator?> FindAliasRowAcrossPagesAsync(string aliasText, string? context)
    {
        var deadline = DateTime.UtcNow.AddSeconds(60);
        while (true)
        {
            var row = await FindAliasRowAcrossPagesOnceAsync(aliasText, context);
            if (row is not null)
                return row;

            if (DateTime.UtcNow >= deadline)
                break;

            await Task.Delay(500);
            // Re-select the active tab so a stale/partial grid load cannot pin the search
            // on the wrong page of results (same pattern as GetProjectAliasGridRowAsync).
            await RefreshActiveAliasesTabAsync();
        }

        return null;
    }

    private async Task<ILocator?> FindAliasRowAcrossPagesOnceAsync(string aliasText, string? context)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        await WaitForPaginationReadyAsync();
        await GoToFirstGridPageAsync();

        while (true)
        {
            var row = await FindAliasRowOnCurrentPageAsync(aliasText, context);
            if (row is not null)
                return row;

            if (!await TryGoToNextGridPageAsync())
                return null;
        }
    }

    private async Task RefreshActiveAliasesTabAsync()
    {
        if (_refreshActiveTabAsync is not null)
        {
            await _refreshActiveTabAsync();
            return;
        }

        if (await IsTabSelectedAsync(ProjectsTab))
        {
            await ClickProjectsTabAsync();
            return;
        }

        if (await IsTabSelectedAsync(EmissionTypesTab))
        {
            await ClickEmissionTypesTabAsync();
            return;
        }

        if (await IsTabSelectedAsync(UnitsOfMeasureTab))
        {
            await ClickUnitsOfMeasureTabAsync();
            return;
        }

        await WaitForGridDataAsync();
        await WaitForPaginationReadyAsync();
    }

    private static async Task<bool> IsTabSelectedAsync(ILocator tab)
    {
        if (await tab.CountAsync() == 0 || !await tab.IsVisibleAsync())
            return false;

        return await tab.EvaluateAsync<bool>(
            """
            el => el.getAttribute('aria-selected') === 'true'
              || el.getAttribute('aria-current') === 'page'
              || el.classList.contains('active')
              || el.classList.contains('is-active')
              || el.classList.contains('selected')
              || el.getAttribute('data-state') === 'active'
            """);
    }

    private async Task WaitForGridDataAsync()
    {
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
            // Fall through and try to wait for real cell text.
        }

        try
        {
            var skeleton = Grid.Locator(".table__skeleton-bar").First;
            if (await skeleton.CountAsync() > 0)
            {
                await skeleton.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Detached,
                    Timeout = 30_000,
                });
            }
        }
        catch (TimeoutException)
        {
            // Fall through.
        }

        try
        {
            await Page.WaitForFunctionAsync(
                """
                () => {
                  const table = [...document.querySelectorAll('table.table')]
                    .find(t => t.offsetParent !== null);
                  if (!table) return false;
                  if (table.closest('.table-wrapper--loading')) return false;
                  if (table.querySelector('.table__skeleton-bar')) return false;
                  return [...table.querySelectorAll('tbody tr')]
                    .some(tr => [...tr.querySelectorAll('td')]
                      .some(td => {
                        const text = (td.innerText || '').trim();
                        return text.length > 0 && !text.includes('Edit');
                      }));
                }
                """,
                null,
                new PageWaitForFunctionOptions { Timeout = 30_000 });
        }
        catch (TimeoutException)
        {
            // Grid may legitimately be empty.
        }
    }

    private async Task WaitForPaginationReadyAsync(bool expectFirstPage = false)
    {
        (int Start, int End, int Total)? previous = null;
        for (var attempt = 0; attempt < 40; attempt++)
        {
            var showing = await TryGetShowingRangeAsync();
            if (showing is not null && showing.Value.Total >= 0)
            {
                if (expectFirstPage && showing.Value.Start != 1 && showing.Value.Total > 0)
                {
                    previous = showing;
                    await Task.Delay(150);
                    continue;
                }

                if (previous is not null
                    && previous.Value.Start == showing.Value.Start
                    && previous.Value.End == showing.Value.End
                    && previous.Value.Total == showing.Value.Total)
                {
                    return;
                }

                previous = showing;
            }
            else if (!expectFirstPage && await Grid.Locator("tbody tr td").CountAsync() > 0)
            {
                // No Showing summary — treat as ready once rows exist.
                return;
            }

            await Task.Delay(150);
        }
    }

    private async Task GoToFirstGridPageAsync()
    {
        await WaitForPaginationReadyAsync();
        for (var guard = 0; guard < 100; guard++)
        {
            if (!await CanGoToAdjacentPageAsync(PreviousBtn))
                return;

            var showingBefore = await TryGetShowingRangeAsync();
            var fingerprintBefore = await GetPageFingerprintAsync();
            try
            {
                await PreviousBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000, Force = true });
            }
            catch (TimeoutException)
            {
                return;
            }

            if (!await WaitForPageAdvanceAsync(showingBefore, fingerprintBefore, expectIncrease: false))
                return;
        }
    }

    private async Task<bool> TryGoToNextGridPageAsync()
    {
        await WaitForPaginationReadyAsync();

        var showingBefore = await TryGetShowingRangeAsync();
        var canByShowing = showingBefore is not null && showingBefore.Value.End < showingBefore.Value.Total;
        var canByButton = await IsPagerButtonEnabledAsync(NextBtn);
        if (!canByShowing && !canByButton)
            return false;

        var fingerprintBefore = await GetPageFingerprintAsync();
        try
        {
            await NextBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000, Force = true });
        }
        catch (TimeoutException)
        {
            return false;
        }

        return await WaitForPageAdvanceAsync(showingBefore, fingerprintBefore, expectIncrease: true);
    }

    private async Task<bool> WaitForPageAdvanceAsync(
        (int Start, int End, int Total)? showingBefore,
        string fingerprintBefore,
        bool expectIncrease)
    {
        var deadline = DateTime.UtcNow.AddSeconds(12);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var skeleton = Grid.Locator(".table__skeleton-bar").First;
                if (await skeleton.CountAsync() > 0)
                {
                    await skeleton.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Detached,
                        Timeout = 5_000,
                    });
                }
            }
            catch (TimeoutException)
            {
            }

            var showingAfter = await TryGetShowingRangeAsync();
            if (showingBefore is not null && showingAfter is not null)
            {
                if (expectIncrease && showingAfter.Value.Start > showingBefore.Value.Start)
                    return true;
                if (!expectIncrease && showingAfter.Value.Start < showingBefore.Value.Start)
                    return true;
                if (expectIncrease
                    && showingAfter.Value.Start == showingBefore.Value.Start
                    && showingAfter.Value.End >= showingAfter.Value.Total
                    && !await IsPagerButtonEnabledAsync(NextBtn))
                {
                    return false;
                }
            }

            var fingerprintAfter = await GetPageFingerprintAsync();
            if (!string.IsNullOrEmpty(fingerprintBefore)
                && !fingerprintBefore.Equals(fingerprintAfter, StringComparison.Ordinal)
                && !string.IsNullOrEmpty(fingerprintAfter))
            {
                return true;
            }

            await Task.Delay(200);
        }

        var finalShowing = await TryGetShowingRangeAsync();
        if (showingBefore is not null && finalShowing is not null)
        {
            return expectIncrease
                ? finalShowing.Value.Start > showingBefore.Value.Start
                : finalShowing.Value.Start < showingBefore.Value.Start;
        }

        var finalFingerprint = await GetPageFingerprintAsync();
        return !string.IsNullOrEmpty(fingerprintBefore)
               && !fingerprintBefore.Equals(finalFingerprint, StringComparison.Ordinal);
    }

    private async Task<string> GetPageFingerprintAsync()
    {
        return await Grid.EvaluateAsync<string>(
            """
            table => {
              const rows = [...table.querySelectorAll('tbody tr')]
                .filter(tr => !tr.querySelector('.table__skeleton-bar'))
                .slice(0, 3);
              return rows.map(tr => [...tr.querySelectorAll('td')]
                .slice(0, 3)
                .map(td => (td.innerText || '').replace(/\u00a0/g, ' ').trim())
                .join('|')).join('||');
            }
            """) ?? string.Empty;
    }

    private async Task<bool> CanGoToAdjacentPageAsync(ILocator button)
    {
        if (await button.CountAsync() == 0 || !await button.IsVisibleAsync())
            return false;

        var isNext = (await button.InnerTextAsync()).Contains("Next", StringComparison.OrdinalIgnoreCase);
        var showing = await TryGetShowingRangeAsync();
        if (showing is not null)
        {
            var byShowing = isNext
                ? showing.Value.End < showing.Value.Total
                : showing.Value.Start > 1;
            if (byShowing)
                return true;

            // Showing can lag behind the enabled state of the button right after loads.
            return await IsPagerButtonEnabledAsync(button);
        }

        return await IsPagerButtonEnabledAsync(button);
    }

    private async Task<(int Start, int End, int Total)?> TryGetShowingRangeAsync()
    {
        var candidates = new[]
        {
            Page.Locator("p.pagination__info, .pagination__info, [class*='pagination']").First,
            Page.Locator("//*[contains(normalize-space(.),'Showing') and contains(normalize-space(.),'of')]").First,
        };

        foreach (var showing in candidates)
        {
            try
            {
                if (await showing.CountAsync() == 0 || !await showing.IsVisibleAsync())
                    continue;

                var text = (await showing.InnerTextAsync()).Replace('\u00A0', ' ').Replace('–', '-').Replace('—', '-');
                var match = System.Text.RegularExpressions.Regex.Match(
                    text,
                    @"Showing\s+(\d+)\s*-\s*(\d+)\s+of\s+(\d+)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!match.Success)
                    continue;

                return (
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value));
            }
            catch (PlaywrightException)
            {
                // Try next candidate.
            }
        }

        return null;
    }

    private static async Task<bool> IsPagerButtonEnabledAsync(ILocator button)
    {
        if (await button.CountAsync() == 0 || !await button.IsVisibleAsync())
            return false;

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
        var raw = await Grid.EvaluateAsync<string[]?>(
            """
            (table, aliasText) => {
              const target = (aliasText || '').trim().toLowerCase();
              const rows = [...table.querySelectorAll('tbody tr')];
              for (const tr of rows) {
                if (tr.querySelector('.table__skeleton-bar')) continue;
                const tds = [...tr.querySelectorAll('td')];
                const cells = tds.map(td => (td.innerText || '').replace(/\u00a0/g, ' ').trim());
                if (cells.length < 2 || !cells[0]) continue;
                const title = (tds[0]?.getAttribute('title') || '').trim();
                if (cells[0].toLowerCase() !== target && title.toLowerCase() !== target) continue;
                return [cells[0] || title, cells[1] || ''];
              }
              return null;
            }
            """,
            aliasText);

        if (raw is null)
            return null;

        return new AliasUnitGridRow
        {
            Context = string.Empty,
            AliasText = raw[0],
            ResolvesTo = raw[1],
        };
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

    private async Task<IReadOnlyList<string>> GetAliasTextCellValuesAsync()
    {
        var aliasTextColumnIndex = await GetAliasTextColumnIndexAsync();
        var values = await Grid.EvaluateAsync<string[]>(
            """
            (table, aliasTextColumnIndex) => [...table.querySelectorAll('tbody tr')]
              .map(tr => {
                const cells = tr.querySelectorAll('td');
                const cell = cells[aliasTextColumnIndex];
                return cell ? (cell.innerText || '').replace(/\u00a0/g, ' ').trim() : '';
              })
              .filter(text => !!text)
            """,
            aliasTextColumnIndex);
        return values;
    }

    private static string NormalizeContext(string context)
    {
        var compact = context.Replace(" ", string.Empty).Replace("-", string.Empty);
        if (compact.Contains("CatalogFactorMapping", StringComparison.OrdinalIgnoreCase)
            || context.Contains("Catalog Factor Mapping", StringComparison.OrdinalIgnoreCase))
            return "Catalog Factor Mapping";

        if (compact.Contains("DataIngestion", StringComparison.OrdinalIgnoreCase)
            || context.Contains("Data Ingestion", StringComparison.OrdinalIgnoreCase))
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
