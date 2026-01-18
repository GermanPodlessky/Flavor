using Flavor.Options;

namespace Flavor.Extensions;

/// <summary>
///     Extension methods for string to enable fluent PDF generation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     Converts an HTML string to a PDF document.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <example>
    ///     <code>
    /// var pdf = await "&lt;h1&gt;Hello&lt;/h1&gt;".ToPdfAsync();
    /// await pdf.SaveAsync("hello.pdf");
    /// </code>
    /// </example>
    public static Task<PdfDocument> ToPdfAsync(this string html, CancellationToken cancellationToken = default)
    {
        return Flavor.ConvertAsync(html, cancellationToken);
    }

    /// <summary>
    ///     Converts an HTML string to a PDF document with custom options.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="options">PDF generation settings.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    public static Task<PdfDocument> ToPdfAsync(this string html, PdfOptions options, CancellationToken cancellationToken = default)
    {
        return Flavor.ConvertAsync(html, options, cancellationToken);
    }

    /// <summary>
    ///     Converts an HTML string to a PDF document using fluent options configuration.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the generated PDF.</returns>
    /// <example>
    ///     <code>
    /// var pdf = await html.ToPdfAsync(opts => opts
    ///     .WithPageSize(PageSize.A4)
    ///     .WithBackground(true));
    /// </code>
    /// </example>
    public static Task<PdfDocument> ToPdfAsync(this string html, Action<PdfOptionsBuilder> configure, CancellationToken cancellationToken = default)
    {
        return Flavor.ConvertAsync(html, configure, cancellationToken);
    }

    /// <summary>
    ///     Converts an HTML string to a PDF and saves it to a file.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="outputPath">The path where the PDF will be saved.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <example>
    ///     <code>
    /// await "&lt;h1&gt;Hello&lt;/h1&gt;".ToPdfAsync("hello.pdf");
    /// </code>
    /// </example>
    public static Task ToPdfAsync(this string html, string outputPath, CancellationToken cancellationToken = default)
    {
        return Flavor.ConvertAndSaveAsync(html, outputPath, cancellationToken);
    }

    /// <summary>
    ///     Converts an HTML string to a PDF and saves it to a file with custom options.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="outputPath">The path where the PDF will be saved.</param>
    /// <param name="options">PDF generation settings.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public static Task ToPdfAsync(this string html, string outputPath, PdfOptions options, CancellationToken cancellationToken = default)
    {
        return Flavor.ConvertAndSaveAsync(html, outputPath, options, cancellationToken);
    }
}