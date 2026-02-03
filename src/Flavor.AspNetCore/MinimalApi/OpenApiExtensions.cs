using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Flavor.AspNetCore.MinimalApi;

/// <summary>
///     Extension methods for OpenAPI integration with PDF endpoints.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    ///     Adds OpenAPI metadata for PDF responses to the endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="description">Description for the PDF response.</param>
    /// <param name="operationId">Optional operation ID for OpenAPI.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example>
    ///     <code>
    /// app.MapPost("/pdf/invoice", (InvoiceRequest r) => PdfResults.FromHtml(html, "invoice.pdf"))
    ///     .WithPdfResponse("Returns an invoice PDF document", "GenerateInvoice")
    ///     .WithOpenApi();
    /// </code>
    /// </example>
    public static RouteHandlerBuilder WithPdfResponse(
        this RouteHandlerBuilder builder,
        string description = "Returns a PDF document",
        string? operationId = null)
    {
        builder.Produces<byte[]>(StatusCodes.Status200OK, "application/pdf");

        if (!string.IsNullOrEmpty(operationId))
        {
            builder.WithName(operationId);
        }

        builder.WithMetadata(new ProducesPdfAttribute { Description = description });

        return builder;
    }

    /// <summary>
    ///     Adds comprehensive OpenAPI metadata for HTML to PDF endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="operationId">Operation ID for OpenAPI.</param>
    /// <param name="summary">Short summary of the operation.</param>
    /// <param name="description">Detailed description of the operation.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example>
    ///     <code>
    /// app.MapFlavorPdfFromHtml("/pdf/html")
    ///     .WithHtmlToPdfOpenApi(
    ///         "HtmlToPdf",
    ///         "Convert HTML to PDF",
    ///         "Converts the provided HTML content to a PDF document.");
    /// </code>
    /// </example>
    public static RouteHandlerBuilder WithHtmlToPdfOpenApi(
        this RouteHandlerBuilder builder,
        string operationId,
        string summary = "Convert HTML to PDF",
        string description = "Converts HTML content to a PDF document")
    {
        return builder
            .WithName(operationId)
            .WithSummary(summary)
            .WithDescription(description)
            .Produces<byte[]>(StatusCodes.Status200OK, "application/pdf")
            .Produces<PdfValidationError>(StatusCodes.Status400BadRequest)
            .WithTags("PDF");
    }

    /// <summary>
    ///     Adds comprehensive OpenAPI metadata for URL to PDF endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="operationId">Operation ID for OpenAPI.</param>
    /// <param name="summary">Short summary of the operation.</param>
    /// <param name="description">Detailed description of the operation.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example>
    ///     <code>
    /// app.MapFlavorPdfFromUrl("/pdf/url")
    ///     .WithUrlToPdfOpenApi(
    ///         "UrlToPdf",
    ///         "Convert URL to PDF",
    ///         "Renders a web page as a PDF document.");
    /// </code>
    /// </example>
    public static RouteHandlerBuilder WithUrlToPdfOpenApi(
        this RouteHandlerBuilder builder,
        string operationId,
        string summary = "Convert URL to PDF",
        string description = "Renders a web page at the specified URL as a PDF document")
    {
        return builder
            .WithName(operationId)
            .WithSummary(summary)
            .WithDescription(description)
            .Produces<byte[]>(StatusCodes.Status200OK, "application/pdf")
            .Produces<PdfValidationError>(StatusCodes.Status400BadRequest)
            .WithTags("PDF");
    }

    /// <summary>
    ///     Adds the "PDF" tag to the endpoint for OpenAPI grouping.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static RouteHandlerBuilder WithPdfTag(this RouteHandlerBuilder builder)
    {
        return builder.WithTags("PDF");
    }

    /// <summary>
    ///     Configures the route group with standard PDF OpenAPI settings.
    /// </summary>
    /// <param name="group">The route group builder.</param>
    /// <param name="tag">The OpenAPI tag name. Default is "PDF".</param>
    /// <param name="description">Description for the tag.</param>
    /// <returns>The group builder for chaining.</returns>
    /// <example>
    ///     <code>
    /// app.MapFlavorPdfGroup("/api/pdf")
    ///     .WithPdfGroupOpenApi("PDF Generation", "Endpoints for generating PDF documents");
    /// </code>
    /// </example>
    public static RouteGroupBuilder WithPdfGroupOpenApi(
        this RouteGroupBuilder group,
        string tag = "PDF",
        string? description = null)
    {
        group.WithTags(tag);

        if (!string.IsNullOrEmpty(description))
        {
            group.WithDescription(description);
        }

        return group;
    }
}
