using System.Globalization;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class ImportPage : BasePage
{
    public ImportPage(IPage page) : base(page) { }

    private ILocator ImportTitle => Page.Locator(
        "//h1[@id='invoice-import-title'] | //h1[contains(@class,'page-title')] | //h1[contains(@id,'title')]");
    private ILocator ImportMessage => Page.Locator(
        "//p[@class='invoice-import__lead'] | //p[@class='telemetry-import__lead'] | //p[contains(@class,'page-header__lead')]");
    private ILocator BackBtn => Page.Locator("//a[contains(text(),'Back')]");
    private ILocator CsvFileLabel => Page.Locator("//span[@id='invoice-csv-file-label']");
    private ILocator PdfFileLabel => Page.Locator("//span[@id='invoice-pdf-file-label']");
    private ILocator ExcelFileLabel => Page.Locator(
        "//span[contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'excel file')]");
    private ILocator ImportSection => Page.Locator("//div[@class='form-file-picker']");
    private ILocator SelectFileSection => Page.Locator(
        "//div[contains(@class,'telemetry-import__field') and .//input[@type='file']] | //div[contains(@class,'form-file-picker')]");
    private ILocator ChooseFileBtn => Page.Locator("//label[normalize-space()='Choose file']");
    private ILocator CsvFileInput => Page.Locator("#invoice-csv-file");
    private ILocator PdfFileInput => Page.Locator("#invoice-pdf-file");
    private ILocator FileInput => Page.Locator("input[type='file']");
    private ILocator ImportBtn => Page.Locator("//button[normalize-space()='Import']");
    private ILocator SaveBtn => Page.Locator("//button[normalize-space()='Save']");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    private ILocator AlertMessage => Page.Locator("//span[@role='alert'] | //p[@role='alert']");
    private ILocator AlertBody => Page.Locator("//div[@class='alert__body']");
    private ILocator PreviewTable => Page.Locator("//table[contains(@class,'table')]");
    private ILocator ReportingMonthInput => Page.Locator("#telemetry-reporting-month");
    private ILocator CompanyInput => Page.Locator("#telemetry-company-name");
    private ILocator MonthYearError => Page.Locator("//input[@id='telemetry-reporting-month']/following-sibling::span");
    private ILocator CompanyError => Page.Locator("//input[@id='telemetry-company-name']/following-sibling::span");
    private ILocator UnrecognizedRowsAlert => Page.Locator("//app-alert[@aria-label='Unrecognized row validation messages']");
    private ILocator ValidationLead => Page.Locator("//p[@class='telemetry-import__validation-lead']");
    private ILocator ValidationList => Page.Locator("//ul[@class='telemetry-import__validation-list']");
    private ILocator SearchableSelectList => Page.Locator("//div[@role='listbox' and contains(@class,'select__list')]");

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
        var utilityBillUploadPage = new UtilityBillUploadPage(Page);
        await utilityBillUploadPage.OpenAsync();
        await utilityBillUploadPage.ClickImportPDFBtnAsync();
        await WaitForLoadedAsync();
    }

    public async Task OpenTelemetryAsync()
    {
        var telemetryPage = new TelemetryPage(Page);
        await telemetryPage.OpenAsync();
        await telemetryPage.ClickImportBtnAsync();
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
    public Task<bool> IsExcelFileLabelVisibleAsync() => ExcelFileLabel.IsVisibleAsync();
    public Task<bool> IsImportSectionVisibleAsync() => ImportSection.IsVisibleAsync();
    public Task<bool> IsSelectFileSectionVisibleAsync() => SelectFileSection.IsVisibleAsync();
    public Task<bool> IsChooseFileBtnVisibleAsync() => ChooseFileBtn.IsVisibleAsync();
    public Task<bool> IsImportBtnVisibleAsync() => ImportBtn.IsVisibleAsync();
    public Task<bool> IsImportBtnEnabledAsync() => ImportBtn.IsEnabledAsync();
    public Task<bool> IsReportingMonthVisibleAsync() => ReportingMonthInput.IsVisibleAsync();
    public Task<bool> IsCompanyVisibleAsync() => CompanyInput.IsVisibleAsync();
    public Task<bool> IsPreviewGridVisibleAsync() => PreviewTable.IsVisibleAsync();

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

    public async Task UploadFileAsync(string fileName)
    {
        var filePath = ResolveTestDataPath(fileName);
        await FileInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
        await FileInput.SetInputFilesAsync(filePath);

        if (fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            await ImportBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await Assertions.Expect(ImportBtn).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions
            {
                Timeout = 30_000,
            });
        }
    }

    public async Task ClickImportBtnAsync()
    {
        await ImportBtn.ClickAsync(new LocatorClickOptions { Force = true });
    }

    public async Task ClickCancelBtnAsync()
    {
        await CancelBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await CancelBtn.ClickAsync();
        await SelectFileSection.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000,
        });
        await ReportingMonthInput.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = 30_000,
        });
    }

    public async Task<TelemetryPage> ClickSaveBtnAsync()
    {
        await SaveBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Assertions.Expect(SaveBtn).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions
        {
            Timeout = 30_000,
        });
        await SaveBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url =>
            {
                var path = new Uri(url).AbsolutePath.TrimEnd('/');
                return path.Equals("/telemetry", StringComparison.OrdinalIgnoreCase);
            },
            new PageWaitForURLOptions { Timeout = 60_000 });

        var telemetryPage = new TelemetryPage(Page);
        await telemetryPage.WaitForLoadedAsync();
        return telemetryPage;
    }

    public async Task ClickSaveExpectingValidationAsync()
    {
        await SaveBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await SaveBtn.ClickAsync(new LocatorClickOptions { Force = true });
    }

    public async Task ClearReportingMonthAsync()
    {
        await ReportingMonthInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await ReportingMonthInput.FillAsync(string.Empty);
        await ReportingMonthInput.BlurAsync();
    }

    public async Task SetReportingMonthAsync(string yearMonth)
    {
        await ReportingMonthInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await ReportingMonthInput.FillAsync(yearMonth);
        await ReportingMonthInput.BlurAsync();
    }

    public async Task<string> SelectRandomDifferentLocationForRowAsync(int previewRowIndex = 0)
    {
        var row = PreviewTable.Locator("tbody tr").Nth(previewRowIndex);
        await row.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var trigger = row.Locator("button.select__trigger");
        await trigger.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var currentLocation = NormalizeCell(await row.Locator(".select__value").First.InnerTextAsync());
        await trigger.ClickAsync();
        await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var options = SearchableSelectList.Locator("[role='option']");
        var optionCount = await options.CountAsync();
        var candidates = new List<(int Index, string Text)>();
        for (var i = 0; i < optionCount; i++)
        {
            var text = NormalizeCell(await options.Nth(i).InnerTextAsync());
            if (string.IsNullOrWhiteSpace(text)
                || text.StartsWith("Select", StringComparison.OrdinalIgnoreCase)
                || text.Equals(currentLocation, StringComparison.Ordinal))
            {
                continue;
            }

            candidates.Add((i, text));
        }

        if (candidates.Count == 0)
            throw new InvalidOperationException("No alternative location options found in the dropdown.");

        var chosen = candidates[Random.Shared.Next(candidates.Count)];
        await options.Nth(chosen.Index).ClickAsync();

        try
        {
            await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 5_000,
            });
        }
        catch (TimeoutException)
        {
            // List may close without animation.
        }

        var selectedLocation = NormalizeCell(await row.Locator(".select__value").First.InnerTextAsync());
        if (!selectedLocation.Equals(chosen.Text, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Expected location '{chosen.Text}' to be selected, but found '{selectedLocation}'.");

        return selectedLocation;
    }

    public async Task ClearCompanyAsync()
    {
        await CompanyInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await CompanyInput.FillAsync(string.Empty);
        await CompanyInput.BlurAsync();
    }

    public async Task<string> GetMonthYearErrorAsync()
    {
        await MonthYearError.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await MonthYearError.InnerTextAsync()).Trim();
    }

    public async Task<string> GetCompanyErrorAsync()
    {
        await CompanyError.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await CompanyError.InnerTextAsync()).Trim();
    }

    public async Task<bool> IsUnrecognizedRowsAlertVisibleAsync()
    {
        await UnrecognizedRowsAlert.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60_000,
        });
        return await UnrecognizedRowsAlert.IsVisibleAsync();
    }

    public async Task<string> GetValidationLeadAsync()
    {
        await ValidationLead.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return NormalizeMultiline(await ValidationLead.InnerTextAsync());
    }

    public async Task<string> GetValidationListTextAsync()
    {
        await ValidationList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return NormalizeMultiline(await ValidationList.InnerTextAsync());
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

    public async Task<string> GetAlertBodyTextAsync()
    {
        await AlertBody.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertBody.InnerTextAsync()).Replace('\u00A0', ' ').Trim();
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

    public async Task<bool> IsPreviewTableVisibleAsync()
    {
        await PreviewTable.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60_000,
        });
        return await PreviewTable.IsVisibleAsync();
    }

    public async Task<string> GetReportingMonthValueAsync()
    {
        await ReportingMonthInput.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60_000,
        });

        var inputValue = await ReportingMonthInput.InputValueAsync();
        var inputType = await ReportingMonthInput.GetAttributeAsync("type");

        if (string.Equals(inputType, "month", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(inputValue))
        {
            return "--------- ----";
        }

        if (string.Equals(inputType, "month", StringComparison.OrdinalIgnoreCase)
            && DateTime.TryParseExact(
                inputValue,
                "yyyy-MM",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var monthValue))
        {
            return monthValue.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        }

        return inputValue?.Trim() ?? string.Empty;
    }

    public async Task<string> GetCompanyValueAsync()
    {
        await CompanyInput.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60_000,
        });

        var value = await CompanyInput.InputValueAsync();
        if (!string.IsNullOrWhiteSpace(value))
            return value.Trim();

        return (await CompanyInput.InnerTextAsync()).Trim();
    }

    public async Task<IReadOnlyList<TelemetryImportPreviewRow>> GetPreviewRowsAsync()
    {
        await PreviewTable.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60_000,
        });

        var headers = (await PreviewTable.Locator("thead th").AllInnerTextsAsync())
            .Select(NormalizeHeader)
            .ToList();
        var rows = PreviewTable.Locator("tbody tr");
        var count = await rows.CountAsync();
        var result = new List<TelemetryImportPreviewRow>();

        for (var i = 0; i < count; i++)
        {
            var cells = rows.Nth(i).Locator("td");
            var cellCount = await cells.CountAsync();
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < cellCount && c < headers.Count; c++)
            {
                var cell = cells.Nth(c);
                var selectValue = cell.Locator(".select__value");
                values[headers[c]] = await selectValue.CountAsync() > 0
                    ? NormalizeCell(await selectValue.First.InnerTextAsync())
                    : NormalizeCell(await cell.InnerTextAsync());
            }

            result.Add(new TelemetryImportPreviewRow
            {
                RowNumber = GetCell(values, "ROW #", "ROW"),
                EquipmentTag = GetCell(values, "EQUIPMENT TAG"),
                EquipmentType = GetCell(values, "EQUIPMENT TYPE"),
                LocationText = await ResolveLocationTextAsync(rows.Nth(i), values),
                OperatingHours = GetCell(values, "OPERATING HOURS"),
                FuelType = GetCell(values, "FUEL TYPE"),
            });
        }

        return result;
    }

    public async Task<InvoicesPage> ClickBackToInvoicesAsync()
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

    public async Task<UtilityBillUploadPage> ClickBackToUtilityBillUploadAsync()
    {
        await BackBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url =>
            {
                var path = new Uri(url).AbsolutePath.TrimEnd('/');
                return path.Equals("/utility-bills", StringComparison.OrdinalIgnoreCase);
            });

        var utilityBillUploadPage = new UtilityBillUploadPage(Page);
        await utilityBillUploadPage.WaitForLoadedAsync();
        return utilityBillUploadPage;
    }

    public async Task<TelemetryPage> ClickBackToTelemetryAsync()
    {
        await BackBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url =>
            {
                var path = new Uri(url).AbsolutePath.TrimEnd('/');
                return path.Equals("/telemetry", StringComparison.OrdinalIgnoreCase);
            });

        var telemetryPage = new TelemetryPage(Page);
        await telemetryPage.WaitForLoadedAsync();
        return telemetryPage;
    }

    private static async Task<string> ResolveLocationTextAsync(
        ILocator row,
        IReadOnlyDictionary<string, string> values)
    {
        var unmatchedSelect = row.Locator("td.telemetry-import__location-cell--unmatched .select__value");
        if (await unmatchedSelect.CountAsync() > 0)
            return NormalizeCell(await unmatchedSelect.First.InnerTextAsync());

        return GetCell(values, "LOCATION TEXT", "LOCATION");
    }

    private static string NormalizeHeader(string header) =>
        header.Replace('\u00A0', ' ').Trim().ToUpperInvariant();

    private static string NormalizeCell(string value) =>
        value.Replace('\u00A0', ' ').Trim();

    private static string NormalizeMultiline(string value)
    {
        var lines = value
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Replace('\u00A0', ' ')
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0);
        return string.Join("\n", lines);
    }

    private static string GetCell(IReadOnlyDictionary<string, string> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value))
                return value;
        }

        return string.Empty;
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

public sealed class TelemetryImportPreviewRow
{
    public required string RowNumber { get; init; }
    public required string EquipmentTag { get; init; }
    public required string EquipmentType { get; init; }
    public required string LocationText { get; init; }
    public required string OperatingHours { get; init; }
    public required string FuelType { get; init; }
}
