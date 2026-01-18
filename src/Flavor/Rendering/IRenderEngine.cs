using Flavor.Options;

namespace Flavor.Rendering;

/// <summary>
///     Defines the contract for PDF rendering engines.
/// </summary>
public interface IRenderEngine : IAsyncDisposable
{
    /// <summary>
    ///     Gets a value indicating whether the engine is initialized and ready.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    ///     Initializes the rendering engine.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the initialization operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Renders HTML content to PDF.
    /// </summary>
    /// <param name="html">The HTML content to render.</param>
    /// <param name="options">The PDF generation options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task containing the generated PDF document.</returns>
    Task<PdfDocument> RenderHtmlAsync(string html, PdfOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Renders a URL to PDF.
    /// </summary>
    /// <param name="url">The URL to render.</param>
    /// <param name="options">The PDF generation options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task containing the generated PDF document.</returns>
    Task<PdfDocument> RenderUrlAsync(string url, PdfOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Warms up the engine by performing any necessary initialization.
    ///     Call this during application startup for faster first-request performance.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the warmup operation.</returns>
    Task WarmupAsync(CancellationToken cancellationToken = default);
}