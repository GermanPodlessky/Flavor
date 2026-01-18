namespace Flavor.Operations;

/// <summary>
///     Represents an entry in a table of contents.
/// </summary>
public class TocEntry
{
    /// <summary>
    ///     Gets or sets the title text.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    ///     Gets or sets the page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    ///     Gets or sets the heading level (1-6). Default is 1.
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    ///     Gets or sets child entries (for nested TOC).
    /// </summary>
    public List<TocEntry>? Children { get; set; }

    /// <summary>
    ///     Creates a new TOC entry.
    /// </summary>
    /// <param name="title">The entry title.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="level">The heading level (1-6).</param>
    public static TocEntry Create(string title, int pageNumber, int level = 1)
    {
        return new TocEntry { Title = title, PageNumber = pageNumber, Level = level };
    }
}

/// <summary>
///     Options for configuring the table of contents.
/// </summary>
public class TableOfContentsOptions
{
    /// <summary>
    ///     Gets or sets the TOC title. Default is "Table of Contents".
    /// </summary>
    public string Title { get; set; } = "Table of Contents";

    /// <summary>
    ///     Gets or sets the title font size. Default is 24.
    /// </summary>
    public double TitleFontSize { get; set; } = 24;

    /// <summary>
    ///     Gets or sets the entry font size. Default is 12.
    /// </summary>
    public double EntryFontSize { get; set; } = 12;

    /// <summary>
    ///     Gets or sets the font family. Default is "Arial".
    /// </summary>
    public string FontFamily { get; set; } = "Arial";

    /// <summary>
    ///     Gets or sets whether to show dotted leaders. Default is true.
    /// </summary>
    public bool ShowDottedLeaders { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether to show page numbers. Default is true.
    /// </summary>
    public bool ShowPageNumbers { get; set; } = true;

    /// <summary>
    ///     Gets or sets the indent per level in points. Default is 20.
    /// </summary>
    public double IndentPerLevel { get; set; } = 20;

    /// <summary>
    ///     Gets or sets the line spacing multiplier. Default is 1.5.
    /// </summary>
    public double LineSpacing { get; set; } = 1.5;

    /// <summary>
    ///     Gets or sets the page margins in points. Default is 72 (1 inch).
    /// </summary>
    public double Margin { get; set; } = 72;

    /// <summary>
    ///     Gets or sets the page size. Default is Letter.
    /// </summary>
    public TocPageSize PageSize { get; set; } = TocPageSize.Letter;
}

/// <summary>
///     Page size for table of contents.
/// </summary>
public enum TocPageSize
{
    /// <summary>Letter size (8.5 x 11 inches).</summary>
    Letter,

    /// <summary>A4 size (210 x 297 mm).</summary>
    A4,

    /// <summary>Legal size (8.5 x 14 inches).</summary>
    Legal
}