using Flavor.AspNetCore.MinimalApi;
using Flavor.AspNetCore.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace Flavor.AspNetCore.Extensions;

/// <summary>
///     Extension methods for adding Flavor ASP.NET Core services to the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds Flavor ASP.NET Core services including Razor view rendering.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    ///     This method registers:
    ///     <list type="bullet">
    ///         <item><see cref="IRazorViewRenderer" /> for rendering Razor views to HTML</item>
    ///     </list>
    ///     Note: You must also call <c>services.AddFlavor()</c> to register the PDF converter.
    /// </remarks>
    /// <example>
    ///     <code>
    /// // In Program.cs
    /// builder.Services.AddFlavor();
    /// builder.Services.AddFlavorAspNetCore();
    /// </code>
    /// </example>
    public static IServiceCollection AddFlavorAspNetCore(this IServiceCollection services)
    {
        services.AddSingleton<IRazorViewRenderer, RazorViewRenderer>();
        return services;
    }

    /// <summary>
    ///     Adds Flavor ASP.NET Core services with Minimal API configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action to configure Minimal API options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    ///     This method registers:
    ///     <list type="bullet">
    ///         <item><see cref="IRazorViewRenderer" /> for rendering Razor views to HTML</item>
    ///         <item><see cref="MinimalApiOptions" /> for Minimal API configuration</item>
    ///     </list>
    ///     Note: You must also call <c>services.AddFlavor()</c> to register the PDF converter.
    /// </remarks>
    /// <example>
    ///     <code>
    /// // In Program.cs
    /// builder.Services.AddFlavor();
    /// builder.Services.AddFlavorAspNetCore(options =>
    /// {
    ///     options.DefaultFileName = "report.pdf";
    ///     options.DefaultInline = false;
    ///     options.IncludePageCountHeader = true;
    ///     options.CachePolicy = new PdfCachePolicy
    ///     {
    ///         Enabled = true,
    ///         MaxAgeSeconds = 3600,
    ///         Private = true
    ///     };
    ///     options.CustomHeaders["X-Generator"] = "Flavor";
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddFlavorAspNetCore(
        this IServiceCollection services,
        Action<MinimalApiOptions> configure)
    {
        services.AddSingleton<IRazorViewRenderer, RazorViewRenderer>();
        services.Configure(configure);
        return services;
    }

    /// <summary>
    ///     Adds Flavor ASP.NET Core services with Minimal API options instance.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The Minimal API options instance.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    ///     <code>
    /// // In Program.cs
    /// var pdfOptions = new MinimalApiOptions
    /// {
    ///     DefaultFileName = "document.pdf",
    ///     DefaultInline = true
    /// };
    /// builder.Services.AddFlavor();
    /// builder.Services.AddFlavorAspNetCore(pdfOptions);
    /// </code>
    /// </example>
    public static IServiceCollection AddFlavorAspNetCore(
        this IServiceCollection services,
        MinimalApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton<IRazorViewRenderer, RazorViewRenderer>();
        services.Configure<MinimalApiOptions>(o =>
        {
            o.DefaultFileName = options.DefaultFileName;
            o.DefaultInline = options.DefaultInline;
            o.DefaultPdfOptions = options.DefaultPdfOptions;
            o.ContentType = options.ContentType;
            o.IncludePageCountHeader = options.IncludePageCountHeader;
            o.CachePolicy = options.CachePolicy;
            foreach (var header in options.CustomHeaders)
            {
                o.CustomHeaders[header.Key] = header.Value;
            }
        });
        return services;
    }
}