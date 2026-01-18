using Flavor.Extensions;
using Flavor.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Flavor.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFlavor_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddFlavor();

        // Assert
        var provider = services.BuildServiceProvider();
        var converter = provider.GetService<IFlavorConverter>();

        converter.Should().NotBeNull();
        converter.Should().BeOfType<FlavorConverter>();
    }

    [Fact]
    public void AddFlavor_RegistersAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlavor();
        var provider = services.BuildServiceProvider();

        // Act
        var converter1 = provider.GetRequiredService<IFlavorConverter>();
        var converter2 = provider.GetRequiredService<IFlavorConverter>();

        // Assert
        converter1.Should().BeSameAs(converter2);
    }

    [Fact]
    public void AddFlavor_WithOptions_AppliesConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddFlavor(options =>
        {
            options.PoolSize = 4;
            options.Headless = false;
            options.DefaultPdfOptions.PageSize = PageSize.Legal;
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<FlavorConverterOptions>();

        options.Should().NotBeNull();
        options!.PoolSize.Should().Be(4);
        options.Headless.Should().BeFalse();
        options.DefaultPdfOptions.PageSize.Should().Be(PageSize.Legal);
    }

    [Fact]
    public void AddFlavor_CalledMultipleTimes_DoesNotOverride()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddFlavor(options => options.PoolSize = 2);
        services.AddFlavor(options => options.PoolSize = 8);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<FlavorConverterOptions>();

        // First registration wins (TryAddSingleton behavior)
        options.PoolSize.Should().Be(2);
    }

    [Fact]
    public void AddFlavor_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddFlavor();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddFlavor_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<FlavorConverterOptions>? configure = null;

        // Act
        var act = () => services.AddFlavor(configure!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configure");
    }

    [Fact]
    public void AddFlavorWarmup_RegistersHostedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlavor();

        // Act
        services.AddFlavorWarmup();

        // Assert
        var provider = services.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>();

        hostedServices.Should().ContainSingle(s => s is FlavorWarmupService);
    }

    [Fact]
    public void AddFlavorWarmup_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddFlavorWarmup();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }
}