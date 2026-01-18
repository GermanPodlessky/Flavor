using Flavor.AspNetCore.Extensions;
using Flavor.Options;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Flavor.AspNetCore.Tests;

public class ControllerExtensionsTests
{
    private readonly ControllerBase _controller = Substitute.For<ControllerBase>();

    [Fact]
    public void Pdf_WithHtmlAndFileName_ReturnsPdfResult()
    {
        // Act
        var result = _controller.Pdf("<h1>Test</h1>", "test.pdf");

        // Assert
        result.Should().BeOfType<PdfResult>();
    }

    [Fact]
    public void Pdf_WithHtmlOnly_ReturnsPdfResultWithInlineDisposition()
    {
        // Act
        var result = _controller.Pdf("<h1>Test</h1>");

        // Assert
        result.Should().BeOfType<PdfResult>();
    }

    [Fact]
    public void Pdf_WithOptions_ReturnsPdfResult()
    {
        // Act
        var result = _controller.Pdf("<h1>Test</h1>", "test.pdf", opt => opt
            .WithPageSize(PageSize.A4)
            .WithLandscape());

        // Assert
        result.Should().BeOfType<PdfResult>();
    }

    [Fact]
    public void PdfFromUrl_WithUrlAndFileName_ReturnsUrlPdfResult()
    {
        // Act
        var result = _controller.PdfFromUrl("https://example.com", "page.pdf");

        // Assert
        result.Should().BeOfType<UrlPdfResult>();
    }

    [Fact]
    public void PdfFromUrl_WithUrlOnly_ReturnsUrlPdfResultWithInlineDisposition()
    {
        // Act
        var result = _controller.PdfFromUrl("https://example.com");

        // Assert
        result.Should().BeOfType<UrlPdfResult>();
    }

    [Fact]
    public void PdfFromView_WithViewName_ReturnsViewPdfResult()
    {
        // Act
        var result = _controller.PdfFromView("Invoice");

        // Assert
        result.Should().BeOfType<ViewPdfResult>();
    }

    [Fact]
    public void PdfFromView_WithViewNameAndModel_ReturnsViewPdfResult()
    {
        // Arrange
        var model = new { Id = 1, Name = "Test" };

        // Act
        var result = _controller.PdfFromView("Invoice", model, "invoice.pdf");

        // Assert
        result.Should().BeOfType<ViewPdfResult>();
    }

    [Fact]
    public void PdfFromView_WithOptions_ReturnsViewPdfResult()
    {
        // Arrange
        var model = new { Id = 1 };

        // Act
        var result = _controller.PdfFromView("Invoice", model, "invoice.pdf", opt => opt
            .WithPageSize(PageSize.A4)
            .WithMargins(Margins.Narrow));

        // Assert
        result.Should().BeOfType<ViewPdfResult>();
    }
}