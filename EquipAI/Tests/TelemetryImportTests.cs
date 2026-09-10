using System.Globalization;
using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class TelemetryImportTests : BaseTest
{
    [Test]
    public async Task T01_TelemetryImport_DefaultView()
    {
        const string expectedTitle = "Import Telemetry";
        const string expectedMessage =
            "Upload an Excel (.xlsx) file named CompanyName_Month_Year.xlsx (example: Herc_07_26.xlsx). Review the recognized rows, Month/Year, and Company before saving.";

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();

        (await importTelemetryPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await importTelemetryPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await importTelemetryPage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await importTelemetryPage.IsExcelFileLabelVisibleAsync()).Should().BeTrue();
        (await importTelemetryPage.IsSelectFileSectionVisibleAsync()).Should().BeTrue();
        (await importTelemetryPage.IsImportBtnVisibleAsync()).Should().BeTrue();
        (await importTelemetryPage.IsImportBtnEnabledAsync()).Should().BeFalse();
    }

    [Test]
    public async Task T02_TelemetryImport_ClickBackBtn()
    {
        const string expectedTitle = "Telemetry";

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        var telemetryPage = await importTelemetryPage.ClickBackBtnAsync();

        (await telemetryPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_TelemetryImport_IncorrectFormat()
    {
        await AssertImportAlertAsync("SC_Fuels_3.pdf", "Only .xlsx files are accepted.");
    }

    [Test]
    public async Task T04_TelemetryImport_Herc_OnlyHeaders()
    {
        await AssertImportAlertAsync(
            "Herc_08_26_OnlyHeaders.xlsx",
            "Excel file must contain at least one telemetry row.");
    }

    [Test]
    public async Task T05_TelemetryImport_Herc_NoCatClass()
    {
        await AssertImportAlertAsync(
            "Herc_08_26_no_CatClass.xlsx",
            "Missing required column(s): Cat-Class.");
    }

    [Test]
    public async Task T06_TelemetryImport_Herc_NoDescription()
    {
        await AssertImportAlertAsync(
            "Herc_08_26_no_Description.xlsx",
            "Missing required column(s): Description.");
    }

    [Test]
    public async Task T07_TelemetryImport_Herc_NoJobNumber()
    {
        await AssertImportAlertAsync(
            "Herc_08_26_no_JobNumber.xlsx",
            "Missing required column(s): Job Number.");
    }

    [Test]
    public async Task T08_TelemetryImport_Herc_NoMultipleColumns()
    {
        await AssertImportAlertAsync(
            "Herc_08_26_no_MultipleColumns.xlsx",
            "Missing required column(s): Cat-Class, Description.");
    }

    [Test]
    public async Task T09_TelemetryImport_Herc_NoHours()
    {
        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync("Herc_08_26_no_Hours.xlsx");
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T10_TelemetryImport_Herc_NoMonthYear()
    {
        const string expectedMonthYear = "--------- ----";

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync("Herc_no_MonthYear.xlsx");
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
        (await importTelemetryPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
    }

    [Test]
    public async Task T11_TelemetryImport_Herc_SuccessImport()
    {
        const string fileName = "Herc_08_26.xlsx";
        const string expectedMonthYear = "August 2026";
        const string expectedCompany = "Herc";
        const string expectedFuelType = "—";
        const string expectedSource = "Herc";
        var expectedImportDate = DateTime.Now.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

        try
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

            var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
            await importTelemetryPage.OpenAsync();
            await importTelemetryPage.UploadFileAsync(fileName);
            await importTelemetryPage.ClickImportBtnAsync();

            (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
            (await importTelemetryPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
            (await importTelemetryPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

            var previewRows = await importTelemetryPage.GetPreviewRowsAsync();
            previewRows.Should().HaveCount(2);
            previewRows[0].RowNumber.Should().Be("3");
            previewRows[0].EquipmentTag.Should().Be("111-2222");
            previewRows[0].EquipmentType.Should().Be("AUTOTEST DESCRIPTION 1");
            previewRows[0].LocationText.Should().Be("BOEING SOUTH");
            previewRows[0].OperatingHours.Should().Be("22.52");
            previewRows[0].FuelType.Should().Be(expectedFuelType);
            previewRows[1].RowNumber.Should().Be("4");
            previewRows[1].EquipmentTag.Should().Be("333-4444");
            previewRows[1].EquipmentType.Should().Be("AUTOTEST DESCRIPTION 2");
            previewRows[1].LocationText.Should().Be("JAN200");
            previewRows[1].OperatingHours.Should().Be("1.32");
            previewRows[1].FuelType.Should().Be(expectedFuelType);

            var telemetryPage = await importTelemetryPage.ClickSaveBtnAsync();
            (await telemetryPage.GetTitleAsync()).Should().Be("Telemetry");
            (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);

            var gridRows = await telemetryPage.GetGridRowsAsync();
            var row1 = gridRows.FirstOrDefault(r => r.EquipmentTag == "111-2222");
            var row2 = gridRows.FirstOrDefault(r => r.EquipmentTag == "333-4444");
            row1.Should().NotBeNull();
            row2.Should().NotBeNull();
            row1!.EquipmentType.Should().Be("AUTOTEST DESCRIPTION 1");
            row1.Location.Should().Be("5300904001 — Boeing South Yard");
            row1.OperatingHours.Should().Be("22.52");
            row1.FuelType.Should().Be(expectedFuelType);
            row1.ImportDate.Should().Be(expectedImportDate);
            row1.Source.Should().Be(expectedSource);
            row2!.EquipmentType.Should().Be("AUTOTEST DESCRIPTION 2");
            row2.Location.Should().Be("6000020001 — JAN200");
            row2.OperatingHours.Should().Be("1.32");
            row2.FuelType.Should().Be(expectedFuelType);
            row2.ImportDate.Should().Be(expectedImportDate);
            row2.Source.Should().Be(expectedSource);
        }
        finally
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);
        }
    }

    [Test]
    public async Task T12_TelemetryImport_Herc_CancelImport()
    {
        const string fileName = "Herc_08_26.xlsx";
        const string expectedMonthYear = "August 2026";
        const string expectedCompany = "Herc";
        const string expectedFuelType = "—";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync(fileName);
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
        (await importTelemetryPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importTelemetryPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        var previewRows = await importTelemetryPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(2);
        previewRows[0].RowNumber.Should().Be("3");
        previewRows[0].EquipmentTag.Should().Be("111-2222");
        previewRows[0].EquipmentType.Should().Be("AUTOTEST DESCRIPTION 1");
        previewRows[0].LocationText.Should().Be("BOEING SOUTH");
        previewRows[0].OperatingHours.Should().Be("22.52");
        previewRows[0].FuelType.Should().Be(expectedFuelType);
        previewRows[1].RowNumber.Should().Be("4");
        previewRows[1].EquipmentTag.Should().Be("333-4444");
        previewRows[1].EquipmentType.Should().Be("AUTOTEST DESCRIPTION 2");
        previewRows[1].LocationText.Should().Be("JAN200");
        previewRows[1].OperatingHours.Should().Be("1.32");
        previewRows[1].FuelType.Should().Be(expectedFuelType);

        await importTelemetryPage.ClickCancelBtnAsync();

        (await importTelemetryPage.IsReportingMonthVisibleAsync()).Should().BeFalse();
        (await importTelemetryPage.IsCompanyVisibleAsync()).Should().BeFalse();
        (await importTelemetryPage.IsPreviewGridVisibleAsync()).Should().BeFalse();
        (await importTelemetryPage.IsSelectFileSectionVisibleAsync()).Should().BeTrue();

        var telemetryPage = await importTelemetryPage.ClickBackBtnAsync();
        await telemetryPage.SetMonthAsync("2026-08");

        (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);
        (await telemetryPage.GetGridRowByEquipmentTagAsync("111-2222")).Should().BeNull();
        (await telemetryPage.GetGridRowByEquipmentTagAsync("333-4444")).Should().BeNull();
    }

    [Test]
    public async Task T13_TelemetryImport_Herc_EmptyFields()
    {
        const string fileName = "Herc_08_26.xlsx";
        const string expectedMonthYear = "August 2026";
        const string expectedCompany = "Herc";
        const string expectedMonthYearError = "Month/Year is required.";
        const string expectedCompanyError = "Company is required.";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync(fileName);
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importTelemetryPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        await importTelemetryPage.ClearReportingMonthAsync();
        await importTelemetryPage.ClearCompanyAsync();
        await importTelemetryPage.ClickSaveExpectingValidationAsync();

        (await importTelemetryPage.GetMonthYearErrorAsync()).Should().Be(expectedMonthYearError);
        (await importTelemetryPage.GetCompanyErrorAsync()).Should().Be(expectedCompanyError);
    }

    [Test]
    public async Task T14_TelemetryImport_Herc_MissingValues()
    {
        const string fileName = "Herc_08_26_MissingValues.xlsx";
        const string expectedFuelType = "—";
        const string expectedValidationLead =
            "Some rows were not fully recognized and are omitted from the table. Required columns: Cat-Class, Description, and Job Number.";
        var expectedValidationList = """
            Row 4: Cat-Class is required.
            Cat-Class: —
            Description: AUTOTEST DESCRIPTION 2
            Job Number: JAN200
            Total Hours Used: 1.32
            Fuel Type: —
            Row 5: Description is required.
            Cat-Class: 111-2222
            Description: —
            Job Number: BOEING SOUTH
            Total Hours Used: 22.52
            Fuel Type: —
            Row 6: Job Number is required.
            Cat-Class: 333-4444
            Description: AUTOTEST DESCRIPTION 3
            Job Number: —
            Total Hours Used: 1.32
            Fuel Type: —
            Row 8: Cat-Class and Description are required.
            Cat-Class: —
            Description: —
            Job Number: BOEING SOUTH
            Total Hours Used: 22.52
            Fuel Type: —
            Row 9: Description and Job Number are required.
            Cat-Class: 111-2222
            Description: —
            Job Number: —
            Total Hours Used: 22.52
            Fuel Type: —
            Row 10: Cat-Class, Description, and Job Number are required.
            Cat-Class: —
            Description: —
            Job Number: —
            Total Hours Used: 22.52
            Fuel Type: —
            """.Replace("\r\n", "\n").Trim();

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync(fileName);
        await importTelemetryPage.ClickImportBtnAsync();

        var previewRows = await importTelemetryPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(2);
        previewRows[0].RowNumber.Should().Be("3");
        previewRows[0].EquipmentTag.Should().Be("111-2222");
        previewRows[0].EquipmentType.Should().Be("AUTOTEST DESCRIPTION 1");
        previewRows[0].LocationText.Should().Be("BOEING SOUTH");
        previewRows[0].OperatingHours.Should().Be("22.52");
        previewRows[0].FuelType.Should().Be(expectedFuelType);
        previewRows[1].RowNumber.Should().Be("7");
        previewRows[1].EquipmentTag.Should().Be("444-5555");
        previewRows[1].EquipmentType.Should().Be("AUTOTEST DESCRIPTION 4");
        previewRows[1].LocationText.Should().Be("BOEING SOUTH");
        previewRows[1].OperatingHours.Should().Be(expectedFuelType);
        previewRows[1].FuelType.Should().Be(expectedFuelType);

        (await importTelemetryPage.IsUnrecognizedRowsAlertVisibleAsync()).Should().BeTrue();
        (await importTelemetryPage.GetValidationLeadAsync()).Should().Be(expectedValidationLead);
        (await importTelemetryPage.GetValidationListTextAsync()).Should().Be(expectedValidationList);
    }

    [Test]
    public async Task T15_TelemetryImport_JCB_OnlyHeaders()
    {
        await AssertImportAlertAsync(
            "JCB_08_26_OnlyHeaders.xlsx",
            "Excel file must contain at least one telemetry row.");
    }

    [Test]
    public async Task T16_TelemetryImport_JCB_NoEquipmentTag()
    {
        await AssertImportAlertAsync(
            "JCB_08_26_no_EquipmentTag.xlsx",
            "Missing required column(s): Equipment Tag.");
    }

    [Test]
    public async Task T17_TelemetryImport_JCB_NoEquipmentType()
    {
        await AssertImportAlertAsync(
            "JCB_08_26_no_EquipmentType.xlsx",
            "Missing required column(s): Equipment Type.");
    }

    [Test]
    public async Task T18_TelemetryImport_JCB_NoLocation()
    {
        await AssertImportAlertAsync(
            "JCB_08_26_no_Location.xlsx",
            "Missing required column(s): Location.");
    }

    [Test]
    public async Task T19_TelemetryImport_JCB_NoMultipleColumns()
    {
        await AssertImportAlertAsync(
            "JCB_08_26_no_MultipleColumns.xlsx",
            "Missing required column(s): Equipment Tag, Equipment Type.");
    }

    [Test]
    public async Task T20_TelemetryImport_JCB_NoHours()
    {
        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync("JCB_08_26_no_Hours.xlsx");
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T21_TelemetryImport_JCB_NoFuelType()
    {
        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync("JCB_08_26_no_FuelType.xlsx");
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T22_TelemetryImport_JCB_SuccessImport()
    {
        const string fileName = "JCB_08_26.xlsx";
        const string expectedMonthYear = "August 2026";
        const string expectedCompany = "JCB";
        const string expectedSource = "JCB";
        var expectedImportDate = DateTime.Now.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

        try
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

            var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
            await importTelemetryPage.OpenAsync();
            await importTelemetryPage.UploadFileAsync(fileName);
            await importTelemetryPage.ClickImportBtnAsync();

            (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
            (await importTelemetryPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
            (await importTelemetryPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

            var previewRows = await importTelemetryPage.GetPreviewRowsAsync();
            previewRows.Should().HaveCount(2);
            previewRows[0].RowNumber.Should().Be("3");
            previewRows[0].EquipmentTag.Should().Be("111-2222");
            previewRows[0].EquipmentType.Should().Be("AUTOTEST TYPE 1");
            previewRows[0].LocationText.Should().Be("BOEING SOUTH");
            previewRows[0].OperatingHours.Should().Be("22.52");
            previewRows[0].FuelType.Should().Be("P");
            previewRows[1].RowNumber.Should().Be("4");
            previewRows[1].EquipmentTag.Should().Be("333-4444");
            previewRows[1].EquipmentType.Should().Be("AUTOTEST TYPE 2");
            previewRows[1].LocationText.Should().Be("BOTAL INDUSTRIES");
            previewRows[1].OperatingHours.Should().Be("1.32");
            previewRows[1].FuelType.Should().Be("D");

            var telemetryPage = await importTelemetryPage.ClickSaveBtnAsync();
            (await telemetryPage.GetTitleAsync()).Should().Be("Telemetry");
            (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);

            var gridRows = await telemetryPage.GetGridRowsAsync();
            var row1 = gridRows.FirstOrDefault(r => r.EquipmentTag == "111-2222");
            var row2 = gridRows.FirstOrDefault(r => r.EquipmentTag == "333-4444");
            row1.Should().NotBeNull();
            row2.Should().NotBeNull();
            row1!.EquipmentType.Should().Be("AUTOTEST TYPE 1");
            row1.Location.Should().Be("5300904001 — Boeing South Yard");
            row1.OperatingHours.Should().Be("22.52");
            row1.FuelType.Should().Be("P");
            row1.ImportDate.Should().Be(expectedImportDate);
            row1.Source.Should().Be(expectedSource);
            row2!.EquipmentType.Should().Be("AUTOTEST TYPE 2");
            row2.Location.Should().Be("2094761001 — Abbott Sturgis InstrumntCal");
            row2.OperatingHours.Should().Be("1.32");
            row2.FuelType.Should().Be("D");
            row2.ImportDate.Should().Be(expectedImportDate);
            row2.Source.Should().Be(expectedSource);
        }
        finally
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);
        }
    }

    [Test]
    public async Task T23_TelemetryImport_JCB_CancelImport()
    {
        const string fileName = "JCB_08_26.xlsx";
        const string expectedMonthYear = "August 2026";
        const string expectedCompany = "JCB";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync(fileName);
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
        (await importTelemetryPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importTelemetryPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        var previewRows = await importTelemetryPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(2);
        previewRows[0].RowNumber.Should().Be("3");
        previewRows[0].EquipmentTag.Should().Be("111-2222");
        previewRows[0].EquipmentType.Should().Be("AUTOTEST TYPE 1");
        previewRows[0].LocationText.Should().Be("BOEING SOUTH");
        previewRows[0].OperatingHours.Should().Be("22.52");
        previewRows[0].FuelType.Should().Be("P");
        previewRows[1].RowNumber.Should().Be("4");
        previewRows[1].EquipmentTag.Should().Be("333-4444");
        previewRows[1].EquipmentType.Should().Be("AUTOTEST TYPE 2");
        previewRows[1].LocationText.Should().Be("BOTAL INDUSTRIES");
        previewRows[1].OperatingHours.Should().Be("1.32");
        previewRows[1].FuelType.Should().Be("D");

        await importTelemetryPage.ClickCancelBtnAsync();

        (await importTelemetryPage.IsReportingMonthVisibleAsync()).Should().BeFalse();
        (await importTelemetryPage.IsCompanyVisibleAsync()).Should().BeFalse();
        (await importTelemetryPage.IsPreviewGridVisibleAsync()).Should().BeFalse();
        (await importTelemetryPage.IsSelectFileSectionVisibleAsync()).Should().BeTrue();

        var telemetryPage = await importTelemetryPage.ClickBackBtnAsync();
        await telemetryPage.SetMonthAsync("2026-08");

        (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);
        (await telemetryPage.GetGridRowByEquipmentTagAsync("111-2222")).Should().BeNull();
        (await telemetryPage.GetGridRowByEquipmentTagAsync("333-4444")).Should().BeNull();
    }

    [Test]
    public async Task T24_TelemetryImport_JCB_EmptyFields()
    {
        const string fileName = "JCB_08_26.xlsx";
        const string expectedMonthYear = "August 2026";
        const string expectedCompany = "JCB";
        const string expectedMonthYearError = "Month/Year is required.";
        const string expectedCompanyError = "Company is required.";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync(fileName);
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importTelemetryPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        await importTelemetryPage.ClearReportingMonthAsync();
        await importTelemetryPage.ClearCompanyAsync();
        await importTelemetryPage.ClickSaveExpectingValidationAsync();

        (await importTelemetryPage.GetMonthYearErrorAsync()).Should().Be(expectedMonthYearError);
        (await importTelemetryPage.GetCompanyErrorAsync()).Should().Be(expectedCompanyError);
    }

    [Test]
    public async Task T25_TelemetryImport_JCB_MissingValues()
    {
        const string fileName = "JCB_08_26_MissingValues.xlsx";
        const string expectedFuelType = "—";
        const string expectedValidationLead =
            "Some rows were not fully recognized and are omitted from the table. Required columns: Equipment Tag, Equipment Type, and Location.";
        var expectedValidationList = """
            Row 3: Equipment Tag is required.
            Equipment Tag: —
            Equipment Type: AUTOTEST TYPE 2
            Location: BOTAL INDUSTRIES
            Operating Hours: 1.32
            Fuel Type: D
            Row 4: Equipment Type is required.
            Equipment Tag: 111-2224
            Equipment Type: —
            Location: BOEING SOUTH
            Operating Hours: 22.52
            Fuel Type: P
            Row 5: Location is required.
            Equipment Tag: 111-2225
            Equipment Type: AUTOTEST TYPE 4
            Location: —
            Operating Hours: 1.32
            Fuel Type: D
            Row 8: Equipment Tag and Equipment Type are required.
            Equipment Tag: —
            Equipment Type: —
            Location: BOEING SOUTH
            Operating Hours: 22.52
            Fuel Type: P
            Row 9: Equipment Tag and Location are required.
            Equipment Tag: —
            Equipment Type: AUTOTEST TYPE 8
            Location: —
            Operating Hours: 1.32
            Fuel Type: D
            Row 11: Equipment Tag, Equipment Type, and Location are required.
            Equipment Tag: —
            Equipment Type: —
            Location: —
            Operating Hours: 1.32
            Fuel Type: D
            """.Replace("\r\n", "\n").Trim();

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync(fileName);
        await importTelemetryPage.ClickImportBtnAsync();

        var previewRows = await importTelemetryPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(4);
        previewRows[0].RowNumber.Should().Be("2");
        previewRows[0].EquipmentTag.Should().Be("111-2222");
        previewRows[0].EquipmentType.Should().Be("AUTOTEST TYPE 1");
        previewRows[0].LocationText.Should().Be("BOEING SOUTH");
        previewRows[0].OperatingHours.Should().Be("22.52");
        previewRows[0].FuelType.Should().Be("P");
        previewRows[1].RowNumber.Should().Be("6");
        previewRows[1].EquipmentTag.Should().Be("111-2226");
        previewRows[1].EquipmentType.Should().Be("AUTOTEST TYPE 5");
        previewRows[1].LocationText.Should().Be("BOEING SOUTH");
        previewRows[1].OperatingHours.Should().Be("22.52");
        previewRows[1].FuelType.Should().Be(expectedFuelType);
        previewRows[2].RowNumber.Should().Be("7");
        previewRows[2].EquipmentTag.Should().Be("111-2227");
        previewRows[2].EquipmentType.Should().Be("AUTOTEST TYPE 6");
        previewRows[2].LocationText.Should().Be("BOTAL INDUSTRIES");
        previewRows[2].OperatingHours.Should().Be(expectedFuelType);
        previewRows[2].FuelType.Should().Be("D");
        previewRows[3].RowNumber.Should().Be("10");
        previewRows[3].EquipmentTag.Should().Be("111-2230");
        previewRows[3].EquipmentType.Should().Be("AUTOTEST TYPE 9");
        previewRows[3].LocationText.Should().Be("BOEING SOUTH");
        previewRows[3].OperatingHours.Should().Be("22.52");
        previewRows[3].FuelType.Should().Be(expectedFuelType);

        (await importTelemetryPage.IsUnrecognizedRowsAlertVisibleAsync()).Should().BeTrue();
        (await importTelemetryPage.GetValidationLeadAsync()).Should().Be(expectedValidationLead);
        (await importTelemetryPage.GetValidationListTextAsync()).Should().Be(expectedValidationList);
    }

    [Test]
    public async Task T26_TelemetryImport_Custom_OnlyHeaders()
    {
        await AssertImportAlertAsync(
            "Custom_08_26_OnlyHeaders.xlsx",
            "Excel file must contain at least one telemetry row.");
    }

    [Test]
    public async Task T27_TelemetryImport_Custom_NoEquipmentTag()
    {
        await AssertImportAlertAsync(
            "Custom_08_26_no_EquipmentTag.xlsx",
            "Missing required column(s): Equipment Tag.");
    }

    [Test]
    public async Task T28_TelemetryImport_Custom_NoEquipmentType()
    {
        await AssertImportAlertAsync(
            "Custom_08_26_no_EquipmentType.xlsx",
            "Missing required column(s): Equipment Type.");
    }

    [Test]
    public async Task T29_TelemetryImport_Custom_NoLocation()
    {
        await AssertImportAlertAsync(
            "Custom_08_26_no_Location.xlsx",
            "Missing required column(s): Location.");
    }

    [Test]
    public async Task T30_TelemetryImport_Custom_NoMultipleColumns()
    {
        await AssertImportAlertAsync(
            "Custom_08_26_no_MultipleColumns.xlsx",
            "Missing required column(s): Equipment Type, Location.");
    }

    [Test]
    public async Task T31_TelemetryImport_Custom_NoHours()
    {
        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync("Custom_08_26_no_Hours.xlsx");
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T32_TelemetryImport_Custom_NoFuelType()
    {
        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync("Custom_08_26_no_FuelType.xlsx");
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T33_TelemetryImport_Custom_NoMonthYear()
    {
        await AssertImportAlertAsync(
            "Custom_no_MonthYear.xlsx",
            "File name must match 'CompanyName_Month_Year.xlsx' (example: Herc_07_26.xlsx).");
    }

    [Test]
    public async Task T34_TelemetryImport_Custom_SuccessImport()
    {
        const string fileName = "Custom_08_26.xlsx";
        const string expectedMonthYear = "August 2026";
        const string expectedCompany = "Custom";
        const string expectedSource = "Custom";
        var expectedImportDate = DateTime.Now.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

        try
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

            var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
            await importTelemetryPage.OpenAsync();
            await importTelemetryPage.UploadFileAsync(fileName);
            await importTelemetryPage.ClickImportBtnAsync();

            (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
            (await importTelemetryPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
            (await importTelemetryPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

            var previewRows = await importTelemetryPage.GetPreviewRowsAsync();
            previewRows.Should().HaveCount(2);
            previewRows[0].RowNumber.Should().Be("2");
            previewRows[0].EquipmentTag.Should().Be("111-2221");
            previewRows[0].EquipmentType.Should().Be("AUTOTEST TYPE 1");
            previewRows[0].LocationText.Should().Be("BOEING SOUTH");
            previewRows[0].OperatingHours.Should().Be("22.52");
            previewRows[0].FuelType.Should().Be("P");
            previewRows[1].RowNumber.Should().Be("3");
            previewRows[1].EquipmentTag.Should().Be("111-2222");
            previewRows[1].EquipmentType.Should().Be("AUTOTEST TYPE 2");
            previewRows[1].LocationText.Should().Be("BOTAL INDUSTRIES");
            previewRows[1].OperatingHours.Should().Be("1.32");
            previewRows[1].FuelType.Should().Be("D");

            var telemetryPage = await importTelemetryPage.ClickSaveBtnAsync();
            (await telemetryPage.GetTitleAsync()).Should().Be("Telemetry");
            (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);

            var gridRows = await telemetryPage.GetGridRowsAsync();
            var row1 = gridRows.FirstOrDefault(r => r.EquipmentTag == "111-2221");
            var row2 = gridRows.FirstOrDefault(r => r.EquipmentTag == "111-2222");
            row1.Should().NotBeNull();
            row2.Should().NotBeNull();
            row1!.EquipmentType.Should().Be("AUTOTEST TYPE 1");
            row1.Location.Should().Be("5300904001 — Boeing South Yard");
            row1.OperatingHours.Should().Be("22.52");
            row1.FuelType.Should().Be("P");
            row1.ImportDate.Should().Be(expectedImportDate);
            row1.Source.Should().Be(expectedSource);
            row2!.EquipmentType.Should().Be("AUTOTEST TYPE 2");
            row2.Location.Should().Be("2094761001 — Abbott Sturgis InstrumntCal");
            row2.OperatingHours.Should().Be("1.32");
            row2.FuelType.Should().Be("D");
            row2.ImportDate.Should().Be(expectedImportDate);
            row2.Source.Should().Be(expectedSource);
        }
        finally
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);
        }
    }

    [Test]
    public async Task T35_TelemetryImport_Custom_CancelImport()
    {
        const string fileName = "Custom_08_26.xlsx";
        const string expectedMonthYear = "August 2026";
        const string expectedCompany = "Custom";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync(fileName);
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
        (await importTelemetryPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importTelemetryPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        var previewRows = await importTelemetryPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(2);
        previewRows[0].RowNumber.Should().Be("2");
        previewRows[0].EquipmentTag.Should().Be("111-2221");
        previewRows[0].EquipmentType.Should().Be("AUTOTEST TYPE 1");
        previewRows[0].LocationText.Should().Be("BOEING SOUTH");
        previewRows[0].OperatingHours.Should().Be("22.52");
        previewRows[0].FuelType.Should().Be("P");
        previewRows[1].RowNumber.Should().Be("3");
        previewRows[1].EquipmentTag.Should().Be("111-2222");
        previewRows[1].EquipmentType.Should().Be("AUTOTEST TYPE 2");
        previewRows[1].LocationText.Should().Be("BOTAL INDUSTRIES");
        previewRows[1].OperatingHours.Should().Be("1.32");
        previewRows[1].FuelType.Should().Be("D");

        await importTelemetryPage.ClickCancelBtnAsync();

        (await importTelemetryPage.IsReportingMonthVisibleAsync()).Should().BeFalse();
        (await importTelemetryPage.IsCompanyVisibleAsync()).Should().BeFalse();
        (await importTelemetryPage.IsPreviewGridVisibleAsync()).Should().BeFalse();
        (await importTelemetryPage.IsSelectFileSectionVisibleAsync()).Should().BeTrue();

        var telemetryPage = await importTelemetryPage.ClickBackBtnAsync();
        await telemetryPage.SetMonthAsync("2026-08");

        (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);
        (await telemetryPage.GetGridRowByEquipmentTagAsync("111-2221")).Should().BeNull();
        (await telemetryPage.GetGridRowByEquipmentTagAsync("111-2222")).Should().BeNull();
    }

    [Test]
    public async Task T36_TelemetryImport_Custom_EmptyFields()
    {
        const string fileName = "Custom_08_26.xlsx";
        const string expectedMonthYear = "August 2026";
        const string expectedCompany = "Custom";
        const string expectedMonthYearError = "Month/Year is required.";
        const string expectedCompanyError = "Company is required.";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync(fileName);
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importTelemetryPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        await importTelemetryPage.ClearReportingMonthAsync();
        await importTelemetryPage.ClearCompanyAsync();
        await importTelemetryPage.ClickSaveExpectingValidationAsync();

        (await importTelemetryPage.GetMonthYearErrorAsync()).Should().Be(expectedMonthYearError);
        (await importTelemetryPage.GetCompanyErrorAsync()).Should().Be(expectedCompanyError);
    }

    [Test]
    public async Task T37_TelemetryImport_Custom_MissingValues()
    {
        const string fileName = "Custom_08_26_MissingValues.xlsx";
        const string expectedFuelType = "—";
        const string expectedValidationLead =
            "Some rows were not fully recognized and are omitted from the table. Required columns: Equipment Tag, Equipment Type, and Location.";
        var expectedValidationList = """
            Row 2: Equipment Tag is required.
            Equipment Tag: —
            Equipment Type: AUTOTEST TYPE 1
            Location: BOEING SOUTH
            Operating Hours: 22.52
            Fuel Type: P
            Row 3: Equipment Type is required.
            Equipment Tag: 111-2222
            Equipment Type: —
            Location: BOTAL INDUSTRIES
            Operating Hours: 1.32
            Fuel Type: D
            Row 4: Location is required.
            Equipment Tag: 111-2223
            Equipment Type: AUTOTEST TYPE 2
            Location: —
            Operating Hours: 22.52
            Fuel Type: P
            Row 8: Equipment Tag and Equipment Type are required.
            Equipment Tag: —
            Equipment Type: —
            Location: BOEING SOUTH
            Operating Hours: 22.52
            Fuel Type: P
            Row 9: Equipment Type and Location are required.
            Equipment Tag: 222-3333
            Equipment Type: —
            Location: —
            Operating Hours: 1.32
            Fuel Type: D
            Row 10: Location is required.
            Equipment Tag: 222-3334
            Equipment Type: AUTOTEST TYPE 4
            Location: —
            Operating Hours: 22.52
            Fuel Type: —
            Row 12: Equipment Tag is required.
            Equipment Tag: —
            Equipment Type: AUTOTEST TYPE 6
            Location: BOEING SOUTH
            Operating Hours: —
            Fuel Type: P
            Row 13: Equipment Type is required.
            Equipment Tag: 333-4444
            Equipment Type: —
            Location: BOTAL INDUSTRIES
            Operating Hours: 1.32
            Fuel Type: —
            Row 14: Location is required.
            Equipment Tag: 333-4445
            Equipment Type: AUTOTEST TYPE 7
            Location: —
            Operating Hours: 22.52
            Fuel Type: —
            """.Replace("\r\n", "\n").Trim();

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync(fileName);
        await importTelemetryPage.ClickImportBtnAsync();

        var previewRows = await importTelemetryPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(4);
        previewRows[0].RowNumber.Should().Be("5");
        previewRows[0].EquipmentTag.Should().Be("111-2224");
        previewRows[0].EquipmentType.Should().Be("AUTOTEST TYPE 3");
        previewRows[0].LocationText.Should().Be("BOTAL INDUSTRIES");
        previewRows[0].OperatingHours.Should().Be("1.32");
        previewRows[0].FuelType.Should().Be(expectedFuelType);
        previewRows[1].RowNumber.Should().Be("6");
        previewRows[1].EquipmentTag.Should().Be("111-2225");
        previewRows[1].EquipmentType.Should().Be("AUTOTEST TYPE 4");
        previewRows[1].LocationText.Should().Be("BOEING SOUTH");
        previewRows[1].OperatingHours.Should().Be(expectedFuelType);
        previewRows[1].FuelType.Should().Be("P");
        previewRows[2].RowNumber.Should().Be("11");
        previewRows[2].EquipmentTag.Should().Be("222-3335");
        previewRows[2].EquipmentType.Should().Be("AUTOTEST TYPE 5");
        previewRows[2].LocationText.Should().Be("BOTAL INDUSTRIES");
        previewRows[2].OperatingHours.Should().Be(expectedFuelType);
        previewRows[2].FuelType.Should().Be(expectedFuelType);
        previewRows[3].RowNumber.Should().Be("15");
        previewRows[3].EquipmentTag.Should().Be("333-4446");
        previewRows[3].EquipmentType.Should().Be("AUTOTEST TYPE 8");
        previewRows[3].LocationText.Should().Be("BOTAL INDUSTRIES");
        previewRows[3].OperatingHours.Should().Be("1.32");
        previewRows[3].FuelType.Should().Be("D");

        (await importTelemetryPage.IsUnrecognizedRowsAlertVisibleAsync()).Should().BeTrue();
        (await importTelemetryPage.GetValidationLeadAsync()).Should().Be(expectedValidationLead);
        (await importTelemetryPage.GetValidationListTextAsync()).Should().Be(expectedValidationList);
    }

    [Test]
    public async Task T38_TelemetryImport_Custom_ChangeMonthYearLocation()
    {
        const string fileName = "Custom_08_26.xlsx";
        const string expectedCompany = "Custom";
        const string initialMonthYear = "August 2026";
        const string changedMonthYear = "June 2025";
        const string changedMonthValue = "2025-06";

        try
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

            var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
            await importTelemetryPage.OpenAsync();
            await importTelemetryPage.UploadFileAsync(fileName);
            await importTelemetryPage.ClickImportBtnAsync();

            (await importTelemetryPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
            (await importTelemetryPage.GetReportingMonthValueAsync()).Should().Be(initialMonthYear);
            (await importTelemetryPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

            await importTelemetryPage.SetReportingMonthAsync(changedMonthValue);
            (await importTelemetryPage.GetReportingMonthValueAsync()).Should().Be(changedMonthYear);

            var selectedLocation = await importTelemetryPage.SelectRandomDifferentLocationForRowAsync(0);

            var telemetryPage = await importTelemetryPage.ClickSaveBtnAsync();
            (await telemetryPage.GetTitleAsync()).Should().Be("Telemetry");

            await telemetryPage.SetMonthAsync(changedMonthValue);
            (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(changedMonthYear);

            var gridRows = await telemetryPage.GetGridRowsAsync();
            var row1 = gridRows.FirstOrDefault(r => r.EquipmentTag == "111-2221");
            var row2 = gridRows.FirstOrDefault(r => r.EquipmentTag == "111-2222");
            row1.Should().NotBeNull();
            row2.Should().NotBeNull();
            row1!.EquipmentType.Should().Be("AUTOTEST TYPE 1");
            row1.Location.Should().Be(selectedLocation);
            row1.OperatingHours.Should().Be("22.52");
            row1.FuelType.Should().Be("P");
            row2!.EquipmentType.Should().Be("AUTOTEST TYPE 2");
            row2.Location.Should().Be("2094761001 — Abbott Sturgis InstrumntCal");
            row2.OperatingHours.Should().Be("1.32");
            row2.FuelType.Should().Be("D");
        }
        finally
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);
        }
    }

    [Test]
    public async Task T39_TelemetryImport_Custom_UnknownProject()
    {
        const string fileName = "Custom_08_26_UnknownProject.xlsx";
        const string expectedAlert =
            "2 rows could not be matched to a location from the Location column and will be saved under the corporate fallback location unless you choose a location from the dropdown. Add a project alias to attribute them automatically.";
        const string expectedLocation = "000000 — Haskell";

        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync(fileName);
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.GetAlertBodyTextAsync()).Should().Be(expectedAlert);

        var previewRows = await importTelemetryPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(2);
        previewRows[0].LocationText.Should().Be(expectedLocation);
        previewRows[1].LocationText.Should().Be(expectedLocation);
    }

    private async Task AssertImportAlertAsync(string fileName, string expectedAlertMessage)
    {
        var importTelemetryPage = new ImportTelemetryPage(Fixture.Page);
        await importTelemetryPage.OpenAsync();
        await importTelemetryPage.UploadFileAsync(fileName);
        await importTelemetryPage.ClickImportBtnAsync();

        (await importTelemetryPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }
}
