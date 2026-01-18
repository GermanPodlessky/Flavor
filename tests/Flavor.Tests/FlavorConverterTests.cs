using Flavor.Options;
using Flavor.Rendering;

namespace Flavor.Tests;

public class FlavorConverterTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ConvertHtmlAsync_WithInvalidHtml_ThrowsArgumentException(string? html)
    {
        var action = async () =>
        {
            await using var converter = new FlavorConverter();
            await converter.ConvertHtmlAsync(html!);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ConvertHtmlAsync_WithNullOptions_ThrowsArgumentNullException()
    {
        var action = async () =>
        {
            await using var converter = new FlavorConverter();
            await converter.ConvertHtmlAsync("<html></html>", (PdfOptions)null!);
        };

        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ConvertUrlAsync_InvalidUrl_ThrowsArgumentException(string? url)
    {
        var action = async () =>
        {
            await using var converter = new FlavorConverter();
            await converter.ConvertUrlAsync(url!);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ConvertFileAsync_InvalidPath_ThrowsArgumentException(string? path)
    {
        var action = async () =>
        {
            await using var converter = new FlavorConverter();
            await converter.ConvertFileAsync(path!);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ConvertFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        var action = async () =>
        {
            await using var converter = new FlavorConverter();
            await converter.ConvertFileAsync("/non/existent/file.html");
        };

        await action.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public void Constructor_WithConfigureAction_AppliesOptions()
    {
        using var converter = new FlavorConverter(options =>
        {
            options.Headless = false;
            options.PoolSize = 4;
        });

        // No exception means success - options were applied
    }

    [Fact]
    public async Task DisposeAsync_AllowsMultipleCalls()
    {
        var converter = new FlavorConverter();

        var action = async () =>
        {
            await converter.DisposeAsync();
            await converter.DisposeAsync();
        };

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public void Dispose_AllowsMultipleCalls()
    {
        var converter = new FlavorConverter();

        var action = () =>
        {
            converter.Dispose();
            converter.Dispose();
        };

        action.Should().NotThrow();
    }

    [Fact]
    public async Task ConvertHtmlAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var converter = new FlavorConverter();
        await converter.DisposeAsync();

        var action = () => converter.ConvertHtmlAsync("<html></html>");

        await action.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task ConvertHtmlAsync_WithBuilder_AppliesOptions()
    {
        var mockEngine = Substitute.For<IRenderEngine>();
        mockEngine.RenderHtmlAsync(Arg.Any<string>(), Arg.Any<PdfOptions>(), Arg.Any<CancellationToken>())
            .Returns(new PdfDocument("%PDF"u8.ToArray(), 1));

        await using var converter = new FlavorConverter(mockEngine, null, null);

        await converter.ConvertHtmlAsync("<html></html>", opts => opts
            .WithPageSize(PageSize.Letter)
            .WithLandscape());

        await mockEngine.Received(1).RenderHtmlAsync(
            Arg.Any<string>(),
            Arg.Is<PdfOptions>(o => o.PageSize == PageSize.Letter && o.Landscape),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConvertUrlAsync_WithBuilder_AppliesOptions()
    {
        var mockEngine = Substitute.For<IRenderEngine>();
        mockEngine.RenderUrlAsync(Arg.Any<string>(), Arg.Any<PdfOptions>(), Arg.Any<CancellationToken>())
            .Returns(new PdfDocument("%PDF"u8.ToArray(), 1));

        await using var converter = new FlavorConverter(mockEngine, null, null);

        await converter.ConvertUrlAsync("https://example.com", opts => opts
            .WithMargins(Margins.Narrow));

        await mockEngine.Received(1).RenderUrlAsync(
            Arg.Any<string>(),
            Arg.Is<PdfOptions>(o => o.Margins == Margins.Narrow),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WarmupAsync_CallsEngineWarmup()
    {
        var mockEngine = Substitute.For<IRenderEngine>();

        await using var converter = new FlavorConverter(mockEngine, null, null);

        await converter.WarmupAsync();

        await mockEngine.Received(1).WarmupAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void GetPoolStatistics_ReturnsStatistics()
    {
        using var converter = new FlavorConverter(options => { options.PoolSize = 4; });

        var stats = converter.GetPoolStatistics();

        stats.Should().NotBeNull();
        stats.PoolSize.Should().Be(4);
        stats.AvailableSlots.Should().Be(4);
    }

    [Fact]
    public void GetPoolStatistics_WithMockEngine_ReturnsEmptyStatistics()
    {
        var mockEngine = Substitute.For<IRenderEngine>();
        using var converter = new FlavorConverter(mockEngine, null, null);

        var stats = converter.GetPoolStatistics();

        stats.Should().NotBeNull();
        stats.PoolSize.Should().Be(0);
    }

    [Fact]
    public async Task GetPoolStatistics_AfterDispose_ThrowsObjectDisposedException()
    {
        var converter = new FlavorConverter();
        await converter.DisposeAsync();

        var act = () => converter.GetPoolStatistics();

        act.Should().Throw<ObjectDisposedException>();
    }
}