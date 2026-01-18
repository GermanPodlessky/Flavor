using Flavor.Options;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Xunit;

namespace Flavor.AspNetCore.Tests;

public class PdfResultTests
{
    [Fact]
    public void Constructor_WithHtml_SetsProperties()
    {
        // Arrange & Act
        var result = new PdfResult("<h1>Test</h1>", "test.pdf");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullHtml_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new PdfResult(null!, "test.pdf");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("html");
    }

    [Fact]
    public void Constructor_WithBuilderAction_BuildsOptions()
    {
        // Arrange & Act
        var result = new PdfResult("<h1>Test</h1>", "test.pdf", options => options
            .WithPageSize(PageSize.A4)
            .WithLandscape());

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteResultAsync_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var result = new PdfResult("<h1>Test</h1>", "test.pdf");

        // Act
        var act = () => result.ExecuteResultAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteResultAsync_WithoutConverter_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = new PdfResult("<h1>Test</h1>", "test.pdf");

        var services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(IFlavorConverter)).Returns((object?)null);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        // Act
        var act = () => result.ExecuteResultAsync(actionContext);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*IFlavorConverter*");
    }

    [Fact]
    public async Task ExecuteResultAsync_WithConverter_WritesPdfToResponse()
    {
        // Arrange
        var html = "<h1>Test</h1>";
        var pdfBytes = "%PDF"u8.ToArray();
        var pdfDocument = new PdfDocument(pdfBytes, 1);

        var converter = Substitute.For<IFlavorConverter>();
        converter.ConvertHtmlAsync(html, Arg.Any<PdfOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(pdfDocument));

        var services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(IFlavorConverter)).Returns(converter);

        var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
            Response =
            {
                Body = responseBody
            }
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var result = new PdfResult(html, "test.pdf");

        // Act
        await result.ExecuteResultAsync(actionContext);

        // Assert
        httpContext.Response.ContentType.Should().Be("application/pdf");
        httpContext.Response.Headers.ContentDisposition.ToString().Should().Contain("test.pdf");
        responseBody.ToArray().Should().Equal(pdfBytes);
    }

    [Fact]
    public async Task ExecuteResultAsync_WithNullFileName_SetsInlineDisposition()
    {
        // Arrange
        const string html = "<h1>Test</h1>";
        var pdfBytes = "%PDF"u8.ToArray();
        var pdfDocument = new PdfDocument(pdfBytes, 1);

        var converter = Substitute.For<IFlavorConverter>();
        converter.ConvertHtmlAsync(html, Arg.Any<PdfOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(pdfDocument));

        var services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(IFlavorConverter)).Returns(converter);

        var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
            Response =
            {
                Body = responseBody
            }
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var result = new PdfResult(html); // No filename = inline

        // Act
        await result.ExecuteResultAsync(actionContext);

        // Assert
        httpContext.Response.Headers.ContentDisposition.ToString().Should().Be("inline");
    }
}