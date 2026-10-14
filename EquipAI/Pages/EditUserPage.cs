using Microsoft.Playwright;

namespace EquipAI.Pages;

public class EditUserPage : BasePage
{
    public EditUserPage(IPage page) : base(page) { }

    private ILocator BackBtn => Page.Locator("//a[contains(normalize-space(),'Back to users')]");
    private ILocator Subtitle => Page.Locator("//span[@class='page-header__subtitle']");
    private ILocator ProfileBtn => Page.Locator("//button[normalize-space()='Profile']");
    private ILocator RolesBtn => Page.Locator("//button[normalize-space()='Roles']");
    private ILocator EmailInput => Page.Locator("//input[@id='user-email']");
    private ILocator FirstNameInput => Page.Locator("//input[@id='user-first-name']");
    private ILocator LastNameInput => Page.Locator("//input[@id='user-last-name']");
    private ILocator SaveProfileBtn => Page.Locator("//button[normalize-space()='Save profile']");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    private ILocator EmailError => Page.Locator("//input[@id='user-email']/following-sibling::span[@class='form-hint form-hint--error']");
    private ILocator FirstNameError => Page.Locator("//input[@id='user-first-name']/following-sibling::span[@class='form-hint form-hint--error']");
    private ILocator LastNameError => Page.Locator("//input[@id='user-last-name']/following-sibling::span[@class='form-hint form-hint--error']");
    private ILocator AlertMessage => Page.Locator("//div[@class='alert__body']");
    private ILocator RolesMessage => Page.Locator("//p[@class='admin-user-edit__roles-lead']");
    private ILocator RoleOptions => Page.Locator("//p[@class='admin-user-edit__roles-lead']/following-sibling::fieldset/label");

    public async Task WaitForLoadedAsync()
    {
        await Page.Locator("//h1[@id='admin-user-edit-title']")
            .Filter(new LocatorFilterOptions { HasTextString = "Edit User" })
            .WaitForAsync();
    }

    public async Task WaitForEmailAsync(string email)
    {
        await EmailInput.WaitForAsync();
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            var value = (await EmailInput.InputValueAsync()).Trim();
            if (value.Equals(email, StringComparison.OrdinalIgnoreCase))
                return;

            await Task.Delay(100);
        }

        throw new TimeoutException($"Email input was not loaded with '{email}'.");
    }

    public new async Task<string> GetPageTitleAsync()
    {
        var title = Page.Locator("//h1[@id='admin-user-edit-title']");
        await title.WaitForAsync();
        return await title.EvaluateAsync<string>(
            """
            el => {
              const clone = el.cloneNode(true);
              clone.querySelectorAll('.page-header__subtitle').forEach(node => node.remove());
              return (clone.textContent || '').replace(/\s+/g, ' ').trim();
            }
            """);
    }

    public Task<bool> IsBackBtnVisibleAsync() => IsVisibleAsync(BackBtn);

    public async Task<string> GetSubtitleAsync()
    {
        await Subtitle.WaitForAsync();
        return (await Subtitle.InnerTextAsync()).Trim();
    }

    public Task<bool> IsProfileBtnVisibleAsync() => IsVisibleAsync(ProfileBtn);

    public async Task<bool> IsProfileBtnSelectedAsync()
    {
        await ProfileBtn.WaitForAsync();
        var className = await ProfileBtn.GetAttributeAsync("class") ?? string.Empty;
        return className.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Contains("tabs__item--active");
    }

    public async Task<string> GetEmailAsync()
    {
        await EmailInput.WaitForAsync();
        return (await EmailInput.InputValueAsync()).Trim();
    }

    public async Task<string> GetFirstNameAsync()
    {
        await FirstNameInput.WaitForAsync();
        return await FirstNameInput.InputValueAsync();
    }

    public async Task<string> GetLastNameAsync()
    {
        await LastNameInput.WaitForAsync();
        return await LastNameInput.InputValueAsync();
    }

    public Task<bool> IsSaveProfileBtnVisibleAsync() => IsVisibleAsync(SaveProfileBtn);
    public Task<bool> IsCancelBtnVisibleAsync() => IsVisibleAsync(CancelBtn);

    public async Task<UsersPage> ClickBackBtnAsync()
    {
        await BackBtn.ClickAsync();
        var usersPage = new UsersPage(Page);
        await usersPage.WaitForLoadedAsync();
        return usersPage;
    }

    public async Task ClickSaveProfileBtnAsync() =>
        await SaveProfileBtn.ClickAsync();

    public async Task ClearEmailAsync() =>
        await EmailInput.ClearAsync();

    public async Task FillEmailAsync(string email) =>
        await EmailInput.FillAsync(email);

    public async Task FillFirstNameAsync(string firstName) =>
        await FirstNameInput.FillAsync(firstName);

    public async Task FillLastNameAsync(string lastName) =>
        await LastNameInput.FillAsync(lastName);

    public async Task<string> GetEmailErrorAsync()
    {
        await EmailError.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await EmailError.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetFirstNameErrorAsync()
    {
        await FirstNameError.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await FirstNameError.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetLastNameErrorAsync()
    {
        await LastNameError.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await LastNameError.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task<string> GetAlertMessageAsync()
    {
        await AlertMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return (await AlertMessage.TextContentAsync())?.Trim() ?? string.Empty;
    }

    public async Task SelectRoleAsync(string roleName)
    {
        var option = RoleOptions.Filter(new LocatorFilterOptions
        {
            HasTextRegex = new System.Text.RegularExpressions.Regex($@"^\s*{System.Text.RegularExpressions.Regex.Escape(roleName)}\s*$")
        });
        var checkbox = option.Locator("input[type='checkbox']");
        if (!await checkbox.IsCheckedAsync())
            await option.ClickAsync();

        if (!await checkbox.IsCheckedAsync())
            await option.ClickAsync();
    }

    public async Task<UsersPage> ClickCancelBtnAsync()
    {
        var cancelBtn = Page.Locator("//p[@class='admin-user-edit__roles-lead']/parent::*//button[normalize-space()='Cancel']");
        await cancelBtn.ClickAsync();
        var usersPage = new UsersPage(Page);
        await usersPage.WaitForLoadedAsync();
        return usersPage;
    }

    public async Task<UsersPage> SaveRolesAsync()
    {
        var saveRolesBtn = Page.Locator("//button[normalize-space()='Save roles']");
        var saved = Page.WaitForResponseAsync(
            response => response.Ok && response.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = 15_000 });
        await saveRolesBtn.ClickAsync();
        await saved;
        var usersPage = new UsersPage(Page);
        await usersPage.WaitForLoadedAsync();
        return usersPage;
    }

    public async Task<UsersPage> SaveProfileAsync()
    {
        await SaveProfileBtn.ClickAsync();
        var usersPage = new UsersPage(Page);
        await usersPage.WaitForLoadedAsync();
        return usersPage;
    }

    public async Task ClickRolesBtnAsync()
    {
        await RolesBtn.ClickAsync();
        await RolesMessage.WaitForAsync();
    }

    public async Task<string> GetRolesMessageAsync()
    {
        await RolesMessage.WaitForAsync();
        return (await RolesMessage.InnerTextAsync()).Trim();
    }

    public async Task<IReadOnlyList<string>> GetRoleOptionNamesAsync()
    {
        await RoleOptions.First.WaitForAsync();
        var names = await RoleOptions.AllInnerTextsAsync();
        return names
            .Select(name => System.Text.RegularExpressions.Regex.Replace(name.Replace('\u00A0', ' '), @"\s+", " ").Trim())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();
    }

    public async Task<bool> RoleCheckboxesAreUncheckedAsync()
    {
        var count = await RoleOptions.CountAsync();
        if (count == 0)
            return false;

        for (var i = 0; i < count; i++)
        {
            var checkbox = RoleOptions.Nth(i).Locator("input[type='checkbox']");
            if (await checkbox.CountAsync() == 0 || !await checkbox.IsVisibleAsync() || await checkbox.IsCheckedAsync())
                return false;
        }

        return true;
    }

    private static async Task<bool> IsVisibleAsync(ILocator locator)
    {
        try
        {
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10_000
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
