using EquipAI.Pages;
using EquipAI.Utils;
using FluentAssertions;

namespace EquipAI.Tests;

public class UsersTests : BaseTest
{
    [Test]
    public async Task T01_Users_DefaultView()
    {
        const string expectedPageTitle = "Users";
        const string expectedMessage = "Manage platform accounts, capability presets, and permissions.";

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();

        (await usersPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await usersPage.GetMessageAsync()).Should().Be(expectedMessage);
        (await usersPage.IsAddUserBtnVisibleAsync()).Should().BeTrue();
        (await usersPage.IsGridVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T02_Users_ClickAddUserBtn()
    {
        const string expectedPageTitle = "Add User";

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var addUserPage = await usersPage.ClickAddUserBtnAsync();

        (await addUserPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T03_Users_Grid_Columns()
    {
        var expectedColumns = new[]
        {
            "EMAIL",
            "NAME",
            "ROLES",
            "ACTIONS",
        };

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();

        (await usersPage.GetGridColumnHeadersAsync()).Should().Equal(expectedColumns);
    }

    [Test]
    public async Task T04_Users_Grid_Records()
    {
        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();

        var expected = (await SqlHelper.GetUserGridRowsAsync())
            .Select(user => new UserGridRecord(
                user.Email,
                string.IsNullOrWhiteSpace(user.Name) ? "—" : user.Name,
                FormatRoles(user.Capabilities)))
            .ToList();
        if (expected.Count == 0)
            Assert.Pass("No Users");

        var actual = await usersPage.GetAllGridRecordsAsync();
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public async Task T05_Users_Grid_Sorting()
    {
        var columns = new[] { "EMAIL", "NAME" };

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();

        var users = (await SqlHelper.GetUserGridRowsAsync())
            .Select(ToGridRecord)
            .ToList();
        if (users.Count == 0)
            Assert.Pass("No Users");

        foreach (var column in columns)
        {
            await usersPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(usersPage, users, column, ascending: true);

            await usersPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(usersPage, users, column, ascending: false);

            await usersPage.ClickGridColumnAsync(column);
            await AssertCurrentPageSortedAsync(usersPage, users, "EMAIL", ascending: true);
        }
    }

    [Test]
    public async Task T06_Users_Grid_Search()
    {
        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();

        (await usersPage.IsSearchInputVisibleAsync()).Should().BeTrue();

        await usersPage.SearchAsync(RandomUserSearch());
        (await usersPage.GetGridEmptyMessageAsync()).Should().Be("No users match your search.");

        await AssertDisplayedUsersContainAsync(usersPage, "admin");
        await AssertDisplayedUsersContainAsync(usersPage, "str");
    }

    [Test]
    public async Task T07_Users_ClickEditBtn()
    {
        const string expectedPageTitle = "Edit User";

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var editUserPage = await usersPage.ClickEditBtnAsync(Config.TestUserEmail);

        (await editUserPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T08_Users_ClickDeleteBtn()
    {
        var email = Config.TestUserEmail2;
        const string expectedTitle = "Delete User";
        var expectedMessage = $"Delete user {email}? This cannot be undone.";

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();

        if (!await usersPage.IsUserInGridAsync(email))
            Assert.Fail("Please create a Test User first.");

        await usersPage.ClickDeleteBtnAsync(email);
        (await usersPage.IsDeleteDialogVisibleAsync()).Should().BeTrue();
        (await usersPage.GetDeleteDialogTitleAsync()).Should().Be(expectedTitle);
        (await usersPage.GetDeleteDialogMessageAsync()).Should().Be(expectedMessage);
        (await usersPage.IsDeleteDialogCancelBtnVisibleAsync()).Should().BeTrue();
        (await usersPage.IsDeleteDialogConfirmBtnVisibleAsync()).Should().BeTrue();

        await usersPage.ClickDeleteDialogCancelBtnAsync();
        (await usersPage.IsDeleteDialogVisibleAsync()).Should().BeFalse();
        (await usersPage.IsUserInGridAsync(email)).Should().BeTrue();

        await usersPage.ClickDeleteBtnAsync(email);
        await usersPage.ClickDeleteDialogConfirmBtnAsync(email);
        (await usersPage.IsDeleteDialogVisibleAsync()).Should().BeFalse();
        (await usersPage.IsUserInGridAsync(email)).Should().BeFalse();
    }

    [Test]
    public async Task T09_Users_AddUser_DefaultView()
    {
        const string expectedPageTitle = "Add User";

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var addUserPage = await usersPage.ClickAddUserBtnAsync();

        (await addUserPage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await addUserPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await addUserPage.IsEmailInputVisibleAsync()).Should().BeTrue();
        (await addUserPage.IsFirstNameInputVisibleAsync()).Should().BeTrue();
        (await addUserPage.IsLastNameInputVisibleAsync()).Should().BeTrue();
        (await addUserPage.IsRolesSectionVisibleAsync()).Should().BeTrue();
        (await addUserPage.IsCreateUserBtnVisibleAsync()).Should().BeTrue();
        (await addUserPage.IsCancelBtnVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T10_Users_AddUser_ClickBackBtn()
    {
        const string expectedPageTitle = "Users";

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var addUserPage = await usersPage.ClickAddUserBtnAsync();
        usersPage = await addUserPage.ClickBackBtnAsync();

        (await usersPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T11_Users_AddUser_EmptyFields()
    {
        const string expectedEmailError = "Email is required.";

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var addUserPage = await usersPage.ClickAddUserBtnAsync();
        await addUserPage.ClickCreateUserBtnAsync();

        (await addUserPage.GetEmailErrorAsync()).Should().Be(expectedEmailError);
    }

    [Test]
    public async Task T12_Users_AddUser_InvalidEmail()
    {
        const string expectedEmailError = "Valid email is required.";

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var addUserPage = await usersPage.ClickAddUserBtnAsync();
        await addUserPage.FillEmailAsync("aaa");
        await addUserPage.ClickCreateUserBtnAsync();

        (await addUserPage.GetEmailErrorAsync()).Should().Be(expectedEmailError);
    }

    [Test]
    public async Task T13_Users_AddUser_TakenEmail()
    {
        const string expectedAlertMessage = $"Email '{Config.TestUserEmail}' is already taken.";

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var addUserPage = await usersPage.ClickAddUserBtnAsync();
        await addUserPage.FillEmailAsync(Config.TestUserEmail);
        await addUserPage.ClickCreateUserBtnAsync();

        (await addUserPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T14_Users_AddUser_FirstNameTooLong()
    {
        const string expectedFirstNameError = "First name must be at most 128 characters.";

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var addUserPage = await usersPage.ClickAddUserBtnAsync();
        await addUserPage.FillFirstNameAsync(new string('a', 129));
        await addUserPage.ClickCreateUserBtnAsync();

        (await addUserPage.GetFirstNameErrorAsync()).Should().Be(expectedFirstNameError);
    }

    [Test]
    public async Task T15_Users_AddUser_LastNameTooLong()
    {
        const string expectedLastNameError = "Last name must be at most 128 characters.";

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var addUserPage = await usersPage.ClickAddUserBtnAsync();
        await addUserPage.FillLastNameAsync(new string('a', 129));
        await addUserPage.ClickCreateUserBtnAsync();

        (await addUserPage.GetLastNameErrorAsync()).Should().Be(expectedLastNameError);
    }

    [Test]
    public async Task T16_Users_AddUser_RolesSection()
    {
        const string expectedLegend = "Roles (optional)";
        var expectedRoles = new[]
        {
            "DataSteward",
            "ProjectManager",
            "ProjectsOperator",
            "SuperAdmin",
            "UserManager",
        };

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var addUserPage = await usersPage.ClickAddUserBtnAsync();

        (await addUserPage.GetRolesLegendAsync()).Should().Be(expectedLegend);
        (await addUserPage.EachRoleOptionHasVisibleCheckboxAsync()).Should().BeTrue();
        (await addUserPage.GetRoleOptionNamesAsync()).Should().Equal(expectedRoles);
    }

    [Test]
    public async Task T17_Users_AddUser_Success_OnlyEmail()
    {
        var email = Config.TestUserEmail4;
        var expected = new UserGridRecord(email, "—", "None");

        await SqlHelper.DeleteUserByEmailAsync(email);
        try
        {
            var usersPage = new UsersPage(Fixture.Page);
            await usersPage.OpenAsync();
            var addUserPage = await usersPage.ClickAddUserBtnAsync();
            await addUserPage.FillEmailAsync(email);
            usersPage = await addUserPage.CreateUserAsync();

            (await usersPage.GetGridRecordByEmailAsync(email)).Should().Be(expected);
        }
        finally
        {
            await SqlHelper.DeleteUserByEmailAsync(email);
        }
    }

    [Test]
    public async Task T18_Users_AddUser_Success_AllFields()
    {
        var email = Config.TestUserEmail4;
        var expectedRoles = new[]
        {
            "DataSteward",
            "ProjectManager",
            "ProjectsOperator",
            "SuperAdmin",
            "UserManager",
        };
        var expected = new UserGridRecord(
            email,
            $"{Config.TestUserFirstName3} {Config.TestUserLastName3}",
            string.Join(", ", expectedRoles));

        await SqlHelper.DeleteUserByEmailAsync(email);
        try
        {
            var usersPage = new UsersPage(Fixture.Page);
            await usersPage.OpenAsync();
            var addUserPage = await usersPage.ClickAddUserBtnAsync();
            await addUserPage.FillEmailAsync(email);
            await addUserPage.FillFirstNameAsync(Config.TestUserFirstName3);
            await addUserPage.FillLastNameAsync(Config.TestUserLastName3);
            await addUserPage.SelectRolesAsync(expectedRoles);
            usersPage = await addUserPage.CreateUserAsync();

            (await usersPage.GetGridRecordByEmailAsync(email)).Should().Be(expected);
        }
        finally
        {
            await SqlHelper.DeleteUserByEmailAsync(email);
        }
    }

    [Test]
    public async Task T19_Users_EditUser_DefaultView()
    {
        var email = Config.TestUserEmail3;
        const string expectedPageTitle = "Edit User";
        const string expectedRolesMessage = "Assign capability presets. Changes apply on save.";
        var expectedRoles = new[]
        {
            "DataSteward",
            "ProjectManager",
            "ProjectsOperator",
            "SuperAdmin",
            "UserManager",
        };

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var editUserPage = await usersPage.ClickEditBtnAsync(email);

        (await editUserPage.IsBackBtnVisibleAsync()).Should().BeTrue();
        (await editUserPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
        (await editUserPage.GetSubtitleAsync()).Should().Be(email);
        (await editUserPage.IsProfileBtnVisibleAsync()).Should().BeTrue();
        (await editUserPage.IsProfileBtnSelectedAsync()).Should().BeTrue();
        (await editUserPage.GetEmailAsync()).Should().Be(email);
        (await editUserPage.GetFirstNameAsync()).Should().BeEmpty();
        (await editUserPage.GetLastNameAsync()).Should().BeEmpty();
        (await editUserPage.IsSaveProfileBtnVisibleAsync()).Should().BeTrue();
        (await editUserPage.IsCancelBtnVisibleAsync()).Should().BeTrue();

        await editUserPage.ClickRolesBtnAsync();
        (await editUserPage.GetRolesMessageAsync()).Should().Be(expectedRolesMessage);
        (await editUserPage.GetRoleOptionNamesAsync()).Should().Equal(expectedRoles);
        (await editUserPage.RoleCheckboxesAreUncheckedAsync()).Should().BeTrue();
    }

    [Test]
    public async Task T20_Users_EditUser_ClickBackBtn()
    {
        const string expectedPageTitle = "Users";

        var editUserPage = await OpenTestUser3Async();
        var usersPage = await editUserPage.ClickBackBtnAsync();

        (await usersPage.GetPageTitleAsync()).Should().Be(expectedPageTitle);
    }

    [Test]
    public async Task T21_Users_EditUser_EmptyFields()
    {
        const string expectedEmailError = "Email is required.";

        var editUserPage = await OpenTestUser3Async();
        await editUserPage.ClearEmailAsync();
        await editUserPage.ClickSaveProfileBtnAsync();

        (await editUserPage.GetEmailErrorAsync()).Should().Be(expectedEmailError);
    }

    [Test]
    public async Task T22_Users_EditUser_InvalidEmail()
    {
        const string expectedEmailError = "Valid email is required.";

        var editUserPage = await OpenTestUser3Async();
        await editUserPage.FillEmailAsync("aaa");
        await editUserPage.ClickSaveProfileBtnAsync();

        (await editUserPage.GetEmailErrorAsync()).Should().Be(expectedEmailError);
    }

    [Test]
    public async Task T23_Users_EditUser_TakenEmail()
    {
        const string expectedAlertMessage = "Request failed.";

        var editUserPage = await OpenTestUser3Async();
        await editUserPage.FillEmailAsync(Config.TestUserEmail);
        await editUserPage.ClickSaveProfileBtnAsync();

        (await editUserPage.GetAlertMessageAsync()).Should().Be(expectedAlertMessage);
    }

    [Test]
    public async Task T24_Users_EditUser_FirstNameTooLong()
    {
        const string expectedFirstNameError = "First name must be at most 128 characters.";

        var editUserPage = await OpenTestUser3Async();
        await editUserPage.FillFirstNameAsync(new string('a', 129));
        await editUserPage.ClickSaveProfileBtnAsync();

        (await editUserPage.GetFirstNameErrorAsync()).Should().Be(expectedFirstNameError);
    }

    [Test]
    public async Task T25_Users_EditUser_LastNameTooLong()
    {
        const string expectedLastNameError = "Last name must be at most 128 characters.";

        var editUserPage = await OpenTestUser3Async();
        await editUserPage.FillLastNameAsync(new string('a', 129));
        await editUserPage.ClickSaveProfileBtnAsync();

        (await editUserPage.GetLastNameErrorAsync()).Should().Be(expectedLastNameError);
    }

    [Test]
    public async Task T26_Users_EditUser_ClickCancel()
    {
        var email = Config.TestUserEmail3;
        var roles = new[]
        {
            "DataSteward",
            "ProjectManager",
            "ProjectsOperator",
            "SuperAdmin",
            "UserManager",
        };
        var expected = new UserGridRecord(email, "—", "None");

        var userId = await SqlHelper.GetAspNetUserIdByEmailAsync(email);
        await SqlHelper.ResetAspNetUserToDefaultAsync(userId);

        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var editUserPage = await usersPage.ClickEditBtnAsync(email);
        await editUserPage.WaitForEmailAsync(email);
        await editUserPage.FillEmailAsync("testuser3updated@devessence.com");
        await editUserPage.FillFirstNameAsync(Config.TestUserFirstName3);
        await editUserPage.FillLastNameAsync(Config.TestUserLastName3);
        await editUserPage.ClickRolesBtnAsync();
        foreach (var role in roles)
            await editUserPage.SelectRoleAsync(role);
        usersPage = await editUserPage.ClickCancelBtnAsync();

        (await usersPage.GetGridRecordByEmailAsync(email)).Should().Be(expected);
    }

    [Test]
    public async Task T27_Users_EditUser_Success()
    {
        var email = Config.TestUserEmail3;
        var updatedEmail = "testuser3updated@devessence.com";
        var roles = new[]
        {
            "DataSteward",
            "ProjectManager",
            "ProjectsOperator",
            "SuperAdmin",
            "UserManager",
        };
        var expected = new UserGridRecord(
            updatedEmail,
            $"{Config.TestUserFirstName3} {Config.TestUserLastName3}",
            string.Join(", ", roles));
        string? userId = null;

        try
        {
            userId = await SqlHelper.GetAspNetUserIdByEmailAsync(email);
            await SqlHelper.ResetAspNetUserToDefaultAsync(userId);

            var usersPage = new UsersPage(Fixture.Page);
            await usersPage.OpenAsync();
            var editUserPage = await usersPage.ClickEditBtnAsync(email);
            await editUserPage.WaitForEmailAsync(email);
            await editUserPage.FillEmailAsync(updatedEmail);
            await editUserPage.FillFirstNameAsync(Config.TestUserFirstName3);
            await editUserPage.FillLastNameAsync(Config.TestUserLastName3);
            usersPage = await editUserPage.SaveProfileAsync();

            editUserPage = await usersPage.ClickEditBtnAsync(updatedEmail);
            await editUserPage.WaitForEmailAsync(updatedEmail);
            await editUserPage.ClickRolesBtnAsync();
            foreach (var role in roles)
                await editUserPage.SelectRoleAsync(role);
            usersPage = await editUserPage.SaveRolesAsync();

            (await usersPage.GetGridRecordByEmailAsync(updatedEmail)).Should().Be(expected);
        }
        finally
        {
            if (userId is not null)
                await SqlHelper.ResetAspNetUserToDefaultAsync(userId);
        }
    }

    private async Task<EditUserPage> OpenTestUser3Async()
    {
        var usersPage = new UsersPage(Fixture.Page);
        await usersPage.OpenAsync();
        var editUserPage = await usersPage.ClickEditBtnAsync(Config.TestUserEmail3);
        await editUserPage.WaitForEmailAsync(Config.TestUserEmail3);
        return editUserPage;
    }

    private static async Task AssertCurrentPageSortedAsync(
        UsersPage usersPage,
        IReadOnlyList<UserGridRecord> users,
        string column,
        bool ascending)
    {
        var actual = await usersPage.GetCurrentPageGridRecordsAsync();
        var expected = ExpectedSortedUsers(users, column, ascending).Take(actual.Count).ToList();
        actual.Should().Equal(expected, $"{column} {(ascending ? "ASC" : "DESC")}");
    }

    private static async Task AssertDisplayedUsersContainAsync(UsersPage usersPage, string term)
    {
        await usersPage.SearchAsync(term);
        var rows = await usersPage.GetAllGridRecordsAsync();
        rows.Should().NotBeEmpty();
        rows.Should().OnlyContain(row => UserRowContains(row, term));
    }

    private static bool UserRowContains(UserGridRecord row, string term) =>
        string.Join(' ', row.Email, row.Name, row.Roles)
            .Contains(term, StringComparison.OrdinalIgnoreCase);

    private static string RandomUserSearch()
    {
        var chars = GenerateRandomAlphanumericString(10).ToCharArray();
        chars[0] = (char)('a' + Random.Shared.Next(26));
        chars[1] = (char)('0' + Random.Shared.Next(10));
        return new string(chars);
    }

    private static UserGridRecord ToGridRecord(UserGridRow user) =>
        new(
            user.Email,
            string.IsNullOrWhiteSpace(user.Name) ? "—" : user.Name,
            FormatRoles(user.Capabilities));

    private static List<UserGridRecord> ExpectedSortedUsers(
        IReadOnlyList<UserGridRecord> users,
        string column,
        bool ascending)
    {
        // Email order comes from SQL (ORDER BY Email). Ordinal comparison puts
        // "user01@" before "user@" because '0' < '@', which is not how the grid sorts.
        if (column == "EMAIL")
            return ascending ? users.ToList() : users.Reverse().ToList();

        if (column != "NAME")
            throw new ArgumentOutOfRangeException(nameof(column), column, "Unknown users grid column.");

        var indexed = users.Select((user, index) => (user, index, name: SortName(user.Name)));
        var ordered = ascending
            ? indexed.OrderBy(row => row.name, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.index)
            : indexed.OrderByDescending(row => row.name, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.index);
        return ordered.Select(row => row.user).ToList();
    }

    private static string SortName(string name) =>
        name == "—" || string.IsNullOrWhiteSpace(name) ? string.Empty : name;

    private static string FormatRoles(int capabilities)
    {
        if (capabilities == 0)
            return "None";

        (int Flag, string Name)[] roles =
        [
            (2, "DataSteward"),
            (16, "ProjectManager"),
            (32, "ProjectsOperator"),
            (1, "SuperAdmin"),
            (8, "UserManager"),
        ];

        var names = new List<string>();
        foreach (var (flag, name) in roles)
        {
            if ((capabilities & flag) != flag)
                continue;

            names.Add(name);
        }

        if (names.Count == 0)
            throw new InvalidOperationException($"Unknown capabilities value {capabilities}.");

        return string.Join(", ", names);
    }
}
