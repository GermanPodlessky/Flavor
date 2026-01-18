using Flavor.Options;
using Microsoft.AspNetCore.Mvc;

namespace Flavor.AspNetCore;

/// <summary>
///     An <see cref="IActionResult" /> that renders HTML content as a PDF file.
/// </summary>
/// <example>
///     <code>
/// public IActionResult GetReport()
/// {
///     var html = "&lt;h1&gt;Report&lt;/h1&gt;";
///     return new PdfResult(html, "report.pdf");
/// }
/// </code>
/// </example>
public class PdfResult : IActionResult
{
    private readonly string? _fileName;
    private readonly string _html;
    private readonly bool _inline;
    private readonly PdfOptions _options;

    /// <summary>
    ///     Initializes a new instance of <see cref="PdfResult" /> with HTML content.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download. If null, displays inline.</param>
    public PdfResult(string html, string? fileName = null)
        : this(html, fileName, new PdfOptions())
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="PdfResult" /> with HTML content and options.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download. If null, displays inline.</param>
    /// <param name="options">The PDF generation options.</param>
    public PdfResult(string html, string? fileName, PdfOptions options)
    {
        _html = html ?? throw new ArgumentNullException(nameof(html));
        _fileName = fileName;
        _options = options ?? new PdfOptions();
        _inline = fileName == null;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="PdfResult" /> with HTML content and a builder action.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    public PdfResult(string html, string? fileName, Action<PdfOptionsBuilder> configure)
    {
        _html = html ?? throw new ArgumentNullException(nameof(html));
        _fileName = fileName;
        _inline = fileName == null;

        var builder = new PdfOptionsBuilder();
        configure?.Invoke(builder);
        _options = builder.Build();
    }

    /// <inheritdoc />
    public async Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var converter = context.HttpContext.RequestServices.GetService(typeof(IFlavorConverter)) as IFlavorConverter
                        ?? throw new InvalidOperationException(
                            "IFlavorConverter is not registered. Call services.AddFlavor() in your DI configuration.");

        var pdf = await converter.ConvertHtmlAsync(_html, _options, context.HttpContext.RequestAborted);
        var pdfBytes = pdf.ToBytes();

        var response = context.HttpContext.Response;
        response.ContentType = "application/pdf";
        response.ContentLength = pdfBytes.Length;

        var contentDisposition = _inline
            ? "inline"
            : $"attachment; filename=\"{_fileName}\"";
        response.Headers.ContentDisposition = contentDisposition;

        await response.Body.WriteAsync(pdfBytes, context.HttpContext.RequestAborted);
    }
}