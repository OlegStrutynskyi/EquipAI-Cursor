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

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();

        (await importPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await importPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await importPage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await importPage.IsExcelFileLabelVisibleAsync()).Should().BeTrue();
        (await importPage.IsSelectFileSectionVisibleAsync()).Should().BeTrue();
        (await importPage.IsImportBtnVisibleAsync()).Should().BeTrue();
        (await importPage.IsImportBtnEnabledAsync()).Should().BeFalse();
    }

    [Test]
    public async Task T02_TelemetryImport_ClickBackBtn()
    {
        const string expectedTitle = "Telemetry Upload";

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        var telemetryPage = await importPage.ClickBackToTelemetryAsync();

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
            "Herc_07_26_new_only_headers.xlsx",
            "Excel file must contain at least one telemetry row.");
    }

    [Test]
    public async Task T05_TelemetryImport_Herc_NoCatClass()
    {
        await AssertImportAlertAsync(
            "Herc_07_26_new_no_Cat_Class.xlsx",
            "Missing required column(s): Cat Class.");
    }

    [Test]
    public async Task T06_TelemetryImport_Herc_NoDescription()
    {
        await AssertImportAlertAsync(
            "Herc_07_26_new_no_Cat_Class_Description.xlsx",
            "Missing required column(s): Cat Class Description.");
    }

    [Test]
    public async Task T07_TelemetryImport_Herc_NoJobName()
    {
        await AssertImportAlertAsync(
            "Herc_07_26_new_no_Job_Name.xlsx",
            "Missing required column(s): Job Name.");
    }

    [Test]
    public async Task T08_TelemetryImport_Herc_NoICNumber()
    {
        await AssertImportAlertAsync(
            "Herc_07_26_new_no_IC_Number.xlsx",
            "Missing required column(s): IC Number.");
    }

    [Test]
    public async Task T09_TelemetryImport_Herc_NoMultipleColumns()
    {
        await AssertImportAlertAsync(
            "Herc_07_26_new_no_multiple_columns.xlsx",
            "Missing required column(s): IC Number, Cat Class Description, Job Name.");
    }

    [Test]
    public async Task T10_TelemetryImport_Herc_NoHours()
    {
        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync("Herc_07_26_new_no_Hours.xlsx");
        await importPage.ClickImportBtnAsync();

        (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T11_TelemetryImport_Herc_NoMonthYear()
    {
        const string expectedMonthYear = "--------- ----";

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync("Herc_new_no_MonthYear.xlsx");
        await importPage.ClickImportBtnAsync();

        (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
        (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
    }

    [Test]
    public async Task T12_TelemetryImport_Herc_SuccessImport()
    {
        const string fileName = "Herc_07_26_new_correct.xlsx";
        const string expectedMonthYear = "July 2026";
        const string expectedCompany = "Herc";
        const string expectedFuelType = "—";
        const string expectedSource = "Herc";
        var expectedImportDate = DateTime.Now.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

        try
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

            var importPage = new ImportPage(Fixture.Page);
            await importPage.OpenTelemetryAsync();
            await importPage.UploadFileAsync(fileName);
            await importPage.ClickImportBtnAsync();

            (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
            (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
            (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

            var previewRows = await importPage.GetPreviewRowsAsync();
            previewRows.Should().HaveCount(2);
            previewRows[0].RowNumber.Should().Be("5");
            previewRows[0].EquipmentTag.Should().Be("9207403");
            previewRows[0].EquipmentType.Should().Be("920-7403 RUBBER DISCHARGE HOSE CAM CPLNG 2X50");
            previewRows[0].LocationText.Should().Be("CITY CREEK WATER PLANT");
            previewRows[0].OperatingHours.Should().Be("0");
            previewRows[0].FuelType.Should().Be(expectedFuelType);
            previewRows[1].RowNumber.Should().Be("6");
            previewRows[1].EquipmentTag.Should().Be("800486519");
            previewRows[1].EquipmentType.Should().Be("630-1265 CART UTV 4 PASSENGER GAS WITH CAB HVAC");
            previewRows[1].LocationText.Should().Be("AMAZON RIDGELAND");
            previewRows[1].OperatingHours.Should().Be("1.9");
            previewRows[1].FuelType.Should().Be(expectedFuelType);

            var telemetryPage = await importPage.ClickSaveBtnAsync();
            (await telemetryPage.GetTitleAsync()).Should().Be("Telemetry Upload");
            await telemetryPage.SetMonthAsync("2026-07");
            (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);

            var gridRows = await telemetryPage.GetGridRowsAsync();
            var row1 = gridRows.FirstOrDefault(r => r.EquipmentTag == "9207403");
            var row2 = gridRows.FirstOrDefault(r => r.EquipmentTag == "800486519");
            row1.Should().NotBeNull();
            row2.Should().NotBeNull();
            row1!.EquipmentType.Should().Be("920-7403 RUBBER DISCHARGE HOSE CAM CPLNG 2X50");
            row1.Location.Should().Be("6704204001 — City Creek Treatment Self Perf");
            row1.OperatingHours.Should().Be("0");
            row1.FuelType.Should().Be(expectedFuelType);
            row1.ImportDate.Should().Be(expectedImportDate);
            row1.Source.Should().Be(expectedSource);
            row2!.EquipmentType.Should().Be("630-1265 CART UTV 4 PASSENGER GAS WITH CAB HVAC");
            row2.Location.Should().Be("6000025001 — JAN200 - AWS Internal");
            row2.OperatingHours.Should().Be("1.9");
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
    public async Task T13_TelemetryImport_Herc_CancelImport()
    {
        const string fileName = "Herc_07_26_new_correct.xlsx";
        const string expectedMonthYear = "July 2026";
        const string expectedCompany = "Herc";
        const string expectedFuelType = "—";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
        (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        var previewRows = await importPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(2);
        previewRows[0].RowNumber.Should().Be("5");
        previewRows[0].EquipmentTag.Should().Be("9207403");
        previewRows[0].EquipmentType.Should().Be("920-7403 RUBBER DISCHARGE HOSE CAM CPLNG 2X50");
        previewRows[0].LocationText.Should().Be("CITY CREEK WATER PLANT");
        previewRows[0].OperatingHours.Should().Be("0");
        previewRows[0].FuelType.Should().Be(expectedFuelType);
        previewRows[1].RowNumber.Should().Be("6");
        previewRows[1].EquipmentTag.Should().Be("800486519");
        previewRows[1].EquipmentType.Should().Be("630-1265 CART UTV 4 PASSENGER GAS WITH CAB HVAC");
        previewRows[1].LocationText.Should().Be("AMAZON RIDGELAND");
        previewRows[1].OperatingHours.Should().Be("1.9");
        previewRows[1].FuelType.Should().Be(expectedFuelType);

        await importPage.ClickCancelBtnAsync();

        (await importPage.IsReportingMonthVisibleAsync()).Should().BeFalse();
        (await importPage.IsCompanyVisibleAsync()).Should().BeFalse();
        (await importPage.IsPreviewGridVisibleAsync()).Should().BeFalse();
        (await importPage.IsSelectFileSectionVisibleAsync()).Should().BeTrue();

        var telemetryPage = await importPage.ClickBackToTelemetryAsync();
        await telemetryPage.SetMonthAsync("2026-07");

        (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);
        (await telemetryPage.GetGridRowByEquipmentTagAsync("9207403")).Should().BeNull();
        (await telemetryPage.GetGridRowByEquipmentTagAsync("800486519")).Should().BeNull();
    }

    [Test]
    public async Task T14_TelemetryImport_Herc_EmptyFields()
    {
        const string fileName = "Herc_07_26_new_correct.xlsx";
        const string expectedMonthYear = "July 2026";
        const string expectedCompany = "Herc";
        const string expectedMonthYearError = "Month/Year is required.";
        const string expectedCompanyError = "Company is required.";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        await importPage.ClearReportingMonthAsync();
        await importPage.ClearCompanyAsync();
        await importPage.ClickSaveExpectingValidationAsync();

        (await importPage.GetMonthYearErrorAsync()).Should().Be(expectedMonthYearError);
        (await importPage.GetCompanyErrorAsync()).Should().Be(expectedCompanyError);
    }

    [Test]
    public async Task T15_TelemetryImport_Herc_MissingValues()
    {
        const string fileName = "Herc_07_26_new_missing_values.xlsx";
        const string expectedFuelType = "—";
        const string expectedValidationLead =
            "They are omitted from the table. Required columns: IC Number, Cat Class and Cat Class Description, and Job Name.";
        var expectedValidationList = """
            Row 9: IC Number is required.
            IC Number: —
            Cat Class and Cat Class Description: 405-1087 SCISSOR LIFT 19FT 32IN COMPACT LCS ELEC
            Job Name: CITY CREEK WATER PLANT
            Hr/Miles Out: 225.000
            Row 10: Job Name is required.
            IC Number: 800443474
            Cat Class and Cat Class Description: 501-1100 100KW GENERATOR DSL
            Job Name: —
            Hr/Miles Out: 6278.100
            Row 13: At least one of Cat Class and Cat Class Description should have value.
            IC Number: 568801149
            Cat Class and Cat Class Description: —
            Job Name: CITY CREEK WATER PLANT
            Hr/Miles Out: 3337.000
            Row 14: IC Number is required.
            IC Number: —
            Cat Class and Cat Class Description: 20KW GENERATOR DSL
            Job Name: CITY CREEK WTP
            Hr/Miles Out: 2137.800
            Row 15: Job Name is required.
            IC Number: 9207403
            Cat Class and Cat Class Description: 920-7403
            Job Name: —
            Hr/Miles Out: 0.000
            Row 16: IC Number is required. At least one of Cat Class and Cat Class Description should have value.
            IC Number: —
            Cat Class and Cat Class Description: —
            Job Name: AMAZON RIDGELAND
            Hr/Miles Out: —
            Row 17: IC Number is required. Job Name is required.
            IC Number: —
            Cat Class and Cat Class Description: 630-1265
            Job Name: —
            Hr/Miles Out: —
            Row 18: IC Number is required. Job Name is required.
            IC Number: —
            Cat Class and Cat Class Description: 630-1265 CART UTV 4 PASSENGER GAS WITH CAB HVAC
            Job Name: —
            Hr/Miles Out: 1.600
            Row 19: IC Number is required. At least one of Cat Class and Cat Class Description should have value. Job Name is required.
            IC Number: —
            Cat Class and Cat Class Description: —
            Job Name: —
            Hr/Miles Out: 1144.000
            """.Replace("\r\n", "\n").Trim();

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        var previewRows = await importPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(4);
        previewRows[0].RowNumber.Should().Be("6");
        previewRows[0].EquipmentTag.Should().Be("210372663");
        previewRows[0].EquipmentType.Should().Be("SCISSOR LIFT 40FT 47IN ELEC");
        previewRows[0].LocationText.Should().Be("000000 — Haskell");
        previewRows[0].OperatingHours.Should().Be("127");
        previewRows[0].FuelType.Should().Be(expectedFuelType);
        previewRows[1].RowNumber.Should().Be("7");
        previewRows[1].EquipmentTag.Should().Be("800453796");
        previewRows[1].EquipmentType.Should().Be("460-1085");
        previewRows[1].LocationText.Should().Be("CITY CREEK WATER PLANT");
        previewRows[1].OperatingHours.Should().Be("730.87");
        previewRows[1].FuelType.Should().Be(expectedFuelType);
        previewRows[2].RowNumber.Should().Be("8");
        previewRows[2].EquipmentTag.Should().Be("210342678");
        previewRows[2].EquipmentType.Should().Be("405-1087 SCISSOR LIFT 19FT 32IN COMPACT LCS ELEC");
        previewRows[2].LocationText.Should().Be("CITY CREEK WATER PLANT");
        previewRows[2].OperatingHours.Should().Be(expectedFuelType);
        previewRows[2].FuelType.Should().Be(expectedFuelType);
        previewRows[3].RowNumber.Should().Be("12");
        previewRows[3].EquipmentTag.Should().Be("800431523");
        previewRows[3].EquipmentType.Should().Be("800-1080 PUMP TRASH 2IN GAS");
        previewRows[3].LocationText.Should().Be("CITY CREEK WATER PLANT");
        previewRows[3].OperatingHours.Should().Be("0");
        previewRows[3].FuelType.Should().Be(expectedFuelType);

        (await importPage.IsUnrecognizedRowsAlertVisibleAsync()).Should().BeTrue();
        (await importPage.GetValidationLeadAsync()).Should().Be(expectedValidationLead);
        (await importPage.GetValidationListTextAsync()).Should().Be(expectedValidationList);
    }

    [Test]
    public async Task T16_TelemetryImport_JCB_OnlyHeaders()
    {
        await AssertImportAlertAsync(
            "JCB_07_26_new_only_headers.xlsx",
            "Excel file must contain at least one telemetry row.");
    }

    [Test]
    public async Task T17_TelemetryImport_JCB_NoIC()
    {
        await AssertImportAlertAsync(
            "JCB_07_26_new_no_IC.xlsx",
            "Missing required column(s): IC.");
    }

    [Test]
    public async Task T18_TelemetryImport_JCB_NoCatClass()
    {
        await AssertImportAlertAsync(
            "JCB_07_26_new_no_Cat-Class.xlsx",
            "Missing required column(s): Cat-Class.");
    }

    [Test]
    public async Task T19_TelemetryImport_JCB_NoDescription()
    {
        await AssertImportAlertAsync(
            "JCB_07_26_new_no_Description.xlsx",
            "Missing required column(s): Description.");
    }

    [Test]
    public async Task T20_TelemetryImport_JCB_NoJobName()
    {
        await AssertImportAlertAsync(
            "JCB_07_26_new_no_JobName.xlsx",
            "Missing required column(s): Job Name.");
    }

    [Test]
    public async Task T21_TelemetryImport_JCB_NoMultipleColumns()
    {
        await AssertImportAlertAsync(
            "JCB_07_26_new_no_multiple_columns.xlsx",
            "Missing required column(s): IC, Cat-Class, Description.");
    }

    [Test]
    public async Task T22_TelemetryImport_JCB_NoHours()
    {
        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync("JCB_07_26_new_no_Hours.xlsx");
        await importPage.ClickImportBtnAsync();

        (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T23_TelemetryImport_JCB_NoMonthYear()
    {
        const string expectedMonthYear = "--------- ----";

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync("JCB_new_no_MonthYear.xlsx");
        await importPage.ClickImportBtnAsync();

        (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
        (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
    }

    [Test]
    public async Task T24_TelemetryImport_JCB_SuccessImport()
    {
        const string fileName = "JCB_07_26_new_correct.xlsx";
        const string expectedMonthYear = "July 2026";
        const string expectedCompany = "JCB";
        const string expectedFuelType = "—";
        const string expectedSource = "JCB";
        var expectedImportDate = DateTime.Now.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

        try
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

            var importPage = new ImportPage(Fixture.Page);
            await importPage.OpenTelemetryAsync();
            await importPage.UploadFileAsync(fileName);
            await importPage.ClickImportBtnAsync();

            (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
            (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
            (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

            var previewRows = await importPage.GetPreviewRowsAsync();
            previewRows.Should().HaveCount(5);
            previewRows[0].RowNumber.Should().Be("2");
            previewRows[0].EquipmentTag.Should().Be("210342678");
            previewRows[0].EquipmentType.Should().Be("405-1087 SCISSOR LIFT 19FT 32IN COMPACT LCS ELEC");
            previewRows[0].LocationText.Should().Be("CITY CREEK WATER PLANT");
            previewRows[0].OperatingHours.Should().Be("140.92");
            previewRows[0].FuelType.Should().Be(expectedFuelType);
            previewRows[1].RowNumber.Should().Be("3");
            previewRows[1].EquipmentTag.Should().Be("210398716");
            previewRows[1].EquipmentType.Should().Be("630-1280 CART UTV 4 PASSENGER DSL");
            previewRows[1].LocationText.Should().Be("AMAZON RIDGELAND");
            previewRows[1].OperatingHours.Should().Be("20.64");
            previewRows[1].FuelType.Should().Be(expectedFuelType);
            previewRows[2].RowNumber.Should().Be("4");
            previewRows[2].EquipmentTag.Should().Be("210438462");
            previewRows[2].EquipmentType.Should().Be("630-1260 4-Person Gas Utility Vehicle Rental");
            previewRows[2].LocationText.Should().Be("AWS 200");
            previewRows[2].OperatingHours.Should().Be("96.49");
            previewRows[2].FuelType.Should().Be(expectedFuelType);
            previewRows[3].RowNumber.Should().Be("5");
            previewRows[3].EquipmentTag.Should().Be("210441502");
            previewRows[3].EquipmentType.Should().Be("630-1260 4-Person Gas Utility Vehicle Rental");
            previewRows[3].LocationText.Should().Be("AWS 200");
            previewRows[3].OperatingHours.Should().Be("0");
            previewRows[3].FuelType.Should().Be(expectedFuelType);
            previewRows[4].RowNumber.Should().Be("6");
            previewRows[4].EquipmentTag.Should().Be("800213755");
            previewRows[4].EquipmentType.Should().Be("405-1085 SCISSOR LIFT 19FT 32IN COMPACT ELEC");
            previewRows[4].LocationText.Should().Be("CITY CREEK WATER PLANT");
            previewRows[4].OperatingHours.Should().Be("138.72");
            previewRows[4].FuelType.Should().Be(expectedFuelType);

            var telemetryPage = await importPage.ClickSaveBtnAsync();
            (await telemetryPage.GetTitleAsync()).Should().Be("Telemetry Upload");
            await telemetryPage.SetMonthAsync("2026-07");
            (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);

            var gridRows = await telemetryPage.GetGridRowsAsync();
            AssertJcbGridRow(gridRows, "210342678", "405-1087 SCISSOR LIFT 19FT 32IN COMPACT LCS ELEC",
                "6704204001 — City Creek Treatment Self Perf", "140.92", expectedFuelType, expectedImportDate, expectedSource);
            AssertJcbGridRow(gridRows, "210398716", "630-1280 CART UTV 4 PASSENGER DSL",
                "6000025001 — JAN200 - AWS Internal", "20.64", expectedFuelType, expectedImportDate, expectedSource);
            AssertJcbGridRow(gridRows, "210438462", "630-1260 4-Person Gas Utility Vehicle Rental",
                "6000025001 — JAN200 - AWS Internal", "96.49", expectedFuelType, expectedImportDate, expectedSource);
            AssertJcbGridRow(gridRows, "210441502", "630-1260 4-Person Gas Utility Vehicle Rental",
                "6000025001 — JAN200 - AWS Internal", "0", expectedFuelType, expectedImportDate, expectedSource);
            AssertJcbGridRow(gridRows, "800213755", "405-1085 SCISSOR LIFT 19FT 32IN COMPACT ELEC",
                "6704204001 — City Creek Treatment Self Perf", "138.72", expectedFuelType, expectedImportDate, expectedSource);
        }
        finally
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);
        }
    }

    private static void AssertJcbGridRow(
        IReadOnlyList<TelemetryGridRow> gridRows,
        string equipmentTag,
        string equipmentType,
        string location,
        string operatingHours,
        string fuelType,
        string importDate,
        string source)
    {
        var row = gridRows.FirstOrDefault(r => r.EquipmentTag == equipmentTag);
        row.Should().NotBeNull(
            $"Equipment tag '{equipmentTag}' was not found in the telemetry grid. Found: [{string.Join(", ", gridRows.Select(r => r.EquipmentTag))}]");
        row!.EquipmentType.Should().Be(equipmentType);
        row.Location.Should().Be(location);
        row.OperatingHours.Should().Be(operatingHours);
        row.FuelType.Should().Be(fuelType);
        row.ImportDate.Should().Be(importDate);
        row.Source.Should().Be(source);
    }

    [Test]
    public async Task T25_TelemetryImport_JCB_CancelImport()
    {
        const string fileName = "JCB_07_26_new_correct.xlsx";
        const string expectedMonthYear = "July 2026";
        const string expectedCompany = "JCB";
        const string expectedFuelType = "—";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
        (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);
                
        var previewRows = await importPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(5);
        previewRows[0].RowNumber.Should().Be("2");
        previewRows[0].EquipmentTag.Should().Be("210342678");
        previewRows[0].EquipmentType.Should().Be("405-1087 SCISSOR LIFT 19FT 32IN COMPACT LCS ELEC");
        previewRows[0].LocationText.Should().Be("CITY CREEK WATER PLANT");
        previewRows[0].OperatingHours.Should().Be("140.92");
        previewRows[0].FuelType.Should().Be(expectedFuelType);
        previewRows[1].RowNumber.Should().Be("3");
        previewRows[1].EquipmentTag.Should().Be("210398716");
        previewRows[1].EquipmentType.Should().Be("630-1280 CART UTV 4 PASSENGER DSL");
        previewRows[1].LocationText.Should().Be("AMAZON RIDGELAND");
        previewRows[1].OperatingHours.Should().Be("20.64");
        previewRows[1].FuelType.Should().Be(expectedFuelType);
        previewRows[2].RowNumber.Should().Be("4");
        previewRows[2].EquipmentTag.Should().Be("210438462");
        previewRows[2].EquipmentType.Should().Be("630-1260 4-Person Gas Utility Vehicle Rental");
        previewRows[2].LocationText.Should().Be("AWS 200");
        previewRows[2].OperatingHours.Should().Be("96.49");
        previewRows[2].FuelType.Should().Be(expectedFuelType);
        previewRows[3].RowNumber.Should().Be("5");
        previewRows[3].EquipmentTag.Should().Be("210441502");
        previewRows[3].EquipmentType.Should().Be("630-1260 4-Person Gas Utility Vehicle Rental");
        previewRows[3].LocationText.Should().Be("AWS 200");
        previewRows[3].OperatingHours.Should().Be("0");
        previewRows[3].FuelType.Should().Be(expectedFuelType);
        previewRows[4].RowNumber.Should().Be("6");
        previewRows[4].EquipmentTag.Should().Be("800213755");
        previewRows[4].EquipmentType.Should().Be("405-1085 SCISSOR LIFT 19FT 32IN COMPACT ELEC");
        previewRows[4].LocationText.Should().Be("CITY CREEK WATER PLANT");
        previewRows[4].OperatingHours.Should().Be("138.72");
        previewRows[4].FuelType.Should().Be(expectedFuelType);

        await importPage.ClickCancelBtnAsync();

        (await importPage.IsReportingMonthVisibleAsync()).Should().BeFalse();
        (await importPage.IsCompanyVisibleAsync()).Should().BeFalse();
        (await importPage.IsPreviewGridVisibleAsync()).Should().BeFalse();
        (await importPage.IsSelectFileSectionVisibleAsync()).Should().BeTrue();

        var telemetryPage = await importPage.ClickBackToTelemetryAsync();
        await telemetryPage.SetMonthAsync("2026-07");

        (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);
        (await telemetryPage.GetGridRowByEquipmentTagAsync("210342678")).Should().BeNull();
        (await telemetryPage.GetGridRowByEquipmentTagAsync("210398716")).Should().BeNull();
        (await telemetryPage.GetGridRowByEquipmentTagAsync("210438462")).Should().BeNull();
        (await telemetryPage.GetGridRowByEquipmentTagAsync("210441502")).Should().BeNull();
        (await telemetryPage.GetGridRowByEquipmentTagAsync("800213755")).Should().BeNull();
    }

    [Test]
    public async Task T26_TelemetryImport_JCB_EmptyFields()
    {
        const string fileName = "JCB_07_26_new_correct.xlsx";
        const string expectedMonthYear = "July 2026";
        const string expectedCompany = "JCB";
        const string expectedMonthYearError = "Month/Year is required.";
        const string expectedCompanyError = "Company is required.";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        await importPage.ClearReportingMonthAsync();
        await importPage.ClearCompanyAsync();
        await importPage.ClickSaveExpectingValidationAsync();

        (await importPage.GetMonthYearErrorAsync()).Should().Be(expectedMonthYearError);
        (await importPage.GetCompanyErrorAsync()).Should().Be(expectedCompanyError);
    }

    [Test]
    public async Task T27_TelemetryImport_JCB_MissingValues()
    {
        const string fileName = "JCB_07_26_new_missing_values.xlsx";
        const string expectedFuelType = "—";
        const string expectedValidationLead =
            "They are omitted from the table. Required columns: IC, Cat-Class and Description, and Job Name.";
        var expectedValidationList = """
            Row 3: IC is required.
            IC: —
            Cat-Class and Description: 630-1280 CART UTV 4 PASSENGER DSL
            Job Name: AMAZON RIDGELAND
            Current Hours Meter: 20.64
            Row 6: Job Name is required.
            IC: 800213755
            Cat-Class and Description: 405-1085 SCISSOR LIFT 19FT 32IN COMPACT ELEC
            Job Name: —
            Current Hours Meter: 138.72
            Row 11: IC is required. At least one of Cat-Class and Description should have value.
            IC: —
            Cat-Class and Description: —
            Job Name: CITY CREEK WATER PLANT
            Current Hours Meter: 799.38
            Row 12: At least one of Cat-Class and Description should have value.
            IC: 800474744
            Cat-Class and Description: —
            Job Name: AWS 200
            Current Hours Meter: 683.1
            Row 13: IC is required. Job Name is required.
            IC: —
            Cat-Class and Description: 410-2020 BOOM ARTICULATED 30FT NAR ROT JIB ELEC
            Job Name: —
            Current Hours Meter: 10574.55
            Row 14: At least one of Cat-Class and Description should have value. Job Name is required.
            IC: 800481172
            Cat-Class and Description: —
            Job Name: —
            Current Hours Meter: 268.75
            """.Replace("\r\n", "\n").Trim();

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        var previewRows = await importPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(6);
        previewRows[0].RowNumber.Should().Be("2");
        previewRows[0].EquipmentTag.Should().Be("210342678");
        previewRows[0].EquipmentType.Should().Be("405-1087 SCISSOR LIFT 19FT 32IN COMPACT LCS ELEC");
        previewRows[0].LocationText.Should().Be("CITY CREEK WATER PLANT");
        previewRows[0].OperatingHours.Should().Be("140.92");
        previewRows[0].FuelType.Should().Be(expectedFuelType);
        previewRows[1].RowNumber.Should().Be("4");
        previewRows[1].EquipmentTag.Should().Be("210438462");
        previewRows[1].EquipmentType.Should().Be("4-Person Gas Utility Vehicle Rental");
        previewRows[1].LocationText.Should().Be("AWS 200");
        previewRows[1].OperatingHours.Should().Be("96.49");
        previewRows[1].FuelType.Should().Be(expectedFuelType);
        previewRows[2].RowNumber.Should().Be("5");
        previewRows[2].EquipmentTag.Should().Be("210441502");
        previewRows[2].EquipmentType.Should().Be("630-1260");
        previewRows[2].LocationText.Should().Be("AWS 200");
        previewRows[2].OperatingHours.Should().Be("0");
        previewRows[2].FuelType.Should().Be(expectedFuelType);
        previewRows[3].RowNumber.Should().Be("7");
        previewRows[3].EquipmentTag.Should().Be("800395291");
        previewRows[3].EquipmentType.Should().Be("460-1060 TELEHANDLER 10000LB 42-44FT LIFT ROPS");
        previewRows[3].LocationText.Should().Be("PERRY WASTEWATER PLANT");
        previewRows[3].OperatingHours.Should().Be(expectedFuelType);
        previewRows[3].FuelType.Should().Be(expectedFuelType);
        previewRows[4].RowNumber.Should().Be("9");
        previewRows[4].EquipmentTag.Should().Be("800443474");
        previewRows[4].EquipmentType.Should().Be("100KW GENERATOR DSL");
        previewRows[4].LocationText.Should().Be("PROJECT POWEHOUSE WAREHOUSE");
        previewRows[4].OperatingHours.Should().Be("7,350.85");
        previewRows[4].FuelType.Should().Be(expectedFuelType);
        previewRows[5].RowNumber.Should().Be("10");
        previewRows[5].EquipmentTag.Should().Be("800451697");
        previewRows[5].EquipmentType.Should().Be("630-1280");
        previewRows[5].LocationText.Should().Be("AWS 200");
        previewRows[5].OperatingHours.Should().Be("458.73");
        previewRows[5].FuelType.Should().Be(expectedFuelType);

        (await importPage.IsUnrecognizedRowsAlertVisibleAsync()).Should().BeTrue();
        (await importPage.GetValidationLeadAsync()).Should().Be(expectedValidationLead);
        (await importPage.GetValidationListTextAsync()).Should().Be(expectedValidationList);
    }

    [Test]
    public async Task T28_TelemetryImport_Custom_OnlyHeaders()
    {
        await AssertImportAlertAsync(
            "Custom_08_26_OnlyHeaders.xlsx",
            "Excel file must contain at least one telemetry row.");
    }

    [Test]
    public async Task T29_TelemetryImport_Custom_NoEquipmentTag()
    {
        await AssertImportAlertAsync(
            "Custom_08_26_no_EquipmentTag.xlsx",
            "Missing required column(s): Equipment Tag.");
    }

    [Test]
    public async Task T30_TelemetryImport_Custom_NoEquipmentType()
    {
        await AssertImportAlertAsync(
            "Custom_08_26_no_EquipmentType.xlsx",
            "Missing required column(s): Equipment Type.");
    }

    [Test]
    public async Task T31_TelemetryImport_Custom_NoLocation()
    {
        await AssertImportAlertAsync(
            "Custom_08_26_no_Location.xlsx",
            "Missing required column(s): Location.");
    }

    [Test]
    public async Task T32_TelemetryImport_Custom_NoMultipleColumns()
    {
        await AssertImportAlertAsync(
            "Custom_08_26_no_MultipleColumns.xlsx",
            "Missing required column(s): Cat-Class, Description, Job Name.");
    }

    [Test]
    public async Task T33_TelemetryImport_Custom_NoHours()
    {
        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync("Custom_08_26_no_Hours.xlsx");
        await importPage.ClickImportBtnAsync();

        (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T34_TelemetryImport_Custom_NoFuelType()
    {
        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync("Custom_08_26_no_FuelType.xlsx");
        await importPage.ClickImportBtnAsync();

        (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T35_TelemetryImport_Custom_NoMonthYear()
    {
        await AssertImportAlertAsync(
            "Custom_no_MonthYear.xlsx",
            "File name must match 'CompanyName_Month_Year.xlsx' (example: Herc_07_26.xlsx).");
    }

    [Test]
    public async Task T36_TelemetryImport_Custom_SuccessImport()
    {
        const string fileName = "Custom_08_26.xlsx";
        const string expectedMonthYear = "August 2026";
        const string expectedCompany = "Custom";
        const string expectedSource = "Custom";
        var expectedImportDate = DateTime.Now.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

        try
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

            var importPage = new ImportPage(Fixture.Page);
            await importPage.OpenTelemetryAsync();
            await importPage.UploadFileAsync(fileName);
            await importPage.ClickImportBtnAsync();

            (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
            (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
            (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

            var previewRows = await importPage.GetPreviewRowsAsync();
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

            var telemetryPage = await importPage.ClickSaveBtnAsync();
            (await telemetryPage.GetTitleAsync()).Should().Be("Telemetry Upload");
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
    public async Task T37_TelemetryImport_Custom_CancelImport()
    {
        const string fileName = "Custom_08_26.xlsx";
        const string expectedMonthYear = "August 2026";
        const string expectedCompany = "Custom";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
        (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        var previewRows = await importPage.GetPreviewRowsAsync();
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

        await importPage.ClickCancelBtnAsync();

        (await importPage.IsReportingMonthVisibleAsync()).Should().BeFalse();
        (await importPage.IsCompanyVisibleAsync()).Should().BeFalse();
        (await importPage.IsPreviewGridVisibleAsync()).Should().BeFalse();
        (await importPage.IsSelectFileSectionVisibleAsync()).Should().BeTrue();

        var telemetryPage = await importPage.ClickBackToTelemetryAsync();
        await telemetryPage.SetMonthAsync("2026-08");

        (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);
        (await telemetryPage.GetGridRowByEquipmentTagAsync("111-2221")).Should().BeNull();
        (await telemetryPage.GetGridRowByEquipmentTagAsync("111-2222")).Should().BeNull();
    }

    [Test]
    public async Task T38_TelemetryImport_Custom_EmptyFields()
    {
        const string fileName = "Custom_08_26.xlsx";
        const string expectedMonthYear = "August 2026";
        const string expectedCompany = "Custom";
        const string expectedMonthYearError = "Month/Year is required.";
        const string expectedCompanyError = "Company is required.";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        await importPage.ClearReportingMonthAsync();
        await importPage.ClearCompanyAsync();
        await importPage.ClickSaveExpectingValidationAsync();

        (await importPage.GetMonthYearErrorAsync()).Should().Be(expectedMonthYearError);
        (await importPage.GetCompanyErrorAsync()).Should().Be(expectedCompanyError);
    }

    [Test]
    public async Task T39_TelemetryImport_Custom_MissingValues()
    {
        const string fileName = "Custom_08_26_MissingValues.xlsx";
        const string expectedFuelType = "—";
        const string expectedValidationLead =
            "They are omitted from the table. Required columns: Equipment Tag, Equipment Type, and Location.";
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
            Row 8: Equipment Tag is required. Equipment Type is required.
            Equipment Tag: —
            Equipment Type: —
            Location: BOEING SOUTH
            Operating Hours: 22.52
            Fuel Type: P
            Row 9: Equipment Type is required. Location is required.
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

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        var previewRows = await importPage.GetPreviewRowsAsync();
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

        (await importPage.IsUnrecognizedRowsAlertVisibleAsync()).Should().BeTrue();
        (await importPage.GetValidationLeadAsync()).Should().Be(expectedValidationLead);
        (await importPage.GetValidationListTextAsync()).Should().Be(expectedValidationList);
    }

    [Test]
    public async Task T40_TelemetryImport_Custom_ChangeMonthYearLocation()
    {
        const string fileName = "Custom_08_26.xlsx";
        const string expectedCompany = "Custom";
        const string initialMonthYear = "August 2026";
        const string changedMonthYear = "June 2025";
        const string changedMonthValue = "2025-06";

        try
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

            var importPage = new ImportPage(Fixture.Page);
            await importPage.OpenTelemetryAsync();
            await importPage.UploadFileAsync(fileName);
            await importPage.ClickImportBtnAsync();

            (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
            (await importPage.GetReportingMonthValueAsync()).Should().Be(initialMonthYear);
            (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

            await importPage.SetReportingMonthAsync(changedMonthValue);
            (await importPage.GetReportingMonthValueAsync()).Should().Be(changedMonthYear);

            var selectedLocation = await importPage.SelectRandomDifferentLocationForRowAsync(0);

            var telemetryPage = await importPage.ClickSaveBtnAsync();
            (await telemetryPage.GetTitleAsync()).Should().Be("Telemetry Upload");

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
    public async Task T41_TelemetryImport_Custom_UnknownProject()
    {
        const string fileName = "Custom_08_26_UnknownProject.xlsx";
        const string expectedAlert =
            "They'll be saved under the corporate fallback location. Choose a location from the dropdown, or add a project alias to attribute them automatically.";
        const string expectedLocation = "000000 — Haskell";

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        (await importPage.GetAlertBodyTextAsync()).Should().Be(expectedAlert);

        var previewRows = await importPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(2);
        previewRows[0].LocationText.Should().Be(expectedLocation);
        previewRows[1].LocationText.Should().Be(expectedLocation);
    }

    [Test]
    public async Task T42_TelemetryImport_United_OnlyHeaders()
    {
        await AssertImportAlertAsync(
            "United_7_26_new_only_headers.xlsx",
            "Excel file must contain at least one telemetry row.");
    }

    [Test]
    public async Task T43_TelemetryImport_United_NoCategory()
    {
        await AssertImportAlertAsync(
            "United_7_26_new_no_Category.xlsx",
            "Missing required column(s): EquipmentCategory.");
    }

    [Test]
    public async Task T44_TelemetryImport_United_NoClass()
    {
        await AssertImportAlertAsync(
            "United_7_26_new_no_Class.xlsx",
            "Missing required column(s): EquipmentClass.");
    }

    [Test]
    public async Task T45_TelemetryImport_United_NoDescription()
    {
        await AssertImportAlertAsync(
            "United_7_26_new_no_Description.xlsx",
            "Missing required column(s): EqpDescription.");
    }

    [Test]
    public async Task T46_TelemetryImport_United_NoJobName()
    {
        await AssertImportAlertAsync(
            "United_7_26_new_no_JobName.xlsx",
            "Missing required column(s): JobName.");
    }

    [Test]
    public async Task T47_TelemetryImport_United_NoEquipmentNumber()
    {
        await AssertImportAlertAsync(
            "United_7_26_new_no_EquipmentNumber.xlsx",
            "Missing required column(s): EquipmentNumber.");
    }

    [Test]
    public async Task T48_TelemetryImport_United_NoMultipleColumns()
    {
        await AssertImportAlertAsync(
            "United_7_26_new_no_multiple_columns.xlsx",
            "Missing required column(s): EquipmentCategory, EquipmentClass, EqpDescription, JobName.");
    }

    [Test]
    public async Task T49_TelemetryImport_United_NoHours()
    {
        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync("United_7_26_new_no_Hours.xlsx");
        await importPage.ClickImportBtnAsync();

        (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T50_TelemetryImport_United_NoMonthYear()
    {
        const string expectedMonthYear = "--------- ----";

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync("United_new_no_MonthYear.xlsx");
        await importPage.ClickImportBtnAsync();

        (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
        (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
    }

    [Test]
    public async Task T51_TelemetryImport_United_SuccessImport()
    {
        const string fileName = "United_7_26_new_correct.xlsx";
        const string expectedMonthYear = "July 2026";
        const string expectedCompany = "United";
        const string expectedFuelType = "—";
        const string expectedSource = "United";
        var expectedImportDate = DateTime.Now.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

        try
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

            var importPage = new ImportPage(Fixture.Page);
            await importPage.OpenTelemetryAsync();
            await importPage.UploadFileAsync(fileName);
            await importPage.ClickImportBtnAsync();

            (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
            (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
            (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

            var previewRows = await importPage.GetPreviewRowsAsync();
            previewRows.Should().HaveCount(5);
            previewRows[0].RowNumber.Should().Be("2");
            previewRows[0].EquipmentTag.Should().Be("12302337");
            previewRows[0].EquipmentType.Should().Be("233-1650 FORKLIFT VARIABLE REACH 10000# 50'-62'");
            previewRows[0].LocationText.Should().Be("NEW PERRY WASTEWATER");
            previewRows[0].OperatingHours.Should().Be("84.353");
            previewRows[0].FuelType.Should().Be(expectedFuelType);
            previewRows[1].RowNumber.Should().Be("3");
            previewRows[1].EquipmentTag.Should().Be("3750525");
            previewRows[1].EquipmentType.Should().Be("375-525 FENCE MODULAR 12' L X 6' H TEMPORARY PAN");
            previewRows[1].LocationText.Should().Be("000000 — Haskell");
            previewRows[1].OperatingHours.Should().Be("0");
            previewRows[1].FuelType.Should().Be(expectedFuelType);
            previewRows[2].RowNumber.Should().Be("4");
            previewRows[2].EquipmentTag.Should().Be("5514405");
            previewRows[2].EquipmentType.Should().Be("551-4405 FENCE PANEL METAL BASES");
            previewRows[2].LocationText.Should().Be("000000 — Haskell");
            previewRows[2].OperatingHours.Should().Be("0");
            previewRows[2].FuelType.Should().Be(expectedFuelType);
            previewRows[3].RowNumber.Should().Be("5");
            previewRows[3].EquipmentTag.Should().Be("3750414");
            previewRows[3].EquipmentType.Should().Be("375-414 FENCE GATE WHEEL");
            previewRows[3].LocationText.Should().Be("000000 — Haskell");
            previewRows[3].OperatingHours.Should().Be("0");
            previewRows[3].FuelType.Should().Be(expectedFuelType);
            previewRows[4].RowNumber.Should().Be("6");
            previewRows[4].EquipmentTag.Should().Be("5514047");
            previewRows[4].EquipmentType.Should().Be("551-4047 FENCE PANEL BASE WEIGHT");
            previewRows[4].LocationText.Should().Be("000000 — Haskell");
            previewRows[4].OperatingHours.Should().Be("0");
            previewRows[4].FuelType.Should().Be(expectedFuelType);

            (await importPage.IsSaveBtnVisibleAsync()).Should().BeTrue();
            (await importPage.IsSaveBtnEnabledAsync()).Should().BeTrue();

            var telemetryPage = await importPage.ClickSaveBtnAsync();
            (await telemetryPage.GetTitleAsync()).Should().Be("Telemetry Upload");
            await telemetryPage.SetMonthAsync("2026-07");
            (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);

            var gridRows = await telemetryPage.GetGridRowsAsync();
            var row1 = gridRows.FirstOrDefault(r => r.EquipmentTag == "12302337");
            var row2 = gridRows.FirstOrDefault(r => r.EquipmentTag == "3750525");
            var row3 = gridRows.FirstOrDefault(r => r.EquipmentTag == "5514405");
            var row4 = gridRows.FirstOrDefault(r => r.EquipmentTag == "3750414");
            var row5 = gridRows.FirstOrDefault(r => r.EquipmentTag == "5514047");
            row1.Should().NotBeNull();
            row2.Should().NotBeNull();
            row3.Should().NotBeNull();
            row4.Should().NotBeNull();
            row5.Should().NotBeNull();
            row1!.EquipmentType.Should().Be("233-1650 FORKLIFT VARIABLE REACH 10000# 50'-62'");
            row1.Location.Should().Be("6704178001 — Perry East WWTP Upgrades & Ex");
            row1.OperatingHours.Should().Be("84.353");
            row1.FuelType.Should().Be(expectedFuelType);
            row1.ImportDate.Should().Be(expectedImportDate);
            row1.Source.Should().Be(expectedSource);
            row2!.EquipmentType.Should().Be("375-525 FENCE MODULAR 12' L X 6' H TEMPORARY PAN");
            row2.Location.Should().Be("000000 — Haskell");
            row2.OperatingHours.Should().Be("0");
            row2.FuelType.Should().Be(expectedFuelType);
            row2.ImportDate.Should().Be(expectedImportDate);
            row2.Source.Should().Be(expectedSource);
            row3!.EquipmentType.Should().Be("551-4405 FENCE PANEL METAL BASES");
            row3.Location.Should().Be("000000 — Haskell");
            row3.OperatingHours.Should().Be("0");
            row3.FuelType.Should().Be(expectedFuelType);
            row3.ImportDate.Should().Be(expectedImportDate);
            row3.Source.Should().Be(expectedSource);
            row4!.EquipmentType.Should().Be("375-414 FENCE GATE WHEEL");
            row4.Location.Should().Be("000000 — Haskell");
            row4.OperatingHours.Should().Be("0");
            row4.FuelType.Should().Be(expectedFuelType);
            row4.ImportDate.Should().Be(expectedImportDate);
            row4.Source.Should().Be(expectedSource);
            row5!.EquipmentType.Should().Be("551-4047 FENCE PANEL BASE WEIGHT");
            row5.Location.Should().Be("000000 — Haskell");
            row5.OperatingHours.Should().Be("0");
            row5.FuelType.Should().Be(expectedFuelType);
            row5.ImportDate.Should().Be(expectedImportDate);
            row5.Source.Should().Be(expectedSource);
        }
        finally
        {
            await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);
        }
    }

    [Test]
    public async Task T52_TelemetryImport_United_CancelImport()
    {
        const string fileName = "United_7_26_new_correct.xlsx";
        const string expectedMonthYear = "July 2026";
        const string expectedCompany = "United";
        const string expectedFuelType = "—";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        (await importPage.IsPreviewTableVisibleAsync()).Should().BeTrue();
        (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        var previewRows = await importPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(5);
        previewRows[0].RowNumber.Should().Be("2");
        previewRows[0].EquipmentTag.Should().Be("12302337");
        previewRows[0].EquipmentType.Should().Be("233-1650 FORKLIFT VARIABLE REACH 10000# 50'-62'");
        previewRows[0].LocationText.Should().Be("NEW PERRY WASTEWATER");
        previewRows[0].OperatingHours.Should().Be("84.353");
        previewRows[0].FuelType.Should().Be(expectedFuelType);
        previewRows[1].RowNumber.Should().Be("3");
        previewRows[1].EquipmentTag.Should().Be("3750525");
        previewRows[1].EquipmentType.Should().Be("375-525 FENCE MODULAR 12' L X 6' H TEMPORARY PAN");
        previewRows[1].LocationText.Should().Be("000000 — Haskell");
        previewRows[1].OperatingHours.Should().Be("0");
        previewRows[1].FuelType.Should().Be(expectedFuelType);

        await importPage.ClickCancelBtnAsync();

        (await importPage.IsReportingMonthVisibleAsync()).Should().BeFalse();
        (await importPage.IsCompanyVisibleAsync()).Should().BeFalse();
        (await importPage.IsPreviewGridVisibleAsync()).Should().BeFalse();
        (await importPage.IsSelectFileSectionVisibleAsync()).Should().BeTrue();

        var telemetryPage = await importPage.ClickBackToTelemetryAsync();
        await telemetryPage.SetMonthAsync("2026-07");

        (await telemetryPage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedMonthYear);
        (await telemetryPage.GetGridRowByEquipmentTagAsync("12302337")).Should().BeNull();
        (await telemetryPage.GetGridRowByEquipmentTagAsync("3750525")).Should().BeNull();
    }

    [Test]
    public async Task T53_TelemetryImport_United_EmptyFields()
    {
        const string fileName = "United_7_26_new_correct.xlsx";
        const string expectedMonthYear = "July 2026";
        const string expectedCompany = "United";
        const string expectedMonthYearError = "Month/Year is required.";
        const string expectedCompanyError = "Company is required.";

        await SqlHelper.DeleteTelemetryByExternalReferenceAsync(fileName);

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        (await importPage.GetReportingMonthValueAsync()).Should().Be(expectedMonthYear);
        (await importPage.GetCompanyValueAsync()).Should().Be(expectedCompany);

        await importPage.ClearReportingMonthAsync();
        await importPage.ClearCompanyAsync();
        await importPage.ClickSaveExpectingValidationAsync();

        (await importPage.GetMonthYearErrorAsync()).Should().Be(expectedMonthYearError);
        (await importPage.GetCompanyErrorAsync()).Should().Be(expectedCompanyError);
    }

    [Test]
    public async Task T54_TelemetryImport_United_MissingValues()
    {
        const string fileName = "United_7_26_new_missing_values.xlsx";
        const string expectedFuelType = "—";
        const string expectedValidationLead =
            "They are omitted from the table. Required columns: EquipmentNumber, EquipmentCategory, EquipmentClass, and EqpDescription, and JobName.";
        var expectedValidationList = """
            Row 3: EquipmentNumber is required.
            EquipmentNumber: —
            EquipmentCategory, EquipmentClass, and EqpDescription: 375-525 FENCE MODULAR 12' L X 6' H TEMPORARY PAN
            JobName: TAMPA PREP
            MeterReadingOut: 0
            Row 8: JobName is required.
            EquipmentNumber: 10497746
            EquipmentCategory, EquipmentClass, and EqpDescription: 310-4001 BOOM 40-50' ARTICULATING
            JobName: —
            MeterReadingOut: 1693.05
            Row 13: EquipmentNumber is required.
            EquipmentNumber: —
            EquipmentCategory, EquipmentClass, and EqpDescription: 233 FORKLIFT VARIABLE REACH 8000# 40-49'
            JobName: RCCB
            MeterReadingOut: 10.3
            Row 14: At least one of EquipmentCategory, EquipmentClass, and EqpDescription should have value.
            EquipmentNumber: 11126765
            EquipmentCategory, EquipmentClass, and EqpDescription: —
            JobName: PROJECT WATERFALL
            MeterReadingOut: 1833
            Row 15: EquipmentNumber is required.
            EquipmentNumber: —
            EquipmentCategory, EquipmentClass, and EqpDescription: 545-250 2.5 X 50 FIREHOSE
            JobName: PROJECT WATERFALL
            MeterReadingOut: —
            Row 16: EquipmentNumber is required. JobName is required.
            EquipmentNumber: —
            EquipmentCategory, EquipmentClass, and EqpDescription: 240-3412 GENERATOR 600-699 KVA TIER 4
            JobName: —
            MeterReadingOut: 5731.2
            """.Replace("\r\n", "\n").Trim();

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        var previewRows = await importPage.GetPreviewRowsAsync();
        previewRows.Should().HaveCount(10);
        previewRows[0].RowNumber.Should().Be("2");
        previewRows[0].EquipmentTag.Should().Be("12302337");
        previewRows[0].EquipmentType.Should().Be("233-1650 FORKLIFT VARIABLE REACH 10000# 50'-62'");
        previewRows[0].LocationText.Should().Be("NEW PERRY WASTEWATER");
        previewRows[0].OperatingHours.Should().Be("84.353");
        previewRows[0].FuelType.Should().Be(expectedFuelType);
        previewRows[1].RowNumber.Should().Be("4");
        previewRows[1].EquipmentTag.Should().Be("5514405");
        previewRows[1].EquipmentType.Should().Be("4405 FENCE PANEL METAL BASES");
        previewRows[1].LocationText.Should().Be("000000 — Haskell");
        previewRows[1].OperatingHours.Should().Be("0");
        previewRows[1].FuelType.Should().Be(expectedFuelType);
        previewRows[2].RowNumber.Should().Be("5");
        previewRows[2].EquipmentTag.Should().Be("3750414");
        previewRows[2].EquipmentType.Should().Be("375 FENCE GATE WHEEL");
        previewRows[2].LocationText.Should().Be("000000 — Haskell");
        previewRows[2].OperatingHours.Should().Be("0");
        previewRows[2].FuelType.Should().Be(expectedFuelType);
        previewRows[3].RowNumber.Should().Be("6");
        previewRows[3].EquipmentTag.Should().Be("5514047");
        previewRows[3].EquipmentType.Should().Be("551-4047");
        previewRows[3].LocationText.Should().Be("000000 — Haskell");
        previewRows[3].OperatingHours.Should().Be("0");
        previewRows[3].FuelType.Should().Be(expectedFuelType);
        previewRows[4].RowNumber.Should().Be("7");
        previewRows[4].EquipmentTag.Should().Be("1274HR0485");
        previewRows[4].EquipmentType.Should().Be("310-4026 BOOM 37-44' TELESCOPIC");
        previewRows[4].LocationText.Should().Be("BOEING SOUTH YARD");
        previewRows[4].OperatingHours.Should().Be(expectedFuelType);
        previewRows[4].FuelType.Should().Be(expectedFuelType);
        previewRows[5].RowNumber.Should().Be("10");
        previewRows[5].EquipmentTag.Should().Be("11899173");
        previewRows[5].EquipmentType.Should().Be("FORKLIFT VARIABLE REACH 12000# 53'-69'");
        previewRows[5].LocationText.Should().Be("PROJECT WATERFALL");
        previewRows[5].OperatingHours.Should().Be("812.3");
        previewRows[5].FuelType.Should().Be(expectedFuelType);
        previewRows[6].RowNumber.Should().Be("11");
        previewRows[6].EquipmentTag.Should().Be("3750525");
        previewRows[6].EquipmentType.Should().Be("375");
        previewRows[6].LocationText.Should().Be("000000 — Haskell");
        previewRows[6].OperatingHours.Should().Be("0");
        previewRows[6].FuelType.Should().Be(expectedFuelType);
        previewRows[7].RowNumber.Should().Be("12");
        previewRows[7].EquipmentTag.Should().Be("3750423");
        previewRows[7].EquipmentType.Should().Be("423");
        previewRows[7].LocationText.Should().Be("000000 — Haskell");
        previewRows[7].OperatingHours.Should().Be("0");
        previewRows[7].FuelType.Should().Be(expectedFuelType);
        previewRows[8].RowNumber.Should().Be("17");
        previewRows[8].EquipmentTag.Should().Be("11940897");
        previewRows[8].EquipmentType.Should().Be("240-3303 GENERATOR 250-299 KVA TIER 4");
        previewRows[8].LocationText.Should().Be("AWS RIDGELAND");
        previewRows[8].OperatingHours.Should().Be("4,793");
        previewRows[8].FuelType.Should().Be(expectedFuelType);
        previewRows[9].RowNumber.Should().Be("18");
        previewRows[9].EquipmentTag.Should().Be("11712305");
        previewRows[9].EquipmentType.Should().Be("241-4915 1200 AMP 45\" MULTI PANEL");
        previewRows[9].LocationText.Should().Be("AWS RIDGELAND");
        previewRows[9].OperatingHours.Should().Be("0");
        previewRows[9].FuelType.Should().Be(expectedFuelType);

        (await importPage.IsUnrecognizedRowsAlertVisibleAsync()).Should().BeTrue();
        (await importPage.GetValidationLeadAsync()).Should().Be(expectedValidationLead);
        (await importPage.GetValidationListTextAsync()).Should().Be(expectedValidationList);
    }

    private async Task AssertImportAlertAsync(string fileName, string expectedAlertMessage)
    {
        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenTelemetryAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        (await importPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }
}
