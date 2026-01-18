using System.Collections.Concurrent;
using Flavor.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PuppeteerSharp;

namespace Flavor.Rendering;

/// <summary>
///     Manages a pool of browser instances for efficient PDF generation.
/// </summary>
internal sealed class BrowserPool : IAsyncDisposable
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private readonly ConcurrentBag<BrowserInstance> _instances = [];
    private readonly ILogger _logger;
    private readonly FlavorConverterOptions _options;
    private readonly SemaphoreSlim _poolSemaphore;
    private readonly object _statsLock = new();
    private bool _disposed;
    private int _nextInstanceId;
    private long _totalPagesCreated;

    // Statistics
    private long _totalRequests;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BrowserPool" /> class.
    /// </summary>
    /// <param name="options">The converter options.</param>
    /// <param name="logger">The logger instance.</param>
    public BrowserPool(FlavorConverterOptions options, ILogger? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger.Instance;
        _poolSemaphore = new SemaphoreSlim(options.PoolSize, options.PoolSize);
    }

    /// <summary>
    ///     Gets the current number of browser instances in the pool.
    /// </summary>
    public int InstanceCount => _instances.Count;

    /// <summary>
    ///     Gets the configured pool size.
    /// </summary>
    public int PoolSize => _options.PoolSize;

    /// <summary>
    ///     Gets a value indicating whether the pool is initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        _logger.LogDebug("Disposing browser pool with {Count} instances", _instances.Count);

        var disposeTasks = _instances.Select(i => i.DisposeAsync().AsTask());
        await Task.WhenAll(disposeTasks).ConfigureAwait(false);

        _instances.Clear();
        _poolSemaphore.Dispose();
        _initLock.Dispose();

        _logger.LogInformation("Browser pool disposed");
    }

    /// <summary>
    ///     Gets pool statistics.
    /// </summary>
    public BrowserPoolStatistics GetStatistics()
    {
        lock (_statsLock)
        {
            var instances = _instances.ToArray();
            return new BrowserPoolStatistics
            {
                PoolSize = _options.PoolSize,
                ActiveInstances = instances.Length,
                HealthyInstances = instances.Count(i => i.IsHealthy),
                TotalRequests = _totalRequests,
                TotalPagesCreated = _totalPagesCreated,
                AvailableSlots = _poolSemaphore.CurrentCount
            };
        }
    }

    /// <summary>
    ///     Initializes the pool by downloading the browser if needed.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsInitialized) return;

        await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsInitialized) return;

            _logger.LogDebug("Initializing browser pool with size {PoolSize}", _options.PoolSize);

            if (_options.AutoDownloadBrowser && string.IsNullOrEmpty(_options.BrowserExecutablePath))
            {
                _logger.LogDebug("Downloading Chromium browser");
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync().ConfigureAwait(false);
                _logger.LogInformation("Chromium browser downloaded successfully");
            }

            IsInitialized = true;
            _logger.LogInformation("Browser pool initialized");
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    ///     Acquires a page from the pool.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A page lease that must be disposed when done.</returns>
    public async Task<PageLease> AcquirePageAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!IsInitialized) await InitializeAsync(cancellationToken).ConfigureAwait(false);

        Interlocked.Increment(ref _totalRequests);

        _logger.LogDebug("Waiting to acquire page from pool (available: {Available}/{PoolSize})",
            _poolSemaphore.CurrentCount, _options.PoolSize);

        await _poolSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var instance = await GetOrCreateInstanceAsync(cancellationToken).ConfigureAwait(false);
            var page = await instance.CreatePageAsync().ConfigureAwait(false);

            Interlocked.Increment(ref _totalPagesCreated);

            _logger.LogDebug("Page acquired from browser instance {InstanceId}", instance.Id);

            return new PageLease(page, instance, this);
        }
        catch
        {
            _poolSemaphore.Release();
            throw;
        }
    }

    /// <summary>
    ///     Warms up the pool by creating browser instances.
    /// </summary>
    /// <param name="count">Number of instances to create. Defaults to pool size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task WarmupAsync(int? count = null, CancellationToken cancellationToken = default)
    {
        if (!IsInitialized) await InitializeAsync(cancellationToken).ConfigureAwait(false);

        var targetCount = Math.Min(count ?? _options.PoolSize, _options.PoolSize);
        var currentCount = _instances.Count;

        if (currentCount >= targetCount)
        {
            _logger.LogDebug("Pool already has {Count} instances, no warmup needed", currentCount);
            return;
        }

        _logger.LogInformation("Warming up pool with {Count} browser instances", targetCount - currentCount);

        var tasks = new List<Task>();
        for (var i = currentCount; i < targetCount; i++) tasks.Add(CreateInstanceAsync(cancellationToken));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        _logger.LogInformation("Pool warmup complete, {Count} instances ready", _instances.Count);
    }

    internal void ReleasePage(BrowserInstance instance)
    {
        instance.NotifyPageClosed();
        _poolSemaphore.Release();

        _logger.LogDebug("Page released back to pool (available: {Available}/{PoolSize})",
            _poolSemaphore.CurrentCount, _options.PoolSize);
    }

    private async Task<BrowserInstance> GetOrCreateInstanceAsync(CancellationToken cancellationToken)
    {
        // Try to find a healthy instance
        foreach (var instance in _instances)
            if (instance.IsHealthy)
                return instance;

        // Create new instance if pool not full
        if (_instances.Count < _options.PoolSize) return await CreateInstanceAsync(cancellationToken).ConfigureAwait(false);

        // All instances unhealthy, try to replace one
        _logger.LogWarning("All browser instances are unhealthy, creating replacement");
        return await CreateInstanceAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<BrowserInstance> CreateInstanceAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var instanceId = Interlocked.Increment(ref _nextInstanceId);
        _logger.LogDebug("Creating browser instance {InstanceId}", instanceId);

        var launchOptions = new LaunchOptions
        {
            Headless = _options.Headless,
            ExecutablePath = _options.BrowserExecutablePath,
            Args = BuildBrowserArgs(),
            Timeout = (int)_options.BrowserLaunchTimeout.TotalMilliseconds,
            UserDataDir = _options.UserDataDir
        };

        try
        {
            var browser = await Puppeteer.LaunchAsync(launchOptions).ConfigureAwait(false);
            var instance = new BrowserInstance(browser, instanceId);
            _instances.Add(instance);

            _logger.LogInformation("Browser instance {InstanceId} created successfully", instanceId);

            return instance;
        }
        catch (Exception ex)
        {
            throw new BrowserException(
                $"Failed to create browser instance: {ex.Message}",
                ex,
                _options.BrowserExecutablePath);
        }
    }

    private string[] BuildBrowserArgs()
    {
        var args = new List<string>(_options.BrowserArgs)
        {
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu"
        };

        return [.. args];
    }
}

/// <summary>
///     Statistics about the browser pool.
/// </summary>
public sealed class BrowserPoolStatistics
{
    /// <summary>
    ///     Gets the configured pool size.
    /// </summary>
    public int PoolSize { get; init; }

    /// <summary>
    ///     Gets the current number of active browser instances.
    /// </summary>
    public int ActiveInstances { get; init; }

    /// <summary>
    ///     Gets the number of healthy browser instances.
    /// </summary>
    public int HealthyInstances { get; init; }

    /// <summary>
    ///     Gets the total number of page requests handled.
    /// </summary>
    public long TotalRequests { get; init; }

    /// <summary>
    ///     Gets the total number of pages created.
    /// </summary>
    public long TotalPagesCreated { get; init; }

    /// <summary>
    ///     Gets the number of available slots in the pool.
    /// </summary>
    public int AvailableSlots { get; init; }
}