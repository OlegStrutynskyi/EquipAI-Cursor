using Microsoft.Playwright;

namespace EquipAI.Pages;

public class AddUserPage : BasePage
{
    public AddUserPage(IPage page) : base(page) { }

    private ILocator PageTitle => Page.Locator("//h1[contains(@id,'title')]")
        .Filter(new LocatorFilterOptions { HasTextString = "Add User" });
    private ILocator BackBtn => Page.Locator("//a[contains(normalize-space(),'Back to users')]");
    private ILocator EmailInput => Page.Locator("//input[@id='user-email']");
    private ILocator FirstNameInput => Page.Locator("//input[@id='user-first-name']");
    private ILocator LastNameInput => Page.Locator("//input[@id='user-last-name']");
    private ILocator RolesSection => Page.Locator("//fieldset[@class='admin-user-edit__roles']");
    private ILocator RolesLegend => Page.Locator("//fieldset[@class='admin-user-edit__roles']/legend");
    private ILocator RoleOptions => Page.Locator("//fieldset[@class='admin-user-edit__roles']/label");
    private ILocator CreateUserBtn => Page.Locator("//button[normalize-space()='Create user']");
    private ILocator CancelBtn => Page.Locator("//button[normalize-space()='Cancel']");
    private ILocator EmailError => Page.Locator("//input[@id='user-email']/following-sibling::span[@class='form-hint form-hint--error']");
    private ILocator FirstNameError => Page.Locator("//input[@id='user-first-name']/following-sibling::span[@class='form-hint form-hint--error']");
    private ILocator LastNameError => Page.Locator("//input[@id='user-last-name']/following-sibling::span[@class='form-hint form-hint--error']");
    private ILocator AlertMessage => Page.Locator("//div[@class='alert__body']");

    public async Task WaitForLoadedAsync() =>
        await PageTitle.WaitForAsync();

    public Task<bool> IsBackBtnVisibleAsync() => IsVisibleAsync(BackBtn);
    public Task<bool> IsEmailInputVisibleAsync() => IsVisibleAsync(EmailInput);
    public Task<bool> IsFirstNameInputVisibleAsync() => IsVisibleAsync(FirstNameInput);
    public Task<bool> IsLastNameInputVisibleAsync() => IsVisibleAsync(LastNameInput);
    public Task<bool> IsRolesSectionVisibleAsync() => IsVisibleAsync(RolesSection);
    public Task<bool> IsCreateUserBtnVisibleAsync() => IsVisibleAsync(CreateUserBtn);
    public Task<bool> IsCancelBtnVisibleAsync() => IsVisibleAsync(CancelBtn);

    public async Task<string> GetRolesLegendAsync()
    {
        await RolesLegend.WaitForAsync();
        return Normalize(await RolesLegend.InnerTextAsync());
    }

    public async Task<IReadOnlyList<string>> GetRoleOptionNamesAsync()
    {
        await RoleOptions.First.WaitForAsync();
        var names = await RoleOptions.AllInnerTextsAsync();
        return names.Select(Normalize).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
    }

    public async Task<bool> EachRoleOptionHasVisibleCheckboxAsync()
    {
        var count = await RoleOptions.CountAsync();
        if (count == 0)
            return false;

        for (var i = 0; i < count; i++)
        {
            var checkbox = RoleOptions.Nth(i).Locator("input[type='checkbox']");
            if (await checkbox.CountAsync() == 0 || !await checkbox.IsVisibleAsync())
                return false;
        }

        return true;
    }

    public async Task SelectRoleAsync(string roleName)
    {
        var checkbox = RoleCheckbox(roleName);
        for (var attempt = 0; attempt < 3; attempt++)
        {
            if (await IsCheckedStableAsync(checkbox))
                return;

            await checkbox.EvaluateAsync(
                """
                input => {
                  input.checked = true;
                  input.dispatchEvent(new Event('change', { bubbles: true }));
                }
                """);
            await Task.Delay(200);
        }

        if (!await IsCheckedStableAsync(checkbox))
            throw new TimeoutException($"Role '{roleName}' was not selected.");
    }

    public async Task SelectRolesAsync(IEnumerable<string> roleNames)
    {
        var roles = roleNames.ToList();
        for (var round = 0; round < 3; round++)
        {
            foreach (var role in roles)
                await SelectRoleAsync(role);

            var allChecked = true;
            foreach (var role in roles)
            {
                if (await IsCheckedStableAsync(RoleCheckbox(role)))
                    continue;

                allChecked = false;
                break;
            }

            if (allChecked)
                return;
        }

        throw new TimeoutException("Not all roles stayed selected.");
    }

    private ILocator RoleCheckbox(string roleName) =>
        RoleOptions.Filter(new LocatorFilterOptions
        {
            HasTextRegex = new System.Text.RegularExpressions.Regex($@"^\s*{System.Text.RegularExpressions.Regex.Escape(roleName)}\s*$")
        }).Locator("input[type='checkbox']");

    private static async Task<bool> IsCheckedStableAsync(ILocator checkbox)
    {
        if (!await checkbox.IsCheckedAsync())
            return false;

        await Task.Delay(200);
        return await checkbox.IsCheckedAsync();
    }

    public async Task<UsersPage> CreateUserAsync()
    {
        await CreateUserBtn.ClickAsync();
        var usersPage = new UsersPage(Page);
        await usersPage.WaitForLoadedAsync();
        return usersPage;
    }

    public async Task<UsersPage> ClickBackBtnAsync()
    {
        await BackBtn.ClickAsync();
        var usersPage = new UsersPage(Page);
        await usersPage.WaitForLoadedAsync();
        return usersPage;
    }

    public async Task ClickCreateUserBtnAsync() =>
        await CreateUserBtn.ClickAsync();

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

    private static string Normalize(string? value) =>
        System.Text.RegularExpressions.Regex.Replace((value ?? string.Empty).Replace('\u00A0', ' '), @"\s+", " ").Trim();
}
