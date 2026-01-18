namespace Flavor.Options;

/// <summary>
///     Specifies when to consider a page loaded and ready for PDF generation.
/// </summary>
public enum WaitCondition
{
    /// <summary>
    ///     Consider navigation to be finished when the load event is fired.
    ///     This is the default behavior.
    /// </summary>
    Load,

    /// <summary>
    ///     Consider navigation to be finished when the DOMContentLoaded event is fired.
    ///     Faster than Load but may miss dynamically loaded content.
    /// </summary>
    DomContentLoaded,

    /// <summary>
    ///     Consider navigation to be finished when there are no more than 0 network connections for at least 500 ms.
    ///     Best for pages with heavy async content loading.
    /// </summary>
    NetworkIdle0,

    /// <summary>
    ///     Consider navigation to be finished when there are no more than 2 network connections for at least 500 ms.
    ///     Good balance between speed and completeness.
    /// </summary>
    NetworkIdle2
}