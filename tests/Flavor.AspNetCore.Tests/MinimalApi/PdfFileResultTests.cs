using Flavor.AspNetCore.MinimalApi;
using Flavor.Options;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Flavor.AspNetCore.Tests.MinimalApi;

public class PdfFromHtmlResultTests
{
    [Fact]
    public void Constructor_WithHtml_SetsProperties()
    {
        // Arrange & Act
        var result = new PdfFromHtmlResult("<h1>Test</h1>", "test.pdf");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullHtml_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new PdfFromHtmlResult(null!, "test.pdf");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("html");
    }

    [Fact]
    public void Constructor_WithBuilderAction_DoesNotThrow()
    {
        // Arrange & Act
        var result = new PdfFromHtmlResult("<h1>Test</h1>", "test.pdf", options => options
            .WithPageSize(PageSize.A4)
            .WithLandscape());

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithoutConverter_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = new PdfFromHtmlResult("<h1>Test</h1>", "test.pdf");

        var services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(IFlavorConverter)).Returns((object?)null);
        services.GetService(typeof(IOptions<MinimalApiOptions>)).Returns((object?)null);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => result.ExecuteAsync(httpContext));
    }

    [Fact]
    public async Task ExecuteAsync_WithConverter_WritesPdfToResponse()
    {
        // Arrange
        var html = "<h1>Test</h1>";
        var pdfBytes = "%PDF"u8.ToArray();
        var pdfDocument = new PdfDocument(pdfBytes, 1);

        var converter = Substitute.For<IFlavorConverter>();
        converter.ConvertHtmlAsync(html, Arg.Any<PdfOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(pdfDocument));

        var services = new ServiceCollection();
        services.AddSingleton(converter);
        services.Configure<MinimalApiOptions>(o => o.DefaultFileName = "default.pdf");
        var serviceProvider = services.BuildServiceProvider();

        var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Response =
            {
                Body = responseBody
            }
        };

        var result = new PdfFromHtmlResult(html, "test.pdf");

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.ContentType.Should().Be("application/pdf");
        httpContext.Response.Headers.ContentDisposition.ToString().Should().Contain("test.pdf");
        responseBody.ToArray().Should().Equal(pdfBytes);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullFileName_UsesDefaultFromOptions()
    {
        // Arrange
        const string html = "<h1>Test</h1>";
        var pdfBytes = "%PDF"u8.ToArray();
        var pdfDocument = new PdfDocument(pdfBytes, 1);

        var converter = Substitute.For<IFlavorConverter>();
        converter.ConvertHtmlAsync(html, Arg.Any<PdfOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(pdfDocument));

        var services = new ServiceCollection();
        services.AddSingleton(converter);
        services.Configure<MinimalApiOptions>(o =>
        {
            o.DefaultFileName = "default-file.pdf";
            o.DefaultInline = false;
        });
        var serviceProvider = services.BuildServiceProvider();

        var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Response =
            {
                Body = responseBody
            }
        };

        var result = new PdfFromHtmlResult(html); // No filename

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.Headers.ContentDisposition.ToString().Should().Contain("default-file.pdf");
    }

    [Fact]
    public async Task ExecuteAsync_WithInlineTrue_SetsInlineDisposition()
    {
        // Arrange
        const string html = "<h1>Test</h1>";
        var pdfBytes = "%PDF"u8.ToArray();
        var pdfDocument = new PdfDocument(pdfBytes, 1);

        var converter = Substitute.For<IFlavorConverter>();
        converter.ConvertHtmlAsync(html, Arg.Any<PdfOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(pdfDocument));

        var services = new ServiceCollection();
        services.AddSingleton(converter);
        services.Configure<MinimalApiOptions>(o => { });
        var serviceProvider = services.BuildServiceProvider();

        var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Response =
            {
                Body = responseBody
            }
        };

        var result = new PdfFromHtmlResult(html, "test.pdf", inline: true);

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.Headers.ContentDisposition.ToString().Should().Be("inline");
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomHeaders_AddsHeaders()
    {
        // Arrange
        const string html = "<h1>Test</h1>";
        var pdfBytes = "%PDF"u8.ToArray();
        var pdfDocument = new PdfDocument(pdfBytes, 1);

        var converter = Substitute.For<IFlavorConverter>();
        converter.ConvertHtmlAsync(html, Arg.Any<PdfOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(pdfDocument));

        var services = new ServiceCollection();
        services.AddSingleton(converter);
        services.Configure<MinimalApiOptions>(o =>
        {
            o.CustomHeaders["X-Generator"] = "Flavor";
            o.CustomHeaders["X-Custom"] = "Value";
        });
        var serviceProvider = services.BuildServiceProvider();

        var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Response =
            {
                Body = responseBody
            }
        };

        var result = new PdfFromHtmlResult(html, "test.pdf");

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.Headers["X-Generator"].ToString().Should().Be("Flavor");
        httpContext.Response.Headers["X-Custom"].ToString().Should().Be("Value");
    }

    [Fact]
    public async Task ExecuteAsync_WithPageCountHeader_AddsPageCount()
    {
        // Arrange
        const string html = "<h1>Test</h1>";
        var pdfBytes = "%PDF"u8.ToArray();
        var pdfDocument = new PdfDocument(pdfBytes, 5);

        var converter = Substitute.For<IFlavorConverter>();
        converter.ConvertHtmlAsync(html, Arg.Any<PdfOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(pdfDocument));

        var services = new ServiceCollection();
        services.AddSingleton(converter);
        services.Configure<MinimalApiOptions>(o => o.IncludePageCountHeader = true);
        var serviceProvider = services.BuildServiceProvider();

        var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Response =
            {
                Body = responseBody
            }
        };

        var result = new PdfFromHtmlResult(html, "test.pdf");

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.Headers["X-Pdf-Page-Count"].ToString().Should().Be("5");
    }
}

public class PdfFromUrlResultTests
{
    [Fact]
    public void Constructor_WithUrl_SetsProperties()
    {
        // Arrange & Act
        var result = new PdfFromUrlResult("https://example.com", "page.pdf");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullUrl_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new PdfFromUrlResult(null!, "page.pdf");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("url");
    }

    [Fact]
    public async Task ExecuteAsync_WithConverter_WritesPdfToResponse()
    {
        // Arrange
        var url = "https://example.com";
        var pdfBytes = "%PDF"u8.ToArray();
        var pdfDocument = new PdfDocument(pdfBytes, 1);

        var converter = Substitute.For<IFlavorConverter>();
        converter.ConvertUrlAsync(url, Arg.Any<PdfOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(pdfDocument));

        var services = new ServiceCollection();
        services.AddSingleton(converter);
        services.Configure<MinimalApiOptions>(o => { });
        var serviceProvider = services.BuildServiceProvider();

        var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Response =
            {
                Body = responseBody
            }
        };

        var result = new PdfFromUrlResult(url, "page.pdf");

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.ContentType.Should().Be("application/pdf");
        httpContext.Response.Headers.ContentDisposition.ToString().Should().Contain("page.pdf");
        responseBody.ToArray().Should().Equal(pdfBytes);
    }
}

public class PdfBytesResultTests
{
    [Fact]
    public void Constructor_WithBytes_SetsProperties()
    {
        // Arrange & Act
        var result = new PdfBytesResult("%PDF"u8.ToArray(), "doc.pdf");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullBytes_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new PdfBytesResult(null!, "doc.pdf");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("pdfBytes");
    }

    [Fact]
    public async Task ExecuteAsync_WritesPdfToResponse()
    {
        // Arrange
        var pdfBytes = "%PDF-test"u8.ToArray();

        var services = new ServiceCollection();
        services.Configure<MinimalApiOptions>(o => { });
        var serviceProvider = services.BuildServiceProvider();

        var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Response =
            {
                Body = responseBody
            }
        };

        var result = new PdfBytesResult(pdfBytes, "doc.pdf");

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.ContentType.Should().Be("application/pdf");
        responseBody.ToArray().Should().Equal(pdfBytes);
    }
}

public class PdfDocumentResultTests
{
    [Fact]
    public void Constructor_WithDocument_SetsProperties()
    {
        // Arrange & Act
        var pdf = new PdfDocument("%PDF"u8.ToArray(), 1);
        var result = new PdfDocumentResult(pdf, "doc.pdf");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullDocument_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new PdfDocumentResult(null!, "doc.pdf");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("pdf");
    }

    [Fact]
    public async Task ExecuteAsync_WritesPdfToResponse()
    {
        // Arrange
        var pdfBytes = "%PDF-test"u8.ToArray();
        var pdf = new PdfDocument(pdfBytes, 3);

        var services = new ServiceCollection();
        services.Configure<MinimalApiOptions>(o => o.IncludePageCountHeader = true);
        var serviceProvider = services.BuildServiceProvider();

        var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Response =
            {
                Body = responseBody
            }
        };

        var result = new PdfDocumentResult(pdf, "doc.pdf");

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.ContentType.Should().Be("application/pdf");
        httpContext.Response.Headers["X-Pdf-Page-Count"].ToString().Should().Be("3");
        responseBody.ToArray().Should().Equal(pdfBytes);
    }
}
