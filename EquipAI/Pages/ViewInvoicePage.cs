using Microsoft.Playwright;

namespace EquipAI.Pages;

public class ViewInvoicePage : BasePage
{
    public ViewInvoicePage(IPage page) : base(page) { }

    private ILocator BackBtn => Page.Locator("//a[contains(text(),'Back')]");
    private ILocator ViewTitle => Page.Locator("//h1[@id='invoice-detail-title']");
    private ILocator Status => Page.Locator("//p[@class='invoice-detail__meta']/span[contains(@class,'pill')]");
    private ILocator Source => Page.Locator("//p[@class='invoice-detail__meta']/span[contains(@class,'pill')]/following-sibling::span");
    private ILocator EditDraftBtn => Page.Locator("//button[normalize-space()='Edit Draft']");
    private ILocator HeaderSection => Page.Locator("//h2[@id='invoice-detail-header-heading']/..");
    private ILocator CompanyName => Page.Locator("//dt[normalize-space()='Company']/following-sibling::dd");
    private ILocator Address => Page.Locator("//dt[normalize-space()='Address']/following-sibling::dd");
    private ILocator Project => Page.Locator("//dt[normalize-space()='Project']/following-sibling::dd");
    private ILocator InvoiceDate => Page.Locator("//dt[normalize-space()='Invoice date']/following-sibling::dd");
    private ILocator Category => Page.Locator("//dt[normalize-space()='Category']/following-sibling::dd");
    private ILocator Total => Page.Locator("//dt[normalize-space()='Total']/following-sibling::dd");
    private ILocator LineItemsSection => Page.Locator("//h2[@id='invoice-detail-lines-heading']/..");
    private ILocator Description1 => Page.Locator("//td[normalize-space()='1']/following-sibling::td[1]");
    private ILocator Quantity1 => Page.Locator("//td[normalize-space()='1']/following-sibling::td[2]");
    private ILocator UnitPrice1 => Page.Locator("//td[normalize-space()='1']/following-sibling::td[3]");
    private ILocator Cost1 => Page.Locator("//td[normalize-space()='1']/following-sibling::td[4]");
    private ILocator EmissionType1 => Page.Locator("//td[normalize-space()='1']/following-sibling::td[5]");
    private ILocator Unit1 => Page.Locator("//td[normalize-space()='1']/following-sibling::td[6]");
    private ILocator Description2 => Page.Locator("//td[normalize-space()='2']/following-sibling::td[1]");
    private ILocator Quantity2 => Page.Locator("//td[normalize-space()='2']/following-sibling::td[2]");
    private ILocator UnitPrice2 => Page.Locator("//td[normalize-space()='2']/following-sibling::td[3]");
    private ILocator Cost2 => Page.Locator("//td[normalize-space()='2']/following-sibling::td[4]");
    private ILocator EmissionType2 => Page.Locator("//td[normalize-space()='2']/following-sibling::td[5]");
    private ILocator Unit2 => Page.Locator("//td[normalize-space()='2']/following-sibling::td[6]");
    private ILocator LineItemRows => Page.Locator("//h2[@id='invoice-detail-lines-heading']/..//tbody/tr");

    public async Task<string> GetTitleAsync()
    {
        await ViewTitle.WaitForAsync();
        return (await ViewTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetStatusAsync()
    {
        await Status.WaitForAsync();
        return (await Status.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetSourceAsync()
    {
        await Source.WaitForAsync();
        return (await Source.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetCompanyNameAsync()
    {
        await CompanyName.WaitForAsync();
        return (await CompanyName.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetAddressAsync()
    {
        await Address.WaitForAsync();
        return (await Address.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetProjectAsync()
    {
        await Project.WaitForAsync();
        return (await Project.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetInvoiceDateAsync()
    {
        await InvoiceDate.WaitForAsync();
        return (await InvoiceDate.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetCategoryAsync()
    {
        await Category.WaitForAsync();
        return (await Category.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetTotalAsync()
    {
        await Total.WaitForAsync();
        return (await Total.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetDescription1Async()
    {
        await Description1.WaitForAsync();
        return (await Description1.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetQuantity1Async()
    {
        await Quantity1.WaitForAsync();
        return (await Quantity1.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetUnitPrice1Async()
    {
        await UnitPrice1.WaitForAsync();
        return (await UnitPrice1.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetCost1Async()
    {
        await Cost1.WaitForAsync();
        return (await Cost1.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetEmissionType1Async()
    {
        await EmissionType1.WaitForAsync();
        return (await EmissionType1.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetUnit1Async()
    {
        await Unit1.WaitForAsync();
        return (await Unit1.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetDescription2Async()
    {
        await Description2.WaitForAsync();
        return (await Description2.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetQuantity2Async()
    {
        await Quantity2.WaitForAsync();
        return (await Quantity2.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetUnitPrice2Async()
    {
        await UnitPrice2.WaitForAsync();
        return (await UnitPrice2.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetCost2Async()
    {
        await Cost2.WaitForAsync();
        return (await Cost2.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetEmissionType2Async()
    {
        await EmissionType2.WaitForAsync();
        return (await EmissionType2.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetUnit2Async()
    {
        await Unit2.WaitForAsync();
        return (await Unit2.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<int> GetLineItemRowCountAsync()
    {
        await LineItemsSection.WaitForAsync();
        return await LineItemRows.CountAsync();
    }

    public async Task<EditInvoicePage> ClickEditDraftBtnAsync()
    {
        await EditDraftBtn.ClickAsync();
        var editInvoicePage = new EditInvoicePage(Page);
        await editInvoicePage.WaitForLoadedAsync();
        return editInvoicePage;
    }

    public async Task<InvoicesPage> ClickBackBtnAsync()
    {
        await BackBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url =>
            {
                var path = new Uri(url).AbsolutePath.TrimEnd('/');
                return path.Equals("/invoices", StringComparison.OrdinalIgnoreCase);
            },
            new PageWaitForURLOptions { Timeout = 60_000 });

        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.OpenAsync();
        return invoicesPage;
    }

    public Task<bool> IsBackBtnVisibleAsync() => BackBtn.IsVisibleAsync();
    public Task<bool> IsViewTitleVisibleAsync() => ViewTitle.IsVisibleAsync();
    public Task<bool> IsStatusVisibleAsync() => Status.IsVisibleAsync();
    public Task<bool> IsSourceVisibleAsync() => Source.IsVisibleAsync();
    public Task<bool> IsEditDraftBtnVisibleAsync() => EditDraftBtn.IsVisibleAsync();
    public Task<bool> IsHeaderSectionVisibleAsync() => HeaderSection.IsVisibleAsync();
    public Task<bool> IsLineItemsSectionVisibleAsync() => LineItemsSection.IsVisibleAsync();
}
