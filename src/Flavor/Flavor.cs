using Flavor.Options;

namespace Flavor;

/// <summary>
///     Static entry point for quick PDF generation.
/// </summary>
/// <remarks>
///     Use this class for simple, one-off PDF generation.
///     For repeated conversions, create and reuse a <see cref="FlavorConverter" /> instance instead.
/// </remarks>
/// <example>
///     <code>
/// // Simplest possible usage
/// var pdf = await Flavor.ConvertAsync("&lt;h1&gt;Hello World&lt;/h1&gt;");
/// await pdf.SaveAsync("hello.pdf");
/// </code>
/// </example>
public static class Flavor
{
    /// <summary>
    ///     Converts HTML content to a PDF document using default options.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <example>
    ///     <code>
    /// var pdf = await Flavor.ConvertAsync("&lt;h1&gt;Hello&lt;/h1&gt;");
    /// </code>
    /// </example>
    public static async Task<PdfDocument> ConvertAsync(string html, CancellationToken cancellationToken = default)
    {
        await using var converter = new FlavorConverter();
        return await converter.ConvertHtmlAsync(html, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Converts HTML content to a PDF document with custom options.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="options">PDF generation settings.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    public static async Task<PdfDocument> ConvertAsync(string html, PdfOptions options, CancellationToken cancellationToken = default)
    {
        await using var converter = new FlavorConverter();
        return await converter.ConvertHtmlAsync(html, options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Converts HTML content to a PDF document using fluent options configuration.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <example>
    ///     <code>
    /// var pdf = await Flavor.ConvertAsync(html, options => options
    ///     .WithPageSize(PageSize.Letter)
    ///     .WithMargins(Margins.Narrow));
    /// </code>
    /// </example>
    public static async Task<PdfDocument> ConvertAsync(string html, Action<PdfOptionsBuilder> configure,
        CancellationToken cancellationToken = default)
    {
        await using var converter = new FlavorConverter();
        return await converter.ConvertHtmlAsync(html, configure, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Converts a URL to a PDF document using default options.
    /// </summary>
    /// <param name="url">The URL to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    public static async Task<PdfDocument> ConvertUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        await using var converter = new FlavorConverter();
        return await converter.ConvertUrlAsync(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Converts a URL to a PDF document with custom options.
    /// </summary>
    /// <param name="url">The URL to convert.</param>
    /// <param name="options">PDF generation settings.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    public static async Task<PdfDocument> ConvertUrlAsync(string url, PdfOptions options, CancellationToken cancellationToken = default)
    {
        await using var converter = new FlavorConverter();
        return await converter.ConvertUrlAsync(url, options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Converts HTML content and saves it directly to a file.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="outputPath">The path where the PDF will be saved.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <example>
    ///     <code>
    /// await Flavor.ConvertAndSaveAsync("&lt;h1&gt;Hello&lt;/h1&gt;", "hello.pdf");
    /// </code>
    /// </example>
    public static async Task ConvertAndSaveAsync(string html, string outputPath, CancellationToken cancellationToken = default)
    {
        await using var converter = new FlavorConverter();
        using var pdf = await converter.ConvertHtmlAsync(html, cancellationToken).ConfigureAwait(false);
        await pdf.SaveAsync(outputPath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Converts HTML content and saves it directly to a file with custom options.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="outputPath">The path where the PDF will be saved.</param>
    /// <param name="options">PDF generation settings.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public static async Task ConvertAndSaveAsync(string html, string outputPath, PdfOptions options, CancellationToken cancellationToken = default)
    {
        await using var converter = new FlavorConverter();
        using var pdf = await converter.ConvertHtmlAsync(html, options, cancellationToken).ConfigureAwait(false);
        await pdf.SaveAsync(outputPath, cancellationToken).ConfigureAwait(false);
    }
}