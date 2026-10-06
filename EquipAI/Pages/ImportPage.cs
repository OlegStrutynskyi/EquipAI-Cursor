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
    private ILocator ReviewLead => Page.Locator("//p[@class='invoice-pdf-review-page__lead']");
    private ILocator ReviewApproveBtn => Page.Locator("//button[normalize-space()='Approve']");
    private ILocator ReviewRejectBtn => Page.Locator("//button[normalize-space()='Reject']");
    private ILocator ReviewSaveAsDraftBtn => Page.Locator("//button[normalize-space()='Save as Draft']");
    private ILocator ReviewDraftSavedMessage => Page.Locator(
        "app-alert.alert--success, app-alert[tone='success'], .alert.alert--success")
        .Filter(new LocatorFilterOptions { HasTextString = "Draft saved" })
        .First;
    private ILocator ReviewAlertMessage => Page.Locator(
        "app-alert, .alert__body, .alert__content, .alert__title").First;
    private ILocator ReviewPreviewSection => Page.Locator("//div[@class='ng2-pdf-viewer-container']//div[1]//div[2]");
    private ILocator RejectDialog => Page.Locator("//div[@role='dialog']");
    private ILocator RejectDialogTitle => Page.Locator("//h2[@id='invoice-reject-dialog-title']");
    private ILocator RejectDialogLabel => Page.Locator("//label[@for='invoice-rejection-reason']");
    private ILocator RejectionReasonInput => Page.Locator("//textarea[@id='invoice-rejection-reason']");
    private ILocator RejectDialogCancelBtn => RejectDialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" });
    private ILocator RejectDialogConfirmBtn => RejectDialog.GetByRole(AriaRole.Button, new() { Name = "Confirm" });

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
    public Task<bool> IsSaveBtnVisibleAsync() => SaveBtn.IsVisibleAsync();
    public Task<bool> IsSaveBtnEnabledAsync() => SaveBtn.IsEnabledAsync();

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
            var row = rows.Nth(i);
            try
            {
                await row.ScrollIntoViewIfNeededAsync();
            }
            catch (TimeoutException)
            {
                // Virtualized/detached placeholder rows may not scroll.
            }

            var cells = row.Locator("td");
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

            var equipmentTag = GetCell(values, "EQUIPMENT TAG");
            if (string.IsNullOrWhiteSpace(equipmentTag))
                continue;

            result.Add(new TelemetryImportPreviewRow
            {
                RowNumber = GetCell(values, "ROW #", "ROW"),
                EquipmentTag = equipmentTag,
                EquipmentType = GetCell(values, "EQUIPMENT TYPE"),
                LocationText = await ResolveLocationTextAsync(row, values),
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

    public async Task<string> GetReviewMessageAsync()
    {
        await Page.Locator("#company-name").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await ReviewLead.WaitForAsync();
        return (await ReviewLead.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsReviewApproveBtnVisibleAsync() => ReviewApproveBtn.IsVisibleAsync();
    public Task<bool> IsReviewRejectBtnVisibleAsync() => ReviewRejectBtn.IsVisibleAsync();

    public async Task<ViewInvoicePage> ClickReviewApproveBtnAsync()
    {
        await ReviewApproveBtn.ClickAsync();
        var viewPage = new ViewInvoicePage(Page);
        await viewPage.GetTitleAsync();
        return viewPage;
    }

    public async Task ClickReviewRejectBtnAsync()
    {
        await ReviewRejectBtn.ClickAsync();
        await RejectDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetRejectDialogTitleAsync()
    {
        await RejectDialogTitle.WaitForAsync();
        return (await RejectDialogTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetRejectDialogLabelAsync()
    {
        await RejectDialogLabel.WaitForAsync();
        return (await RejectDialogLabel.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsRejectDialogVisibleAsync() => RejectDialog.IsVisibleAsync();
    public Task<bool> IsRejectionReasonInputVisibleAsync() => RejectionReasonInput.IsVisibleAsync();
    public Task<bool> IsRejectDialogCancelBtnVisibleAsync() => RejectDialogCancelBtn.IsVisibleAsync();
    public Task<bool> IsRejectDialogConfirmBtnVisibleAsync() => RejectDialogConfirmBtn.IsVisibleAsync();

    public async Task<bool> IsRejectDialogConfirmBtnDisabledAsync()
    {
        if (await RejectDialogConfirmBtn.GetAttributeAsync("disabled") is not null)
            return true;

        return !await RejectDialogConfirmBtn.IsEnabledAsync();
    }

    public async Task<bool> IsRejectDialogConfirmBtnEnabledAsync()
    {
        if (await RejectDialogConfirmBtn.GetAttributeAsync("disabled") is not null)
            return false;

        return await RejectDialogConfirmBtn.IsEnabledAsync();
    }

    public async Task FillRejectionReasonAsync(string reason)
    {
        await RejectionReasonInput.FillAsync(reason);
    }

    public async Task ClickRejectDialogCancelBtnAsync()
    {
        await RejectDialogCancelBtn.ClickAsync();
        await RejectDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
    }

    public async Task<ViewInvoicePage> ClickRejectDialogConfirmBtnAsync()
    {
        await RejectDialogConfirmBtn.ClickAsync();
        var viewPage = new ViewInvoicePage(Page);
        await viewPage.GetTitleAsync();
        return viewPage;
    }

    public async Task<UtilityBillUploadPage> ClickReviewCancelBtnAsync()
    {
        await CancelBtn.ClickAsync();
        await Page.WaitForURLAsync(
            url =>
            {
                var path = new Uri(url).AbsolutePath.TrimEnd('/');
                return path.Equals("/utility-bills", StringComparison.OrdinalIgnoreCase);
            },
            new PageWaitForURLOptions { Timeout = 60_000 });

        var utilityBillUploadPage = new UtilityBillUploadPage(Page);
        await utilityBillUploadPage.WaitForLoadedAsync();
        return utilityBillUploadPage;
    }

    public async Task ClickReviewSaveAsDraftBtnAsync()
    {
        await ReviewSaveAsDraftBtn.ClickAsync();
    }

    public async Task WaitForReviewDraftSavedMessageAsync()
    {
        try
        {
            await ReviewDraftSavedMessage.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30_000,
            });
        }
        catch (TimeoutException)
        {
            var alertText = string.Empty;
            try
            {
                if (await ReviewAlertMessage.CountAsync() > 0 && await ReviewAlertMessage.IsVisibleAsync())
                    alertText = (await ReviewAlertMessage.InnerTextAsync()).Trim();

                var fieldErrors = await Page.Locator(".form-hint--error").AllInnerTextsAsync();
                var errorText = string.Join(" | ", fieldErrors.Select(error => error.Trim()).Where(error => error.Length > 0));
                if (!string.IsNullOrWhiteSpace(errorText))
                    alertText = string.IsNullOrWhiteSpace(alertText) ? errorText : $"{alertText} | {errorText}";
            }
            catch (PlaywrightException)
            {
            }

            throw new TimeoutException(
                string.IsNullOrWhiteSpace(alertText)
                    ? "Timed out waiting for 'Draft saved' confirmation."
                    : $"Timed out waiting for 'Draft saved' confirmation. Visible alert: '{alertText}'");
        }
    }

    public Task<bool> IsReviewDraftSavedMessageVisibleAsync() => ReviewDraftSavedMessage.IsVisibleAsync();

    public Task ClearReviewCompanyNameAsync() => ClearReviewInputAsync("#company-name");
    public Task ClearReviewAddressAsync() => ClearReviewInputAsync("#address");
    public Task ClearReviewTotalCostAsync() => ClearReviewInputAsync("#total-cost");
    public Task ClearReviewLineDescriptionAsync(int linePosition) => ClearReviewInputAsync($"#line-description-{linePosition - 1}");
    public Task FillReviewLineDescriptionAsync(int linePosition, string description) =>
        FillReviewInputAsync($"#line-description-{linePosition - 1}", description);

    public Task FillReviewLineQuantityAsync(int linePosition, string quantity) =>
        FillReviewInputAsync($"#quantity-{linePosition - 1}", quantity);

    public Task FillReviewBillDateAsync(string billDate) => FillReviewInputAsync("#invoice-date", billDate);

    public Task FillReviewTotalCostAsync(string totalCost) => FillReviewInputAsync("#total-cost", totalCost);

    public Task<string> SelectDifferentReviewProjectAsync() => SelectDifferentReviewOptionAsync("#invoice-project");

    public Task<string> SelectDifferentReviewCurrencyAsync() => SelectDifferentReviewOptionAsync("#currency-code");

    public Task<string> SelectDifferentReviewLineEmissionTypeAsync(int linePosition) =>
        SelectDifferentReviewOptionAsync($"#emission-type-{linePosition - 1}");

    public Task<string> SelectDifferentReviewLineUnitAsync(int linePosition) =>
        SelectDifferentReviewOptionAsync($"#unit-of-measure-{linePosition - 1}");

    public Task<string> SelectDifferentReviewLineCategoryAsync(int linePosition) =>
        SelectDifferentReviewOptionAsync($"#line-emission-category-{linePosition - 1}");

    public async Task<int> ClickReviewAddRowBtnAsync()
    {
        var currentLineCount = await Page.Locator("//legend[starts-with(normalize-space(),'Line ')]").CountAsync();
        var newLine = currentLineCount + 1;
        await Page.Locator("//button[normalize-space()='Add row']").ClickAsync();
        await Page.Locator($"//legend[normalize-space()='Line {newLine}']").WaitForAsync();
        return newLine;
    }

    public Task ClearReviewLineQuantityAsync(int linePosition) => ClearReviewInputAsync($"#quantity-{linePosition - 1}");

    public Task<string> GetReviewCompanyNameErrorAsync() => ReadReviewFieldErrorAsync("company-name");
    public Task<string> GetReviewAddressErrorAsync() => ReadReviewFieldErrorAsync("address");
    public Task<string> GetReviewTotalCostErrorAsync() => ReadReviewFieldErrorAsync("total-cost");
    public Task<string> GetReviewLineDescriptionErrorAsync(int linePosition) => ReadReviewFieldErrorAsync($"line-description-{linePosition - 1}");
    public Task<string> GetReviewLineQuantityErrorAsync(int linePosition) => ReadReviewFieldErrorAsync($"quantity-{linePosition - 1}");
    public Task<bool> IsCancelBtnVisibleAsync() => CancelBtn.IsVisibleAsync();
    public Task<bool> IsReviewSaveAsDraftBtnVisibleAsync() => ReviewSaveAsDraftBtn.IsVisibleAsync();

    public async Task<bool> IsReviewPreviewSectionVisibleAsync()
    {
        try
        {
            await ReviewPreviewSection.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30_000,
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public Task<string> GetReviewCompanyNameAsync() => ReadInputValueAsync("#company-name");
    public Task<string> GetReviewAddressAsync() => ReadInputValueAsync("#address");
    public Task<string> GetReviewProjectAsync() => ReadSelectTextAsync("#invoice-project");
    public Task<string> GetReviewBillDateAsync() => ReadInputValueAsync("#invoice-date");
    public Task<string> GetReviewTotalCostAsync() => ReadInputValueAsync("#total-cost");
    public Task<string> GetReviewCurrencyAsync() => ReadSelectTextAsync("#currency-code");
    public Task<string> GetReviewLineDescriptionAsync(int linePosition) => ReadInputValueAsync($"#line-description-{linePosition - 1}");
    public Task<string> GetReviewLineQuantityAsync(int linePosition) => ReadInputValueAsync($"#quantity-{linePosition - 1}");
    public Task<string> GetReviewLineCostAsync(int linePosition) => ReadInputValueAsync($"#cost-{linePosition - 1}");
    public Task<string> GetReviewLineUnitAsync(int linePosition) => ReadSelectTextAsync($"#unit-of-measure-{linePosition - 1}");
    public Task<string> GetReviewLineEmissionTypeAsync(int linePosition) => ReadSelectTextAsync($"#emission-type-{linePosition - 1}");
    public Task<string> GetReviewLineEmissionCategoryAsync(int linePosition) => ReadSelectTextAsync($"#line-emission-category-{linePosition - 1}");

    public async Task FillReviewCompanyNameAsync(string companyName)
    {
        var input = Page.Locator("//label[contains(normalize-space(),'Company')]/following::input[not(@type='hidden')][1]");
        await input.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await input.FillAsync(companyName);
    }

    public Task FillReviewAddressAsync(string address) => FillReviewInputAsync("#address", address);

    public async Task SelectReviewProjectAsync(string projectName)
    {
        var dropdown = Page.Locator(
            "//label[normalize-space()='Project']/following::*[self::select or self::button or @role='combobox'][1]");
        await dropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var tagName = await dropdown.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
        if (tagName == "select")
        {
            await dropdown.SelectOptionAsync(new SelectOptionValue { Label = projectName });
            return;
        }

        await dropdown.ClickAsync();
        await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var search = SearchableSelectList.Locator("input");
        if (await search.CountAsync() > 0)
            await search.First.FillAsync(projectName);

        var option = SearchableSelectList.Locator("[role='option']")
            .Filter(new LocatorFilterOptions { HasTextString = projectName })
            .First;
        await option.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await option.ClickAsync();
    }

    public async Task ClickSaveAsDraftBtnAsync()
    {
        var saveAsDraftBtn = Page.Locator("//button[normalize-space()='Save as Draft']");
        await saveAsDraftBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await saveAsDraftBtn.ClickAsync();

        var savedMessage = Page.Locator("app-alert, .alert__body, .alert__content")
            .Filter(new LocatorFilterOptions { HasTextString = "saved" });
        try
        {
            await savedMessage.First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30_000,
            });
        }
        catch (TimeoutException)
        {
            await Assertions.Expect(saveAsDraftBtn).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions
            {
                Timeout = 30_000,
            });
        }
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

    private async Task ClearReviewInputAsync(string selector) => await FillReviewInputAsync(selector, string.Empty);

    private async Task FillReviewInputAsync(string selector, string value)
    {
        var input = Page.Locator(selector);
        await input.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await input.FillAsync(value);
    }

    private async Task<string> ReadReviewFieldErrorAsync(string inputId)
    {
        var error = Page.Locator($"//input[@id='{inputId}']/following-sibling::span");
        await error.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await error.TextContentAsync())?.Trim() ?? string.Empty;
    }

    private async Task<string> ReadInputValueAsync(string selector)
    {
        var input = Page.Locator(selector);
        await input.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await input.InputValueAsync()).Trim();
    }

    private async Task<string> SelectDifferentReviewOptionAsync(string selector)
    {
        var control = Page.Locator(selector);
        var current = (await ReadSelectTextAsync(selector)).Trim();
        var tagName = await control.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
        if (tagName == "select")
        {
            var options = await control.Locator("option").AllAsync();
            foreach (var option in options)
            {
                var value = await option.GetAttributeAsync("value");
                var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(value) || value.Contains(": null", StringComparison.Ordinal))
                    continue;
                if (IsReviewPlaceholderOption(text) || text.Equals(current, StringComparison.Ordinal))
                    continue;

                await control.SelectOptionAsync(value);
                return text;
            }

            throw new InvalidOperationException("Dropdown has no alternative option.");
        }

        await OpenReviewSelectAsync(control);
        var searchableOptions = SearchableSelectList.Locator("[role='option']");
        var count = await searchableOptions.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var option = searchableOptions.Nth(i);
            var text = (await option.TextContentAsync())?.Trim() ?? string.Empty;
            if (IsReviewPlaceholderOption(text) || text.Equals(current, StringComparison.Ordinal))
                continue;

            await option.ClickAsync();
            return text;
        }

        throw new InvalidOperationException("Dropdown has no alternative option.");
    }

    private async Task OpenReviewSelectAsync(ILocator control)
    {
        var expanded = await control.GetAttributeAsync("aria-expanded");
        if (string.Equals(expanded, "true", StringComparison.OrdinalIgnoreCase)
            && await SearchableSelectList.IsVisibleAsync())
            return;

        if (await SearchableSelectList.IsVisibleAsync())
        {
            await Page.Keyboard.PressAsync("Escape");
            try
            {
                await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = 2_000,
                });
            }
            catch (TimeoutException)
            {
            }
        }

        await control.ClickAsync();
        await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    private static bool IsReviewPlaceholderOption(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return true;

        if (text.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            return true;

        if (text.Equals("No project", StringComparison.OrdinalIgnoreCase))
            return true;

        return text is "—" or "-" or "–";
    }

    private async Task<string> ReadSelectTextAsync(string selector)
    {
        var control = Page.Locator(selector);
        await control.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var selected = control.Locator(".select__value");
        if (await selected.CountAsync() > 0)
            return NormalizeCell(await selected.First.InnerTextAsync());

        return NormalizeCell(await control.InnerTextAsync());
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
