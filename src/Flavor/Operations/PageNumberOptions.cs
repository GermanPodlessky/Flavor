namespace Flavor.Operations;

/// <summary>
///     Options for configuring page numbers.
/// </summary>
public class PageNumberOptions
{
    /// <summary>
    ///     Gets or sets the format string for page numbers.
    ///     Use {0} for current page, {1} for total pages.
    ///     Default is "Page {0} of {1}".
    /// </summary>
    public string Format { get; set; } = "Page {0} of {1}";

    /// <summary>
    ///     Gets or sets the font size in points. Default is 10.
    /// </summary>
    public double FontSize { get; set; } = 10;

    /// <summary>
    ///     Gets or sets the font family name. Default is "Arial".
    /// </summary>
    public string FontFamily { get; set; } = "Arial";

    /// <summary>
    ///     Gets or sets the text color in hex format. Default is black.
    /// </summary>
    public string Color { get; set; } = "#000000";

    /// <summary>
    ///     Gets or sets the horizontal alignment. Default is Center.
    /// </summary>
    public PageNumberAlignment HorizontalAlignment { get; set; } = PageNumberAlignment.Center;

    /// <summary>
    ///     Gets or sets the vertical position. Default is Bottom.
    /// </summary>
    public PageNumberVerticalPosition VerticalPosition { get; set; } = PageNumberVerticalPosition.Bottom;

    /// <summary>
    ///     Gets or sets the margin from the edge in points. Default is 36 (0.5 inch).
    /// </summary>
    public double Margin { get; set; } = 36;

    /// <summary>
    ///     Gets or sets the starting page number. Default is 1.
    /// </summary>
    public int StartNumber { get; set; } = 1;

    /// <summary>
    ///     Gets or sets whether to skip the first page. Default is false.
    /// </summary>
    public bool SkipFirstPage { get; set; }

    /// <summary>
    ///     Gets or sets the page range to number (null = all pages).
    /// </summary>
    public PageRange? PageRange { get; set; }
}

/// <summary>
///     Horizontal alignment for page numbers.
/// </summary>
public enum PageNumberAlignment
{
    /// <summary>Align to the left.</summary>
    Left,

    /// <summary>Align to the center.</summary>
    Center,

    /// <summary>Align to the right.</summary>
    Right
}

/// <summary>
///     Vertical position for page numbers.
/// </summary>
public enum PageNumberVerticalPosition
{
    /// <summary>Position at the top of the page.</summary>
    Top,

    /// <summary>Position at the bottom of the page.</summary>
    Bottom
}