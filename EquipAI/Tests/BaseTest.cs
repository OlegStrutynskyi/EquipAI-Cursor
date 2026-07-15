using EquipAI.Fixtures;
using EquipAI.Utils;

namespace EquipAI.Tests;

public class BaseTest
{
    private static PlaywrightFixture? _authenticatedFixture;

    protected PlaywrightFixture Fixture { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _authenticatedFixture = new PlaywrightFixture();
        await _authenticatedFixture.InitializeAsync();
        await SetUserCapabilitiesAndLoginAsync(_authenticatedFixture, 1);
    }

    [SetUp]
    public Task SetUp()
    {
        Fixture = _authenticatedFixture
            ?? throw new InvalidOperationException("Authenticated fixture is not initialized.");
        return Task.CompletedTask;
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_authenticatedFixture is not null)
        {
            await _authenticatedFixture.DisposeAsync();
            _authenticatedFixture = null;
        }
    }

    protected static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+{}[];'\\\":|,.<>?-=/";
        return new string(Enumerable.Range(0, length).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }

    protected static Task SetUserCapabilitiesAndLoginAsync(PlaywrightFixture fixture, int capabilities) =>
        SetUserCapabilitiesAndLoginAsync(fixture, Config.MicrosoftEmail, capabilities);

    protected Task SetUserCapabilitiesAndLoginAsync(int capabilities) =>
        SetUserCapabilitiesAndLoginAsync(Fixture, capabilities);

    private static async Task SetUserCapabilitiesAndLoginAsync(
        PlaywrightFixture fixture,
        string email,
        int capabilities)
    {
        await SqlHelper.SetUserCapabilitiesAsync(email, capabilities);
        await MicrosoftLoginHelper.LoginAsync(fixture);
    }
}
