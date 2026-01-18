namespace Flavor.Operations;

/// <summary>
///     Options for configuring a text watermark.
/// </summary>
public class WatermarkOptions
{
    /// <summary>
    ///     Gets or sets the watermark text.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    ///     Gets or sets the font size in points. Default is 48.
    /// </summary>
    public double FontSize { get; set; } = 48;

    /// <summary>
    ///     Gets or sets the font family name. Default is "Arial".
    /// </summary>
    public string FontFamily { get; set; } = "Arial";

    /// <summary>
    ///     Gets or sets the text color in hex format (e.g., "#FF0000"). Default is gray.
    /// </summary>
    public string Color { get; set; } = "#808080";

    /// <summary>
    ///     Gets or sets the opacity (0.0 to 1.0). Default is 0.3.
    /// </summary>
    public double Opacity { get; set; } = 0.3;

    /// <summary>
    ///     Gets or sets the rotation angle in degrees. Default is -45 (diagonal).
    /// </summary>
    public double Rotation { get; set; } = -45;

    /// <summary>
    ///     Gets or sets the horizontal position. Default is Center.
    /// </summary>
    public WatermarkPosition HorizontalPosition { get; set; } = WatermarkPosition.Center;

    /// <summary>
    ///     Gets or sets the vertical position. Default is Center.
    /// </summary>
    public WatermarkPosition VerticalPosition { get; set; } = WatermarkPosition.Center;

    /// <summary>
    ///     Gets or sets whether to place the watermark behind the content. Default is true.
    /// </summary>
    public bool BehindContent { get; set; } = true;

    /// <summary>
    ///     Gets or sets the page range to apply the watermark (null = all pages).
    /// </summary>
    public PageRange? PageRange { get; set; }
}

/// <summary>
///     Represents a position for watermark placement.
/// </summary>
public enum WatermarkPosition
{
    /// <summary>Start position (left or top).</summary>
    Start,

    /// <summary>Center position.</summary>
    Center,

    /// <summary>End position (right or bottom).</summary>
    End
}

/// <summary>
///     Represents a range of pages.
/// </summary>
public class PageRange
{
    /// <summary>
    ///     Gets or sets the starting page (1-based). Null means from the beginning.
    /// </summary>
    public int? Start { get; set; }

    /// <summary>
    ///     Gets or sets the ending page (1-based, inclusive). Null means to the end.
    /// </summary>
    public int? End { get; set; }

    /// <summary>
    ///     Gets or sets specific page numbers to include.
    /// </summary>
    public int[]? Pages { get; set; }

    /// <summary>
    ///     Creates a page range for all pages.
    /// </summary>
    public static PageRange All => new();

    /// <summary>
    ///     Creates a page range for a single page.
    /// </summary>
    public static PageRange Single(int page)
    {
        return new PageRange { Pages = [page] };
    }

    /// <summary>
    ///     Creates a page range from start to end (inclusive).
    /// </summary>
    public static PageRange FromTo(int start, int end)
    {
        return new PageRange { Start = start, End = end };
    }

    /// <summary>
    ///     Creates a page range for specific pages.
    /// </summary>
    public static PageRange Only(params int[] pages)
    {
        return new PageRange { Pages = pages };
    }

    /// <summary>
    ///     Checks if a page number is within this range.
    /// </summary>
    public bool Contains(int pageNumber, int totalPages)
    {
        if (Pages != null && Pages.Length > 0)
            return Pages.Contains(pageNumber);

        var start = Start ?? 1;
        var end = End ?? totalPages;

        return pageNumber >= start && pageNumber <= end;
    }
}