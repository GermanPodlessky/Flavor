using Flavor.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using NavigationException = Flavor.Exceptions.NavigationException;
using PdfOptions = Flavor.Options.PdfOptions;
using WaitCondition = Flavor.Options.WaitCondition;

namespace Flavor.Rendering;

/// <summary>
///     Chromium-based PDF rendering engine using PuppeteerSharp.
/// </summary>
internal sealed class ChromiumEngine : IRenderEngine
{
    private readonly ILogger _logger;
    private readonly FlavorConverterOptions _options;
    private readonly BrowserPool _pool;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChromiumEngine" /> class.
    /// </summary>
    /// <param name="options">The converter options.</param>
    /// <param name="logger">The logger instance.</param>
    public ChromiumEngine(FlavorConverterOptions options, ILogger? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger.Instance;
        _pool = new BrowserPool(options, logger);
    }

    /// <summary>
    ///     Gets the browser pool statistics.
    /// </summary>
    public BrowserPoolStatistics PoolStatistics => _pool.GetStatistics();

    /// <inheritdoc />
    public bool IsInitialized => _pool.IsInitialized;

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _pool.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PdfDocument> RenderHtmlAsync(string html, PdfOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(options);

        _logger.LogDebug("Rendering HTML to PDF, length: {Length} characters", html.Length);

        await using var lease = await _pool.AcquirePageAsync(cancellationToken).ConfigureAwait(false);
        var page = lease.Page;

        try
        {
            await ConfigurePageAsync(page, options).ConfigureAwait(false);

            await page.SetContentAsync(html, new NavigationOptions
            {
                WaitUntil = [ConvertWaitCondition(options.WaitCondition)],
                Timeout = (int)options.Timeout.TotalMilliseconds
            }).ConfigureAwait(false);

            return await GeneratePdfAsync(page, options, cancellationToken).ConfigureAwait(false);
        }
        catch (PuppeteerException ex)
        {
            throw new RenderingException(
                $"Failed to render HTML to PDF: {ex.Message}",
                ex,
                html.Length,
                options.Timeout);
        }
    }

    /// <inheritdoc />
    public async Task<PdfDocument> RenderUrlAsync(string url, PdfOptions options, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        ArgumentNullException.ThrowIfNull(options);

        _logger.LogDebug("Rendering URL to PDF: {Url}", url);

        await using var lease = await _pool.AcquirePageAsync(cancellationToken).ConfigureAwait(false);
        var page = lease.Page;

        try
        {
            await ConfigurePageAsync(page, options).ConfigureAwait(false);

            var response = await page.GoToAsync(url, new NavigationOptions
            {
                WaitUntil = [ConvertWaitCondition(options.WaitCondition)],
                Timeout = (int)options.Timeout.TotalMilliseconds
            }).ConfigureAwait(false);

            if (response is { Ok: false })
                throw new NavigationException(
                    $"Failed to load URL: {url}. Status: {response.Status}",
                    url,
                    (int)response.Status);

            return await GeneratePdfAsync(page, options, cancellationToken).ConfigureAwait(false);
        }
        catch (PuppeteerException ex) when (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            throw new FlavorTimeoutException(
                $"Navigation to {url} timed out after {options.Timeout.TotalSeconds}s",
                options.Timeout,
                "Navigation");
        }
        catch (PuppeteerException ex)
        {
            throw new NavigationException(
                $"Failed to navigate to URL: {ex.Message}",
                url);
        }
    }

    /// <inheritdoc />
    public async Task WarmupAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Warming up Chromium engine with pool size {PoolSize}", _options.PoolSize);

        await _pool.WarmupAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        // Create and close a test page to verify functionality
        await using var lease = await _pool.AcquirePageAsync(cancellationToken).ConfigureAwait(false);
        await lease.Page.SetContentAsync("<html><body>Warmup</body></html>").ConfigureAwait(false);

        _logger.LogDebug("Chromium engine warmed up");
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        _logger.LogDebug("Disposing Chromium engine");
        await _pool.DisposeAsync().ConfigureAwait(false);
        _logger.LogInformation("Chromium engine disposed");
    }

    private async Task ConfigurePageAsync(IPage page, PdfOptions options)
    {
        await page.SetViewportAsync(new ViewPortOptions
        {
            Width = _options.ViewportWidth,
            Height = _options.ViewportHeight
        }).ConfigureAwait(false);

        if (!options.JavaScriptEnabled) await page.SetJavaScriptEnabledAsync(false).ConfigureAwait(false);
    }

    private async Task<PdfDocument> GeneratePdfAsync(IPage page, PdfOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Execute pre-render script if specified
        if (!string.IsNullOrEmpty(options.PreRenderScript))
        {
            _logger.LogDebug("Executing pre-render script");
            await page.EvaluateExpressionAsync(options.PreRenderScript).ConfigureAwait(false);
        }

        // Wait for additional delay if specified
        if (options.WaitDelay.HasValue && options.WaitDelay.Value > TimeSpan.Zero)
        {
            _logger.LogDebug("Waiting additional {Delay}ms before PDF generation", options.WaitDelay.Value.TotalMilliseconds);
            await Task.Delay(options.WaitDelay.Value, cancellationToken).ConfigureAwait(false);
        }

        var pdfOptions = new PuppeteerSharp.PdfOptions
        {
            Width = $"{options.PageSize.Width}in",
            Height = $"{options.PageSize.Height}in",
            MarginOptions = new MarginOptions
            {
                Top = $"{options.Margins.Top}in",
                Right = $"{options.Margins.Right}in",
                Bottom = $"{options.Margins.Bottom}in",
                Left = $"{options.Margins.Left}in"
            },
            Landscape = options.Landscape,
            PrintBackground = options.PrintBackground,
            Scale = (decimal)options.Scale,
            DisplayHeaderFooter = options.DisplayHeaderFooter,
            HeaderTemplate = options.HeaderTemplate ?? string.Empty,
            FooterTemplate = options.FooterTemplate ?? string.Empty,
            PageRanges = options.PageRanges ?? string.Empty,
            PreferCSSPageSize = options.PreferCssPageSize,
            OmitBackground = options.OmitBackground
        };

        _logger.LogDebug("Generating PDF with options: PageSize={PageSize}, Landscape={Landscape}",
            options.PageSize, options.Landscape);

        var pdfData = await page.PdfDataAsync(pdfOptions).ConfigureAwait(false);

        _logger.LogDebug("PDF generated successfully, size: {Size} bytes", pdfData.Length);

        return new PdfDocument(pdfData);
    }

    private static WaitUntilNavigation ConvertWaitCondition(WaitCondition condition)
    {
        return condition switch
        {
            WaitCondition.Load => WaitUntilNavigation.Load,
            WaitCondition.DomContentLoaded => WaitUntilNavigation.DOMContentLoaded,
            WaitCondition.NetworkIdle0 => WaitUntilNavigation.Networkidle0,
            WaitCondition.NetworkIdle2 => WaitUntilNavigation.Networkidle2,
            _ => WaitUntilNavigation.Load
        };
    }
}