using EquipAI.Fixtures;
using EquipAI.Pages;
using Microsoft.Playwright;
using OtpNet;

namespace EquipAI.Utils;

public static class MicrosoftLoginHelper
{
    public static async Task LoginAsync(PlaywrightFixture fixture)
    {
        await fixture.Page.Context.ClearCookiesAsync();

        var loginPage = new LoginPage(fixture.Page);
        await loginPage.OpenAsync();
        await loginPage.ClickLoginBtnAsync();

        await fixture.Page.WaitForURLAsync("**/login.microsoftonline.com/**");

        await fixture.Page.Locator("input[name='loginfmt']").FillAsync(Config.MicrosoftEmail);
        await fixture.Page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();

        var passwordInput = fixture.Page.Locator("input[name='passwd']");
        await passwordInput.WaitForAsync();
        await passwordInput.FillAsync(Config.MicrosoftPassword);
        await fixture.Page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

        await CompleteMicrosoftMfaIfRequiredAsync(fixture);
        await ConfirmStaySignedInIfPromptedAsync(fixture);

        await fixture.Page.WaitForURLAsync(IsDashboardHomeUrl);

        await fixture.Page.WaitForTimeoutAsync(1_000);
    }

    private static bool IsDashboardHomeUrl(string url)
    {
        if (!url.StartsWith(Config.BaseUrl, StringComparison.OrdinalIgnoreCase) || url.Contains("login", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var path = new Uri(url).AbsolutePath.TrimEnd('/');
        return path.Length == 0;
    }

    private static async Task CompleteMicrosoftMfaIfRequiredAsync(PlaywrightFixture fixture)
    {
        var totpInput = fixture.Page.Locator("input[name='otc']");
        var staySignedInButton = fixture.Page.GetByRole(AriaRole.Button, new() { Name = "Yes" });

        for (var attempt = 0; attempt < 30; attempt++)
        {
            if (await staySignedInButton.IsVisibleAsync())
            {
                return;
            }

            if (await totpInput.IsVisibleAsync())
            {
                break;
            }

            foreach (var linkName in new[]
                     {
                         "Use a verification code",
                         "I can't use my Microsoft Authenticator app right now",
                         "Sign in another way",
                         "Use a different verification option",
                     })
            {
                var link = fixture.Page.GetByRole(AriaRole.Link, new() { Name = linkName });
                if (await link.IsVisibleAsync())
                {
                    await link.ClickAsync();
                    break;
                }
            }

            var verificationCodeButton = fixture.Page.GetByRole(AriaRole.Button, new() { Name = "Use a verification code" });
            if (await verificationCodeButton.IsVisibleAsync())
            {
                await verificationCodeButton.ClickAsync();
            }

            await fixture.Page.WaitForTimeoutAsync(1_000);
        }

        if (!await totpInput.IsVisibleAsync())
        {
            return;
        }

        var totp = new Totp(Base32Encoding.ToBytes(Config.MicrosoftSecretKey));
        await totpInput.FillAsync(totp.ComputeTotp());
        await fixture.Page.GetByRole(AriaRole.Button, new() { Name = "Verify" }).ClickAsync();
    }

    private static async Task ConfirmStaySignedInIfPromptedAsync(PlaywrightFixture fixture)
    {
        var yesButton = fixture.Page.GetByRole(AriaRole.Button, new() { Name = "Yes" });

        try
        {
            await yesButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10_000,
            });
        }
        catch (TimeoutException)
        {
            return;
        }

        await yesButton.ClickAsync();
    }
}
