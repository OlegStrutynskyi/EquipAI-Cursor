using System.Globalization;
using EquipAI.Pages;
using FluentAssertions;

namespace EquipAI.Tests;

public class TelemetryTests : BaseTest
{
    [Test]
    public async Task T01_Telemetry_DefaultView()
    {
        const string expectedTitle = "Telemetry Upload";
        const string expectedMessage = "Browse imported equipment telemetry readings.";
        var expectedMonth = DateTime.Today.AddMonths(-1).ToString("yyyy-MM", CultureInfo.InvariantCulture);

        var telemetryPage = new TelemetryPage(Fixture.Page);
        await telemetryPage.OpenAsync();

        (await telemetryPage.GetTitleAsync()).Should().Be(expectedTitle);
        (await telemetryPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await telemetryPage.IsImportBtnVisibleAsync()).Should().BeTrue();
        (await telemetryPage.IsMonthLabelVisibleAsync()).Should().BeTrue();
        (await telemetryPage.IsMonthFieldVisibleAsync()).Should().BeTrue();
        (await telemetryPage.GetMonthFieldValueAsync()).Should().Be(expectedMonth);
        (await telemetryPage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Telemetry_ClickImportBtn()
    {
        const string expectedTitle = "Import Telemetry";

        var telemetryPage = new TelemetryPage(Fixture.Page);
        await telemetryPage.OpenAsync();
        var ImportPage = await telemetryPage.ClickImportBtnAsync();

        (await ImportPage.GetTitleAsync()).Should().Be(expectedTitle);
    }

    [Test]
    public async Task T03_Telemetry_GridColumns()
    {
        var expectedColumns = new[]
        {
            "MONTH",
            "EQUIPMENT TAG",
            "EQUIPMENT TYPE",
            "LOCATION",
            "OPERATING HOURS",
            "FUEL TYPE",
            "IMPORT DATE",
            "SOURCE",
        };

        var telemetryPage = new TelemetryPage(Fixture.Page);
        await telemetryPage.OpenAsync();

        (await telemetryPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }
}
