namespace Flavor.Options;

/// <summary>
///     Fluent builder for <see cref="PdfOptions" />.
/// </summary>
public sealed class PdfOptionsBuilder
{
    private readonly PdfOptions _options = new();

    /// <summary>
    ///     Sets the page size.
    /// </summary>
    /// <param name="pageSize">The page size.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithPageSize(PageSize pageSize)
    {
        _options.PageSize = pageSize;
        return this;
    }

    /// <summary>
    ///     Sets the page size using custom dimensions in inches.
    /// </summary>
    /// <param name="widthInches">Width in inches.</param>
    /// <param name="heightInches">Height in inches.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithPageSize(double widthInches, double heightInches)
    {
        _options.PageSize = new PageSize(widthInches, heightInches);
        return this;
    }

    /// <summary>
    ///     Sets the page margins.
    /// </summary>
    /// <param name="margins">The margins.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithMargins(Margins margins)
    {
        _options.Margins = margins;
        return this;
    }

    /// <summary>
    ///     Sets uniform page margins.
    /// </summary>
    /// <param name="allSidesInches">Margin for all sides in inches.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithMargins(double allSidesInches)
    {
        _options.Margins = new Margins(allSidesInches);
        return this;
    }

    /// <summary>
    ///     Sets individual page margins.
    /// </summary>
    /// <param name="topInches">Top margin in inches.</param>
    /// <param name="rightInches">Right margin in inches.</param>
    /// <param name="bottomInches">Bottom margin in inches.</param>
    /// <param name="leftInches">Left margin in inches.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithMargins(double topInches, double rightInches, double bottomInches, double leftInches)
    {
        _options.Margins = new Margins(topInches, rightInches, bottomInches, leftInches);
        return this;
    }

    /// <summary>
    ///     Sets landscape orientation.
    /// </summary>
    /// <param name="landscape">True for landscape, false for portrait.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithLandscape(bool landscape = true)
    {
        _options.Landscape = landscape;
        return this;
    }

    /// <summary>
    ///     Sets whether to print background graphics.
    /// </summary>
    /// <param name="printBackground">True to print backgrounds.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithBackground(bool printBackground = true)
    {
        _options.PrintBackground = printBackground;
        return this;
    }

    /// <summary>
    ///     Sets the scale of the webpage rendering.
    /// </summary>
    /// <param name="scale">Scale value between 0.1 and 2.</param>
    /// <returns>The builder for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when scale is not between 0.1 and 2.</exception>
    public PdfOptionsBuilder WithScale(double scale)
    {
        if (scale < 0.1 || scale > 2)
            throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be between 0.1 and 2.");

        _options.Scale = scale;
        return this;
    }

    /// <summary>
    ///     Sets the header template.
    /// </summary>
    /// <param name="htmlTemplate">HTML template for the header.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithHeader(string htmlTemplate)
    {
        _options.HeaderTemplate = htmlTemplate;
        _options.DisplayHeaderFooter = true;
        return this;
    }

    /// <summary>
    ///     Sets the footer template.
    /// </summary>
    /// <param name="htmlTemplate">HTML template for the footer.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithFooter(string htmlTemplate)
    {
        _options.FooterTemplate = htmlTemplate;
        _options.DisplayHeaderFooter = true;
        return this;
    }

    /// <summary>
    ///     Sets a standard page number footer.
    /// </summary>
    /// <param name="format">Format string. Use {page} and {pages} placeholders.</param>
    /// <param name="fontSize">Font size in pixels. Default is 10.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithPageNumbers(string format = "Page {page} of {pages}", int fontSize = 10)
    {
        var template = format
            .Replace("{page}", "<span class='pageNumber'></span>")
            .Replace("{pages}", "<span class='totalPages'></span>");

        _options.FooterTemplate = $"<div style='font-size:{fontSize}px;text-align:center;width:100%'>{template}</div>";
        _options.DisplayHeaderFooter = true;
        return this;
    }

    /// <summary>
    ///     Sets the page ranges to print.
    /// </summary>
    /// <param name="ranges">Page ranges, e.g., "1-5, 8, 11-13".</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithPageRanges(string ranges)
    {
        _options.PageRanges = ranges;
        return this;
    }

    /// <summary>
    ///     Sets the timeout for PDF generation.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithTimeout(TimeSpan timeout)
    {
        _options.Timeout = timeout;
        return this;
    }

    /// <summary>
    ///     Sets the wait condition before PDF generation.
    /// </summary>
    /// <param name="condition">The wait condition.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithWaitCondition(WaitCondition condition)
    {
        _options.WaitCondition = condition;
        return this;
    }

    /// <summary>
    ///     Sets an additional delay after the wait condition is met.
    /// </summary>
    /// <param name="delay">The delay duration.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithWaitDelay(TimeSpan delay)
    {
        _options.WaitDelay = delay;
        return this;
    }

    /// <summary>
    ///     Sets whether JavaScript is enabled.
    /// </summary>
    /// <param name="enabled">True to enable JavaScript.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithJavaScript(bool enabled = true)
    {
        _options.JavaScriptEnabled = enabled;
        return this;
    }

    /// <summary>
    ///     Sets custom JavaScript to execute before PDF generation.
    /// </summary>
    /// <param name="script">JavaScript code to execute.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithPreRenderScript(string script)
    {
        _options.PreRenderScript = script;
        return this;
    }

    /// <summary>
    ///     Sets whether to prefer CSS page size over the configured page size.
    /// </summary>
    /// <param name="prefer">True to prefer CSS page size.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithPreferCssPageSize(bool prefer = true)
    {
        _options.PreferCssPageSize = prefer;
        return this;
    }

    /// <summary>
    ///     Sets whether to omit the PDF background.
    /// </summary>
    /// <param name="omit">True to omit background.</param>
    /// <returns>The builder for chaining.</returns>
    public PdfOptionsBuilder WithOmitBackground(bool omit = true)
    {
        _options.OmitBackground = omit;
        return this;
    }

    /// <summary>
    ///     Builds the <see cref="PdfOptions" /> instance.
    /// </summary>
    /// <returns>The configured <see cref="PdfOptions" />.</returns>
    public PdfOptions Build()
    {
        return _options.Clone();
    }

    /// <summary>
    ///     Implicitly converts the builder to <see cref="PdfOptions" />.
    /// </summary>
    public static implicit operator PdfOptions(PdfOptionsBuilder builder)
    {
        return builder.Build();
    }
}