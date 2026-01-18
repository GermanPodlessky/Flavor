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
}