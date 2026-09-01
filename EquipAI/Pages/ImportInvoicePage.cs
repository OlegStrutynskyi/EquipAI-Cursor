using Microsoft.Playwright;

namespace EquipAI.Pages;

public class ImportInvoicePage : BasePage
{
    public ImportInvoicePage(IPage page) : base(page) { }

    private ILocator ImportTitle => Page.Locator("//h1[@id='invoice-import-title']");
    private ILocator ImportMessage => Page.Locator(
        "//p[@class='invoice-import__lead'] | //p[contains(@class,'page-header__lead')]");
    private ILocator BackBtn => Page.Locator("//a[contains(text(),'Back')]");
    private ILocator CsvFileLabel => Page.Locator("//span[@id='invoice-csv-file-label']");
    private ILocator ImportSection => Page.Locator("//div[@class='form-file-picker']");
    private ILocator ChooseFileBtn => Page.Locator("//label[normalize-space()='Choose file']");
    private ILocator FileInput => Page.Locator("#invoice-csv-file");
    private ILocator ImportBtn => Page.Locator("//button[normalize-space()='Import']");
    private ILocator AlertMessage => Page.Locator("//span[@role='alert']");

    public async Task OpenAsync()
    {
        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.OpenAsync();
        await invoicesPage.ClickImportBtnAsync();
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await ImportTitle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetTitleAsync()
    {
        await ImportTitle.WaitForAsync();
        return (await ImportTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessageAsync()
    {
        await ImportMessage.WaitForAsync();
        return (await ImportMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsImportTitleVisibleAsync() => ImportTitle.IsVisibleAsync();
    public Task<bool> IsImportMessageVisibleAsync() => ImportMessage.IsVisibleAsync();
    public Task<bool> IsBackBtnVisibleAsync() => BackBtn.IsVisibleAsync();
    public Task<bool> IsCsvFileLabelVisibleAsync() => CsvFileLabel.IsVisibleAsync();
    public Task<bool> IsImportSectionVisibleAsync() => ImportSection.IsVisibleAsync();
    public Task<bool> IsChooseFileBtnVisibleAsync() => ChooseFileBtn.IsVisibleAsync();
    public Task<bool> IsImportBtnVisibleAsync() => ImportBtn.IsVisibleAsync();
    public Task<bool> IsImportBtnEnabledAsync() => ImportBtn.IsEnabledAsync();

    public async Task UploadCsvFileAsync(string fileName)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Test data file was not found: {filePath}");

        await FileInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
        await FileInput.SetInputFilesAsync(filePath);
        await ImportBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Assertions.Expect(ImportBtn).ToBeEnabledAsync();
    }

    public async Task ClickImportBtnAsync()
    {
        await ImportBtn.ClickAsync();
    }

    public async Task<InvoicesPage> ImportCsvAsync(string fileName)
    {
        await UploadCsvFileAsync(fileName);

        var navigationTask = Page.WaitForURLAsync(
            url =>
            {
                var path = new Uri(url).AbsolutePath.TrimEnd('/');
                return path.Equals("/invoices", StringComparison.OrdinalIgnoreCase);
            },
            new PageWaitForURLOptions
            {
                Timeout = 60_000,
                WaitUntil = WaitUntilState.Commit,
            });
        var alertTask = AlertMessage.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60_000,
        });

        await ImportBtn.ClickAsync();

        var completed = await Task.WhenAny(navigationTask, alertTask);
        if (completed == alertTask)
        {
            await alertTask;
            var alert = (await AlertMessage.InnerTextAsync()).Trim();
            throw new InvalidOperationException($"Import failed with alert: {alert}");
        }

        await navigationTask;

        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.GetTitleAsync();
        return invoicesPage;
    }

    public async Task<string> GetAlertMessageAsync()
    {
        await AlertMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertMessage.InnerTextAsync()).Trim();
    }

    public async Task<InvoicesPage> ClickBackBtnAsync()
    {
        await BackBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url =>
            {
                var path = new Uri(url).AbsolutePath.TrimEnd('/');
                return path.Equals("/invoices", StringComparison.OrdinalIgnoreCase);
            });

        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.GetTitleAsync();
        return invoicesPage;
    }
}
