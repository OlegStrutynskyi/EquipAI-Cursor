using EquipAI.Utils;
using Microsoft.Playwright;

namespace EquipAI.Fixtures;

public sealed class PlaywrightFixture : IAsyncDisposable
{
    public bool Initialized { get; private set; }
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    public IPlaywright Playwright
    {
        get
        {
            EnsureInitialized();
            return _playwright!;
        }
    }

    public IBrowser Browser
    {
        get
        {
            EnsureInitialized();
            return _browser!;
        }
    }

    public IBrowserContext Context
    {
        get
        {
            EnsureInitialized();
            return _context!;
        }
    }

    public IPage Page
    {
        get
        {
            EnsureInitialized();
            return _page!;
        }
    }

    public async Task InitializeAsync()
    {
        if (Initialized)
        {
            return;
        }

        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Channel = "msedge",
            Headless = Config.Headless,
        });

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
        });
        _context.SetDefaultTimeout(30_000);
        _context.SetDefaultNavigationTimeout(30_000);
        _page = await _context.NewPageAsync();
        Initialized = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!Initialized)
        {
            return;
        }

        Initialized = false;

        if (_context != null)
        {
            await _context.CloseAsync();
            _context = null;
        }

        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;
        _page = null;
    }

    private void EnsureInitialized()
    {
        if (!Initialized || _playwright is null || _browser is null || _context is null || _page is null)
        {
            throw new InvalidOperationException(
                "PlaywrightFixture is not initialized. Call InitializeAsync() before accessing fixture resources.");
        }
    }
}
