namespace Flavor.Options;

/// <summary>
///     Options for PDF generation.
/// </summary>
public sealed class PdfOptions
{
    /// <summary>
    ///     Gets or sets the page size. Default is <see cref="PageSize.A4" />.
    /// </summary>
    public PageSize PageSize { get; set; } = PageSize.A4;

    /// <summary>
    ///     Gets or sets the page margins. Default is <see cref="Margins.Normal" />.
    /// </summary>
    public Margins Margins { get; set; } = Margins.Normal;

    /// <summary>
    ///     Gets or sets whether to use landscape orientation. Default is false (portrait).
    /// </summary>
    public bool Landscape { get; set; }

    /// <summary>
    ///     Gets or sets whether to print background graphics (colors and images). Default is true.
    /// </summary>
    public bool PrintBackground { get; set; } = true;

    /// <summary>
    ///     Gets or sets the scale of the webpage rendering. Default is 1.
    ///     Must be between 0.1 and 2.
    /// </summary>
    public double Scale { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the HTML template for the page header.
    ///     Supports special classes: date, title, url, pageNumber, totalPages.
    /// </summary>
    /// <example>
    ///     <code>
    /// HeaderTemplate = "&lt;div style='font-size:10px;text-align:center;width:100%'&gt;" +
    ///                  "&lt;span class='title'&gt;&lt;/span&gt;&lt;/div&gt;"
    /// </code>
    /// </example>
    public string? HeaderTemplate { get; set; }

    /// <summary>
    ///     Gets or sets the HTML template for the page footer.
    ///     Supports special classes: date, title, url, pageNumber, totalPages.
    /// </summary>
    /// <example>
    ///     <code>
    /// FooterTemplate = "&lt;div style='font-size:10px;text-align:center;width:100%'&gt;" +
    ///                  "Page &lt;span class='pageNumber'&gt;&lt;/span&gt; of &lt;span class='totalPages'&gt;&lt;/span&gt;&lt;/div&gt;"
    /// </code>
    /// </example>
    public string? FooterTemplate { get; set; }

    /// <summary>
    ///     Gets or sets whether to display header and footer. Default is false.
    ///     Must be true if <see cref="HeaderTemplate" /> or <see cref="FooterTemplate" /> is set.
    /// </summary>
    public bool DisplayHeaderFooter { get; set; }

    /// <summary>
    ///     Gets or sets the paper ranges to print, e.g., "1-5, 8, 11-13".
    ///     Default is empty string which means all pages.
    /// </summary>
    public string? PageRanges { get; set; }

    /// <summary>
    ///     Gets or sets the timeout for PDF generation. Default is 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Gets or sets the wait condition before PDF generation. Default is <see cref="WaitCondition.Load" />.
    /// </summary>
    public WaitCondition WaitCondition { get; set; } = WaitCondition.Load;

    /// <summary>
    ///     Gets or sets an additional delay after the wait condition is met.
    ///     Useful for pages with delayed JavaScript rendering.
    /// </summary>
    public TimeSpan? WaitDelay { get; set; }

    /// <summary>
    ///     Gets or sets whether JavaScript is enabled. Default is true.
    /// </summary>
    public bool JavaScriptEnabled { get; set; } = true;

    /// <summary>
    ///     Gets or sets custom JavaScript to execute before PDF generation.
    /// </summary>
    public string? PreRenderScript { get; set; }

    /// <summary>
    ///     Gets or sets whether to prefer CSS page size over <see cref="PageSize" />. Default is false.
    ///     When true, respects @page CSS rules.
    /// </summary>
    public bool PreferCssPageSize { get; set; }

    /// <summary>
    ///     Gets or sets whether to omit the PDF background. Default is false.
    /// </summary>
    public bool OmitBackground { get; set; }

    /// <summary>
    ///     Creates a copy of this options instance.
    /// </summary>
    /// <returns>A new <see cref="PdfOptions" /> instance with the same values.</returns>
    public PdfOptions Clone()
    {
        return new PdfOptions
        {
            PageSize = PageSize,
            Margins = Margins,
            Landscape = Landscape,
            PrintBackground = PrintBackground,
            Scale = Scale,
            HeaderTemplate = HeaderTemplate,
            FooterTemplate = FooterTemplate,
            DisplayHeaderFooter = DisplayHeaderFooter,
            PageRanges = PageRanges,
            Timeout = Timeout,
            WaitCondition = WaitCondition,
            WaitDelay = WaitDelay,
            JavaScriptEnabled = JavaScriptEnabled,
            PreRenderScript = PreRenderScript,
            PreferCssPageSize = PreferCssPageSize,
            OmitBackground = OmitBackground
        };
    }
}