using Flavor.AspNetCore.MinimalApi;
using FluentAssertions;
using Xunit;

namespace Flavor.AspNetCore.Tests.MinimalApi;

public class MinimalApiOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new MinimalApiOptions();

        // Assert
        options.DefaultFileName.Should().Be("document.pdf");
        options.DefaultInline.Should().BeFalse();
        options.DefaultPdfOptions.Should().BeNull();
        options.CustomHeaders.Should().BeEmpty();
        options.IncludePageCountHeader.Should().BeFalse();
        options.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public void CachePolicy_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var cachePolicy = new PdfCachePolicy();

        // Assert
        cachePolicy.Enabled.Should().BeFalse();
        cachePolicy.MaxAgeSeconds.Should().Be(0);
        cachePolicy.Private.Should().BeTrue();
        cachePolicy.MustRevalidate.Should().BeFalse();
        cachePolicy.VaryHeader.Should().BeNull();
    }

    [Fact]
    public void CachePolicy_BuildCacheControlHeader_WhenDisabled_ReturnsNoStore()
    {
        // Arrange
        var cachePolicy = new PdfCachePolicy { Enabled = false };

        // Act
        var header = cachePolicy.BuildCacheControlHeader();

        // Assert
        header.Should().Be("no-store, no-cache");
    }

    [Fact]
    public void CachePolicy_BuildCacheControlHeader_WhenEnabled_ReturnsCorrectHeader()
    {
        // Arrange
        var cachePolicy = new PdfCachePolicy
        {
            Enabled = true,
            MaxAgeSeconds = 3600,
            Private = true,
            MustRevalidate = true
        };

        // Act
        var header = cachePolicy.BuildCacheControlHeader();

        // Assert
        header.Should().Contain("private");
        header.Should().Contain("max-age=3600");
        header.Should().Contain("must-revalidate");
    }

    [Fact]
    public void CachePolicy_BuildCacheControlHeader_WhenPublic_ReturnsPublic()
    {
        // Arrange
        var cachePolicy = new PdfCachePolicy
        {
            Enabled = true,
            Private = false
        };

        // Act
        var header = cachePolicy.BuildCacheControlHeader();

        // Assert
        header.Should().Contain("public");
        header.Should().NotContain("private");
    }

    [Fact]
    public void CustomHeaders_CanBeAdded()
    {
        // Arrange
        var options = new MinimalApiOptions();

        // Act
        options.CustomHeaders["X-Generator"] = "Flavor";
        options.CustomHeaders["X-Custom"] = "Value";

        // Assert
        options.CustomHeaders.Should().HaveCount(2);
        options.CustomHeaders["X-Generator"].Should().Be("Flavor");
        options.CustomHeaders["X-Custom"].Should().Be("Value");
    }
}
