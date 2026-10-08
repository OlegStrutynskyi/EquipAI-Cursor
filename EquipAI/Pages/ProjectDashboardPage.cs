using Microsoft.Playwright;

namespace EquipAI.Pages;

public class ProjectDashboardPage : BasePage
{
    public ProjectDashboardPage(IPage page) : base(page) { }

    private ILocator ProjectDashboardTitle => Page.Locator("#project-dashboard-title");
    private ILocator ReportingBadge => Page.Locator("//span[contains(@class,'pill pill--sm')]");
    private ILocator SynchedProcore => Page.Locator("//p[@class='page-meta__item']");
    private ILocator TotalEmissionsCard => Page.Locator("//article[@class='card card--subtle stat-card']");
    private ILocator TotalEmissionsIcon => TotalEmissionsCard.Locator("xpath=.//span[@class='icon-badge']");
    private ILocator TotalEmissionsTitle => TotalEmissionsCard.Locator("xpath=.//*[normalize-space()='Total Carbon Emissions']");
    private ILocator TotalEmissionsSubtitle => TotalEmissionsCard.Locator("xpath=.//p[@class='section-header__lead']");
    private ILocator TotalEmissionsMetricValue => TotalEmissionsCard.Locator("xpath=.//span[contains(@class,'metric__value')]");
    private ILocator TotalEmissionsMetricUnit => TotalEmissionsCard.Locator("xpath=.//span[contains(@class,'metric__unit')]");
    private ILocator ScopeTotalText => Page.Locator("//p[@class='breakdown__total']");
    private ILocator ScopeTotalValue => Page.Locator("//span[@class='breakdown__total-value']");
    private ILocator ScopeEmptyMessage => Page.Locator("//p[@class='project-dashboard__breakdown-empty']");
    private ILocator ChartCard => Page.Locator("//div[@class='card card--subtle project-dashboard__chart-card']");
    private ILocator DataGrid => Page.Locator("//section[@class='project-dashboard__data']");

    public async Task WaitForLoadedAsync()
    {
        await ProjectDashboardTitle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task WaitForDefaultViewAsync()
    {
        await WaitForLoadedAsync();
        await ReportingBadge.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await SynchedProcore.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await TotalEmissionsCard.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await ChartCard.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await DataGrid.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public new async Task<string> GetPageTitleAsync()
    {
        await ProjectDashboardTitle.WaitForAsync();
        return (await ProjectDashboardTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetReportingBadgeTextAsync()
    {
        await ReportingBadge.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await ReportingBadge.First.InnerTextAsync()).Trim();
    }

    public Task<bool> IsReportingBadgeVisibleAsync() => ReportingBadge.First.IsVisibleAsync();
    public Task<bool> IsSynchedProcoreVisibleAsync() => SynchedProcore.First.IsVisibleAsync();
    public Task<bool> IsTotalEmissionsCardVisibleAsync() => TotalEmissionsCard.First.IsVisibleAsync();

    public Task<bool> IsTotalEmissionsIconVisibleAsync() => TotalEmissionsIcon.First.IsVisibleAsync();

    public async Task<string> GetTotalEmissionsTitleAsync()
    {
        await TotalEmissionsTitle.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await TotalEmissionsTitle.First.InnerTextAsync()).Trim();
    }

    public async Task<string> GetTotalEmissionsSubtitleAsync()
    {
        await TotalEmissionsSubtitle.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await TotalEmissionsSubtitle.First.InnerTextAsync()).Trim();
    }

    public async Task<string> GetTotalEmissionsValueAsync()
    {
        await TotalEmissionsMetricValue.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var deadline = DateTime.UtcNow.AddSeconds(15);
        var value = string.Empty;
        while (DateTime.UtcNow < deadline)
        {
            value = (await TotalEmissionsMetricValue.First.InnerTextAsync()).Trim();
            if (value.Any(char.IsDigit))
                break;

            await Task.Delay(200);
        }

        if (await TotalEmissionsMetricUnit.CountAsync() == 0)
            return value;

        var unit = (await TotalEmissionsMetricUnit.First.InnerTextAsync()).Trim();
        return value.Contains(unit, StringComparison.OrdinalIgnoreCase) ? value : $"{value} {unit}";
    }

    public ILocator ScopeButton(int scope) =>
        Page.Locator($"//div[@aria-label='Scope']//button[@type='button'][normalize-space()='Scope {scope}']");

    public Task<bool> IsScopeButtonVisibleAsync(int scope) => ScopeButton(scope).IsVisibleAsync();

    public async Task ClickScopeButtonAsync(int scope)
    {
        var button = ScopeButton(scope);
        await button.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await button.ClickAsync();

        var expectedPrefix = $"Total Scope {scope}";
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            var text = NormalizeText(await ScopeTotalText.First.InnerTextAsync());
            if (text.StartsWith(expectedPrefix, StringComparison.Ordinal))
                return;

            await Task.Delay(200);
        }

        throw new TimeoutException(
            $"Scope {scope} breakdown did not load. Actual: '{NormalizeText(await ScopeTotalText.First.InnerTextAsync())}'.");
    }

    public async Task<bool> IsScopeButtonSelectedAsync(int scope)
    {
        var button = ScopeButton(scope);
        await button.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var ariaPressed = await button.GetAttributeAsync("aria-pressed");
        if (string.Equals(ariaPressed, "true", StringComparison.OrdinalIgnoreCase))
            return true;

        var ariaSelected = await button.GetAttributeAsync("aria-selected");
        if (string.Equals(ariaSelected, "true", StringComparison.OrdinalIgnoreCase))
            return true;

        var className = await button.GetAttributeAsync("class") ?? string.Empty;
        return className.Contains("active", StringComparison.OrdinalIgnoreCase)
            || className.Contains("selected", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> GetScopeTotalValueAsync()
    {
        await ScopeTotalValue.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var deadline = DateTime.UtcNow.AddSeconds(15);
        var value = string.Empty;
        while (DateTime.UtcNow < deadline)
        {
            value = NormalizeText(await ScopeTotalValue.First.InnerTextAsync());
            if (value.Any(char.IsDigit))
                break;

            await Task.Delay(200);
        }

        return value;
    }

    public async Task<string> GetScopeTotalTextAsync()
    {
        await ScopeTotalText.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return NormalizeText(await ScopeTotalText.First.InnerTextAsync());
    }

    public async Task<string> GetScopeEmptyMessageAsync()
    {
        await ScopeEmptyMessage.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return NormalizeText(await ScopeEmptyMessage.First.InnerTextAsync());
    }

    public async Task<IReadOnlyList<string>> GetScopeEmissionLinesAsync()
    {
        var rows = Page.Locator(
            "//p[@class='breakdown__total']/following-sibling::*[self::ul or self::ol or self::div][1]//li");
        if (await rows.CountAsync() == 0)
        {
            var html = await ScopeTotalText.First.EvaluateAsync<string>(
                "el => (el.parentElement?.innerHTML || '').slice(0, 4000)");
            throw new InvalidOperationException(html);
        }

        var texts = await rows.AllInnerTextsAsync();
        return texts.Select(NormalizeText).Where(text => text.Length > 0).ToList();
    }

    public Task<bool> IsScopeTotalTextVisibleAsync() => ScopeTotalText.First.IsVisibleAsync();
    public Task<bool> IsChartCardVisibleAsync() => ChartCard.First.IsVisibleAsync();
    public Task<bool> IsDataGridVisibleAsync() => DataGrid.First.IsVisibleAsync();

    private static string NormalizeText(string? value) =>
        System.Text.RegularExpressions.Regex.Replace(value ?? string.Empty, @"\s+", " ").Trim();
}
