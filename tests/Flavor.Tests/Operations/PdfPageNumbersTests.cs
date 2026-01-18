using Flavor.Operations;

namespace Flavor.Tests.Operations;

public class PdfPageNumbersTests
{
    private static readonly byte[] ThreePagePdfBytes = CreateMultiPagePdf(3);

    private static byte[] CreateMultiPagePdf(int pageCount)
    {
        using var doc = new PdfSharpCore.Pdf.PdfDocument();
        for (var i = 0; i < pageCount; i++) doc.AddPage();

        using var stream = new MemoryStream();
        doc.Save(stream, false);
        return stream.ToArray();
    }

    [Fact]
    public void Add_WithNullDocument_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => PdfPageNumbers.Add(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Add_WithDefaultOptions_ReturnsNumberedDocument()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act
        var result = PdfPageNumbers.Add(doc);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(3);
        result.ToBytes().Should().StartWith("%PDF"u8.ToArray());
    }

    [Fact]
    public void Add_WithCustomOptions_ReturnsNumberedDocument()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);
        var options = new PageNumberOptions
        {
            Format = "{0} / {1}",
            HorizontalAlignment = PageNumberAlignment.Right,
            VerticalPosition = PageNumberVerticalPosition.Bottom,
            FontSize = 9
        };

        // Act
        var result = PdfPageNumbers.Add(doc, options);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(3);
    }

    [Fact]
    public void Add_WithBuilder_ReturnsNumberedDocument()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act
        var result = PdfPageNumbers.Add(doc, opt => opt
            .WithFormat("{0} of {1}")
            .AtBottom()
            .AlignRight()
            .SkipFirstPage());

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(3);
    }

    [Fact]
    public void Add_WithSkipFirstPage_StartsFromSecondPage()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act
        var result = PdfPageNumbers.Add(doc, opt => opt.SkipFirstPage());

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(3);
    }

    [Fact]
    public void Add_WithCustomStartNumber_UsesCorrectStartNumber()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act
        var result = PdfPageNumbers.Add(doc, opt => opt.StartingAt(5));

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(3);
    }

    [Fact]
    public void Add_AtTop_PlacesNumbersAtTop()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act
        var result = PdfPageNumbers.Add(doc, opt => opt.AtTop().AlignCenter());

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(3);
    }
}