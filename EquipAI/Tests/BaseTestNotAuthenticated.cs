using EquipAI.Fixtures;

namespace EquipAI.Tests;

public class BaseTestNotAuthenticated
{
    protected PlaywrightFixture Fixture { get; private set; } = null!;

    [SetUp]
    public async Task SetUp()
    {
        Fixture = new PlaywrightFixture();
        await Fixture.InitializeAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await Fixture.DisposeAsync();
    }
}
