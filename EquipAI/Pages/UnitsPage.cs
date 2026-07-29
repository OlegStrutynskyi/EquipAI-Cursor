using Microsoft.Playwright;

namespace EquipAI.Pages;

public class UnitsPage : BasePage
{
    public UnitsPage(IPage page) : base(page) { }

    private ILocator Message => Page.Locator("//p[@class='admin-units__lead']");
    private ILocator AddUnitBtn => Page.Locator("//button[normalize-space()='Add unit']");
    private ILocator Grid => Page.Locator("//table[@class='admin-units__table']");
    private ILocator CodeCells => Page.Locator("//table[@class='admin-units__table']//tr/td[1]");
    private ILocator DisplayNameCells => Page.Locator("//table[@class='admin-units__table']//tr/td[2]");
    private ILocator DeactivateDialog => Page.Locator("//div[@role='alertdialog']");
    private ILocator DeactivateDialogTitle => Page.Locator("//div[@role='alertdialog']//h2");
    private ILocator DeactivateDialogMessage => Page.Locator("//div[@role='alertdialog']//p[@id='confirm-dialog-message']");
    private ILocator DeactivateDialogCancelBtn => Page.Locator("//div[@role='alertdialog']//button[normalize-space()='Cancel']");
    private ILocator DeactivateDialogConfirmBtn => Page.Locator("//div[@role='alertdialog']//button[normalize-space()='Deactivate']");

    public async Task OpenAsync()
    {
        var administrationPage = new AdministrationPage(Page);
        await administrationPage.OpenAsync();
        await administrationPage.ClickUnitsTabAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        await Message.WaitForAsync();
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetMessageAsync()
    {
        await Message.WaitForAsync();
        return (await Message.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsAddUnitBtnVisibleAsync() => AddUnitBtn.IsVisibleAsync();
    public Task<bool> IsAddUnitBtnEnabledAsync() => AddUnitBtn.IsEnabledAsync();
    public Task<bool> IsGridVisibleAsync() => Grid.IsVisibleAsync();

    public async Task<bool> IsCodeInGridAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var codes = await CodeCells.AllInnerTextsAsync();
        return codes.Any(c => c.Trim().Equals(code, StringComparison.Ordinal));
    }

    public async Task<bool> IsDisplayNameInGridAsync(string displayName)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var displayNames = await DisplayNameCells.AllInnerTextsAsync();
        return displayNames.Any(name => name.Trim().Equals(displayName, StringComparison.Ordinal));
    }

    public async Task<(string Code, string DisplayName)> GetCodeAndDisplayNameAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var row = Page.Locator($"//table[@class='admin-units__table']//tr[td[1][normalize-space()='{code}']]");
        await row.WaitForAsync();
        var codeText = (await row.Locator("td").Nth(0).InnerTextAsync()).Trim();
        var displayName = (await row.Locator("td").Nth(1).InnerTextAsync()).Trim();
        return (codeText, displayName);
    }

    public async Task<AddUnitPage> ClickAddUnitBtnAsync()
    {
        await AddUnitBtn.ClickAsync();
        await Page.Locator("//h1[contains(@id,'title')]")
            .Filter(new LocatorFilterOptions { HasTextString = "Add unit" })
            .WaitForAsync();
        return new AddUnitPage(Page);
    }

    public async Task<EditUnitPage> ClickEditBtnAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var row = Page.Locator($"//table[@class='admin-units__table']//tr[td[1][normalize-space()='{code}']]");
        await row.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        var editUnitPage = new EditUnitPage(Page);
        await editUnitPage.WaitForLoadedAsync();
        return editUnitPage;
    }

    public async Task ClickDeactivateBtnAsync(string code)
    {
        await Grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var row = Page.Locator($"//table[@class='admin-units__table']//tr[td[1][normalize-space()='{code}']]");
        await row.GetByRole(AriaRole.Button, new() { Name = "Deactivate" }).ClickAsync();
        await DeactivateDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<string> GetDeactivateDialogTitleAsync()
    {
        await DeactivateDialogTitle.WaitForAsync();
        return (await DeactivateDialogTitle.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetDeactivateDialogMessageAsync()
    {
        await DeactivateDialogMessage.WaitForAsync();
        return (await DeactivateDialogMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public Task<bool> IsDeactivateDialogVisibleAsync() => DeactivateDialog.IsVisibleAsync();
    public Task<bool> IsDeactivateDialogCancelBtnVisibleAsync() => DeactivateDialogCancelBtn.IsVisibleAsync();
    public Task<bool> IsDeactivateDialogConfirmBtnVisibleAsync() => DeactivateDialogConfirmBtn.IsVisibleAsync();

    public async Task ClickDeactivateDialogCancelBtnAsync()
    {
        await DeactivateDialogCancelBtn.ClickAsync();
        await DeactivateDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
    }

    public async Task ClickDeactivateDialogConfirmBtnAsync(string code)
    {
        await DeactivateDialogConfirmBtn.ClickAsync();
        await DeactivateDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        var row = Page.Locator($"//table[@class='admin-units__table']//tr[td[1][normalize-space()='{code}']]");
        await row.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }
}
