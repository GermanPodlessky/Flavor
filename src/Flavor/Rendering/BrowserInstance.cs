using PuppeteerSharp;

namespace Flavor.Rendering;

/// <summary>
///     Represents a managed browser instance in the pool.
/// </summary>
internal sealed class BrowserInstance : IAsyncDisposable
{
    private int _activePageCount;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BrowserInstance" /> class.
    /// </summary>
    /// <param name="browser">The Puppeteer browser instance.</param>
    /// <param name="id">The unique identifier for this instance.</param>
    public BrowserInstance(IBrowser browser, int id)
    {
        Browser = browser ?? throw new ArgumentNullException(nameof(browser));
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Gets the unique identifier for this browser instance.
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     Gets the time when this instance was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    ///     Gets the number of pages created by this browser instance.
    /// </summary>
    public long PagesCreated { get; private set; }

    /// <summary>
    ///     Gets the current number of active pages.
    /// </summary>
    public int ActivePageCount => _activePageCount;

    /// <summary>
    ///     Gets a value indicating whether the browser is healthy and usable.
    /// </summary>
    public bool IsHealthy => !_disposed && !Browser.IsClosed;

    /// <summary>
    ///     Gets the underlying browser instance.
    /// </summary>
    internal IBrowser Browser { get; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (!Browser.IsClosed) await Browser.CloseAsync().ConfigureAwait(false);

        await Browser.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Creates a new page in this browser instance.
    /// </summary>
    /// <returns>A new page.</returns>
    public async Task<IPage> CreatePageAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (Browser.IsClosed) throw new InvalidOperationException("Browser has been closed.");

        Interlocked.Increment(ref _activePageCount);
        PagesCreated++;

        return await Browser.NewPageAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Notifies the instance that a page has been closed.
    /// </summary>
    public void NotifyPageClosed()
    {
        Interlocked.Decrement(ref _activePageCount);
    }
}