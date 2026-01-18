using Flavor.Operations;

namespace Flavor.Tests.Operations;

public class PdfSplitterTests
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
    public void SplitToPages_WithNullDocument_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => PdfSplitter.SplitToPages(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SplitToPages_WithThreePageDocument_ReturnsThreeDocuments()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act
        var pages = PdfSplitter.SplitToPages(doc);

        // Assert
        pages.Should().HaveCount(3);
        pages.Should().OnlyContain(p => p.PageCount == 1);
    }

    [Fact]
    public void ExtractPages_WithValidRange_ReturnsCorrectPages()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act
        var result = PdfSplitter.ExtractPages(doc, 1, 2);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(2);
    }

    [Fact]
    public void ExtractPages_WithInvalidStartPage_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act
        var act = () => PdfSplitter.ExtractPages(doc, 0, 2);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("startPage");
    }

    [Fact]
    public void ExtractPages_WithEndBeforeStart_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act
        var act = () => PdfSplitter.ExtractPages(doc, 3, 1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("endPage");
    }

    [Fact]
    public void ExtractPages_WithSpecificPageNumbers_ReturnsCorrectPages()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act - using params int[] overload to extract pages 1 and 3
        var result = PdfSplitter.ExtractPages(doc, 1, 2, 3);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(3);
    }

    [Fact]
    public void SplitByPageCount_WithTwoPerChunk_ReturnsCorrectChunks()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act
        var chunks = PdfSplitter.SplitByPageCount(doc, 2);

        // Assert
        chunks.Should().HaveCount(2);
        chunks[0].PageCount.Should().Be(2);
        chunks[1].PageCount.Should().Be(1);
    }

    [Fact]
    public void RemovePages_WithMiddlePage_RemovesCorrectPage()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act
        var result = PdfSplitter.RemovePages(doc, 2);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(2);
    }

    [Fact]
    public void RemovePages_AllPages_ThrowsInvalidOperationException()
    {
        // Arrange
        var doc = new PdfDocument(ThreePagePdfBytes, 3);

        // Act
        var act = () => PdfSplitter.RemovePages(doc, 1, 2, 3);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot remove all pages*");
    }
}