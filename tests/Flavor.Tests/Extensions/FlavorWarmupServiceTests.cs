using Flavor.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Flavor.Tests.Extensions;

public class FlavorWarmupServiceTests
{
    [Fact]
    public async Task StartAsync_CallsWarmupOnConverter()
    {
        // Arrange
        var converter = Substitute.For<IFlavorConverter>();
        var logger = NullLogger<FlavorWarmupService>.Instance;
        var service = new FlavorWarmupService(converter, logger);

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        await converter.Received(1).WarmupAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenWarmupFails_DoesNotThrow()
    {
        // Arrange
        var converter = Substitute.For<IFlavorConverter>();
        converter.WarmupAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Browser failed to start")));
        var logger = NullLogger<FlavorWarmupService>.Instance;
        var service = new FlavorWarmupService(converter, logger);

        // Act
        var act = () => service.StartAsync(CancellationToken.None);

        // Assert - should not throw, just log the error
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StartAsync_WhenCancelled_DoesNotThrow()
    {
        // Arrange
        var converter = Substitute.For<IFlavorConverter>();
        converter.WarmupAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromCanceled(new CancellationToken(true)));
        var logger = NullLogger<FlavorWarmupService>.Instance;
        var service = new FlavorWarmupService(converter, logger);

        // Act
        var act = () => service.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_CompletesImmediately()
    {
        // Arrange
        var converter = Substitute.For<IFlavorConverter>();
        var logger = NullLogger<FlavorWarmupService>.Instance;
        var service = new FlavorWarmupService(converter, logger);

        // Act
        var act = () => service.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        await converter.DidNotReceive().DisposeAsync();
    }

    [Fact]
    public void Constructor_WithNullConverter_ThrowsArgumentNullException()
    {
        // Arrange
        IFlavorConverter? converter = null;
        var logger = NullLogger<FlavorWarmupService>.Instance;

        // Act
        var act = () => new FlavorWarmupService(converter!, logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("converter");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var converter = Substitute.For<IFlavorConverter>();
        ILogger<FlavorWarmupService>? logger = null;

        // Act
        var act = () => new FlavorWarmupService(converter, logger!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}