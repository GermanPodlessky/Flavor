using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Flavor.Extensions;

/// <summary>
///     Extension methods for registering Flavor services with <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds Flavor PDF conversion services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection" /> for chaining.</returns>
    /// <remarks>
    ///     This method registers <see cref="IFlavorConverter" /> as a singleton.
    ///     The converter is thread-safe and should be shared across the application.
    /// </remarks>
    /// <example>
    ///     <code>
    /// // In Program.cs
    /// builder.Services.AddFlavor();
    /// </code>
    /// </example>
    public static IServiceCollection AddFlavor(this IServiceCollection services)
    {
        return services.AddFlavor(_ => { });
    }

    /// <summary>
    ///     Adds Flavor PDF conversion services to the specified <see cref="IServiceCollection" /> with custom options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configure">An action to configure <see cref="FlavorConverterOptions" />.</param>
    /// <returns>The <see cref="IServiceCollection" /> for chaining.</returns>
    /// <remarks>
    ///     This method registers <see cref="IFlavorConverter" /> as a singleton.
    ///     The converter is thread-safe and should be shared across the application.
    /// </remarks>
    /// <example>
    ///     <code>
    /// // In Program.cs
    /// builder.Services.AddFlavor(options =>
    /// {
    ///     options.PoolSize = 4;
    ///     options.DefaultPdfOptions.PageSize = PageSize.A4;
    ///     options.DefaultPdfOptions.PrintBackground = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddFlavor(
        this IServiceCollection services,
        Action<FlavorConverterOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        // Register options
        services.TryAddSingleton(sp =>
        {
            var options = new FlavorConverterOptions();
            configure(options);
            return options;
        });

        // Register the converter as singleton
        services.TryAddSingleton<IFlavorConverter>(sp =>
        {
            var options = sp.GetRequiredService<FlavorConverterOptions>();
            var logger = sp.GetRequiredService<ILogger<FlavorConverter>>();
            return new FlavorConverter(options, logger);
        });

        return services;
    }

    /// <summary>
    ///     Adds Flavor PDF conversion services with automatic warmup on application startup.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection" /> for chaining.</returns>
    /// <remarks>
    ///     This registers a hosted service that warms up the browser during application startup,
    ///     reducing the latency of the first PDF generation request.
    /// </remarks>
    /// <example>
    ///     <code>
    /// // In Program.cs
    /// builder.Services
    ///     .AddFlavor()
    ///     .AddFlavorWarmup();
    /// </code>
    /// </example>
    public static IServiceCollection AddFlavorWarmup(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHostedService<FlavorWarmupService>();
        return services;
    }
}