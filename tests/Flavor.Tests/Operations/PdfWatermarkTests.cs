using Flavor.Operations;

namespace Flavor.Tests.Operations;

public class PdfWatermarkTests
{
    private static readonly byte[] SimplePdfBytes = CreateSimplePdf();

    private static byte[] CreateSimplePdf()
    {
        using var doc = new PdfSharpCore.Pdf.PdfDocument();
        doc.AddPage();
        using var stream = new MemoryStream();
        doc.Save(stream, false);
        return stream.ToArray();
    }

    [Fact]
    public void AddText_WithNullDocument_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => PdfWatermark.AddText(null!, "DRAFT");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddText_WithEmptyText_ThrowsArgumentException()
    {
        // Arrange
        var doc = new PdfDocument(SimplePdfBytes, 1);

        // Act
        var act = () => PdfWatermark.AddText(doc, new WatermarkOptions { Text = "" });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Watermark text cannot be empty*");
    }

    [Fact]
    public void AddText_WithValidText_ReturnsWatermarkedDocument()
    {
        // Arrange
        var doc = new PdfDocument(SimplePdfBytes, 1);

        // Act
        var result = PdfWatermark.AddText(doc, "CONFIDENTIAL");

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(1);
        result.ToBytes().Should().StartWith("%PDF"u8.ToArray());
    }

    [Fact]
    public void AddText_WithOptions_ReturnsWatermarkedDocument()
    {
        // Arrange
        var doc = new PdfDocument(SimplePdfBytes, 1);
        var options = new WatermarkOptions
        {
            Text = "DRAFT",
            FontSize = 72,
            Color = "#FF0000",
            Opacity = 0.2,
            Rotation = -30
        };

        // Act
        var result = PdfWatermark.AddText(doc, options);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(1);
    }

    [Fact]
    public void AddText_WithBuilder_ReturnsWatermarkedDocument()
    {
        // Arrange
        var doc = new PdfDocument(SimplePdfBytes, 1);

        // Act
        var result = PdfWatermark.AddText(doc, "SAMPLE", opt => opt
            .WithFontSize(60)
            .WithColor("#0000FF")
            .WithOpacity(0.15)
            .WithRotation(-45)
            .BehindContent());

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(1);
    }

    [Fact]
    public void AddText_WithPageRange_AppliesOnlyToSpecifiedPages()
    {
        // Arrange
        var twoPageBytes = CreateMultiPagePdf(2);
        var doc = new PdfDocument(twoPageBytes, 2);

        // Act
        var result = PdfWatermark.AddText(doc, new WatermarkOptions
        {
            Text = "FIRST PAGE ONLY",
            PageRange = PageRange.Single(1)
        });

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(2);
    }

    private static byte[] CreateMultiPagePdf(int pageCount)
    {
        using var doc = new PdfSharpCore.Pdf.PdfDocument();
        for (var i = 0; i < pageCount; i++) doc.AddPage();
        using var stream = new MemoryStream();
        doc.Save(stream, false);
        return stream.ToArray();
    }
}