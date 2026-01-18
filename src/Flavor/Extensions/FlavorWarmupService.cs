using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Flavor.Extensions;

/// <summary>
///     A hosted service that warms up the Flavor converter during application startup.
/// </summary>
/// <remarks>
///     This service initializes the browser instance during application startup,
///     which significantly reduces the latency of the first PDF generation request.
/// </remarks>
internal sealed class FlavorWarmupService : IHostedService
{
    private readonly IFlavorConverter _converter;
    private readonly ILogger<FlavorWarmupService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorWarmupService" /> class.
    /// </summary>
    /// <param name="converter">The Flavor converter instance.</param>
    /// <param name="logger">The logger instance.</param>
    public FlavorWarmupService(IFlavorConverter converter, ILogger<FlavorWarmupService> logger)
    {
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Flavor warmup service");

        try
        {
            await _converter.WarmupAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Flavor warmup completed successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Flavor warmup was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flavor warmup failed. First PDF request may experience higher latency");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Disposal is handled by the DI container
        return Task.CompletedTask;
    }
}