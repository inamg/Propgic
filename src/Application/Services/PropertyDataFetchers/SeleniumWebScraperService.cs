using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Propgic.Application.Services.PropertyDataFetchers;

public class SeleniumWebScraperService : IDisposable
{
    private IWebDriver? _driver;
    private readonly object _lock = new object();
    private bool _isDisposed = false;

    public SeleniumWebScraperService()
    {
        InitializeDriver();
    }

    private void InitializeDriver()
    {
        var options = new ChromeOptions();

        // Run in headless mode
        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        // Disable images and CSS for faster loading
        options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);

        try
        {
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize Chrome driver: {ex.Message}");
            throw;
        }
    }

    public async Task<string?> SearchAndGetFirstPropertyUrlAsync(string baseUrl, string searchQuery, string urlPattern)
    {
        if (_driver == null)
        {
            Console.WriteLine("WebDriver is not initialized");
            return null;
        }

        try
        {
            lock (_lock)
            {
                // Navigate to search page
                var searchUrl = $"{baseUrl}?search={Uri.EscapeDataString(searchQuery)}";
                _driver.Navigate().GoToUrl(searchUrl);
            }

            // Wait for page to load
            await Task.Delay(2000);

            lock (_lock)
            {
                // Try to find property listing links
                var linkElements = _driver.FindElements(By.TagName("a"));

                foreach (var link in linkElements)
                {
                    var href = link.GetDomProperty("href");
                    if (!string.IsNullOrEmpty(href) && href.Contains(urlPattern))
                    {
                        return href;
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during Selenium search: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetPageContentAsync(string url)
    {
        if (_driver == null)
        {
            Console.WriteLine("WebDriver is not initialized");
            return null;
        }

        try
        {
            lock (_lock)
            {
                _driver.Navigate().GoToUrl(url);
            }

            // Wait for page to load
            await Task.Delay(3000);

            string pageSource;
            lock (_lock)
            {
                pageSource = _driver.PageSource;
            }

            return pageSource;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting page content: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> SearchAndGetContentAsync(string searchUrl, string propertyAddress, string urlPattern)
    {
        if (_driver == null)
        {
            Console.WriteLine("WebDriver is not initialized");
            return null;
        }

        try
        {
            // First, use ChatGPT to get a suggested URL
            var propertyUrl = await SearchAndGetFirstPropertyUrlAsync(searchUrl, propertyAddress, urlPattern);

            if (string.IsNullOrEmpty(propertyUrl))
            {
                return null;
            }

            // Get the content from that URL
            return await GetPageContentAsync(propertyUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in search and get content: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            lock (_lock)
            {
                _driver?.Quit();
                _driver?.Dispose();
                _driver = null;
            }
            _isDisposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~SeleniumWebScraperService()
    {
        Dispose();
    }
}
