using Flavor.AspNetCore.MinimalApi;
using Flavor.Options;
using FluentAssertions;
using Xunit;

namespace Flavor.AspNetCore.Tests.MinimalApi;

public class PdfResultsTests
{
    [Fact]
    public void FromHtml_ReturnsCorrectResultType()
    {
        // Act
        var result = PdfResults.FromHtml("<h1>Test</h1>", "test.pdf");

        // Assert
        result.Should().BeOfType<PdfFromHtmlResult>();
    }

    [Fact]
    public void FromHtml_WithOptions_ReturnsCorrectResultType()
    {
        // Arrange
        var options = new PdfOptions { Landscape = true };

        // Act
        var result = PdfResults.FromHtml("<h1>Test</h1>", "test.pdf", options);

        // Assert
        result.Should().BeOfType<PdfFromHtmlResult>();
    }

    [Fact]
    public void FromHtml_WithBuilder_ReturnsCorrectResultType()
    {
        // Act
        var result = PdfResults.FromHtml("<h1>Test</h1>", "test.pdf", o => o.WithLandscape());

        // Assert
        result.Should().BeOfType<PdfFromHtmlResult>();
    }

    [Fact]
    public void FromUrl_ReturnsCorrectResultType()
    {
        // Act
        var result = PdfResults.FromUrl("https://example.com", "page.pdf");

        // Assert
        result.Should().BeOfType<PdfFromUrlResult>();
    }

    [Fact]
    public void FromUrl_WithOptions_ReturnsCorrectResultType()
    {
        // Arrange
        var options = new PdfOptions { PrintBackground = true };

        // Act
        var result = PdfResults.FromUrl("https://example.com", "page.pdf", options);

        // Assert
        result.Should().BeOfType<PdfFromUrlResult>();
    }

    [Fact]
    public void FromUrl_WithBuilder_ReturnsCorrectResultType()
    {
        // Act
        var result = PdfResults.FromUrl("https://example.com", "page.pdf", o => o.WithBackground());

        // Assert
        result.Should().BeOfType<PdfFromUrlResult>();
    }

    [Fact]
    public void FromBytes_ReturnsCorrectResultType()
    {
        // Arrange
        var pdfBytes = "%PDF"u8.ToArray();

        // Act
        var result = PdfResults.FromBytes(pdfBytes, "doc.pdf");

        // Assert
        result.Should().BeOfType<PdfBytesResult>();
    }

    [Fact]
    public void FromDocument_ReturnsCorrectResultType()
    {
        // Arrange
        var pdf = new PdfDocument("%PDF"u8.ToArray(), 1);

        // Act
        var result = PdfResults.FromDocument(pdf, "doc.pdf");

        // Assert
        result.Should().BeOfType<PdfDocumentResult>();
    }

    [Fact]
    public void Inline_ReturnsCorrectResultType()
    {
        // Act
        var result = PdfResults.Inline("<h1>Test</h1>");

        // Assert
        result.Should().BeOfType<PdfFromHtmlResult>();
    }

    [Fact]
    public void Inline_WithOptions_ReturnsCorrectResultType()
    {
        // Arrange
        var options = new PdfOptions();

        // Act
        var result = PdfResults.Inline("<h1>Test</h1>", options);

        // Assert
        result.Should().BeOfType<PdfFromHtmlResult>();
    }

    [Fact]
    public void Inline_WithBuilder_ReturnsCorrectResultType()
    {
        // Act
        var result = PdfResults.Inline("<h1>Test</h1>", o => o.WithPageSize(PageSize.A4));

        // Assert
        result.Should().BeOfType<PdfFromHtmlResult>();
    }

    [Fact]
    public void Attachment_ReturnsCorrectResultType()
    {
        // Act
        var result = PdfResults.Attachment("<h1>Test</h1>", "download.pdf");

        // Assert
        result.Should().BeOfType<PdfFromHtmlResult>();
    }

    [Fact]
    public void Attachment_WithOptions_ReturnsCorrectResultType()
    {
        // Arrange
        var options = new PdfOptions();

        // Act
        var result = PdfResults.Attachment("<h1>Test</h1>", "download.pdf", options);

        // Assert
        result.Should().BeOfType<PdfFromHtmlResult>();
    }

    [Fact]
    public void Attachment_WithBuilder_ReturnsCorrectResultType()
    {
        // Act
        var result = PdfResults.Attachment("<h1>Test</h1>", "download.pdf", o => o.WithMargins(1));

        // Assert
        result.Should().BeOfType<PdfFromHtmlResult>();
    }
}
