using Flavor.Extensions;
using Flavor.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flavor.Playwright.Extensions;

/// <summary>
///     Extension methods for adding Playwright-based Flavor services to the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds Flavor with Playwright engine to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="browserType">The browser type to use.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    ///     <code>
    /// builder.Services.AddFlavorWithPlaywright(PlaywrightBrowserType.Chromium);
    /// </code>
    /// </example>
    public static IServiceCollection AddFlavorWithPlaywright(
        this IServiceCollection services,
        PlaywrightBrowserType browserType = PlaywrightBrowserType.Chromium)
    {
        return AddFlavorWithPlaywright(services, _ => { }, browserType);
    }

    /// <summary>
    ///     Adds Flavor with Playwright engine to the service collection with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action to configure converter options.</param>
    /// <param name="browserType">The browser type to use.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    ///     <code>
    /// builder.Services.AddFlavorWithPlaywright(options =>
    /// {
    ///     options.Headless = true;
    ///     options.ViewportWidth = 1920;
    /// }, PlaywrightBrowserType.Firefox);
    /// </code>
    /// </example>
    public static IServiceCollection AddFlavorWithPlaywright(
        this IServiceCollection services,
        Action<FlavorConverterOptions> configure,
        PlaywrightBrowserType browserType = PlaywrightBrowserType.Chromium)
    {
        var options = new FlavorConverterOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<IRenderEngine>(sp =>
        {
            var logger = sp.GetService<ILogger<PlaywrightEngine>>();
            return new PlaywrightEngine(options, logger, browserType);
        });

        services.AddSingleton<IFlavorConverter>(sp =>
        {
            var engine = sp.GetRequiredService<IRenderEngine>();
            var opts = sp.GetRequiredService<FlavorConverterOptions>();
            var logger = sp.GetService<ILogger<FlavorConverter>>();
            return new FlavorConverter(engine, opts, logger);
        });

        return services;
    }

    /// <summary>
    ///     Adds Flavor with Playwright engine and warmup service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="browserType">The browser type to use.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFlavorWithPlaywrightAndWarmup(
        this IServiceCollection services,
        PlaywrightBrowserType browserType = PlaywrightBrowserType.Chromium)
    {
        services.AddFlavorWithPlaywright(browserType);
        services.AddHostedService<FlavorWarmupService>();
        return services;
    }
}