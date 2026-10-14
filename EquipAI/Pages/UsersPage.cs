using Microsoft.Playwright;

namespace EquipAI.Pages;

public class UsersPage : BasePage
{
    public UsersPage(IPage page) : base(page) { }

    private ILocator Message => Page.Locator("//p[@class='page-header__lead']");
    private ILocator AddUserBtn => Page.Locator("//button[normalize-space()='Add user']");
    private ILocator SearchInput => Page.Locator("//input[@id='admin-users-search']");
    private ILocator Grid => Page.Locator("//table[@class='table']");
    private ILocator NextPageBtn => Page.Locator("//button[normalize-space()='Next']").First;
    private ILocator PreviousPageBtn => Page.Locator("//button[normalize-space()='Previous']").First;
    private ILocator DeleteDialog => Page.Locator("//div[@role='alertdialog']");
    private ILocator DeleteDialogTitle => Page.Locator("//div[@role='alertdialog']//h2");
    private ILocator DeleteDialogMessage => Page.Locator("//div[@role='alertdialog']//p[@id='confirm-dialog-message']");
    private ILocator DeleteDialogCancelBtn => Page.Locator("//div[@role='alertdialog']//button[normalize-space()='Cancel']");
    private ILocator DeleteDialogConfirmBtn => Page.Locator("//div[@role='alertdialog']//button[normalize-space()='Delete']");

    public async Task OpenAsync()
    {
        var sideMenuPage = new SideMenuPage(Page);
        await sideMenuPage.OpenAsync();
        await sideMenuPage.ClickUsersAsync();
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

    public Task<bool> IsAddUserBtnVisibleAsync() => AddUserBtn.IsVisibleAsync();
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

    public async Task SearchAsync(string text)
    {
        var before = NormalizeGridText(await Grid.Locator("tbody").InnerTextAsync());
        var response = Page.WaitForResponseAsync(
            response => IsUsersSearchResponse(response, text),
            new PageWaitForResponseOptions { Timeout = 15_000 });
        await SearchInput.FillAsync(text);
        await response;

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            if (await IsGridLoadingAsync())
            {
                await Task.Delay(50);
                continue;
            }

            var current = NormalizeGridText(await Grid.Locator("tbody").InnerTextAsync());
            if (current == before)
            {
                await Task.Delay(50);
                continue;
            }

            if (await IsEmptySearchMessageVisibleAsync())
                return;

            var rows = await GetGridRecordsOnCurrentPageAsync();
            if (rows.Count > 0 && rows.All(row => RowContains(row, text)))
                return;

            await Task.Delay(50);
        }

        throw new TimeoutException($"Users search for '{text}' did not finish.");
    }

    public async Task<string> GetGridEmptyMessageAsync()
    {
        var message = Grid.Locator("tbody").GetByText(
            "No users match your search.",
            new LocatorGetByTextOptions { Exact = true });
        await message.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10_000
        });
        return NormalizeGridText(await message.First.InnerTextAsync());
    }

    public async Task ClickGridColumnAsync(string columnName)
    {
        var header = Grid.Locator("thead th").Filter(new LocatorFilterOptions { HasTextString = columnName });
        var ariaBefore = await header.GetAttributeAsync("aria-sort");
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

            await WaitForGridDataAsync();
            return;
        }

        throw new TimeoutException($"Column '{columnName}' sort did not change from '{ariaBefore ?? "none"}'.");
    }

    public async Task<IReadOnlyList<UserGridRecord>> GetCurrentPageGridRecordsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        return await GetGridRecordsOnCurrentPageAsync();
    }

    public async Task<EditUserPage> ClickEditBtnAsync(string email)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        var row = await FindRowByEmailAcrossPagesAsync(email)
            ?? throw new InvalidOperationException($"User '{email}' was not found in the users grid.");
        await row.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        var editUserPage = new EditUserPage(Page);
        await editUserPage.WaitForLoadedAsync();
        return editUserPage;
    }

    public async Task<bool> IsUserInGridAsync(string email)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        return await FindRowByEmailAcrossPagesAsync(email) is not null;
    }

    public async Task<UserGridRecord> GetGridRecordByEmailAsync(string email)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (true)
        {
            await GoToFirstGridPageAsync();
            while (true)
            {
                foreach (var row in await GetGridRecordsOnCurrentPageAsync())
                {
                    if (row.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                        return row;
                }

                if (!await TryGoToNextGridPageAsync())
                    break;
            }

            if (DateTime.UtcNow >= deadline)
                throw new InvalidOperationException($"User '{email}' was not found in the users grid.");

            await Task.Delay(250);
        }
    }

    public async Task ClickDeleteBtnAsync(string email)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        var row = await FindRowByEmailAcrossPagesAsync(email)
            ?? throw new InvalidOperationException($"User '{email}' was not found in the users grid.");
        await row.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        await DeleteDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetDeleteDialogTitleAsync()
    {
        await DeleteDialogTitle.WaitForAsync();
        return (await DeleteDialogTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetDeleteDialogMessageAsync()
    {
        await DeleteDialogMessage.WaitForAsync();
        return (await DeleteDialogMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsDeleteDialogVisibleAsync() => DeleteDialog.IsVisibleAsync();
    public Task<bool> IsDeleteDialogCancelBtnVisibleAsync() => DeleteDialogCancelBtn.IsVisibleAsync();
    public Task<bool> IsDeleteDialogConfirmBtnVisibleAsync() => DeleteDialogConfirmBtn.IsVisibleAsync();

    public async Task ClickDeleteDialogCancelBtnAsync()
    {
        await DeleteDialogCancelBtn.ClickAsync();
        await DeleteDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
    }

    public async Task ClickDeleteDialogConfirmBtnAsync(string email)
    {
        await DeleteDialogConfirmBtn.ClickAsync();
        await DeleteDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            if (!await IsUserInGridAsync(email))
                return;

            await Task.Delay(250);
        }
    }

    public async Task<AddUserPage> ClickAddUserBtnAsync()
    {
        await AddUserBtn.ClickAsync();
        var addUserPage = new AddUserPage(Page);
        await addUserPage.WaitForLoadedAsync();
        return addUserPage;
    }

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await Grid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Replace('\u00A0', ' ').Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header))
            .ToList();
    }

    public async Task<IReadOnlyList<UserGridRecord>> GetAllGridRecordsAsync()
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await WaitForGridDataAsync();
        await GoToFirstGridPageAsync();

        var results = new List<UserGridRecord>();
        var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            foreach (var row in await GetGridRecordsOnCurrentPageAsync())
            {
                if (seenEmails.Add(row.Email))
                    results.Add(row);
            }

            if (!await TryGoToNextGridPageAsync())
                break;
        }

        return results;
    }

    private async Task<ILocator?> FindRowByEmailAcrossPagesAsync(string email)
    {
        await GoToFirstGridPageAsync();
        while (true)
        {
            var index = await FindRowIndexByEmailOnCurrentPageAsync(email);
            if (index is not null)
                return Grid.Locator("tbody tr").Nth(index.Value);

            if (!await TryGoToNextGridPageAsync())
                return null;
        }
    }

    private async Task<int?> FindRowIndexByEmailOnCurrentPageAsync(string email)
    {
        return await Grid.EvaluateAsync<int?>(
            """
            (table, email) => {
              const wanted = email.trim().toLowerCase();
              const rows = [...table.querySelectorAll('tbody tr')];
              for (let i = 0; i < rows.length; i++) {
                const cell = rows[i].querySelectorAll(':scope > td')[0];
                if (!cell) continue;
                const text = (cell.innerText || '').replace(/\u00a0/g, ' ').trim().toLowerCase();
                const title = (cell.getAttribute('title') || '').trim().toLowerCase();
                if (text === wanted || title === wanted) return i;
              }
              return null;
            }
            """,
            email);
    }

    private async Task<IReadOnlyList<UserGridRecord>> GetGridRecordsOnCurrentPageAsync()
    {
        var rows = await Grid.EvaluateAsync<string[][]>(
            """
            table => [...table.querySelectorAll('tbody tr')]
              .filter(tr => !tr.querySelector(':scope > td .table__skeleton-bar'))
              .map(tr => [...tr.querySelectorAll(':scope > td')].slice(0, 3).map(td =>
                (td.innerText || '').replace(/\u00a0/g, ' ').replace(/\s+/g, ' ').trim()))
              .filter(cells => cells.length > 1 && cells[0] && !cells[0].includes('No users'))
            """);

        return rows
            .Select(cells => new UserGridRecord(
                cells[0],
                cells.Length > 1 ? cells[1] : string.Empty,
                cells.Length > 2 ? cells[2] : string.Empty))
            .ToList();
    }

    private static bool IsUsersSearchResponse(IResponse response, string text)
    {
        if (!response.Ok || !response.Url.Contains("/api/admin/users", StringComparison.OrdinalIgnoreCase))
            return false;

        var query = new Uri(response.Url).Query.TrimStart('?');
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

    private async Task<bool> IsGridLoadingAsync() =>
        await Page.Locator(".table-wrapper--loading").CountAsync() > 0;

    private async Task<bool> IsEmptySearchMessageVisibleAsync()
    {
        var message = Grid.Locator("tbody").GetByText(
            "No users match your search.",
            new LocatorGetByTextOptions { Exact = true });
        return await message.CountAsync() > 0 && await message.First.IsVisibleAsync();
    }

    private static bool RowContains(UserGridRecord row, string term) =>
        string.Join(' ', row.Email, row.Name, row.Roles)
            .Contains(term, StringComparison.OrdinalIgnoreCase);

    private static string NormalizeGridText(string value) =>
        System.Text.RegularExpressions.Regex.Replace(value.Replace('\u00A0', ' '), @"\s+", " ").Trim();

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
        }

        try
        {
            await Page.WaitForFunctionAsync(
                """
                () => [...document.querySelectorAll('table.table tbody tr')]
                  .some(tr => {
                    if (tr.closest('.table-wrapper--loading')) return false;
                    if (tr.querySelector('.table__skeleton-bar')) return false;
                    const cell = tr.querySelectorAll('td')[0];
                    return cell && (cell.innerText || '').trim().length > 0;
                  })
                """,
                null,
                new PageWaitForFunctionOptions { Timeout = 30_000 });
        }
        catch (TimeoutException)
        {
        }
    }

    private async Task GoToFirstGridPageAsync()
    {
        while (await CanGoToAdjacentPageAsync(PreviousPageBtn))
        {
            var showingBefore = await TryGetShowingRangeAsync();
            await PreviousPageBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
            await WaitForPageChangeAsync(showingBefore);
        }
    }

    private async Task<bool> TryGoToNextGridPageAsync()
    {
        if (!await CanGoToAdjacentPageAsync(NextPageBtn))
            return false;

        var showingBefore = await TryGetShowingRangeAsync();
        await NextPageBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
        return await WaitForPageChangeAsync(showingBefore);
    }

    private async Task<bool> WaitForPageChangeAsync((int Start, int End, int Total)? showingBefore)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var showingAfter = await TryGetShowingRangeAsync();
            if (showingBefore is null
                || showingAfter is null
                || showingAfter.Value.Start != showingBefore.Value.Start)
            {
                await WaitForGridDataAsync();
                return true;
            }

            await Task.Delay(100);
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

    private async Task<(int Start, int End, int Total)?> TryGetShowingRangeAsync()
    {
        var showing = Page.Locator("nav.pagination .pagination__info").First;
        if (await showing.CountAsync() == 0)
            return null;

        var text = (await showing.InnerTextAsync())
            .Replace('\u00A0', ' ')
            .Replace('–', '-')
            .Replace('—', '-');
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
}

public sealed record UserGridRecord(string Email, string Name, string Roles);
