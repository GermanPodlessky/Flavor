using Flavor.Exceptions;
using Flavor.Options;
using Flavor.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Playwright;

namespace Flavor.Playwright;

/// <summary>
///     Playwright-based rendering engine for PDF generation.
/// </summary>
/// <remarks>
///     This engine uses Microsoft Playwright for browser automation.
///     It supports Chromium, Firefox, and WebKit browsers.
/// </remarks>
public sealed class PlaywrightEngine : IRenderEngine
{
    private readonly PlaywrightBrowserType _browserType;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private readonly ILogger _logger;
    private readonly FlavorConverterOptions _options;
    private IBrowser? _browser;
    private bool _disposed;
    private IPlaywright? _playwright;

    /// <summary>
    ///     Initializes a new instance of <see cref="PlaywrightEngine" />.
    /// </summary>
    /// <param name="options">The converter options.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="browserType">The browser type to use.</param>
    public PlaywrightEngine(
        FlavorConverterOptions options,
        ILogger? logger = null,
        PlaywrightBrowserType browserType = PlaywrightBrowserType.Chromium)
    {
        _options = options ?? new FlavorConverterOptions();
        _logger = logger ?? NullLogger.Instance;
        _browserType = browserType;
    }

    /// <inheritdoc />
    public bool IsInitialized { get; private set; }

    /// <inheritdoc />
    public async Task<PdfDocument> RenderHtmlAsync(string html, PdfOptions options, CancellationToken cancellationToken = default)
    {
        var page = await GetPageAsync(cancellationToken);

        try
        {
            await page.SetContentAsync(html, new PageSetContentOptions
            {
                WaitUntil = MapWaitCondition(options.WaitCondition),
                Timeout = (float)options.Timeout.TotalMilliseconds
            });

            if (options.WaitDelay.HasValue && options.WaitDelay.Value > TimeSpan.Zero) await Task.Delay(options.WaitDelay.Value, cancellationToken);

            var pdfBytes = await GeneratePdfAsync(page, options, cancellationToken);
            return new PdfDocument(pdfBytes);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <inheritdoc />
    public async Task<PdfDocument> RenderUrlAsync(string url, PdfOptions options, CancellationToken cancellationToken = default)
    {
        var page = await GetPageAsync(cancellationToken);

        try
        {
            var response = await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = MapWaitCondition(options.WaitCondition),
                Timeout = (float)options.Timeout.TotalMilliseconds
            });

            if (response == null || !response.Ok) throw new NavigationException($"Failed to navigate to {url}. Status: {response?.Status}");

            if (options.WaitDelay.HasValue && options.WaitDelay.Value > TimeSpan.Zero) await Task.Delay(options.WaitDelay.Value, cancellationToken);

            var pdfBytes = await GeneratePdfAsync(page, options, cancellationToken);
            return new PdfDocument(pdfBytes);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task WarmupAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        _logger.LogInformation("Playwright engine warmed up with {BrowserType}", _browserType);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;

        _initLock.Dispose();
        _disposed = true;
    }

    private async Task<IPage> GetPageAsync(CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);

        var context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = _options.ViewportWidth,
                Height = _options.ViewportHeight
            },
            IgnoreHTTPSErrors = _options.IgnoreHttpsErrors,
            JavaScriptEnabled = true
        });

        return await context.NewPageAsync();
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_browser != null) return;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_browser != null) return;

            _logger.LogDebug("Initializing Playwright with {BrowserType}", _browserType);

            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            var browserType = _browserType switch
            {
                PlaywrightBrowserType.Firefox => _playwright.Firefox,
                PlaywrightBrowserType.WebKit => _playwright.Webkit,
                _ => _playwright.Chromium
            };

            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = _options.Headless,
                Timeout = (float)_options.BrowserLaunchTimeout.TotalMilliseconds
            };

            if (!string.IsNullOrEmpty(_options.BrowserExecutablePath)) launchOptions.ExecutablePath = _options.BrowserExecutablePath;

            if (_options.BrowserArgs?.Length > 0) launchOptions.Args = _options.BrowserArgs;

            _browser = await browserType.LaunchAsync(launchOptions);
            IsInitialized = true;
            _logger.LogInformation("Playwright browser launched: {BrowserType}", _browserType);
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task<byte[]> GeneratePdfAsync(IPage page, PdfOptions options, CancellationToken cancellationToken)
    {
        var pdfOptions = new PagePdfOptions
        {
            PrintBackground = options.PrintBackground,
            Landscape = options.Landscape,
            Scale = (float)options.Scale,
            PreferCSSPageSize = options.PreferCssPageSize
        };

        // Page size
        pdfOptions.Width = $"{options.PageSize.Width}in";
        pdfOptions.Height = $"{options.PageSize.Height}in";

        // Margins
        pdfOptions.Margin = new Margin
        {
            Top = $"{options.Margins.Top}in",
            Right = $"{options.Margins.Right}in",
            Bottom = $"{options.Margins.Bottom}in",
            Left = $"{options.Margins.Left}in"
        };

        // Header and footer
        if (!string.IsNullOrEmpty(options.HeaderTemplate) || !string.IsNullOrEmpty(options.FooterTemplate))
        {
            pdfOptions.DisplayHeaderFooter = true;
            pdfOptions.HeaderTemplate = options.HeaderTemplate ?? "<span></span>";
            pdfOptions.FooterTemplate = options.FooterTemplate ?? "<span></span>";
        }

        // Page ranges
        if (!string.IsNullOrEmpty(options.PageRanges)) pdfOptions.PageRanges = options.PageRanges;

        return await page.PdfAsync(pdfOptions);
    }

    private static WaitUntilState MapWaitCondition(WaitCondition condition)
    {
        return condition switch
        {
            WaitCondition.DomContentLoaded => WaitUntilState.DOMContentLoaded,
            WaitCondition.NetworkIdle0 => WaitUntilState.NetworkIdle,
            WaitCondition.NetworkIdle2 => WaitUntilState.NetworkIdle,
            _ => WaitUntilState.Load
        };
    }
}

/// <summary>
///     Browser types supported by Playwright.
/// </summary>
public enum PlaywrightBrowserType
{
    /// <summary>Chromium browser (default).</summary>
    Chromium,

    /// <summary>Firefox browser.</summary>
    Firefox,

    /// <summary>WebKit browser (Safari engine).</summary>
    WebKit
}