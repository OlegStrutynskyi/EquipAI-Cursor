using Microsoft.Playwright;

namespace EquipAI.Pages;

public class ImportInvoicePage : BasePage
{
    public ImportInvoicePage(IPage page) : base(page) { }

    private ILocator ImportTitle => Page.Locator("//h1[@id='invoice-import-title'] | //h1[contains(@id,'title')]");
    private ILocator ImportMessage => Page.Locator(
        "//p[@class='invoice-import__lead'] | //p[contains(@class,'page-header__lead')]");
    private ILocator BackBtn => Page.Locator("//a[contains(text(),'Back')]");
    private ILocator CsvFileLabel => Page.Locator("//span[@id='invoice-csv-file-label']");
    private ILocator PdfFileLabel => Page.Locator("//span[@id='invoice-pdf-file-label']");
    private ILocator ImportSection => Page.Locator("//div[@class='form-file-picker']");
    private ILocator ChooseFileBtn => Page.Locator("//label[normalize-space()='Choose file']");
    private ILocator CsvFileInput => Page.Locator("#invoice-csv-file");
    private ILocator PdfFileInput => Page.Locator("#invoice-pdf-file");
    private ILocator ImportBtn => Page.Locator("//button[normalize-space()='Import']");
    private ILocator AlertMessage => Page.Locator("//span[@role='alert']");

    public async Task OpenAsync()
    {
        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.OpenAsync();
        await invoicesPage.ClickImportCSVBtnAsync();
        await WaitForLoadedAsync();
    }

    public async Task OpenPdfAsync()
    {
        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.OpenAsync();
        await invoicesPage.ClickImportPDFBtnAsync();
        await WaitForLoadedAsync();
    }

    public async Task OpenUtilityBillAsync()
    {
        var invoicesPage = new InvoicesPage(Page);
        await invoicesPage.OpenAsync();
        await invoicesPage.ClickImportUtilityBillBtnAsync();
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
    public Task<bool> IsPdfFileLabelVisibleAsync() => PdfFileLabel.IsVisibleAsync();
    public Task<bool> IsImportSectionVisibleAsync() => ImportSection.IsVisibleAsync();
    public Task<bool> IsChooseFileBtnVisibleAsync() => ChooseFileBtn.IsVisibleAsync();
    public Task<bool> IsImportBtnVisibleAsync() => ImportBtn.IsVisibleAsync();
    public Task<bool> IsImportBtnEnabledAsync() => ImportBtn.IsEnabledAsync();

    public async Task UploadCsvFileAsync(string fileName)
    {
        var filePath = ResolveTestDataPath(fileName);
        await CsvFileInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
        await CsvFileInput.SetInputFilesAsync(filePath);
        await ImportBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Assertions.Expect(ImportBtn).ToBeEnabledAsync();
    }

    public async Task UploadPdfFileAsync(string fileName)
    {
        var filePath = ResolveTestDataPath(fileName);
        await PdfFileInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
        await PdfFileInput.SetInputFilesAsync(filePath);
    }

    public async Task ClickImportBtnAsync()
    {
        await ImportBtn.ClickAsync(new LocatorClickOptions { Force = true });
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

    public async Task ImportPdfAsync(string fileName, string expectedToasterMessage)
    {
        await UploadPdfFileAsync(fileName);
        await ImportBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Assertions.Expect(ImportBtn).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions
        {
            Timeout = 30_000,
        });

        var toasterTask = GetToasterMessageAsync(expectedToasterMessage);
        var alertTask = AlertMessage.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60_000,
        });

        await ImportBtn.ClickAsync();

        var completed = await Task.WhenAny(toasterTask, alertTask);
        if (completed == alertTask)
        {
            await alertTask;
            var alert = (await AlertMessage.InnerTextAsync()).Trim();
            if (!alert.Contains(expectedToasterMessage, StringComparison.Ordinal))
                throw new InvalidOperationException($"PDF import failed with alert: {alert}");
            return;
        }

        await toasterTask;
    }

    public async Task<string> GetAlertMessageAsync()
    {
        await AlertMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertMessage.InnerTextAsync()).Trim();
    }

    public async Task<string> GetToasterMessageAsync(string expectedText)
    {
        var toast = Page.Locator(
            $"//*[contains(@class,'toast') or contains(@class,'toaster') or contains(@class,'alert--success') or @role='status']" +
            $"[contains(normalize-space(.), \"{expectedText}\")]");
        await toast.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60_000,
        });
        return (await toast.First.InnerTextAsync()).Trim();
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
}
