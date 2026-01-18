using Flavor.Rendering;

namespace Flavor.Tests.Rendering;

public class BrowserPoolTests
{
    [Fact]
    public void BrowserPool_Constructor_SetsPoolSize()
    {
        // Arrange
        var options = new FlavorConverterOptions { PoolSize = 4 };

        // Act
        var pool = new BrowserPool(options);

        // Assert
        pool.PoolSize.Should().Be(4);
        pool.InstanceCount.Should().Be(0);
        pool.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public void BrowserPool_Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        FlavorConverterOptions? options = null;

        // Act
        var act = () => new BrowserPool(options!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void BrowserPool_GetStatistics_ReturnsInitialStats()
    {
        // Arrange
        var options = new FlavorConverterOptions { PoolSize = 2 };
        var pool = new BrowserPool(options);

        // Act
        var stats = pool.GetStatistics();

        // Assert
        stats.PoolSize.Should().Be(2);
        stats.ActiveInstances.Should().Be(0);
        stats.HealthyInstances.Should().Be(0);
        stats.TotalRequests.Should().Be(0);
        stats.TotalPagesCreated.Should().Be(0);
        stats.AvailableSlots.Should().Be(2);
    }

    [Fact]
    public async Task BrowserPool_DisposeAsync_DisposesCleanly()
    {
        // Arrange
        var options = new FlavorConverterOptions { PoolSize = 1 };
        var pool = new BrowserPool(options);

        // Act
        var act = async () => await pool.DisposeAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task BrowserPool_AcquirePageAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var options = new FlavorConverterOptions { PoolSize = 1 };
        var pool = new BrowserPool(options);
        await pool.DisposeAsync();

        // Act
        var act = async () => await pool.AcquirePageAsync();

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }
}