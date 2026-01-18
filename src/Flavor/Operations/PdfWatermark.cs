using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf.IO;

namespace Flavor.Operations;

/// <summary>
///     Provides functionality to add watermarks to PDF documents.
/// </summary>
public static class PdfWatermark
{
    /// <summary>
    ///     Adds a text watermark to a PDF document.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <param name="text">The watermark text.</param>
    /// <returns>A new <see cref="PdfDocument" /> with the watermark applied.</returns>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync(html);
    /// var watermarked = PdfWatermark.AddText(pdf, "CONFIDENTIAL");
    /// await watermarked.SaveAsync("confidential.pdf");
    /// </code>
    /// </example>
    public static PdfDocument AddText(PdfDocument document, string text)
    {
        return AddText(document, new WatermarkOptions { Text = text });
    }

    /// <summary>
    ///     Adds a text watermark to a PDF document with custom options.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <param name="options">The watermark configuration options.</param>
    /// <returns>A new <see cref="PdfDocument" /> with the watermark applied.</returns>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync(html);
    /// var watermarked = PdfWatermark.AddText(pdf, new WatermarkOptions
    /// {
    ///     Text = "DRAFT",
    ///     FontSize = 72,
    ///     Color = "#FF0000",
    ///     Opacity = 0.2,
    ///     Rotation = -30
    /// });
    /// await watermarked.SaveAsync("draft.pdf");
    /// </code>
    /// </example>
    public static PdfDocument AddText(PdfDocument document, WatermarkOptions options)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Text))
            throw new ArgumentException("Watermark text cannot be empty.", nameof(options));

        using var stream = new MemoryStream(document.ToBytes());
        using var pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);

        var font = new XFont(options.FontFamily, options.FontSize, XFontStyle.Bold);
        var color = ParseColor(options.Color, options.Opacity);
        var brush = new XSolidBrush(color);

        var pageRange = options.PageRange ?? PageRange.All;

        for (var i = 0; i < pdfDocument.PageCount; i++)
        {
            var pageNumber = i + 1;
            if (!pageRange.Contains(pageNumber, pdfDocument.PageCount))
                continue;

            var page = pdfDocument.Pages[i];

            using var gfx = options.BehindContent
                ? XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend)
                : XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

            var pageWidth = page.Width.Point;
            var pageHeight = page.Height.Point;

            var textSize = gfx.MeasureString(options.Text, font);

            var x = options.HorizontalPosition switch
            {
                WatermarkPosition.Start => textSize.Width / 2 + 50,
                WatermarkPosition.End => pageWidth - textSize.Width / 2 - 50,
                _ => pageWidth / 2
            };

            var y = options.VerticalPosition switch
            {
                WatermarkPosition.Start => textSize.Height / 2 + 50,
                WatermarkPosition.End => pageHeight - textSize.Height / 2 - 50,
                _ => pageHeight / 2
            };

            var state = gfx.Save();

            gfx.TranslateTransform(x, y);
            gfx.RotateTransform(options.Rotation);

            gfx.DrawString(
                options.Text,
                font,
                brush,
                new XPoint(0, 0),
                XStringFormats.Center);

            gfx.Restore(state);
        }

        using var outputStream = new MemoryStream();
        pdfDocument.Save(outputStream, false);

        return new PdfDocument(outputStream.ToArray(), pdfDocument.PageCount);
    }

    /// <summary>
    ///     Adds a text watermark using a fluent builder.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <param name="text">The watermark text.</param>
    /// <param name="configure">An action to configure watermark options.</param>
    /// <returns>A new <see cref="PdfDocument" /> with the watermark applied.</returns>
    /// <example>
    ///     <code>
    /// var watermarked = PdfWatermark.AddText(pdf, "CONFIDENTIAL", opt => opt
    ///     .WithFontSize(60)
    ///     .WithColor("#0000FF")
    ///     .WithOpacity(0.15)
    ///     .WithRotation(-45));
    /// </code>
    /// </example>
    public static PdfDocument AddText(PdfDocument document, string text, Action<WatermarkOptionsBuilder> configure)
    {
        var builder = new WatermarkOptionsBuilder(text);
        configure?.Invoke(builder);
        return AddText(document, builder.Build());
    }

    private static XColor ParseColor(string hexColor, double opacity)
    {
        if (string.IsNullOrEmpty(hexColor))
            return XColor.FromArgb((int)(opacity * 255), 128, 128, 128);

        hexColor = hexColor.TrimStart('#');

        if (hexColor.Length == 6)
        {
            var r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
            var g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
            var b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
            return XColor.FromArgb((int)(opacity * 255), r, g, b);
        }

        return XColor.FromArgb((int)(opacity * 255), 128, 128, 128);
    }
}

/// <summary>
///     Builder for configuring watermark options fluently.
/// </summary>
public class WatermarkOptionsBuilder
{
    private readonly WatermarkOptions _options;

    /// <summary>
    ///     Initializes a new instance of <see cref="WatermarkOptionsBuilder" />.
    /// </summary>
    /// <param name="text">The watermark text.</param>
    public WatermarkOptionsBuilder(string text)
    {
        _options = new WatermarkOptions { Text = text };
    }

    /// <summary>Sets the font size in points.</summary>
    public WatermarkOptionsBuilder WithFontSize(double size)
    {
        _options.FontSize = size;
        return this;
    }

    /// <summary>Sets the font family.</summary>
    public WatermarkOptionsBuilder WithFontFamily(string fontFamily)
    {
        _options.FontFamily = fontFamily;
        return this;
    }

    /// <summary>Sets the text color in hex format.</summary>
    public WatermarkOptionsBuilder WithColor(string hexColor)
    {
        _options.Color = hexColor;
        return this;
    }

    /// <summary>Sets the opacity (0.0 to 1.0).</summary>
    public WatermarkOptionsBuilder WithOpacity(double opacity)
    {
        _options.Opacity = Math.Clamp(opacity, 0.0, 1.0);
        return this;
    }

    /// <summary>Sets the rotation angle in degrees.</summary>
    public WatermarkOptionsBuilder WithRotation(double degrees)
    {
        _options.Rotation = degrees;
        return this;
    }

    /// <summary>Sets the horizontal position.</summary>
    public WatermarkOptionsBuilder WithHorizontalPosition(WatermarkPosition position)
    {
        _options.HorizontalPosition = position;
        return this;
    }

    /// <summary>Sets the vertical position.</summary>
    public WatermarkOptionsBuilder WithVerticalPosition(WatermarkPosition position)
    {
        _options.VerticalPosition = position;
        return this;
    }

    /// <summary>Places the watermark in front of the content.</summary>
    public WatermarkOptionsBuilder InFrontOfContent()
    {
        _options.BehindContent = false;
        return this;
    }

    /// <summary>Places the watermark behind the content (default).</summary>
    public WatermarkOptionsBuilder BehindContent()
    {
        _options.BehindContent = true;
        return this;
    }

    /// <summary>Applies the watermark to specific pages.</summary>
    public WatermarkOptionsBuilder OnPages(PageRange pageRange)
    {
        _options.PageRange = pageRange;
        return this;
    }

    /// <summary>Applies the watermark to specific page numbers.</summary>
    public WatermarkOptionsBuilder OnPages(params int[] pageNumbers)
    {
        _options.PageRange = PageRange.Only(pageNumbers);
        return this;
    }

    /// <summary>Builds the watermark options.</summary>
    public WatermarkOptions Build()
    {
        return _options;
    }
}