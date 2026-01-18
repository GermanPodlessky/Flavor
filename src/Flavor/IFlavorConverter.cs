using Flavor.Options;
using Flavor.Rendering;

namespace Flavor;

/// <summary>
///     Defines the contract for HTML to PDF conversion operations.
/// </summary>
/// <remarks>
///     Use this interface for dependency injection scenarios.
///     Implementations are thread-safe and can be registered as singletons.
/// </remarks>
public interface IFlavorConverter : IAsyncDisposable, IDisposable
{
    /// <summary>
    ///     Gets the browser pool statistics.
    /// </summary>
    /// <returns>Statistics about the browser pool.</returns>
    BrowserPoolStatistics GetPoolStatistics();

    /// <summary>
    ///     Converts HTML content to a PDF document.
    /// </summary>
    /// <param name="html">The HTML string to convert. Supports full HTML5 and CSS3.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="html" /> is null or empty.</exception>
    /// <exception cref="Exceptions.RenderingException">Thrown when PDF generation fails.</exception>
    Task<PdfDocument> ConvertHtmlAsync(string html, CancellationToken cancellationToken = default);

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
    Task<PdfDocument> ConvertHtmlAsync(string html, PdfOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Converts HTML content to a PDF document using a fluent options builder.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    Task<PdfDocument> ConvertHtmlAsync(string html, Action<PdfOptionsBuilder> configure, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Converts a URL to a PDF document.
    /// </summary>
    /// <param name="url">The URL to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="url" /> is null or empty.</exception>
    /// <exception cref="Exceptions.NavigationException">Thrown when the URL fails to load.</exception>
    /// <exception cref="Exceptions.RenderingException">Thrown when PDF generation fails.</exception>
    Task<PdfDocument> ConvertUrlAsync(string url, CancellationToken cancellationToken = default);

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
    Task<PdfDocument> ConvertUrlAsync(string url, PdfOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Converts a URL to a PDF document using a fluent options builder.
    /// </summary>
    /// <param name="url">The URL to convert.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    Task<PdfDocument> ConvertUrlAsync(string url, Action<PdfOptionsBuilder> configure, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Converts a local HTML file to a PDF document.
    /// </summary>
    /// <param name="filePath">The path to the HTML file.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath" /> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    Task<PdfDocument> ConvertFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Converts a local HTML file to a PDF document with custom options.
    /// </summary>
    /// <param name="filePath">The path to the HTML file.</param>
    /// <param name="options">PDF generation settings.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath" /> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    Task<PdfDocument> ConvertFileAsync(string filePath, PdfOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Warms up the converter by initializing the browser.
    ///     Call this during application startup for faster first-request performance.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the warmup operation.</returns>
    Task WarmupAsync(CancellationToken cancellationToken = default);
}