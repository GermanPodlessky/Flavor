using Flavor.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Flavor.AspNetCore.MinimalApi;

/// <summary>
///     Extension methods for mapping PDF endpoints in Minimal APIs.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    ///     Creates a route group for PDF endpoints with shared configuration.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="prefix">The route prefix for the group. Default is "/pdf".</param>
    /// <returns>A <see cref="RouteGroupBuilder" /> for configuring PDF endpoints.</returns>
    /// <example>
    ///     <code>
    /// var pdfGroup = app.MapFlavorPdfGroup("/api/pdf")
    ///     .WithTags("PDF")
    ///     .RequireAuthorization();
    ///
    /// pdfGroup.MapPost("/html", (HtmlRequest r) => PdfResults.FromHtml(r.Html, "doc.pdf"));
    /// pdfGroup.MapPost("/url", (UrlRequest r) => PdfResults.FromUrl(r.Url, "page.pdf"));
    /// </code>
    /// </example>
    public static RouteGroupBuilder MapFlavorPdfGroup(this IEndpointRouteBuilder endpoints, string prefix = "/pdf")
    {
        var group = endpoints.MapGroup(prefix);
        group.AddEndpointFilter<PdfResponseHeadersFilter>();
        return group;
    }

    /// <summary>
    ///     Maps a POST endpoint that converts HTML to PDF.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="fileName">The default filename for the PDF download.</param>
    /// <returns>A <see cref="RouteHandlerBuilder" /> for further configuration.</returns>
    /// <example>
    ///     <code>
    /// app.MapFlavorPdfFromHtml("/api/pdf/html", "document.pdf")
    ///     .WithName("HtmlToPdf")
    ///     .WithOpenApi();
    /// </code>
    /// </example>
    public static RouteHandlerBuilder MapFlavorPdfFromHtml(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        string? fileName = null)
    {
        return endpoints.MapPost(pattern, (HtmlToPdfRequest request) =>
            PdfResults.FromHtml(request.Html, fileName ?? request.FileName))
            .WithMetadata(new ProducesPdfAttribute())
            .AddEndpointFilter<PdfResponseHeadersFilter>();
    }

    /// <summary>
    ///     Maps a POST endpoint that converts HTML to PDF with custom options.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="configure">An action to configure default PDF options.</param>
    /// <param name="fileName">The default filename for the PDF download.</param>
    /// <returns>A <see cref="RouteHandlerBuilder" /> for further configuration.</returns>
    public static RouteHandlerBuilder MapFlavorPdfFromHtml(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Action<PdfOptionsBuilder> configure,
        string? fileName = null)
    {
        return endpoints.MapPost(pattern, (HtmlToPdfRequest request) =>
            PdfResults.FromHtml(request.Html, fileName ?? request.FileName, configure))
            .WithMetadata(new ProducesPdfAttribute())
            .AddEndpointFilter<PdfResponseHeadersFilter>();
    }

    /// <summary>
    ///     Maps a POST endpoint that converts a URL to PDF.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="fileName">The default filename for the PDF download.</param>
    /// <returns>A <see cref="RouteHandlerBuilder" /> for further configuration.</returns>
    /// <example>
    ///     <code>
    /// app.MapFlavorPdfFromUrl("/api/pdf/url", "page.pdf")
    ///     .WithName("UrlToPdf")
    ///     .WithOpenApi();
    /// </code>
    /// </example>
    public static RouteHandlerBuilder MapFlavorPdfFromUrl(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        string? fileName = null)
    {
        return endpoints.MapPost(pattern, (UrlToPdfRequest request) =>
            PdfResults.FromUrl(request.Url, fileName ?? request.FileName))
            .WithMetadata(new ProducesPdfAttribute())
            .AddEndpointFilter<PdfResponseHeadersFilter>();
    }

    /// <summary>
    ///     Maps a POST endpoint that converts a URL to PDF with custom options.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="configure">An action to configure default PDF options.</param>
    /// <param name="fileName">The default filename for the PDF download.</param>
    /// <returns>A <see cref="RouteHandlerBuilder" /> for further configuration.</returns>
    public static RouteHandlerBuilder MapFlavorPdfFromUrl(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Action<PdfOptionsBuilder> configure,
        string? fileName = null)
    {
        return endpoints.MapPost(pattern, (UrlToPdfRequest request) =>
            PdfResults.FromUrl(request.Url, fileName ?? request.FileName, configure))
            .WithMetadata(new ProducesPdfAttribute())
            .AddEndpointFilter<PdfResponseHeadersFilter>();
    }

    /// <summary>
    ///     Maps a GET endpoint that converts a URL (from query) to PDF.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="fileName">The default filename for the PDF download.</param>
    /// <returns>A <see cref="RouteHandlerBuilder" /> for further configuration.</returns>
    /// <example>
    ///     <code>
    /// // GET /api/pdf/url?url=https://example.com
    /// app.MapFlavorPdfFromUrlGet("/api/pdf/url", "page.pdf")
    ///     .WithName("UrlToPdfGet")
    ///     .WithOpenApi();
    /// </code>
    /// </example>
    public static RouteHandlerBuilder MapFlavorPdfFromUrlGet(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        string? fileName = null)
    {
        return endpoints.MapGet(pattern, (string url, string? file) =>
            PdfResults.FromUrl(url, file ?? fileName))
            .WithMetadata(new ProducesPdfAttribute())
            .AddEndpointFilter<PdfResponseHeadersFilter>();
    }
}

/// <summary>
///     Request model for HTML to PDF conversion endpoints.
/// </summary>
public sealed class HtmlToPdfRequest
{
    /// <summary>
    ///     Gets or sets the HTML content to convert to PDF.
    /// </summary>
    public required string Html { get; set; }

    /// <summary>
    ///     Gets or sets the optional filename for the PDF download.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    ///     Gets or sets whether to display the PDF inline.
    /// </summary>
    public bool? Inline { get; set; }
}

/// <summary>
///     Request model for URL to PDF conversion endpoints.
/// </summary>
public sealed class UrlToPdfRequest
{
    /// <summary>
    ///     Gets or sets the URL to convert to PDF.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    ///     Gets or sets the optional filename for the PDF download.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    ///     Gets or sets whether to display the PDF inline.
    /// </summary>
    public bool? Inline { get; set; }
}

/// <summary>
///     Marker attribute indicating an endpoint produces PDF content.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class ProducesPdfAttribute : Attribute
{
    /// <summary>
    ///     Gets the content type for PDF responses.
    /// </summary>
    public string ContentType { get; } = "application/pdf";

    /// <summary>
    ///     Gets or sets the HTTP status code. Default is 200.
    /// </summary>
    public int StatusCode { get; set; } = StatusCodes.Status200OK;

    /// <summary>
    ///     Gets or sets the description for OpenAPI documentation.
    /// </summary>
    public string? Description { get; set; } = "Returns a PDF document";
}
