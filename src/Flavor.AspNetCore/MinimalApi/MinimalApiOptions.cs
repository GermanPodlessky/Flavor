using Flavor.Options;

namespace Flavor.AspNetCore.MinimalApi;

/// <summary>
///     Configuration options for Flavor Minimal API integration.
/// </summary>
public sealed class MinimalApiOptions
{
    /// <summary>
    ///     Gets or sets the default file name for PDF downloads.
    ///     Default is "document.pdf".
    /// </summary>
    public string DefaultFileName { get; set; } = "document.pdf";

    /// <summary>
    ///     Gets or sets whether to display PDF inline by default (true) or as attachment (false).
    ///     Default is false (attachment).
    /// </summary>
    public bool DefaultInline { get; set; }

    /// <summary>
    ///     Gets or sets the default PDF generation options.
    /// </summary>
    public PdfOptions? DefaultPdfOptions { get; set; }

    /// <summary>
    ///     Gets or sets custom response headers to add to all PDF responses.
    /// </summary>
    public Dictionary<string, string> CustomHeaders { get; set; } = new();

    /// <summary>
    ///     Gets or sets the cache policy for PDF responses.
    /// </summary>
    public PdfCachePolicy CachePolicy { get; set; } = new();

    /// <summary>
    ///     Gets or sets whether to include the X-Pdf-Page-Count header in responses.
    ///     Default is false.
    /// </summary>
    public bool IncludePageCountHeader { get; set; }

    /// <summary>
    ///     Gets or sets the content type for PDF responses.
    ///     Default is "application/pdf".
    /// </summary>
    public string ContentType { get; set; } = "application/pdf";
}

/// <summary>
///     Cache policy configuration for PDF responses.
/// </summary>
public sealed class PdfCachePolicy
{
    /// <summary>
    ///     Gets or sets whether caching is enabled.
    ///     Default is false.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     Gets or sets the max-age value in seconds for Cache-Control header.
    ///     Default is 0.
    /// </summary>
    public int MaxAgeSeconds { get; set; }

    /// <summary>
    ///     Gets or sets whether the cache should be private.
    ///     Default is true.
    /// </summary>
    public bool Private { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether the response must be revalidated.
    ///     Default is false.
    /// </summary>
    public bool MustRevalidate { get; set; }

    /// <summary>
    ///     Gets or sets the Vary header value.
    /// </summary>
    public string? VaryHeader { get; set; }

    /// <summary>
    ///     Builds the Cache-Control header value based on the configured policy.
    /// </summary>
    /// <returns>The Cache-Control header value string.</returns>
    public string BuildCacheControlHeader()
    {
        if (!Enabled)
            return "no-store, no-cache";

        var parts = new List<string>();

        if (Private)
            parts.Add("private");
        else
            parts.Add("public");

        if (MaxAgeSeconds > 0)
            parts.Add($"max-age={MaxAgeSeconds}");

        if (MustRevalidate)
            parts.Add("must-revalidate");

        return string.Join(", ", parts);
    }
}
