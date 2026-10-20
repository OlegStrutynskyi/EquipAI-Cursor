using Microsoft.Playwright;

namespace EquipAI.Pages;

public class OfficesPage : BasePage
{
    public OfficesPage(IPage page) : base(page) { }

    private ILocator Title => Page.Locator("//h1[@id='admin-offices-title']");
    private ILocator Subtitle => Page.Locator("//span[@class='page-header__subtitle']");

    public async Task WaitForLoadedAsync()
    {
        await Title.WaitForAsync();
        await Subtitle.WaitForAsync();
    }

    public new async Task<string> GetPageTitleAsync()
    {
        await Title.WaitForAsync();
        return await Title.EvaluateAsync<string>(
            """
            el => {
              const clone = el.cloneNode(true);
              clone.querySelectorAll('.page-header__subtitle').forEach(node => node.remove());
              return (clone.textContent || '').replace(/\s+/g, ' ').trim();
            }
            """);
    }

    public async Task<string> GetSubtitleAsync()
    {
        await Subtitle.WaitForAsync();
        return ((await Subtitle.InnerTextAsync()) ?? string.Empty).Trim();
    }
}
