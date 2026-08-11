using Microsoft.Playwright;

namespace EquipAI.Pages;

public abstract class BasePage
{
    protected IPage Page { get; }

    protected BasePage(IPage page)
    {
        Page = page;
    }

    private ILocator PageTitle => Page.Locator("//h1[@class='page-title']");

    public async Task<string> GetPageTitleAsync()
    {
        await PageTitle.WaitForAsync();
        return (await PageTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }
}

public sealed class FilledInvoiceFormData
{
    public required string InvoiceNumber { get; init; }
    public required string CompanyName { get; init; }
    public required string Project { get; init; }
    public required string InvoiceDate { get; init; }
    public required string TotalCost { get; init; }
    public required string Currency { get; init; }
}
