using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class WasteImportTests : BaseTest
{
    [Test]
    public async Task T01_WasteImport_DefaultView()
    {
        const string expectedTitle = "Import Waste Report";
        const string expectedMessage =
            "Upload an Excel (.xlsx or .xls) file. Company is read from cell A1 (you can edit it after preview). Column headers must be on row 6. Review the recognized rows and Company before saving. Reporting month on each row comes from that row's haul date.";

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenWasteAsync();

        (await importPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await importPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await importPage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await importPage.IsExcelFileLabelVisibleAsync()).Should().BeTrue();
        (await importPage.IsSelectFileSectionVisibleAsync()).Should().BeTrue();
        (await importPage.IsImportBtnVisibleAsync()).Should().BeTrue();
        (await importPage.IsImportBtnEnabledAsync()).Should().BeFalse();
    }

    [Test]
    public async Task T02_WasteImport_ClickBackBtn()
    {
        const string expectedTitle = "Waste Report Upload";

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenWasteAsync();
        var wastePage = await importPage.ClickBackToWasteReportsAsync();

        (await wastePage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_WasteImport_IncorrectFormat()
    {
        await AssertImportAlertAsync("SC_Fuels_3.pdf", "Only .xlsx or .xls files are accepted.");
    }

    [Test]
    public async Task T04_WasteImport_IncorrectRow()
    {
        await AssertImportAlertAsync(
            @"Waste\Republic_incorrect_row.xlsx",
            "Missing required column(s): Ticket Number, Container ID, Site Name, Site Address, Haul Date, Tonnage, Assigned Waste Type.");
    }

    [Test]
    public async Task T05_WasteImport_OnlyHeaders()
    {
        await AssertImportAlertAsync(@"Waste\Republic_only_headers.xlsx", "Excel file must contain at least one waste-report row.");
    }

    [Test]
    public async Task T06_WasteImport_NoContainerId()
    {
        await AssertImportAlertAsync(@"Waste\Republic_no_Container_ID.xlsx", "Missing required column(s): Container ID.");
    }

    [Test]
    public async Task T07_WasteImport_NoHaulDate()
    {
        await AssertImportAlertAsync(@"Waste\Republic_no_Haul_Date.xlsx", "Missing required column(s): Haul Date.");
    }

    [Test]
    public async Task T08_WasteImport_NoSiteAddress()
    {
        await AssertImportAlertAsync(@"Waste\Republic_no_Site_Address.xlsx", "Missing required column(s): Site Address.");
    }

    [Test]
    public async Task T09_WasteImport_NoSiteName()
    {
        await AssertImportAlertAsync(@"Waste\Republic_no_Site_Name.xlsx", "Missing required column(s): Site Name.");
    }

    [Test]
    public async Task T10_WasteImport_NoSiteNameTonnage()
    {
        await AssertImportAlertAsync(@"Waste\Republic_no_SiteName_Tonnage.xlsx", "Missing required column(s): Site Name, Tonnage.");
    }

    [Test]
    public async Task T11_WasteImport_NoTicketNumber()
    {
        await AssertImportAlertAsync(@"Waste\Republic_no_TicketNumber.xlsx", "Missing required column(s): Ticket Number.");
    }

    [Test]
    public async Task T12_WasteImport_NoTonnage()
    {
        await AssertImportAlertAsync(@"Waste\Republic_no_Tonnage.xlsx", "Missing required column(s): Tonnage.");
    }

    [Test]
    public async Task T13_WasteImport_NoAssignedWasteType()
    {
        await AssertImportAlertAsync(@"Waste\Republic_no_AssignedWasteType.xlsx", "Missing required column(s): Assigned Waste Type.");
    }

    [Test]
    public async Task T14_WasteImport_NoCompanyName()
    {
        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenWasteAsync();
        await importPage.UploadFileAsync(@"Waste\Republic_no_CompanyName.xlsx");
        await importPage.ClickImportBtnAsync();
        await importPage.TouchWasteCompanyAsync();

        (await importPage.GetWasteCompanyErrorAsync()).Should().Be("Company is required.");
    }

    [Test]
    public async Task T15_WasteImport_NoMultipleValues()
    {
        const string expectedLead =
            "They are omitted from the table. Required columns: Ticket Number, Container ID, Site Name, Haul Date, Tonnage, and Assigned Waste Type.";
        const string expectedList =
            """
            Row 7: Container ID is required.
            Ticket Number: 1000001
            Container ID: —
            Site Name: BLUE DIAMOND (WASTE)
            Haul Date: 08/06/2026
            Tonnage: 2.13
            Assigned Waste Type: Solid Waste
            Row 8: Site Name is required.
            Ticket Number: 1000002
            Container ID: 97500514250000201
            Site Name: —
            Haul Date: 08/13/2026
            Tonnage: 3.71
            Assigned Waste Type: Solid Waste
            Row 10: Haul Date is required.
            Ticket Number: 1000004
            Container ID: 97500514250000201
            Site Name: BLUE DIAMOND (WASTE)
            Haul Date: —
            Tonnage: 1.35
            Assigned Waste Type: Solid Waste
            Row 11: Tonnage is required.
            Ticket Number: 1000005
            Container ID: 97500514250000201
            Site Name: BLUE DIAMOND (WASTE)
            Haul Date: 08/26/2026
            Tonnage: —
            Assigned Waste Type: Solid Waste
            Row 12: Assigned Waste Type is required.
            Ticket Number: 1000006
            Container ID: 97500514250000201
            Site Name: BLUE DIAMOND (WASTE)
            Haul Date: 08/27/2026
            Tonnage: 1.14
            Assigned Waste Type: —
            Row 13: Ticket Number is required.
            Ticket Number: —
            Container ID: 97500514250000201
            Site Name: BLUE DIAMOND (WASTE)
            Haul Date: 09/01/2026
            Tonnage: 0.88
            Assigned Waste Type: Solid Waste
            Row 14: Ticket Number is required; Container ID is required; Site Name is required; Assigned Waste Type is required; Haul Date is required; Tonnage is required.
            Ticket Number: —
            Container ID: —
            Site Name: —
            Haul Date: —
            Tonnage: —
            Assigned Waste Type: —
            Row 15: Container ID is required; Site Name is required.
            Ticket Number: 1000007
            Container ID: —
            Site Name: —
            Haul Date: 09/16/2026
            Tonnage: 0.17
            Assigned Waste Type: Solid Waste
            Row 16: Haul Date is required; Tonnage is required.
            Ticket Number: 1000008
            Container ID: 97500514250000201
            Site Name: BLUE DIAMOND (WASTE)
            Haul Date: —
            Tonnage: —
            Assigned Waste Type: Solid Waste
            Row 17: Ticket Number is required; Assigned Waste Type is required; Tonnage is required.
            Ticket Number: —
            Container ID: 97500514250000201
            Site Name: BLUE DIAMOND (WASTE)
            Haul Date: 09/16/2026
            Tonnage: —
            Assigned Waste Type: —
            Row 18: Site Name is required; Haul Date is required.
            Ticket Number: 1000009
            Container ID: 97500514250000401
            Site Name: —
            Haul Date: —
            Tonnage: 1.88
            Assigned Waste Type: Solid Waste
            """;

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenWasteAsync();
        await importPage.UploadFileAsync(@"Waste\Republic_no_multiple_values.xlsx");
        await importPage.ClickImportBtnAsync();

        (await importPage.GetWasteValidationLeadAsync()).Should().Be(expectedLead);
        (await importPage.GetWasteValidationListTextAsync()).Should().Be(expectedList.Replace("\r\n", "\n").Trim());
    }

    [Test]
    public async Task T16_WasteImport_IncorrectEmissionType()
    {
        const string expectedNotice =
            """
            1 row still need an emission type before you can save.
            Choose an emission type from the dropdown for each row before saving.
            """;

        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenWasteAsync();
        await importPage.UploadFileAsync("Republic_IncorrectEmissionType.xlsx");
        await importPage.ClickImportBtnAsync();

        (await importPage.GetWasteEmissionTypeNoticeAsync())
            .Should().Be(expectedNotice.Replace("\r\n", "\n").Trim());

        var previewRows = await importPage.GetWastePreviewRowsAsync();
        previewRows.Should().ContainSingle();
        previewRows[0].EmissionType.Should().Be("Select emission type\u2026");
        (await importPage.GetWasteEmissionTypeRowErrorAsync()).Should().Be("Select an emission type.");
    }

    [Test]
    public async Task T17_WasteImport_EmptyCompany()
    {
        const string fileName = "Republic_correct_1.xlsx";
        const string expectedCompany = "Republic Services, Inc.";
        const string expectedProject = "000000 \u2014 Haskell";
        const string expectedSiteName = "BLUE DIAMOND (WASTE)";
        const string expectedWasteType = "Solid Waste";

        try
        {
            await SqlHelper.DeleteWasteReportByExternalReferenceAsync(fileName);

            var importPage = new ImportPage(Fixture.Page);
            await importPage.OpenWasteAsync();
            await importPage.UploadFileAsync(fileName);
            await importPage.ClickImportBtnAsync();

            (await importPage.GetWasteCompanyValueAsync()).Should().Be(expectedCompany);

            var previewRows = await importPage.GetWastePreviewRowsAsync();
            previewRows.Should().HaveCount(2);
            previewRows[0].TicketNumber.Should().Be("10004101");
            previewRows[0].ContainerId.Should().Be("97500514250000201");
            previewRows[0].SiteName.Should().Be(expectedSiteName);
            previewRows[0].SiteAddress.Should().Be("TEST ADDRESS 1");
            previewRows[0].Project.Should().Be(expectedProject);
            previewRows[0].HaulDate.Should().Be("May 20, 2026");
            previewRows[0].Tonnage.Should().Be("1,000");
            previewRows[0].AssignedWasteType.Should().Be(expectedWasteType);
            previewRows[0].EmissionType.Should().Be(expectedWasteType);
            previewRows[0].Company.Should().Be(expectedCompany);
            previewRows[1].TicketNumber.Should().Be("10004102");
            previewRows[1].ContainerId.Should().Be("97500514250000202");
            previewRows[1].SiteName.Should().Be(expectedSiteName);
            previewRows[1].SiteAddress.Should().Be("TEST ADDRESS 2");
            previewRows[1].Project.Should().Be(expectedProject);
            previewRows[1].HaulDate.Should().Be("May 21, 2026");
            previewRows[1].Tonnage.Should().Be("1,500");
            previewRows[1].AssignedWasteType.Should().Be(expectedWasteType);
            previewRows[1].EmissionType.Should().Be(expectedWasteType);
            previewRows[1].Company.Should().Be(expectedCompany);

            (await importPage.IsSaveBtnEnabledAsync()).Should().BeTrue();
            await importPage.ClearWasteCompanyAsync();
            (await importPage.IsSaveBtnEnabledAsync()).Should().BeFalse();
        }
        finally
        {
            await SqlHelper.DeleteWasteReportByExternalReferenceAsync(fileName);
        }
    }

    [Test]
    public async Task T18_WasteImport_ClickCancel()
    {
        const string fileName = "Republic_correct_1.xlsx";
        const string expectedCompany = "Republic Services, Inc.";
        const string expectedProject = "000000 \u2014 Haskell";
        const string expectedSiteName = "BLUE DIAMOND (WASTE)";
        const string expectedWasteType = "Solid Waste";
        const string expectedHaulMonth = "May 2026";

        try
        {
            await SqlHelper.DeleteWasteReportByExternalReferenceAsync(fileName);

            var importPage = new ImportPage(Fixture.Page);
            await importPage.OpenWasteAsync();
            await importPage.UploadFileAsync(fileName);
            await importPage.ClickImportBtnAsync();

            (await importPage.GetWasteCompanyValueAsync()).Should().Be(expectedCompany);

            var previewRows = await importPage.GetWastePreviewRowsAsync();
            previewRows.Should().HaveCount(2);
            previewRows[0].TicketNumber.Should().Be("10004101");
            previewRows[0].ContainerId.Should().Be("97500514250000201");
            previewRows[0].SiteName.Should().Be(expectedSiteName);
            previewRows[0].SiteAddress.Should().Be("TEST ADDRESS 1");
            previewRows[0].Project.Should().Be(expectedProject);
            previewRows[0].HaulDate.Should().Be("May 20, 2026");
            previewRows[0].Tonnage.Should().Be("1,000");
            previewRows[0].AssignedWasteType.Should().Be(expectedWasteType);
            previewRows[0].EmissionType.Should().Be(expectedWasteType);
            previewRows[0].Company.Should().Be(expectedCompany);
            previewRows[1].TicketNumber.Should().Be("10004102");
            previewRows[1].ContainerId.Should().Be("97500514250000202");
            previewRows[1].SiteName.Should().Be(expectedSiteName);
            previewRows[1].SiteAddress.Should().Be("TEST ADDRESS 2");
            previewRows[1].Project.Should().Be(expectedProject);
            previewRows[1].HaulDate.Should().Be("May 21, 2026");
            previewRows[1].Tonnage.Should().Be("1,500");
            previewRows[1].AssignedWasteType.Should().Be(expectedWasteType);
            previewRows[1].EmissionType.Should().Be(expectedWasteType);
            previewRows[1].Company.Should().Be(expectedCompany);

            await importPage.ClickCancelBtnAsync();

            (await importPage.GetTitleAsync()).Should().Be("Import Waste Report");
            (await importPage.IsSelectFileSectionVisibleAsync()).Should().BeTrue();
            (await importPage.IsPreviewGridVisibleAsync()).Should().BeFalse();

            var wastePage = await importPage.ClickBackToWasteReportsAsync();
            await wastePage.SetMonthAsync("2026-05");

            (await wastePage.GetTitleAsync()).Should().Be("Waste Report Upload");
            (await wastePage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedHaulMonth);

            var ticketNumbers = (await wastePage.GetGridRowsAsync()).Select(row => row.TicketNumber).ToList();
            ticketNumbers.Should().NotContain("auto10004101");
            ticketNumbers.Should().NotContain("auto10004102");
            ticketNumbers.Should().NotContain(previewRows[0].TicketNumber);
            ticketNumbers.Should().NotContain(previewRows[1].TicketNumber);
        }
        finally
        {
            await SqlHelper.DeleteWasteReportByExternalReferenceAsync(fileName);
        }
    }

    [Test]
    public async Task T19_WasteImport_Success()
    {
        const string fileName = "Republic_correct_1.xlsx";
        const string expectedCompany = "Republic Services, Inc.";
        const string expectedProject = "000000 \u2014 Haskell";
        const string expectedSiteName = "BLUE DIAMOND (WASTE)";
        const string expectedWasteType = "Solid Waste";
        const string expectedHaulMonth = "May 2026";

        try
        {
            await SqlHelper.DeleteWasteReportByExternalReferenceAsync(fileName);

            var importPage = new ImportPage(Fixture.Page);
            await importPage.OpenWasteAsync();
            await importPage.UploadFileAsync(fileName);
            await importPage.ClickImportBtnAsync();

            (await importPage.GetWasteCompanyValueAsync()).Should().Be(expectedCompany);

            var previewRows = await importPage.GetWastePreviewRowsAsync();
            previewRows.Should().HaveCount(2);
            previewRows[0].TicketNumber.Should().Be("10004101");
            previewRows[0].ContainerId.Should().Be("97500514250000201");
            previewRows[0].SiteName.Should().Be(expectedSiteName);
            previewRows[0].SiteAddress.Should().Be("TEST ADDRESS 1");
            previewRows[0].Project.Should().Be(expectedProject);
            previewRows[0].HaulDate.Should().Be("May 20, 2026");
            previewRows[0].Tonnage.Should().Be("1,000");
            previewRows[0].AssignedWasteType.Should().Be(expectedWasteType);
            previewRows[0].EmissionType.Should().Be(expectedWasteType);
            previewRows[0].Company.Should().Be(expectedCompany);
            previewRows[1].TicketNumber.Should().Be("10004102");
            previewRows[1].ContainerId.Should().Be("97500514250000202");
            previewRows[1].SiteName.Should().Be(expectedSiteName);
            previewRows[1].SiteAddress.Should().Be("TEST ADDRESS 2");
            previewRows[1].Project.Should().Be(expectedProject);
            previewRows[1].HaulDate.Should().Be("May 21, 2026");
            previewRows[1].Tonnage.Should().Be("1,500");
            previewRows[1].AssignedWasteType.Should().Be(expectedWasteType);
            previewRows[1].EmissionType.Should().Be(expectedWasteType);
            previewRows[1].Company.Should().Be(expectedCompany);

            var wastePage = await importPage.ClickSaveWasteBtnAsync();
            (await wastePage.GetTitleAsync()).Should().Be("Waste Report Upload");
            (await wastePage.GetMonthFieldDisplayValueAsync()).Should().Be(expectedHaulMonth);

            var gridRows = await wastePage.GetGridRowsAsync();
            var row1 = gridRows.Single(row => row.TicketNumber == previewRows[0].TicketNumber);
            var row2 = gridRows.Single(row => row.TicketNumber == previewRows[1].TicketNumber);
            row1.ContainerId.Should().Be(previewRows[0].ContainerId);
            row1.SiteName.Should().Be(previewRows[0].SiteName);
            row1.Location.Should().Be(previewRows[0].SiteAddress);
            row1.Project.Should().Be(previewRows[0].Project);
            row1.HaulDate.Should().Be(previewRows[0].HaulDate);
            row1.Tonnage.Should().Be(previewRows[0].Tonnage);
            row1.EmissionType.Should().Be(previewRows[0].EmissionType);
            row1.Company.Should().Be(previewRows[0].Company);
            row2.ContainerId.Should().Be(previewRows[1].ContainerId);
            row2.SiteName.Should().Be(previewRows[1].SiteName);
            row2.Location.Should().Be(previewRows[1].SiteAddress);
            row2.Project.Should().Be(previewRows[1].Project);
            row2.HaulDate.Should().Be(previewRows[1].HaulDate);
            row2.Tonnage.Should().Be(previewRows[1].Tonnage);
            row2.EmissionType.Should().Be(previewRows[1].EmissionType);
            row2.Company.Should().Be(previewRows[1].Company);
        }
        finally
        {
            await SqlHelper.DeleteWasteReportByExternalReferenceAsync(fileName);
        }
    }

    private async Task AssertImportAlertAsync(string fileName, string expectedAlertMessage)
    {
        var importPage = new ImportPage(Fixture.Page);
        await importPage.OpenWasteAsync();
        await importPage.UploadFileAsync(fileName);
        await importPage.ClickImportBtnAsync();

        (await importPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }
}
