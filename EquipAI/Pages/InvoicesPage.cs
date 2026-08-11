using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class InvoicesPage : BasePage
{
    public InvoicesPage(IPage page) : base(page) { }

    private ILocator InvoicesTitle => Page.Locator("//h1[@id='invoice-list-title']");
    private ILocator InvoicesMessage => Page.Locator("//p[@class='invoice-list__lead']");
    private ILocator ImportBtn => Page.Locator("//a[normalize-space()='Import']");
    private ILocator CreateBtn => Page.Locator("//a[normalize-space()='Create']");
    private ILocator InvoicesGrid => Page.Locator("//table[@class='table invoice-list__table']");
    private ILocator InvoicesGridProject => Page.Locator("//table[@class='table invoice-list__table']//tbody/tr/td[1]");
    private ILocator InvoicesGridInvoiceNumber => Page.Locator("//table[@class='table invoice-list__table']//tbody/tr/td[2]");
    private ILocator InvoicesGridCompany => Page.Locator("//table[@class='table invoice-list__table']//tbody/tr/td[3]");
    private ILocator InvoicesGridDate => Page.Locator("//table[@class='table invoice-list__table']//tbody/tr/td[4]");
    private ILocator InvoicesGridStatus => Page.Locator("//table[@class='table invoice-list__table']//tbody/tr/td[5]");
    private ILocator InvoicesGridImportDate => Page.Locator("//table[@class='table invoice-list__table']//tbody/tr/td[6]");
    private ILocator InvoicesGridApproveRejectDate => Page.Locator("//table[@class='table invoice-list__table']//tbody/tr/td[7]");
    private ILocator InvoicesGridSource => Page.Locator("//table[@class='table invoice-list__table']//tbody/tr/td[8]");
    private ILocator PreviousBtn => Page.Locator("//button[normalize-space()='Previous']");
    private ILocator NextBtn => Page.Locator("//button[normalize-space()='Next']");
    private ILocator InvoiceNumberCells => Page.Locator("//table[@class='table invoice-list__table']//tbody/tr/td[2]");
    private ILocator PaginationSummary => Page.Locator("//p[@class='pagination__info']");

    public async Task OpenAsync()
    {
        await Page.GotoAsync(Config.BaseUrl + "invoices");
        await InvoicesTitle.WaitForAsync();
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetTitleAsync()
    {
        await InvoicesTitle.WaitForAsync();
        return (await InvoicesTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessageAsync()
    {
        await InvoicesMessage.WaitForAsync();
        return (await InvoicesMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsInvoicesTitleVisibleAsync() => InvoicesTitle.IsVisibleAsync();
    public Task<bool> IsInvoicesMessageVisibleAsync() => InvoicesMessage.IsVisibleAsync();
    public Task<bool> IsImportBtnVisibleAsync() => ImportBtn.IsVisibleAsync();
    public Task<bool> IsCreateBtnVisibleAsync() => CreateBtn.IsVisibleAsync();
    public Task<bool> IsInvoicesGridVisibleAsync() => InvoicesGrid.IsVisibleAsync();

    public async Task<IReadOnlyList<string>> GetGridColumnHeadersAsync()
    {
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var headers = await InvoicesGrid.Locator("thead th").AllInnerTextsAsync();
        return headers
            .Select(header => header.Trim())
            .Where(header => !string.IsNullOrWhiteSpace(header)
                             && !header.Equals("Actions", StringComparison.Ordinal))
            .ToList();
    }

    public async Task<CreateInvoicePage> ClickCreateBtnAsync()
    {
        await CreateBtn.ClickAsync();
        await Page.WaitForURLAsync("**/invoices/new**");
        return new CreateInvoicePage(Page);
    }

    public async Task<ImportInvoicePage> ClickImportBtnAsync()
    {
        await ImportBtn.ClickAsync();
        await Page.WaitForURLAsync("**/invoices/import**");
        return new ImportInvoicePage(Page);
    }

    public async Task<ViewInvoicePage> ClickViewBtnAsync(string invoiceNumber)
    {
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForPaginationStableAsync();

        while (true)
        {
            var row = await FindInvoiceRowOnCurrentPageAsync(invoiceNumber);
            if (row is not null)
            {
                var viewBtn = row.GetByRole(AriaRole.Button, new() { Name = "View" });
                await viewBtn.ClickAsync();

                var viewInvoicePage = new ViewInvoicePage(Page);
                await viewInvoicePage.GetTitleAsync();
                return viewInvoicePage;
            }

            if (!await IsPaginationButtonEnabledAsync(NextBtn))
                break;

            try
            {
                await NextBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
            }
            catch (TimeoutException)
            {
                break;
            }

            await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await WaitForPaginationStableAsync();
        }

        throw new InvalidOperationException($"Invoice '{invoiceNumber}' was not found in the grid.");
    }

    public async Task<EditInvoicePage> ClickEditBtnAsync(string invoiceNumber)
    {
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForPaginationStableAsync();

        while (true)
        {
            var row = await FindInvoiceRowOnCurrentPageAsync(invoiceNumber);
            if (row is not null)
            {
                var editBtn = row.GetByRole(AriaRole.Button, new() { Name = "Edit" });
                await editBtn.ClickAsync();

                var editInvoicePage = new EditInvoicePage(Page);
                await editInvoicePage.WaitForLoadedAsync();
                return editInvoicePage;
            }

            if (!await IsPaginationButtonEnabledAsync(NextBtn))
                break;

            try
            {
                await NextBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
            }
            catch (TimeoutException)
            {
                break;
            }

            await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await WaitForPaginationStableAsync();
        }

        throw new InvalidOperationException($"Invoice '{invoiceNumber}' was not found in the grid.");
    }

    public async Task<bool> IsEditBtnVisibleForInvoiceAsync(string invoiceNumber)
    {
        var row = await FindInvoiceRowAcrossPagesAsync(invoiceNumber)
            ?? throw new InvalidOperationException($"Invoice '{invoiceNumber}' was not found in the grid.");

        return await row.GetByRole(AriaRole.Button, new() { Name = "Edit" }).IsVisibleAsync();
    }

    public async Task<bool> IsViewBtnVisibleForInvoiceAsync(string invoiceNumber)
    {
        var row = await FindInvoiceRowAcrossPagesAsync(invoiceNumber)
            ?? throw new InvalidOperationException($"Invoice '{invoiceNumber}' was not found in the grid.");

        return await row.GetByRole(AriaRole.Button, new() { Name = "View" }).IsVisibleAsync();
    }

    public async Task<int> GetTotalInvoiceCountFromPaginationSummaryAsync()
    {
        await PaginationSummary.WaitForAsync();
        var summaryText = (await PaginationSummary.TextContentAsync())?.Trim() ?? string.Empty;

        var matches = System.Text.RegularExpressions.Regex.Matches(summaryText, @"\d+");
        if (matches.Count == 0)
        {
            throw new InvalidOperationException(
                $"Could not parse invoice count from pagination summary: '{summaryText}'");
        }

        return int.Parse(matches[^1].Value);
    }

    public async Task<int> GetInvoiceNumberCountFromAllPagesAsync()
    {
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForPaginationStableAsync();

        var totalCount = 0;
        var visitedNextPage = false;

        while (true)
        {
            totalCount += await InvoiceNumberCells.CountAsync();

            if (!await IsPaginationButtonEnabledAsync(NextBtn))
                break;

            visitedNextPage = true;
            try
            {
                await NextBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
            }
            catch (TimeoutException)
            {
                break;
            }

            await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await WaitForPaginationStableAsync();
        }

        if (visitedNextPage)
        {
            while (await IsPaginationButtonEnabledAsync(PreviousBtn))
            {
                await PreviousBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
                await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
                await WaitForPaginationStableAsync();
            }
        }

        return totalCount;
    }

    public async Task<bool> IsInvoiceNumberInGridAsync(string invoiceNumber)
    {
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForPaginationStableAsync();

        while (true)
        {
            var invoiceNumbers = await InvoiceNumberCells.AllInnerTextsAsync();
            if (invoiceNumbers.Any(number => number.Trim().Equals(invoiceNumber, StringComparison.Ordinal)))
                return true;

            if (!await IsPaginationButtonEnabledAsync(NextBtn))
                break;

            try
            {
                await NextBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
            }
            catch (TimeoutException)
            {
                break;
            }

            await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await WaitForPaginationStableAsync();
        }

        return false;
    }

    public async Task<InvoiceGridRow?> GetInvoiceGridRowAsync(string invoiceNumber)
    {
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForPaginationStableAsync();

        while (true)
        {
            var gridRow = await FindInvoiceGridRowOnCurrentPageAsync(invoiceNumber);
            if (gridRow is not null)
                return gridRow;

            if (!await IsPaginationButtonEnabledAsync(NextBtn))
                break;

            try
            {
                await NextBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
            }
            catch (TimeoutException)
            {
                break;
            }

            await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await WaitForPaginationStableAsync();
        }

        return null;
    }

    private async Task<InvoiceGridRow?> FindInvoiceGridRowOnCurrentPageAsync(string invoiceNumber)
    {
        var row = await FindInvoiceRowOnCurrentPageAsync(invoiceNumber);
        if (row is null)
            return null;

        var cells = row.Locator("td");
        return new InvoiceGridRow
        {
            Project = (await cells.Nth(0).TextContentAsync())?.Trim() ?? string.Empty,
            InvoiceNumber = (await cells.Nth(1).TextContentAsync())?.Trim() ?? string.Empty,
            Company = (await cells.Nth(2).TextContentAsync())?.Trim() ?? string.Empty,
            Date = (await cells.Nth(3).TextContentAsync())?.Trim() ?? string.Empty,
            Status = (await cells.Nth(4).TextContentAsync())?.Trim() ?? string.Empty,
            ImportDate = (await cells.Nth(5).TextContentAsync())?.Trim() ?? string.Empty,
            ApproveRejectDate = (await cells.Nth(6).TextContentAsync())?.Trim() ?? string.Empty,
            Source = (await cells.Nth(7).TextContentAsync())?.Trim() ?? string.Empty,
        };
    }

    private async Task<ILocator?> FindInvoiceRowAcrossPagesAsync(string invoiceNumber)
    {
        await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForPaginationStableAsync();

        while (true)
        {
            var row = await FindInvoiceRowOnCurrentPageAsync(invoiceNumber);
            if (row is not null)
                return row;

            if (!await IsPaginationButtonEnabledAsync(NextBtn))
                break;

            try
            {
                await NextBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
            }
            catch (TimeoutException)
            {
                break;
            }

            await InvoicesGrid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await WaitForPaginationStableAsync();
        }

        return null;
    }

    private async Task<ILocator?> FindInvoiceRowOnCurrentPageAsync(string invoiceNumber)
    {
        var rows = Page.Locator("//table[@class='table invoice-list__table']//tbody/tr");
        var rowCount = await rows.CountAsync();

        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            var row = rows.Nth(rowIndex);
            var currentInvoiceNumber = (await row.Locator("td").Nth(1).TextContentAsync())?.Trim() ?? string.Empty;

            if (currentInvoiceNumber.Equals(invoiceNumber, StringComparison.Ordinal))
                return row;
        }

        return null;
    }

    private async Task WaitForPaginationStableAsync()
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var nextDisabled = await NextBtn.GetAttributeAsync("disabled") is not null;
            var previousDisabled = await PreviousBtn.GetAttributeAsync("disabled") is not null;
            await Task.Delay(100);
            var nextDisabledAfter = await NextBtn.GetAttributeAsync("disabled") is not null;
            var previousDisabledAfter = await PreviousBtn.GetAttributeAsync("disabled") is not null;

            if (nextDisabled == nextDisabledAfter && previousDisabled == previousDisabledAfter)
                return;
        }
    }

    private static async Task<bool> IsPaginationButtonEnabledAsync(ILocator button)
    {
        if (!await button.IsVisibleAsync())
            return false;

        if (await button.GetAttributeAsync("disabled") is not null)
            return false;

        return await button.IsEnabledAsync();
    }
}

public sealed class InvoiceGridRow
{
    public required string Project { get; init; }
    public required string InvoiceNumber { get; init; }
    public required string Company { get; init; }
    public required string Date { get; init; }
    public required string Status { get; init; }
    public required string ImportDate { get; init; }
    public required string ApproveRejectDate { get; init; }
    public required string Source { get; init; }
}