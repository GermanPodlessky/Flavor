using PdfSharpCore.Drawing;

namespace Flavor.Operations;

/// <summary>
///     Provides functionality to generate a table of contents for PDF documents.
/// </summary>
public static class PdfTableOfContents
{
    /// <summary>
    ///     Generates a table of contents PDF page.
    /// </summary>
    /// <param name="entries">The TOC entries.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the table of contents.</returns>
    /// <example>
    ///     <code>
    /// var entries = new[]
    /// {
    ///     TocEntry.Create("Introduction", 1),
    ///     TocEntry.Create("Chapter 1: Getting Started", 3),
    ///     TocEntry.Create("Chapter 2: Advanced Topics", 15),
    ///     TocEntry.Create("Conclusion", 30)
    /// };
    /// var toc = PdfTableOfContents.Generate(entries);
    /// var final = PdfMerger.Merge(toc, mainDocument);
    /// </code>
    /// </example>
    public static PdfDocument Generate(IEnumerable<TocEntry> entries)
    {
        return Generate(entries, new TableOfContentsOptions());
    }

    /// <summary>
    ///     Generates a table of contents PDF page with custom options.
    /// </summary>
    /// <param name="entries">The TOC entries.</param>
    /// <param name="options">The TOC configuration options.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the table of contents.</returns>
    public static PdfDocument Generate(IEnumerable<TocEntry> entries, TableOfContentsOptions options)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(options);

        var entryList = entries.ToList();
        if (entryList.Count == 0)
            throw new ArgumentException("At least one entry is required.", nameof(entries));

        using var document = new PdfSharpCore.Pdf.PdfDocument();

        var pageSize = GetPageSize(options.PageSize);
        var page = document.AddPage();
        page.Width = pageSize.Width;
        page.Height = pageSize.Height;

        using var gfx = XGraphics.FromPdfPage(page);

        var titleFont = new XFont(options.FontFamily, options.TitleFontSize, XFontStyle.Bold);
        var entryFont = new XFont(options.FontFamily, options.EntryFontSize, XFontStyle.Regular);
        var boldEntryFont = new XFont(options.FontFamily, options.EntryFontSize, XFontStyle.Bold);

        var currentY = options.Margin;
        var pageWidth = page.Width.Point;
        var contentWidth = pageWidth - 2 * options.Margin;

        // Draw title
        var titleSize = gfx.MeasureString(options.Title, titleFont);
        gfx.DrawString(
            options.Title,
            titleFont,
            XBrushes.Black,
            new XPoint((pageWidth - titleSize.Width) / 2, currentY + titleSize.Height));

        currentY += titleSize.Height + options.TitleFontSize;

        var lineHeight = options.EntryFontSize * options.LineSpacing;
        var pageCount = 1;

        foreach (var entry in FlattenEntries(entryList))
            // Check if we need a new page
            if (currentY + lineHeight > page.Height.Point - options.Margin)
            {
                page = document.AddPage();
                page.Width = pageSize.Width;
                page.Height = pageSize.Height;
                gfx.Dispose();
                var newGfx = XGraphics.FromPdfPage(page);
                currentY = options.Margin;
                pageCount++;

                DrawEntry(newGfx, entry, options, currentY, contentWidth, entryFont, boldEntryFont);
                currentY += lineHeight;
                newGfx.Dispose();
            }
            else
            {
                DrawEntry(gfx, entry, options, currentY, contentWidth, entryFont, boldEntryFont);
                currentY += lineHeight;
            }

        using var outputStream = new MemoryStream();
        document.Save(outputStream, false);

        return new PdfDocument(outputStream.ToArray(), pageCount);
    }

    /// <summary>
    ///     Generates a table of contents using a fluent builder.
    /// </summary>
    /// <param name="entries">The TOC entries.</param>
    /// <param name="configure">An action to configure TOC options.</param>
    /// <returns>A <see cref="PdfDocument" /> containing the table of contents.</returns>
    public static PdfDocument Generate(IEnumerable<TocEntry> entries, Action<TableOfContentsOptionsBuilder> configure)
    {
        var builder = new TableOfContentsOptionsBuilder();
        configure?.Invoke(builder);
        return Generate(entries, builder.Build());
    }

    private static void DrawEntry(
        XGraphics gfx,
        TocEntry entry,
        TableOfContentsOptions options,
        double y,
        double contentWidth,
        XFont regularFont,
        XFont boldFont)
    {
        var indent = (entry.Level - 1) * options.IndentPerLevel;
        var font = entry.Level == 1 ? boldFont : regularFont;
        var x = options.Margin + indent;

        var titleSize = gfx.MeasureString(entry.Title, font);
        var pageNumText = entry.PageNumber.ToString();
        var pageNumSize = gfx.MeasureString(pageNumText, regularFont);

        // Draw title
        gfx.DrawString(entry.Title, font, XBrushes.Black, new XPoint(x, y + titleSize.Height));

        if (options.ShowPageNumbers)
        {
            var pageNumX = options.Margin + contentWidth - pageNumSize.Width;

            // Draw dotted leaders
            if (options.ShowDottedLeaders)
            {
                var leaderStartX = x + titleSize.Width + 5;
                var leaderEndX = pageNumX - 5;

                if (leaderEndX > leaderStartX)
                {
                    var pen = new XPen(XColors.Gray, 0.5);
                    pen.DashStyle = XDashStyle.Dot;
                    gfx.DrawLine(pen, leaderStartX, y + titleSize.Height - 3, leaderEndX, y + titleSize.Height - 3);
                }
            }

            // Draw page number
            gfx.DrawString(pageNumText, regularFont, XBrushes.Black, new XPoint(pageNumX, y + titleSize.Height));
        }
    }

    private static IEnumerable<TocEntry> FlattenEntries(IEnumerable<TocEntry> entries)
    {
        foreach (var entry in entries)
        {
            yield return entry;

            if (entry.Children != null)
                foreach (var child in FlattenEntries(entry.Children))
                    yield return child;
        }
    }

    private static XSize GetPageSize(TocPageSize size)
    {
        return size switch
        {
            TocPageSize.A4 => new XSize(595.276, 841.890), // 210 x 297 mm
            TocPageSize.Legal => new XSize(612, 1008), // 8.5 x 14 inches
            _ => new XSize(612, 792) // 8.5 x 11 inches (Letter)
        };
    }
}

/// <summary>
///     Builder for configuring table of contents options fluently.
/// </summary>
public class TableOfContentsOptionsBuilder
{
    private readonly TableOfContentsOptions _options = new();

    /// <summary>Sets the TOC title.</summary>
    public TableOfContentsOptionsBuilder WithTitle(string title)
    {
        _options.Title = title;
        return this;
    }

    /// <summary>Sets the title font size.</summary>
    public TableOfContentsOptionsBuilder WithTitleFontSize(double size)
    {
        _options.TitleFontSize = size;
        return this;
    }

    /// <summary>Sets the entry font size.</summary>
    public TableOfContentsOptionsBuilder WithEntryFontSize(double size)
    {
        _options.EntryFontSize = size;
        return this;
    }

    /// <summary>Sets the font family.</summary>
    public TableOfContentsOptionsBuilder WithFontFamily(string fontFamily)
    {
        _options.FontFamily = fontFamily;
        return this;
    }

    /// <summary>Shows dotted leaders between title and page number.</summary>
    public TableOfContentsOptionsBuilder WithDottedLeaders()
    {
        _options.ShowDottedLeaders = true;
        return this;
    }

    /// <summary>Hides dotted leaders.</summary>
    public TableOfContentsOptionsBuilder WithoutDottedLeaders()
    {
        _options.ShowDottedLeaders = false;
        return this;
    }

    /// <summary>Shows page numbers.</summary>
    public TableOfContentsOptionsBuilder WithPageNumbers()
    {
        _options.ShowPageNumbers = true;
        return this;
    }

    /// <summary>Hides page numbers.</summary>
    public TableOfContentsOptionsBuilder WithoutPageNumbers()
    {
        _options.ShowPageNumbers = false;
        return this;
    }

    /// <summary>Sets the indent per heading level.</summary>
    public TableOfContentsOptionsBuilder WithIndentPerLevel(double indent)
    {
        _options.IndentPerLevel = indent;
        return this;
    }

    /// <summary>Sets the line spacing multiplier.</summary>
    public TableOfContentsOptionsBuilder WithLineSpacing(double spacing)
    {
        _options.LineSpacing = spacing;
        return this;
    }

    /// <summary>Sets the page margins.</summary>
    public TableOfContentsOptionsBuilder WithMargin(double margin)
    {
        _options.Margin = margin;
        return this;
    }

    /// <summary>Sets the page size to Letter.</summary>
    public TableOfContentsOptionsBuilder UseLetter()
    {
        _options.PageSize = TocPageSize.Letter;
        return this;
    }

    /// <summary>Sets the page size to A4.</summary>
    public TableOfContentsOptionsBuilder UseA4()
    {
        _options.PageSize = TocPageSize.A4;
        return this;
    }

    /// <summary>Sets the page size to Legal.</summary>
    public TableOfContentsOptionsBuilder UseLegal()
    {
        _options.PageSize = TocPageSize.Legal;
        return this;
    }

    /// <summary>Builds the table of contents options.</summary>
    public TableOfContentsOptions Build()
    {
        return _options;
    }
}