using Flavor.Options;
using Microsoft.AspNetCore.Mvc;

namespace Flavor.AspNetCore;

/// <summary>
///     An <see cref="IActionResult" /> that renders a URL as a PDF file.
/// </summary>
/// <example>
///     <code>
/// public IActionResult GetWebPage()
/// {
///     return new UrlPdfResult("https://example.com", "page.pdf");
/// }
/// </code>
/// </example>
public class UrlPdfResult : IActionResult
{
    private readonly string? _fileName;
    private readonly bool _inline;
    private readonly PdfOptions _options;
    private readonly string _url;

    /// <summary>
    ///     Initializes a new instance of <see cref="UrlPdfResult" /> with a URL.
    /// </summary>
    /// <param name="url">The URL to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download. If null, displays inline.</param>
    public UrlPdfResult(string url, string? fileName = null)
        : this(url, fileName, new PdfOptions())
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="UrlPdfResult" /> with a URL and options.
    /// </summary>
    /// <param name="url">The URL to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download. If null, displays inline.</param>
    /// <param name="options">The PDF generation options.</param>
    public UrlPdfResult(string url, string? fileName, PdfOptions options)
    {
        _url = url ?? throw new ArgumentNullException(nameof(url));
        _fileName = fileName;
        _options = options ?? new PdfOptions();
        _inline = fileName == null;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="UrlPdfResult" /> with a URL and a builder action.
    /// </summary>
    /// <param name="url">The URL to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    public UrlPdfResult(string url, string? fileName, Action<PdfOptionsBuilder> configure)
    {
        _url = url ?? throw new ArgumentNullException(nameof(url));
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

        var pdf = await converter.ConvertUrlAsync(_url, _options, context.HttpContext.RequestAborted);
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