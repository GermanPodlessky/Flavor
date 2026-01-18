using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf.IO;

namespace Flavor.Operations;

/// <summary>
///     Provides functionality to add page numbers to PDF documents.
/// </summary>
public static class PdfPageNumbers
{
    /// <summary>
    ///     Adds page numbers to a PDF document with default options.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <returns>A new <see cref="PdfDocument" /> with page numbers.</returns>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync(html);
    /// var numbered = PdfPageNumbers.Add(pdf);
    /// await numbered.SaveAsync("numbered.pdf");
    /// </code>
    /// </example>
    public static PdfDocument Add(PdfDocument document)
    {
        return Add(document, new PageNumberOptions());
    }

    /// <summary>
    ///     Adds page numbers to a PDF document with custom options.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <param name="options">The page number configuration options.</param>
    /// <returns>A new <see cref="PdfDocument" /> with page numbers.</returns>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync(html);
    /// var numbered = PdfPageNumbers.Add(pdf, new PageNumberOptions
    /// {
    ///     Format = "{0} / {1}",
    ///     HorizontalAlignment = PageNumberAlignment.Right,
    ///     VerticalPosition = PageNumberVerticalPosition.Bottom,
    ///     FontSize = 9
    /// });
    /// await numbered.SaveAsync("numbered.pdf");
    /// </code>
    /// </example>
    public static PdfDocument Add(PdfDocument document, PageNumberOptions options)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(options);

        using var stream = new MemoryStream(document.ToBytes());
        using var pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);

        var font = new XFont(options.FontFamily, options.FontSize, XFontStyle.Regular);
        var brush = new XSolidBrush(ParseColor(options.Color));

        var totalPages = pdfDocument.PageCount;
        var pageRange = options.PageRange ?? PageRange.All;

        for (var i = 0; i < pdfDocument.PageCount; i++)
        {
            var pageNumber = i + 1;

            if (options.SkipFirstPage && pageNumber == 1)
                continue;

            if (!pageRange.Contains(pageNumber, totalPages))
                continue;

            var page = pdfDocument.Pages[i];

            using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

            var displayNumber = options.StartNumber + (options.SkipFirstPage ? i - 1 : i);
            var displayTotal = options.SkipFirstPage ? totalPages - 1 : totalPages;
            var text = string.Format(options.Format, displayNumber, displayTotal);

            var textSize = gfx.MeasureString(text, font);
            var pageWidth = page.Width.Point;
            var pageHeight = page.Height.Point;

            var x = options.HorizontalAlignment switch
            {
                PageNumberAlignment.Left => options.Margin,
                PageNumberAlignment.Right => pageWidth - textSize.Width - options.Margin,
                _ => (pageWidth - textSize.Width) / 2
            };

            var y = options.VerticalPosition switch
            {
                PageNumberVerticalPosition.Top => options.Margin + textSize.Height,
                _ => pageHeight - options.Margin
            };

            gfx.DrawString(text, font, brush, new XPoint(x, y));
        }

        using var outputStream = new MemoryStream();
        pdfDocument.Save(outputStream, false);

        return new PdfDocument(outputStream.ToArray(), pdfDocument.PageCount);
    }

    /// <summary>
    ///     Adds page numbers using a fluent builder.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <param name="configure">An action to configure page number options.</param>
    /// <returns>A new <see cref="PdfDocument" /> with page numbers.</returns>
    /// <example>
    ///     <code>
    /// var numbered = PdfPageNumbers.Add(pdf, opt => opt
    ///     .WithFormat("{0} of {1}")
    ///     .AtBottom()
    ///     .AlignRight()
    ///     .SkipFirstPage());
    /// </code>
    /// </example>
    public static PdfDocument Add(PdfDocument document, Action<PageNumberOptionsBuilder> configure)
    {
        var builder = new PageNumberOptionsBuilder();
        configure?.Invoke(builder);
        return Add(document, builder.Build());
    }

    private static XColor ParseColor(string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor))
            return XColors.Black;

        hexColor = hexColor.TrimStart('#');

        if (hexColor.Length == 6)
        {
            var r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
            var g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
            var b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
            return XColor.FromArgb(r, g, b);
        }

        return XColors.Black;
    }
}

/// <summary>
///     Builder for configuring page number options fluently.
/// </summary>
public class PageNumberOptionsBuilder
{
    private readonly PageNumberOptions _options = new();

    /// <summary>Sets the format string ({0} = current, {1} = total).</summary>
    public PageNumberOptionsBuilder WithFormat(string format)
    {
        _options.Format = format;
        return this;
    }

    /// <summary>Sets the font size in points.</summary>
    public PageNumberOptionsBuilder WithFontSize(double size)
    {
        _options.FontSize = size;
        return this;
    }

    /// <summary>Sets the font family.</summary>
    public PageNumberOptionsBuilder WithFontFamily(string fontFamily)
    {
        _options.FontFamily = fontFamily;
        return this;
    }

    /// <summary>Sets the text color in hex format.</summary>
    public PageNumberOptionsBuilder WithColor(string hexColor)
    {
        _options.Color = hexColor;
        return this;
    }

    /// <summary>Aligns page numbers to the left.</summary>
    public PageNumberOptionsBuilder AlignLeft()
    {
        _options.HorizontalAlignment = PageNumberAlignment.Left;
        return this;
    }

    /// <summary>Aligns page numbers to the center.</summary>
    public PageNumberOptionsBuilder AlignCenter()
    {
        _options.HorizontalAlignment = PageNumberAlignment.Center;
        return this;
    }

    /// <summary>Aligns page numbers to the right.</summary>
    public PageNumberOptionsBuilder AlignRight()
    {
        _options.HorizontalAlignment = PageNumberAlignment.Right;
        return this;
    }

    /// <summary>Positions page numbers at the top.</summary>
    public PageNumberOptionsBuilder AtTop()
    {
        _options.VerticalPosition = PageNumberVerticalPosition.Top;
        return this;
    }

    /// <summary>Positions page numbers at the bottom.</summary>
    public PageNumberOptionsBuilder AtBottom()
    {
        _options.VerticalPosition = PageNumberVerticalPosition.Bottom;
        return this;
    }

    /// <summary>Sets the margin from the edge in points.</summary>
    public PageNumberOptionsBuilder WithMargin(double margin)
    {
        _options.Margin = margin;
        return this;
    }

    /// <summary>Sets the starting page number.</summary>
    public PageNumberOptionsBuilder StartingAt(int number)
    {
        _options.StartNumber = number;
        return this;
    }

    /// <summary>Skips the first page (no page number on cover).</summary>
    public PageNumberOptionsBuilder SkipFirstPage()
    {
        _options.SkipFirstPage = true;
        return this;
    }

    /// <summary>Applies page numbers only to specific pages.</summary>
    public PageNumberOptionsBuilder OnPages(PageRange pageRange)
    {
        _options.PageRange = pageRange;
        return this;
    }

    /// <summary>Builds the page number options.</summary>
    public PageNumberOptions Build()
    {
        return _options;
    }
}