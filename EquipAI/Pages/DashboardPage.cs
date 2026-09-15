using System.Globalization;
using System.Text.RegularExpressions;
using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Pages;

public class DashboardPage : BasePage
{
    public DashboardPage(IPage page) : base(page) { }

    private ILocator Logo => Page.Locator("//div[@class='header-left-bar']//app-logo");
    private ILocator OpenMenuBtn => Page.Locator("//div[@class='header-left-bar']//button[@aria-label='Open menu']");
    private ILocator DashboardTitle => Page.Locator("#dashboard-title");
    private ILocator Subtitle => Page.Locator("//p[@class='page-header__lead']");
    private ILocator TotalCarbonEmissionCard => Page.Locator("//article[@aria-labelledby='dashboard-emissions-title']");
    private ILocator TotalEmissionsLabel => Page.Locator("//h2[@id='dashboard-emissions-title']");
    private ILocator TotalEmissionsValue => Page.Locator("//article[@aria-labelledby='dashboard-emissions-title']//span[@class='metric__value']");
    private ILocator TotalEmissionsUnit => Page.Locator("//article[@aria-labelledby='dashboard-emissions-title']//span[@class='metric__unit']");
    private ILocator TotalEmissionsScope1Label => Page.Locator("//span[normalize-space()='Scope 1']");
    private ILocator TotalEmissionsScope1Value => Page.Locator("//dd[@class='stat__value stat__value--scope-1']");
    private ILocator TotalEmissionsScope2Label => Page.Locator("//span[normalize-space()='Scope 2']");
    private ILocator TotalEmissionsScope2Value => Page.Locator("//dd[@class='stat__value stat__value--scope-2']");
    private ILocator TotalEmissionsScope3Label => Page.Locator("//span[normalize-space()='Scope 3']");
    private ILocator TotalEmissionsScope3Value => Page.Locator("//dd[@class='stat__value stat__value--scope-3']");
    private ILocator TotalActiveProjectsCard => Page.Locator("//article[@aria-labelledby='dashboard-projects-title']");
    private ILocator TotalProjectsTitle => Page.Locator("#dashboard-projects-title");
    private ILocator TotalProjectsValue => Page.Locator("//article[@aria-labelledby='dashboard-projects-title']//span[@class='metric__value']");
    private ILocator TotalProjectsReportingLabel => Page.Locator("//span[normalize-space()='Reporting']");
    private ILocator TotalProjectsReportingValue => Page.Locator("//dd[@class='stat__value stat__value--primary']");
    private ILocator TotalProjectsNonReportingLabel => Page.Locator("//span[normalize-space()='Non Reporting']");
    private ILocator TotalProjectsNonReportingValue => Page.Locator("//dd[@class='stat__value stat__value--danger']");
    private ILocator TotalCarbonEmissionsByScopeCard => Page.Locator("//section[@class='dashboard__chart']");
    private ILocator TotalEmissionsByScopeLabel => Page.Locator("#dashboard-chart-title");
    private ILocator TotalEmissionsByScopeMessage => Page.Locator("//header[@class='section-header']//p[@class='section-header__lead']");
    private ILocator YearLabel => Page.Locator("//label[normalize-space()='Year']");
    private ILocator YearDropdown => Page.Locator("//app-searchable-select[@controlid='dashboard-chart-year']");
    private ILocator SearchableSelectList => Page.Locator("//div[@role='listbox' and contains(@class,'select__list')]");
    private ILocator Scope1Btn => Page.Locator("//button[normalize-space()='Scope 1']");
    private ILocator Scope2Btn => Page.Locator("//button[normalize-space()='Scope 2']");
    private ILocator Scope3Btn => Page.Locator("//button[normalize-space()='Scope 3']");
    private ILocator Chart => Page.Locator("//div[@class='chart__plot']");
    private ILocator ActiveProjectsCard => Page.Locator("//section[@class='dashboard__panel']");
    private ILocator ActiveProjectsLabel => Page.Locator("//h2[@id='dashboard-projects-heading']");
    private ILocator ActiveProjectsMessage => Page.Locator("//section[@class='dashboard__panel']//p[@class='section-header__lead']");
    private ILocator ProjectsList => Page.Locator("//div[@class='card card--subtle dashboard__panel-card']");
    private ILocator ProjectsUl => Page.Locator("//ul[@class='projects']");
    private ILocator ProjectNameLinks => Page.Locator("//p[@class='projects__name']/a");
    private ILocator ProjectTonnesValues => Page.Locator("//span[@class='projects__tonnes']");
    private ILocator ViewAllProjectsLink => Page.Locator("//a[normalize-space()='View all projects']");
    private ILocator CarbonEmissionsByCategoryCard => Page.Locator("//section[@class='dashboard__scopes']");
    private ILocator EmissionsByCategoryLabel => Page.Locator("#dashboard-scopes-heading");
    private ILocator EmissionsByCategoryMessage => Page.Locator("//h2[@id='dashboard-scopes-heading']/following-sibling::p");
    private ILocator Scope1Card => Page.Locator("//article[@aria-labelledby='scope-detail-1']");
    private ILocator Scope1Title => Page.Locator("//h3[@id='scope-detail-1']");
    private ILocator Scope1CategoryNames => Page.Locator("//article[@aria-labelledby='scope-detail-1']//p[@class='scope-detail__source']");
    private ILocator Scope1CategoryValues => Page.Locator("//article[@aria-labelledby='scope-detail-1']//p[@class='scope-detail__value']");
    private ILocator Scope2Card => Page.Locator("//article[@aria-labelledby='scope-detail-2']");
    private ILocator Scope2Title => Page.Locator("//h3[@id='scope-detail-2']");
    private ILocator Scope2CategoryNames => Page.Locator("//article[@aria-labelledby='scope-detail-2']//p[@class='scope-detail__source']");
    private ILocator Scope2CategoryValues => Page.Locator("//article[@aria-labelledby='scope-detail-2']//p[@class='scope-detail__value']");
    private ILocator Scope3Card => Page.Locator("//article[@aria-labelledby='scope-detail-3']");
    private ILocator Scope3Title => Page.Locator("//h3[@id='scope-detail-3']");
    private ILocator Scope3CategoryNames => Page.Locator("//article[@aria-labelledby='scope-detail-3']//p[@class='scope-detail__source']");
    private ILocator Scope3CategoryValues => Page.Locator("//article[@aria-labelledby='scope-detail-3']//p[@class='scope-detail__value']");
    private ILocator SupportCard => Page.Locator("//app-support-card[@class='dashboard__support']");

    public async Task OpenAsync()
    {
        if (Page.Url.Contains("login") || !IsOnDashboardHome())
        {
            await Page.GotoAsync(Config.BaseUrl);
        }

        await DashboardTitle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Logo.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    private bool IsOnDashboardHome()
    {
        var path = new Uri(Page.Url).AbsolutePath.TrimEnd('/');
        return path.Length == 0;
    }

    public new async Task<string> GetPageTitleAsync()
    {
        await DashboardTitle.WaitForAsync();
        return (await DashboardTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetSubtitleAsync()
    {
        await Subtitle.WaitForAsync();
        return (await Subtitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalCarbonEmissionCardVisibleAsync() => TotalCarbonEmissionCard.IsVisibleAsync();
    public Task<bool> IsTotalActiveProjectsCardVisibleAsync() => TotalActiveProjectsCard.IsVisibleAsync();
    public Task<bool> IsTotalCarbonEmissionsByScopeCardVisibleAsync() => TotalCarbonEmissionsByScopeCard.IsVisibleAsync();
    public Task<bool> IsActiveProjectsCardVisibleAsync() => ActiveProjectsCard.IsVisibleAsync();
    public Task<bool> IsCarbonEmissionsByCategoryCardVisibleAsync() => CarbonEmissionsByCategoryCard.IsVisibleAsync();
    public Task<bool> IsSupportCardVisibleAsync() => SupportCard.IsVisibleAsync();

    public async Task<string> GetEmissionsByCategoryLabelAsync()
    {
        await EmissionsByCategoryLabel.WaitForAsync();
        return (await EmissionsByCategoryLabel.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetEmissionsByCategoryMessageAsync()
    {
        await EmissionsByCategoryMessage.WaitForAsync();
        return (await EmissionsByCategoryMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsScope1CardVisibleAsync() => Scope1Card.IsVisibleAsync();
    public Task<bool> IsScope2CardVisibleAsync() => Scope2Card.IsVisibleAsync();
    public Task<bool> IsScope3CardVisibleAsync() => Scope3Card.IsVisibleAsync();

    public async Task<string> GetSelectedChartYearAsync()
    {
        var selectedValue = YearDropdown.Locator(".select__value");
        await selectedValue.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await selectedValue.First.InnerTextAsync()).Replace('\u00A0', ' ').Trim();
    }

    public async Task<string> GetScope1TitleAsync()
    {
        await Scope1Title.WaitForAsync();
        return (await Scope1Title.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<IReadOnlyList<string>> GetScope1CategoryNamesAsync()
    {
        await Scope1Card.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var names = await Scope1CategoryNames.AllInnerTextsAsync();
        return names.Select(name => name.Replace('\u00A0', ' ').Trim()).ToList();
    }

    public async Task<IReadOnlyList<string>> GetScope1CategoryValuesAsync()
    {
        await Scope1Card.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var values = await Scope1CategoryValues.AllInnerTextsAsync();
        return values.Select(value => value.Replace('\u00A0', ' ').Trim()).ToList();
    }

    public async Task<string> GetScope2TitleAsync()
    {
        await Scope2Title.WaitForAsync();
        return (await Scope2Title.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<IReadOnlyList<string>> GetScope2CategoryNamesAsync()
    {
        await Scope2Card.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var names = await Scope2CategoryNames.AllInnerTextsAsync();
        return names.Select(name => name.Replace('\u00A0', ' ').Trim()).ToList();
    }

    public async Task<IReadOnlyList<string>> GetScope2CategoryValuesAsync()
    {
        await Scope2Card.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var values = await Scope2CategoryValues.AllInnerTextsAsync();
        return values.Select(value => value.Replace('\u00A0', ' ').Trim()).ToList();
    }

    public async Task<string> GetScope3TitleAsync()
    {
        await Scope3Title.WaitForAsync();
        return (await Scope3Title.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<IReadOnlyList<string>> GetScope3CategoryNamesAsync()
    {
        await Scope3Card.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var names = await Scope3CategoryNames.AllInnerTextsAsync();
        return names.Select(name => name.Replace('\u00A0', ' ').Trim()).ToList();
    }

    public async Task<IReadOnlyList<string>> GetScope3CategoryValuesAsync()
    {
        await Scope3Card.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var values = await Scope3CategoryValues.AllInnerTextsAsync();
        return values.Select(value => value.Replace('\u00A0', ' ').Trim()).ToList();
    }
    public Task<bool> IsLogoVisibleAsync() => Logo.IsVisibleAsync();
    public Task<bool> IsOpenMenuBtnVisibleAsync() => OpenMenuBtn.IsVisibleAsync();

    public async Task<string> GetTotalEmissionsLabelAsync()
    {
        await TotalEmissionsLabel.WaitForAsync();
        return (await TotalEmissionsLabel.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalEmissionsValueVisibleAsync() => TotalEmissionsValue.IsVisibleAsync();

    public async Task<string> GetTotalEmissionsValueAsync()
    {
        await TotalEmissionsValue.WaitForAsync();
        return (await TotalEmissionsValue.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetTotalEmissionsUnitAsync()
    {
        await TotalEmissionsUnit.WaitForAsync();
        return (await TotalEmissionsUnit.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalEmissionsScope1LabelVisibleAsync() => TotalEmissionsScope1Label.IsVisibleAsync();
    public Task<bool> IsTotalEmissionsScope1ValueVisibleAsync() => TotalEmissionsScope1Value.IsVisibleAsync();

    public async Task<string> GetTotalEmissionsScope1ValueAsync()
    {
        await TotalEmissionsScope1Value.WaitForAsync();
        return (await TotalEmissionsScope1Value.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalEmissionsScope2LabelVisibleAsync() => TotalEmissionsScope2Label.IsVisibleAsync();
    public Task<bool> IsTotalEmissionsScope2ValueVisibleAsync() => TotalEmissionsScope2Value.IsVisibleAsync();

    public async Task<string> GetTotalEmissionsScope2ValueAsync()
    {
        await TotalEmissionsScope2Value.WaitForAsync();
        return (await TotalEmissionsScope2Value.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalEmissionsScope3LabelVisibleAsync() => TotalEmissionsScope3Label.IsVisibleAsync();
    public Task<bool> IsTotalEmissionsScope3ValueVisibleAsync() => TotalEmissionsScope3Value.IsVisibleAsync();

    public async Task<string> GetTotalEmissionsScope3ValueAsync()
    {
        await TotalEmissionsScope3Value.WaitForAsync();
        return (await TotalEmissionsScope3Value.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetTotalProjectsTitleAsync()
    {
        await TotalProjectsTitle.WaitForAsync();
        return (await TotalProjectsTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalProjectsValueVisibleAsync() => TotalProjectsValue.IsVisibleAsync();

    public async Task<string> GetTotalProjectsValueAsync()
    {
        await TotalProjectsValue.WaitForAsync();
        return (await TotalProjectsValue.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalProjectsReportingLabelVisibleAsync() => TotalProjectsReportingLabel.IsVisibleAsync();
    public Task<bool> IsTotalProjectsReportingValueVisibleAsync() => TotalProjectsReportingValue.IsVisibleAsync();

    public async Task<string> GetTotalProjectsReportingValueAsync()
    {
        await TotalProjectsReportingValue.WaitForAsync();
        return (await TotalProjectsReportingValue.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsTotalProjectsNonReportingLabelVisibleAsync() => TotalProjectsNonReportingLabel.IsVisibleAsync();
    public Task<bool> IsTotalProjectsNonReportingValueVisibleAsync() => TotalProjectsNonReportingValue.IsVisibleAsync();

    public async Task<string> GetTotalProjectsNonReportingValueAsync()
    {
        await TotalProjectsNonReportingValue.WaitForAsync();
        return (await TotalProjectsNonReportingValue.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetActiveProjectsLabelAsync()
    {
        await ActiveProjectsLabel.WaitForAsync();
        return (await ActiveProjectsLabel.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetActiveProjectsMessageAsync()
    {
        await ActiveProjectsMessage.WaitForAsync();
        return (await ActiveProjectsMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsProjectsListVisibleAsync() => ProjectsList.IsVisibleAsync();
    public Task<bool> IsViewAllProjectsLinkVisibleAsync() => ViewAllProjectsLink.IsVisibleAsync();

    public async Task<bool> IsActiveProjectsListEmptyAsync()
    {
        await ProjectsUl.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (true)
        {
            if (await ProjectsUl.Locator("li").CountAsync() == 0)
                return true;

            if (DateTime.UtcNow >= deadline)
                return false;

            await Task.Delay(250);
        }
    }

    public async Task<IReadOnlyList<string>> GetActiveProjectNamesAsync()
    {
        await ProjectsUl.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var names = await ProjectNameLinks.AllInnerTextsAsync();
        return names.Select(name => name.Trim()).ToList();
    }

    public async Task<IReadOnlyList<string>> GetActiveProjectTonnesValuesAsync()
    {
        await ProjectsUl.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var values = await ProjectTonnesValues.AllInnerTextsAsync();
        return values.Select(value => value.Trim()).ToList();
    }

    public async Task<string> GetFirstActiveProjectNameAsync()
    {
        await ProjectNameLinks.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await ProjectNameLinks.First.InnerTextAsync()).Trim();
    }

    public async Task<ProjectDashboardPage> ClickFirstActiveProjectAsync()
    {
        await ProjectNameLinks.First.ClickAsync();
        var projectDashboardPage = new ProjectDashboardPage(Page);
        await projectDashboardPage.WaitForLoadedAsync();
        return projectDashboardPage;
    }

    public async Task<ProjectsPage> ClickViewAllProjectsLinkAsync()
    {
        await ViewAllProjectsLink.ClickAsync();
        var projectsPage = new ProjectsPage(Page);
        await projectsPage.WaitForLoadedAsync();
        return projectsPage;
    }

    public async Task<string> GetTotalEmissionsByScopeLabelAsync()
    {
        await TotalEmissionsByScopeLabel.WaitForAsync();
        return (await TotalEmissionsByScopeLabel.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetTotalEmissionsByScopeMessageAsync()
    {
        await TotalEmissionsByScopeMessage.WaitForAsync();
        return (await TotalEmissionsByScopeMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsYearLabelVisibleAsync() => YearLabel.IsVisibleAsync();
    public Task<bool> IsYearDropdownVisibleAsync() => YearDropdown.IsVisibleAsync();

    public async Task<IReadOnlyList<string>> GetYearDropdownOptionsAsync()
    {
        await OpenSearchableSelectAsync(YearDropdown);
        var options = await SearchableSelectList.Locator("[role='option']").AllInnerTextsAsync();
        var result = options
            .Select(option => option.Replace('\u00A0', ' ').Trim())
            .Where(option => !string.IsNullOrEmpty(option)
                             && !option.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            .ToList();

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
            // Dropdown may already be closed.
        }

        return result;
    }

    public async Task SelectYearAsync(string year)
    {
        var selectedValue = YearDropdown.Locator(".select__value");
        if (await selectedValue.CountAsync() > 0)
        {
            var currentSelected = (await selectedValue.First.InnerTextAsync())
                .Replace('\u00A0', ' ')
                .Trim();
            if (currentSelected.Equals(year, StringComparison.OrdinalIgnoreCase))
                return;
        }

        var chartLine = Chart.Locator("path.chart__line");
        var previousPath = await chartLine.CountAsync() > 0
            ? await chartLine.GetAttributeAsync("d")
            : null;

        await OpenSearchableSelectAsync(YearDropdown);
        var options = SearchableSelectList.Locator("[role='option']");
        var count = await options.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var option = options.Nth(i);
            var text = (await option.TextContentAsync())?.Replace('\u00A0', ' ').Trim() ?? string.Empty;
            if (!text.Equals(year, StringComparison.OrdinalIgnoreCase))
                continue;

            await option.ClickAsync();
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

            await selectedValue
                .Filter(new LocatorFilterOptions { HasTextString = text })
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });

            try
            {
                await chartLine.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 5_000,
                });
            }
            catch (TimeoutException)
            {
                // Chart may have no series for the selected year.
                return;
            }

            if (!string.IsNullOrEmpty(previousPath))
            {
                try
                {
                    await Assertions.Expect(chartLine)
                        .Not.ToHaveAttributeAsync("d", previousPath, new LocatorAssertionsToHaveAttributeOptions
                        {
                            Timeout = 15_000,
                        });
                }
                catch (PlaywrightException)
                {
                    // Chart may redraw with an equivalent path for some datasets.
                }
            }

            return;
        }

        throw new InvalidOperationException($"Year option '{year}' was not found.");
    }

    public async Task ClickScope1BtnAsync() => await ClickScopeBtnAsync(Scope1Btn);

    public async Task ClickScope2BtnAsync() => await ClickScopeBtnAsync(Scope2Btn);

    public async Task ClickScope3BtnAsync() => await ClickScopeBtnAsync(Scope3Btn);

    public async Task<bool> HasChartLineAsync()
    {
        var chartLine = Chart.Locator("path.chart__line");
        return await chartLine.CountAsync() > 0 && await chartLine.First.IsVisibleAsync();
    }

    private async Task ClickScopeBtnAsync(ILocator scopeBtn)
    {
        var chartLine = Chart.Locator("path.chart__line");
        var previousPath = await HasChartLineAsync()
            ? await chartLine.GetAttributeAsync("d")
            : null;

        await scopeBtn.ClickAsync();

        try
        {
            await chartLine.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5_000,
            });
        }
        catch (TimeoutException)
        {
            // Chart may have no series for the selected scope/year.
            return;
        }

        if (!string.IsNullOrEmpty(previousPath))
        {
            try
            {
                await Assertions.Expect(chartLine)
                    .Not.ToHaveAttributeAsync("d", previousPath, new LocatorAssertionsToHaveAttributeOptions
                    {
                        Timeout = 5_000,
                    });
            }
            catch (PlaywrightException)
            {
                // Scope may already be selected with the same rendered path.
            }
        }
    }

    public async Task<IReadOnlyDictionary<int, decimal>> GetChartMonthlyTonnesAsync()
    {
        await Chart.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await Chart.Locator("path.chart__line")
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var tooltipValues = await TryGetChartMonthlyTonnesFromTooltipsAsync();
        if (tooltipValues.Count > 0)
            return tooltipValues;

        var (topY, bottomY, topValue) = await GetChartYAxisScaleAsync();
        var points = await GetChartLinePointsAsync();
        if (points.Count == 0)
            return new Dictionary<int, decimal>();

        var result = new Dictionary<int, decimal>();
        var yRange = bottomY - topY;
        for (var i = 0; i < points.Count; i++)
        {
            var tonnes = yRange == 0
                ? 0m
                : (decimal)((bottomY - points[i].Y) / yRange) * topValue;
            result[i + 1] = tonnes;
        }

        return result;
    }

    private async Task<IReadOnlyDictionary<int, decimal>> TryGetChartMonthlyTonnesFromTooltipsAsync()
    {
        var result = new Dictionary<int, decimal>();
        var monthLabels = Chart.Locator("g.chart__x-axis text.chart__axis-label");
        var count = await monthLabels.CountAsync();
        if (count == 0)
            return result;

        var tooltip = Page.Locator(
            "//div[contains(@class,'chart__tooltip') or contains(@class,'tooltip') or @role='tooltip']");

        for (var i = 0; i < count; i++)
        {
            var label = monthLabels.Nth(i);
            await label.HoverAsync();
            try
            {
                await tooltip.First.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 1_500,
                });
            }
            catch (TimeoutException)
            {
                continue;
            }

            var tooltipText = (await tooltip.First.InnerTextAsync())?.Replace('\u00A0', ' ').Trim() ?? string.Empty;
            if (TryParseChartTooltipTonnes(tooltipText, out var tonnes))
                result[i + 1] = tonnes;
        }

        await Chart.HoverAsync(new LocatorHoverOptions { Position = new Position { X = 1, Y = 1 } });
        return result;
    }

    private static bool TryParseChartTooltipTonnes(string tooltipText, out decimal tonnes)
    {
        tonnes = 0m;
        if (string.IsNullOrWhiteSpace(tooltipText))
            return false;

        var lines = tooltipText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith("Scope", StringComparison.OrdinalIgnoreCase))
                continue;

            var normalized = line.Replace(",", string.Empty, StringComparison.Ordinal).Trim();
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out tonnes))
                return true;
        }

        return false;
    }

    private async Task<(double TopY, double BottomY, decimal TopValue)> GetChartYAxisScaleAsync()
    {
        var labels = Chart.Locator("g.chart__grid text.chart__axis-label");
        var count = await labels.CountAsync();
        if (count == 0)
            throw new InvalidOperationException("Chart Y-axis labels were not found.");

        double? topY = null;
        double? bottomY = null;
        decimal topValue = 0m;
        decimal bottomValue = 0m;

        for (var i = 0; i < count; i++)
        {
            var label = labels.Nth(i);
            var yAttr = await label.GetAttributeAsync("y");
            var text = (await label.TextContentAsync())?.Replace('\u00A0', ' ').Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(yAttr) || string.IsNullOrWhiteSpace(text))
                continue;

            var y = double.Parse(yAttr, CultureInfo.InvariantCulture);
            var value = ParseChartAxisValue(text);

            if (topY is null || y < topY)
            {
                topY = y;
                topValue = value;
            }

            if (bottomY is null || y > bottomY)
            {
                bottomY = y;
                bottomValue = value;
            }
        }

        if (topY is null || bottomY is null)
            throw new InvalidOperationException("Unable to resolve chart Y-axis scale.");

        // Prefer the non-zero top of the scale when bottom is 0.
        _ = bottomValue;
        return (topY.Value, bottomY.Value, topValue);
    }

    private async Task<IReadOnlyList<(double X, double Y)>> GetChartLinePointsAsync()
    {
        var pathD = await Chart.Locator("path.chart__line").GetAttributeAsync("d");
        if (string.IsNullOrWhiteSpace(pathD))
            return Array.Empty<(double X, double Y)>();

        var matches = Regex.Matches(pathD, @"[ML]\s*([0-9]+(?:\.[0-9]+)?)\s+([0-9]+(?:\.[0-9]+)?)", RegexOptions.IgnoreCase);
        var points = new List<(double X, double Y)>();
        foreach (Match match in matches)
        {
            var x = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var y = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            points.Add((x, y));
        }

        return points;
    }

    private static decimal ParseChartAxisValue(string label)
    {
        var normalized = label.Replace(",", string.Empty, StringComparison.Ordinal).Trim();
        var hasK = normalized.EndsWith("k", StringComparison.OrdinalIgnoreCase);
        if (hasK)
            normalized = normalized[..^1].Trim();

        var value = decimal.Parse(normalized, CultureInfo.InvariantCulture);
        return hasK ? value * 1000m : value;
    }

    public Task<bool> IsScope1BtnVisibleAsync() => Scope1Btn.IsVisibleAsync();
    public Task<bool> IsScope2BtnVisibleAsync() => Scope2Btn.IsVisibleAsync();
    public Task<bool> IsScope3BtnVisibleAsync() => Scope3Btn.IsVisibleAsync();
    public Task<bool> IsChartVisibleAsync() => Chart.IsVisibleAsync();

    private async Task OpenSearchableSelectAsync(ILocator dropdown)
    {
        await dropdown.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var trigger = dropdown.Locator("button.select__trigger");
        var clickTarget = await trigger.CountAsync() > 0 ? trigger.First : dropdown;

        var expanded = await clickTarget.GetAttributeAsync("aria-expanded");
        if (string.Equals(expanded, "true", StringComparison.OrdinalIgnoreCase)
            && await SearchableSelectList.IsVisibleAsync())
            return;

        if (await SearchableSelectList.IsVisibleAsync()
            || await Page.Locator("button.overlay[aria-label='Close dropdown']").IsVisibleAsync())
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
                // Dropdown may already be closed.
            }
        }

        await clickTarget.ClickAsync();
        await SearchableSelectList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }
}
