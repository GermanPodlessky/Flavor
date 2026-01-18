using Flavor.Options;

namespace Flavor;

/// <summary>
///     Configuration options for the <see cref="FlavorConverter" /> instance.
/// </summary>
public sealed class FlavorConverterOptions
{
    /// <summary>
    ///     Gets or sets the default PDF options used when none are specified.
    /// </summary>
    public PdfOptions DefaultPdfOptions { get; set; } = new();

    /// <summary>
    ///     Gets or sets the browser executable path.
    ///     If null, Chromium will be downloaded automatically on first use.
    /// </summary>
    /// <remarks>
    ///     Set this to use an existing Chrome/Chromium installation:
    ///     - Windows: "C:\Program Files\Google\Chrome\Application\chrome.exe"
    ///     - macOS: "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"
    ///     - Linux: "/usr/bin/google-chrome" or "/usr/bin/chromium-browser"
    /// </remarks>
    public string? BrowserExecutablePath { get; set; }

    /// <summary>
    ///     Gets or sets whether to run the browser in headless mode. Default is true.
    ///     Set to false for debugging purposes.
    /// </summary>
    public bool Headless { get; set; } = true;

    /// <summary>
    ///     Gets or sets additional arguments passed to the browser on launch.
    /// </summary>
    public string[] BrowserArgs { get; set; } = [];

    /// <summary>
    ///     Gets or sets the maximum number of browser instances in the pool. Default is 1.
    ///     Increase this for high-concurrency scenarios.
    /// </summary>
    public int PoolSize { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the timeout for browser launch operations. Default is 30 seconds.
    /// </summary>
    public TimeSpan BrowserLaunchTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Gets or sets whether to automatically download Chromium if not found. Default is true.
    /// </summary>
    public bool AutoDownloadBrowser { get; set; } = true;

    /// <summary>
    ///     Gets or sets the user data directory for the browser.
    ///     If null, a temporary directory will be used.
    /// </summary>
    public string? UserDataDir { get; set; }

    /// <summary>
    ///     Gets or sets whether to ignore HTTPS errors. Default is false.
    ///     Set to true when working with self-signed certificates in development.
    /// </summary>
    public bool IgnoreHttpsErrors { get; set; }

    /// <summary>
    ///     Gets or sets the default viewport width in pixels. Default is 1920.
    /// </summary>
    public int ViewportWidth { get; set; } = 1920;

    /// <summary>
    ///     Gets or sets the default viewport height in pixels. Default is 1080.
    /// </summary>
    public int ViewportHeight { get; set; } = 1080;

    /// <summary>
    ///     Creates a copy of this options instance.
    /// </summary>
    /// <returns>A new <see cref="FlavorConverterOptions" /> instance with the same values.</returns>
    internal FlavorConverterOptions Clone()
    {
        return new FlavorConverterOptions
        {
            DefaultPdfOptions = DefaultPdfOptions.Clone(),
            BrowserExecutablePath = BrowserExecutablePath,
            Headless = Headless,
            BrowserArgs = [.. BrowserArgs],
            PoolSize = PoolSize,
            BrowserLaunchTimeout = BrowserLaunchTimeout,
            AutoDownloadBrowser = AutoDownloadBrowser,
            UserDataDir = UserDataDir,
            IgnoreHttpsErrors = IgnoreHttpsErrors,
            ViewportWidth = ViewportWidth,
            ViewportHeight = ViewportHeight
        };
    }
}