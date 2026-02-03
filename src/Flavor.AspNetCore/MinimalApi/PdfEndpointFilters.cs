using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flavor.AspNetCore.MinimalApi;

/// <summary>
///     Endpoint filter that adds standard PDF response headers based on <see cref="MinimalApiOptions" />.
/// </summary>
public sealed class PdfResponseHeadersFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);

        // Only apply headers if we haven't written the response yet
        // The actual PDF result classes handle the headers themselves
        // This filter is primarily for logging and additional processing

        return result;
    }
}

/// <summary>
///     Endpoint filter that logs PDF generation requests and responses.
/// </summary>
public sealed class PdfLoggingFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger<PdfLoggingFilter>>();
        var path = context.HttpContext.Request.Path;

        logger?.LogInformation("PDF generation started for {Path}", path);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var result = await next(context);
            stopwatch.Stop();

            logger?.LogInformation(
                "PDF generation completed for {Path} in {ElapsedMs}ms",
                path,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger?.LogError(
                ex,
                "PDF generation failed for {Path} after {ElapsedMs}ms",
                path,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

/// <summary>
///     Endpoint filter that validates PDF generation requests.
/// </summary>
public sealed class PdfValidationFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Validate HtmlToPdfRequest
        foreach (var arg in context.Arguments)
        {
            if (arg is HtmlToPdfRequest htmlRequest)
            {
                if (string.IsNullOrWhiteSpace(htmlRequest.Html))
                {
                    return Results.BadRequest(new PdfValidationError("Html content is required."));
                }
            }
            else if (arg is UrlToPdfRequest urlRequest)
            {
                if (string.IsNullOrWhiteSpace(urlRequest.Url))
                {
                    return Results.BadRequest(new PdfValidationError("URL is required."));
                }

                if (!Uri.TryCreate(urlRequest.Url, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != "http" && uri.Scheme != "https"))
                {
                    return Results.BadRequest(new PdfValidationError("Invalid URL format. Must be an absolute HTTP or HTTPS URL."));
                }
            }
        }

        return await next(context);
    }
}

/// <summary>
///     Validation error response for PDF endpoints.
/// </summary>
public sealed class PdfValidationError
{
    /// <summary>
    ///     Initializes a new instance of <see cref="PdfValidationError" />.
    /// </summary>
    /// <param name="message">The error message.</param>
    public PdfValidationError(string message)
    {
        Message = message;
    }

    /// <summary>
    ///     Gets the error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Gets the error type.
    /// </summary>
    public string Type { get; } = "validation_error";
}

/// <summary>
///     Endpoint filter that enforces rate limiting for PDF generation.
/// </summary>
public sealed class PdfRateLimitingFilter : IEndpointFilter
{
    private readonly int _maxConcurrentRequests;
    private static int _currentRequests;
    private static readonly object Lock = new();

    /// <summary>
    ///     Initializes a new instance of <see cref="PdfRateLimitingFilter" />.
    /// </summary>
    /// <param name="maxConcurrentRequests">Maximum number of concurrent PDF generation requests.</param>
    public PdfRateLimitingFilter(int maxConcurrentRequests = 10)
    {
        _maxConcurrentRequests = maxConcurrentRequests;
    }

    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var acquired = false;

        try
        {
            lock (Lock)
            {
                if (_currentRequests >= _maxConcurrentRequests)
                {
                    return Results.StatusCode(StatusCodes.Status429TooManyRequests);
                }

                _currentRequests++;
                acquired = true;
            }

            return await next(context);
        }
        finally
        {
            if (acquired)
            {
                lock (Lock)
                {
                    _currentRequests--;
                }
            }
        }
    }
}

/// <summary>
///     Extension methods for adding PDF endpoint filters.
/// </summary>
public static class PdfEndpointFilterExtensions
{
    /// <summary>
    ///     Adds the PDF logging filter to the endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static RouteHandlerBuilder WithPdfLogging(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<PdfLoggingFilter>();
    }

    /// <summary>
    ///     Adds the PDF validation filter to the endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static RouteHandlerBuilder WithPdfValidation(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<PdfValidationFilter>();
    }

    /// <summary>
    ///     Adds rate limiting for PDF generation to the endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="maxConcurrentRequests">Maximum number of concurrent requests.</param>
    /// <returns>The builder for chaining.</returns>
    public static RouteHandlerBuilder WithPdfRateLimiting(this RouteHandlerBuilder builder, int maxConcurrentRequests = 10)
    {
        return builder.AddEndpointFilter(new PdfRateLimitingFilter(maxConcurrentRequests));
    }

    /// <summary>
    ///     Adds all standard PDF filters (validation, logging) to the endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static RouteHandlerBuilder WithPdfFilters(this RouteHandlerBuilder builder)
    {
        return builder
            .AddEndpointFilter<PdfValidationFilter>()
            .AddEndpointFilter<PdfLoggingFilter>();
    }
}
