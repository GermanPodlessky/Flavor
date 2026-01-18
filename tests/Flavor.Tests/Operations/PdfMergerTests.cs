using Flavor.Operations;

namespace Flavor.Tests.Operations;

public class PdfMergerTests
{
    // Simple valid PDF bytes (minimal PDF structure)
    private static readonly byte[] SimplePdfBytes = CreateSimplePdf();

    private static byte[] CreateSimplePdf()
    {
        // Create a minimal valid PDF using PdfSharpCore
        using var doc = new PdfSharpCore.Pdf.PdfDocument();
        doc.AddPage();
        using var stream = new MemoryStream();
        doc.Save(stream, false);
        return stream.ToArray();
    }

    [Fact]
    public void Merge_WithNullDocuments_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => PdfMerger.Merge((PdfDocument[])null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Merge_WithEmptyDocuments_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PdfMerger.Merge(Array.Empty<PdfDocument>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one document*");
    }

    [Fact]
    public void Merge_WithSingleDocument_ReturnsEquivalentDocument()
    {
        // Arrange
        var doc = new PdfDocument(SimplePdfBytes, 1);

        // Act
        var result = PdfMerger.Merge(doc);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(1);
        result.ToBytes().Should().StartWith("%PDF"u8.ToArray());
    }

    [Fact]
    public void Merge_WithTwoDocuments_CombinesPages()
    {
        // Arrange
        var doc1 = new PdfDocument(SimplePdfBytes, 1);
        var doc2 = new PdfDocument(SimplePdfBytes, 1);

        // Act
        var result = PdfMerger.Merge(doc1, doc2);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(2);
    }

    [Fact]
    public void Merge_WithByteArrays_CombinesPages()
    {
        // Act
        var result = PdfMerger.Merge(SimplePdfBytes, SimplePdfBytes, SimplePdfBytes);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(3);
    }

    [Fact]
    public void Merge_WithEnumerable_CombinesPages()
    {
        // Arrange
        var docs = new[]
        {
            new PdfDocument(SimplePdfBytes, 1),
            new PdfDocument(SimplePdfBytes, 1)
        };

        // Act
        var result = PdfMerger.Merge(docs.AsEnumerable());

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(2);
    }
}