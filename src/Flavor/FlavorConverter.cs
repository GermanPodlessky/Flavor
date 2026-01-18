using Flavor.Options;
using Flavor.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Flavor;

/// <summary>
///     High-performance HTML to PDF converter.
/// </summary>
/// <remarks>
///     This class is thread-safe. A single instance can be shared across multiple threads.
///     For best performance, create one instance and reuse it throughout your application's lifetime.
/// </remarks>
/// <example>
///     Simple usage:
///     <code>
/// await using var converter = new FlavorConverter();
/// var pdf = await converter.ConvertHtmlAsync("&lt;h1&gt;Hello World&lt;/h1&gt;");
/// await pdf.SaveAsync("output.pdf");
/// </code>
/// </example>
public sealed class FlavorConverter : IFlavorConverter
{
    private readonly IRenderEngine _engine;
    private readonly ILogger<FlavorConverter> _logger;
    private readonly FlavorConverterOptions _options;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorConverter" /> class with default options.
    /// </summary>
    public FlavorConverter() : this(new FlavorConverterOptions(), null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorConverter" /> class with custom options.
    /// </summary>
    /// <param name="configure">An action to configure converter options.</param>
    public FlavorConverter(Action<FlavorConverterOptions> configure) : this(CreateOptions(configure), null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorConverter" /> class with options and logger.
    /// </summary>
    /// <param name="options">The converter options.</param>
    /// <param name="logger">The logger instance.</param>
    public FlavorConverter(FlavorConverterOptions options, ILogger<FlavorConverter>? logger)
    {
        _options = options?.Clone() ?? new FlavorConverterOptions();
        _logger = logger ?? NullLoggerFactory.Instance.CreateLogger<FlavorConverter>();
        _engine = new ChromiumEngine(_options, _logger);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorConverter" /> class with a custom render engine.
    /// </summary>
    /// <param name="engine">The render engine to use.</param>
    /// <param name="options">The converter options.</param>
    /// <param name="logger">The logger instance.</param>
    /// <remarks>
    ///     Use this constructor to provide a custom rendering engine (e.g., Playwright).
    /// </remarks>
    public FlavorConverter(IRenderEngine engine, FlavorConverterOptions? options, ILogger<FlavorConverter>? logger)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _options = options?.Clone() ?? new FlavorConverterOptions();
        _logger = logger ?? NullLoggerFactory.Instance.CreateLogger<FlavorConverter>();
    }

    /// <inheritdoc />
    public BrowserPoolStatistics GetPoolStatistics()
    {
        ThrowIfDisposed();

        if (_engine is ChromiumEngine chromiumEngine) return chromiumEngine.PoolStatistics;

        // Return empty statistics for non-Chromium engines
        return new BrowserPoolStatistics();
    }

    /// <summary>
    ///     Converts HTML content to a PDF document.
    /// </summary>
    /// <param name="html">The HTML string to convert. Supports full HTML5 and CSS3.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="html" /> is null or empty.</exception>
    /// <exception cref="Exceptions.RenderingException">Thrown when PDF generation fails.</exception>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync("&lt;h1&gt;Hello&lt;/h1&gt;");
    /// await pdf.SaveAsync("output.pdf");
    /// </code>
    /// </example>
    public Task<PdfDocument> ConvertHtmlAsync(string html, CancellationToken cancellationToken = default)
    {
        return ConvertHtmlAsync(html, _options.DefaultPdfOptions, cancellationToken);
    }

    /// <summary>
    ///     Converts HTML content to a PDF document with custom options.
    /// </summary>
    /// <param name="html">The HTML string to convert. Supports full HTML5 and CSS3.</param>
    /// <param name="options">PDF generation settings.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="html" /> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options" /> is null.</exception>
    /// <exception cref="Exceptions.RenderingException">Thrown when PDF generation fails.</exception>
    public Task<PdfDocument> ConvertHtmlAsync(string html, PdfOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(html))
            throw new ArgumentException("HTML content cannot be null or empty.", nameof(html));

        ArgumentNullException.ThrowIfNull(options);

        _logger.LogDebug("Converting HTML to PDF, length: {Length} characters", html.Length);

        return _engine.RenderHtmlAsync(html, options, cancellationToken);
    }

    /// <summary>
    ///     Converts HTML content to a PDF document using a fluent options builder.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync(html, options => options
    ///     .WithPageSize(PageSize.A4)
    ///     .WithMargins(Margins.Narrow)
    ///     .WithBackground(true));
    /// </code>
    /// </example>
    public Task<PdfDocument> ConvertHtmlAsync(string html, Action<PdfOptionsBuilder> configure, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new PdfOptionsBuilder();
        configure(builder);

        return ConvertHtmlAsync(html, builder.Build(), cancellationToken);
    }

    /// <summary>
    ///     Converts a URL to a PDF document.
    /// </summary>
    /// <param name="url">The URL to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="url" /> is null or empty.</exception>
    /// <exception cref="Exceptions.NavigationException">Thrown when the URL fails to load.</exception>
    /// <exception cref="Exceptions.RenderingException">Thrown when PDF generation fails.</exception>
    public Task<PdfDocument> ConvertUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        return ConvertUrlAsync(url, _options.DefaultPdfOptions, cancellationToken);
    }

    /// <summary>
    ///     Converts a URL to a PDF document with custom options.
    /// </summary>
    /// <param name="url">The URL to convert.</param>
    /// <param name="options">PDF generation settings.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="url" /> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options" /> is null.</exception>
    /// <exception cref="Exceptions.NavigationException">Thrown when the URL fails to load.</exception>
    /// <exception cref="Exceptions.RenderingException">Thrown when PDF generation fails.</exception>
    public Task<PdfDocument> ConvertUrlAsync(string url, PdfOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));

        ArgumentNullException.ThrowIfNull(options);

        _logger.LogDebug("Converting URL to PDF: {Url}", url);

        return _engine.RenderUrlAsync(url, options, cancellationToken);
    }

    /// <summary>
    ///     Converts a URL to a PDF document using a fluent options builder.
    /// </summary>
    /// <param name="url">The URL to convert.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    public Task<PdfDocument> ConvertUrlAsync(string url, Action<PdfOptionsBuilder> configure, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new PdfOptionsBuilder();
        configure(builder);

        return ConvertUrlAsync(url, builder.Build(), cancellationToken);
    }

    /// <summary>
    ///     Converts a local HTML file to a PDF document.
    /// </summary>
    /// <param name="filePath">The path to the HTML file.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath" /> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public async Task<PdfDocument> ConvertFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await ConvertFileAsync(filePath, _options.DefaultPdfOptions, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Converts a local HTML file to a PDF document with custom options.
    /// </summary>
    /// <param name="filePath">The path to the HTML file.</param>
    /// <param name="options">PDF generation settings.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath" /> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public async Task<PdfDocument> ConvertFileAsync(string filePath, PdfOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("HTML file not found.", filePath);

        var absolutePath = Path.GetFullPath(filePath);
        var fileUrl = $"file:///{absolutePath.Replace('\\', '/')}";

        _logger.LogDebug("Converting file to PDF: {FilePath}", absolutePath);

        return await _engine.RenderUrlAsync(fileUrl, options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Warms up the converter by initializing the browser.
    ///     Call this during application startup for faster first-request performance.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the warmup operation.</returns>
    /// <example>
    ///     <code>
    /// // In Program.cs or Startup.cs
    /// var converter = new FlavorConverter();
    /// await converter.WarmupAsync();
    /// </code>
    /// </example>
    public async Task WarmupAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        _logger.LogInformation("Warming up FlavorConverter");
        await _engine.WarmupAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("FlavorConverter warmed up and ready");
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _logger.LogDebug("Disposing FlavorConverter");
        await _engine.DisposeAsync().ConfigureAwait(false);
        _disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        _logger.LogDebug("Disposing FlavorConverter synchronously");
        _engine.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _disposed = true;
    }

    private static FlavorConverterOptions CreateOptions(Action<FlavorConverterOptions>? configure)
    {
        var options = new FlavorConverterOptions();
        configure?.Invoke(options);
        return options;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}