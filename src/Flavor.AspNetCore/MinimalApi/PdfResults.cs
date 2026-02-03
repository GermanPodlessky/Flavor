using Flavor.Options;
using Microsoft.AspNetCore.Http;

namespace Flavor.AspNetCore.MinimalApi;

/// <summary>
///     Static factory class for creating PDF <see cref="IResult" /> instances.
///     Provides a clean, typed API similar to <see cref="Results" />.
/// </summary>
/// <example>
///     <code>
/// // In Minimal API endpoints
/// app.MapPost("/pdf/html", (HtmlRequest request) =>
///     PdfResults.FromHtml(request.Html, "report.pdf"));
///
/// app.MapGet("/pdf/url", (string url) =>
///     PdfResults.FromUrl(url, "page.pdf"));
///
/// app.MapGet("/pdf/invoice/{id}", async (int id, IFlavorConverter converter) =>
/// {
///     var html = await GenerateInvoiceHtml(id);
///     var pdf = await converter.ConvertHtmlAsync(html);
///     return PdfResults.FromDocument(pdf, $"invoice-{id}.pdf");
/// });
/// </code>
/// </example>
public static class PdfResults
{
    /// <summary>
    ///     Creates an <see cref="IResult" /> that renders HTML content as a PDF file.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download. If null, uses default from options.</param>
    /// <param name="inline">Whether to display inline instead of as attachment.</param>
    /// <returns>An <see cref="IResult" /> that produces a PDF response.</returns>
    public static IResult FromHtml(string html, string? fileName = null, bool? inline = null)
    {
        return new PdfFromHtmlResult(html, fileName, inline);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that renders HTML content as a PDF file with options.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="options">The PDF generation options.</param>
    /// <param name="inline">Whether to display inline instead of as attachment.</param>
    /// <returns>An <see cref="IResult" /> that produces a PDF response.</returns>
    public static IResult FromHtml(string html, string? fileName, PdfOptions options, bool? inline = null)
    {
        return new PdfFromHtmlResult(html, fileName, options, inline);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that renders HTML content as a PDF file with a builder action.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <param name="inline">Whether to display inline instead of as attachment.</param>
    /// <returns>An <see cref="IResult" /> that produces a PDF response.</returns>
    public static IResult FromHtml(string html, string? fileName, Action<PdfOptionsBuilder> configure, bool? inline = null)
    {
        return new PdfFromHtmlResult(html, fileName, configure, inline);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that renders a URL as a PDF file.
    /// </summary>
    /// <param name="url">The URL to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download. If null, uses default from options.</param>
    /// <param name="inline">Whether to display inline instead of as attachment.</param>
    /// <returns>An <see cref="IResult" /> that produces a PDF response.</returns>
    public static IResult FromUrl(string url, string? fileName = null, bool? inline = null)
    {
        return new PdfFromUrlResult(url, fileName, inline);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that renders a URL as a PDF file with options.
    /// </summary>
    /// <param name="url">The URL to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="options">The PDF generation options.</param>
    /// <param name="inline">Whether to display inline instead of as attachment.</param>
    /// <returns>An <see cref="IResult" /> that produces a PDF response.</returns>
    public static IResult FromUrl(string url, string? fileName, PdfOptions options, bool? inline = null)
    {
        return new PdfFromUrlResult(url, fileName, options, inline);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that renders a URL as a PDF file with a builder action.
    /// </summary>
    /// <param name="url">The URL to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <param name="inline">Whether to display inline instead of as attachment.</param>
    /// <returns>An <see cref="IResult" /> that produces a PDF response.</returns>
    public static IResult FromUrl(string url, string? fileName, Action<PdfOptionsBuilder> configure, bool? inline = null)
    {
        return new PdfFromUrlResult(url, fileName, configure, inline);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that returns pre-generated PDF bytes.
    /// </summary>
    /// <param name="pdfBytes">The PDF content as bytes.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="inline">Whether to display inline instead of as attachment.</param>
    /// <returns>An <see cref="IResult" /> that produces a PDF response.</returns>
    public static IResult FromBytes(byte[] pdfBytes, string? fileName = null, bool? inline = null)
    {
        return new PdfBytesResult(pdfBytes, fileName, inline);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that returns a pre-generated <see cref="PdfDocument" />.
    /// </summary>
    /// <param name="pdf">The PDF document.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="inline">Whether to display inline instead of as attachment.</param>
    /// <returns>An <see cref="IResult" /> that produces a PDF response.</returns>
    public static IResult FromDocument(PdfDocument pdf, string? fileName = null, bool? inline = null)
    {
        return new PdfDocumentResult(pdf, fileName, inline);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that displays a PDF inline in the browser.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <returns>An <see cref="IResult" /> that produces an inline PDF response.</returns>
    public static IResult Inline(string html)
    {
        return new PdfFromHtmlResult(html, null, inline: true);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that displays a PDF inline in the browser with options.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="options">The PDF generation options.</param>
    /// <returns>An <see cref="IResult" /> that produces an inline PDF response.</returns>
    public static IResult Inline(string html, PdfOptions options)
    {
        return new PdfFromHtmlResult(html, null, options, inline: true);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that displays a PDF inline in the browser with a builder action.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <returns>An <see cref="IResult" /> that produces an inline PDF response.</returns>
    public static IResult Inline(string html, Action<PdfOptionsBuilder> configure)
    {
        return new PdfFromHtmlResult(html, null, configure, inline: true);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that downloads a PDF as an attachment.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <returns>An <see cref="IResult" /> that produces a PDF attachment response.</returns>
    public static IResult Attachment(string html, string fileName)
    {
        return new PdfFromHtmlResult(html, fileName, inline: false);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that downloads a PDF as an attachment with options.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="options">The PDF generation options.</param>
    /// <returns>An <see cref="IResult" /> that produces a PDF attachment response.</returns>
    public static IResult Attachment(string html, string fileName, PdfOptions options)
    {
        return new PdfFromHtmlResult(html, fileName, options, inline: false);
    }

    /// <summary>
    ///     Creates an <see cref="IResult" /> that downloads a PDF as an attachment with a builder action.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <returns>An <see cref="IResult" /> that produces a PDF attachment response.</returns>
    public static IResult Attachment(string html, string fileName, Action<PdfOptionsBuilder> configure)
    {
        return new PdfFromHtmlResult(html, fileName, configure, inline: false);
    }
}
