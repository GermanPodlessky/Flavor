using PuppeteerSharp;

namespace Flavor.Rendering;

/// <summary>
///     Represents a leased page from the browser pool.
///     Disposing this lease returns the page to the pool.
/// </summary>
internal sealed class PageLease : IAsyncDisposable
{
    private readonly BrowserInstance _instance;
    private readonly BrowserPool _pool;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PageLease" /> class.
    /// </summary>
    /// <param name="page">The leased page.</param>
    /// <param name="instance">The browser instance that owns the page.</param>
    /// <param name="pool">The pool to return to.</param>
    public PageLease(IPage page, BrowserInstance instance, BrowserPool pool)
    {
        Page = page ?? throw new ArgumentNullException(nameof(page));
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
    }

    /// <summary>
    ///     Gets the leased page.
    /// </summary>
    public IPage Page { get; }

    /// <summary>
    ///     Gets the ID of the browser instance this page belongs to.
    /// </summary>
    public int BrowserInstanceId => _instance.Id;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (!Page.IsClosed) await Page.CloseAsync().ConfigureAwait(false);
        }
        finally
        {
            _pool.ReleasePage(_instance);
        }
    }
}