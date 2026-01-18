using Flavor.Rendering;

namespace Flavor.Tests.Rendering;

public class BrowserPoolStatisticsTests
{
    [Fact]
    public void BrowserPoolStatistics_DefaultValues_AreZero()
    {
        // Arrange & Act
        var stats = new BrowserPoolStatistics();

        // Assert
        stats.PoolSize.Should().Be(0);
        stats.ActiveInstances.Should().Be(0);
        stats.HealthyInstances.Should().Be(0);
        stats.TotalRequests.Should().Be(0);
        stats.TotalPagesCreated.Should().Be(0);
        stats.AvailableSlots.Should().Be(0);
    }

    [Fact]
    public void BrowserPoolStatistics_WithValues_ReturnsCorrectValues()
    {
        // Arrange & Act
        var stats = new BrowserPoolStatistics
        {
            PoolSize = 4,
            ActiveInstances = 2,
            HealthyInstances = 2,
            TotalRequests = 100,
            TotalPagesCreated = 95,
            AvailableSlots = 2
        };

        // Assert
        stats.PoolSize.Should().Be(4);
        stats.ActiveInstances.Should().Be(2);
        stats.HealthyInstances.Should().Be(2);
        stats.TotalRequests.Should().Be(100);
        stats.TotalPagesCreated.Should().Be(95);
        stats.AvailableSlots.Should().Be(2);
    }
}