using Flavor.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Flavor.AspNetCore.MinimalApi;

/// <summary>
///     An <see cref="IResult" /> that renders HTML content as a PDF file for Minimal APIs.
/// </summary>
/// <example>
///     <code>
/// app.MapPost("/pdf", (string html) => new PdfFromHtmlResult(html, "report.pdf"));
/// </code>
/// </example>
public sealed class PdfFromHtmlResult : IResult
{
    private readonly Action<PdfOptionsBuilder>? _configure;
    private readonly string? _fileName;
    private readonly string _html;
    private readonly bool? _inline;
    private readonly PdfOptions? _options;

    /// <summary>
    ///     Initializes a new instance of <see cref="PdfFromHtmlResult" /> with HTML content.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download. If null, uses default from options.</param>
    /// <param name="inline">Whether to display inline. If null, uses default from options.</param>
    public PdfFromHtmlResult(string html, string? fileName = null, bool? inline = null)
    {
        _html = html ?? throw new ArgumentNullException(nameof(html));
        _fileName = fileName;
        _inline = inline;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="PdfFromHtmlResult" /> with HTML content and PDF options.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="options">The PDF generation options.</param>
    /// <param name="inline">Whether to display inline.</param>
    public PdfFromHtmlResult(string html, string? fileName, PdfOptions options, bool? inline = null)
    {
        _html = html ?? throw new ArgumentNullException(nameof(html));
        _fileName = fileName;
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _inline = inline;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="PdfFromHtmlResult" /> with HTML content and a builder action.
    /// </summary>
    /// <param name="html">The HTML content to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <param name="inline">Whether to display inline.</param>
    public PdfFromHtmlResult(string html, string? fileName, Action<PdfOptionsBuilder> configure, bool? inline = null)
    {
        _html = html ?? throw new ArgumentNullException(nameof(html));
        _fileName = fileName;
        _configure = configure ?? throw new ArgumentNullException(nameof(configure));
        _inline = inline;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var converter = httpContext.RequestServices.GetService<IFlavorConverter>()
                        ?? throw new InvalidOperationException(
                            "IFlavorConverter is not registered. Call services.AddFlavor() in your DI configuration.");

        var apiOptions = httpContext.RequestServices.GetService<IOptions<MinimalApiOptions>>()?.Value
                         ?? new MinimalApiOptions();

        var pdfOptions = GetPdfOptions(apiOptions);
        var pdf = await converter.ConvertHtmlAsync(_html, pdfOptions, httpContext.RequestAborted);

        await WritePdfResponse(httpContext, pdf, apiOptions);
    }

    private PdfOptions GetPdfOptions(MinimalApiOptions apiOptions)
    {
        if (_options != null)
            return _options;

        if (_configure != null)
        {
            var builder = new PdfOptionsBuilder();
            _configure(builder);
            return builder.Build();
        }

        return apiOptions.DefaultPdfOptions ?? new PdfOptions();
    }

    private async Task WritePdfResponse(HttpContext httpContext, PdfDocument pdf, MinimalApiOptions apiOptions)
    {
        var pdfBytes = pdf.ToBytes();
        var response = httpContext.Response;

        var fileName = _fileName ?? apiOptions.DefaultFileName;
        var inline = _inline ?? apiOptions.DefaultInline;

        response.ContentType = apiOptions.ContentType;
        response.ContentLength = pdfBytes.Length;

        var contentDisposition = inline
            ? "inline"
            : $"attachment; filename=\"{fileName}\"";
        response.Headers.ContentDisposition = contentDisposition;

        // Apply cache policy
        response.Headers.CacheControl = apiOptions.CachePolicy.BuildCacheControlHeader();
        if (!string.IsNullOrEmpty(apiOptions.CachePolicy.VaryHeader))
            response.Headers.Vary = apiOptions.CachePolicy.VaryHeader;

        // Add custom headers
        foreach (var header in apiOptions.CustomHeaders)
            response.Headers[header.Key] = header.Value;

        // Add page count header if enabled
        if (apiOptions.IncludePageCountHeader)
            response.Headers["X-Pdf-Page-Count"] = pdf.PageCount.ToString();

        await response.Body.WriteAsync(pdfBytes, httpContext.RequestAborted);
    }
}

/// <summary>
///     An <see cref="IResult" /> that renders a URL as a PDF file for Minimal APIs.
/// </summary>
/// <example>
///     <code>
/// app.MapGet("/invoice", () => new PdfFromUrlResult("https://example.com", "page.pdf"));
/// </code>
/// </example>
public sealed class PdfFromUrlResult : IResult
{
    private readonly Action<PdfOptionsBuilder>? _configure;
    private readonly string? _fileName;
    private readonly bool? _inline;
    private readonly PdfOptions? _options;
    private readonly string _url;

    /// <summary>
    ///     Initializes a new instance of <see cref="PdfFromUrlResult" /> with a URL.
    /// </summary>
    /// <param name="url">The URL to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download. If null, uses default from options.</param>
    /// <param name="inline">Whether to display inline. If null, uses default from options.</param>
    public PdfFromUrlResult(string url, string? fileName = null, bool? inline = null)
    {
        _url = url ?? throw new ArgumentNullException(nameof(url));
        _fileName = fileName;
        _inline = inline;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="PdfFromUrlResult" /> with a URL and PDF options.
    /// </summary>
    /// <param name="url">The URL to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="options">The PDF generation options.</param>
    /// <param name="inline">Whether to display inline.</param>
    public PdfFromUrlResult(string url, string? fileName, PdfOptions options, bool? inline = null)
    {
        _url = url ?? throw new ArgumentNullException(nameof(url));
        _fileName = fileName;
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _inline = inline;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="PdfFromUrlResult" /> with a URL and a builder action.
    /// </summary>
    /// <param name="url">The URL to convert to PDF.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <param name="inline">Whether to display inline.</param>
    public PdfFromUrlResult(string url, string? fileName, Action<PdfOptionsBuilder> configure, bool? inline = null)
    {
        _url = url ?? throw new ArgumentNullException(nameof(url));
        _fileName = fileName;
        _configure = configure ?? throw new ArgumentNullException(nameof(configure));
        _inline = inline;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var converter = httpContext.RequestServices.GetService<IFlavorConverter>()
                        ?? throw new InvalidOperationException(
                            "IFlavorConverter is not registered. Call services.AddFlavor() in your DI configuration.");

        var apiOptions = httpContext.RequestServices.GetService<IOptions<MinimalApiOptions>>()?.Value
                         ?? new MinimalApiOptions();

        var pdfOptions = GetPdfOptions(apiOptions);
        var pdf = await converter.ConvertUrlAsync(_url, pdfOptions, httpContext.RequestAborted);

        await WritePdfResponse(httpContext, pdf, apiOptions);
    }

    private PdfOptions GetPdfOptions(MinimalApiOptions apiOptions)
    {
        if (_options != null)
            return _options;

        if (_configure != null)
        {
            var builder = new PdfOptionsBuilder();
            _configure(builder);
            return builder.Build();
        }

        return apiOptions.DefaultPdfOptions ?? new PdfOptions();
    }

    private async Task WritePdfResponse(HttpContext httpContext, PdfDocument pdf, MinimalApiOptions apiOptions)
    {
        var pdfBytes = pdf.ToBytes();
        var response = httpContext.Response;

        var fileName = _fileName ?? apiOptions.DefaultFileName;
        var inline = _inline ?? apiOptions.DefaultInline;

        response.ContentType = apiOptions.ContentType;
        response.ContentLength = pdfBytes.Length;

        var contentDisposition = inline
            ? "inline"
            : $"attachment; filename=\"{fileName}\"";
        response.Headers.ContentDisposition = contentDisposition;

        // Apply cache policy
        response.Headers.CacheControl = apiOptions.CachePolicy.BuildCacheControlHeader();
        if (!string.IsNullOrEmpty(apiOptions.CachePolicy.VaryHeader))
            response.Headers.Vary = apiOptions.CachePolicy.VaryHeader;

        // Add custom headers
        foreach (var header in apiOptions.CustomHeaders)
            response.Headers[header.Key] = header.Value;

        // Add page count header if enabled
        if (apiOptions.IncludePageCountHeader)
            response.Headers["X-Pdf-Page-Count"] = pdf.PageCount.ToString();

        await response.Body.WriteAsync(pdfBytes, httpContext.RequestAborted);
    }
}

/// <summary>
///     An <see cref="IResult" /> that returns pre-generated PDF bytes for Minimal APIs.
/// </summary>
/// <example>
///     <code>
/// app.MapGet("/cached-pdf", () => new PdfBytesResult(pdfBytes, "cached.pdf"));
/// </code>
/// </example>
public sealed class PdfBytesResult : IResult
{
    private readonly string? _fileName;
    private readonly bool? _inline;
    private readonly byte[] _pdfBytes;

    /// <summary>
    ///     Initializes a new instance of <see cref="PdfBytesResult" /> with PDF bytes.
    /// </summary>
    /// <param name="pdfBytes">The PDF content as bytes.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="inline">Whether to display inline.</param>
    public PdfBytesResult(byte[] pdfBytes, string? fileName = null, bool? inline = null)
    {
        _pdfBytes = pdfBytes ?? throw new ArgumentNullException(nameof(pdfBytes));
        _fileName = fileName;
        _inline = inline;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var apiOptions = httpContext.RequestServices.GetService<IOptions<MinimalApiOptions>>()?.Value
                         ?? new MinimalApiOptions();

        var response = httpContext.Response;
        var fileName = _fileName ?? apiOptions.DefaultFileName;
        var inline = _inline ?? apiOptions.DefaultInline;

        response.ContentType = apiOptions.ContentType;
        response.ContentLength = _pdfBytes.Length;

        var contentDisposition = inline
            ? "inline"
            : $"attachment; filename=\"{fileName}\"";
        response.Headers.ContentDisposition = contentDisposition;

        // Apply cache policy
        response.Headers.CacheControl = apiOptions.CachePolicy.BuildCacheControlHeader();
        if (!string.IsNullOrEmpty(apiOptions.CachePolicy.VaryHeader))
            response.Headers.Vary = apiOptions.CachePolicy.VaryHeader;

        // Add custom headers
        foreach (var header in apiOptions.CustomHeaders)
            response.Headers[header.Key] = header.Value;

        await response.Body.WriteAsync(_pdfBytes, httpContext.RequestAborted);
    }
}

/// <summary>
///     An <see cref="IResult" /> that returns a <see cref="PdfDocument" /> for Minimal APIs.
/// </summary>
/// <example>
///     <code>
/// app.MapGet("/document", async (IFlavorConverter converter) =>
/// {
///     var pdf = await converter.ConvertHtmlAsync(html);
///     return new PdfDocumentResult(pdf, "report.pdf");
/// });
/// </code>
/// </example>
public sealed class PdfDocumentResult : IResult
{
    private readonly string? _fileName;
    private readonly bool? _inline;
    private readonly PdfDocument _pdf;

    /// <summary>
    ///     Initializes a new instance of <see cref="PdfDocumentResult" /> with a PDF document.
    /// </summary>
    /// <param name="pdf">The PDF document.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="inline">Whether to display inline.</param>
    public PdfDocumentResult(PdfDocument pdf, string? fileName = null, bool? inline = null)
    {
        _pdf = pdf ?? throw new ArgumentNullException(nameof(pdf));
        _fileName = fileName;
        _inline = inline;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var apiOptions = httpContext.RequestServices.GetService<IOptions<MinimalApiOptions>>()?.Value
                         ?? new MinimalApiOptions();

        var pdfBytes = _pdf.ToBytes();
        var response = httpContext.Response;

        var fileName = _fileName ?? apiOptions.DefaultFileName;
        var inline = _inline ?? apiOptions.DefaultInline;

        response.ContentType = apiOptions.ContentType;
        response.ContentLength = pdfBytes.Length;

        var contentDisposition = inline
            ? "inline"
            : $"attachment; filename=\"{fileName}\"";
        response.Headers.ContentDisposition = contentDisposition;

        // Apply cache policy
        response.Headers.CacheControl = apiOptions.CachePolicy.BuildCacheControlHeader();
        if (!string.IsNullOrEmpty(apiOptions.CachePolicy.VaryHeader))
            response.Headers.Vary = apiOptions.CachePolicy.VaryHeader;

        // Add custom headers
        foreach (var header in apiOptions.CustomHeaders)
            response.Headers[header.Key] = header.Value;

        // Add page count header if enabled
        if (apiOptions.IncludePageCountHeader)
            response.Headers["X-Pdf-Page-Count"] = _pdf.PageCount.ToString();

        await response.Body.WriteAsync(pdfBytes, httpContext.RequestAborted);
    }
}
