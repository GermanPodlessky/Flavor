using Flavor.Options;

namespace Flavor.Tests.Options;

public class MarginsTests
{
    [Fact]
    public void None_HasZeroMargins()
    {
        var margins = Margins.None;

        margins.Top.Should().Be(0);
        margins.Right.Should().Be(0);
        margins.Bottom.Should().Be(0);
        margins.Left.Should().Be(0);
    }

    [Fact]
    public void Normal_HasOneInchMargins()
    {
        var margins = Margins.Normal;

        margins.Top.Should().Be(1);
        margins.Right.Should().Be(1);
        margins.Bottom.Should().Be(1);
        margins.Left.Should().Be(1);
    }

    [Fact]
    public void Constructor_WithUniformValue_SetsAllSides()
    {
        var margins = new Margins(0.5);

        margins.Top.Should().Be(0.5);
        margins.Right.Should().Be(0.5);
        margins.Bottom.Should().Be(0.5);
        margins.Left.Should().Be(0.5);
    }

    [Fact]
    public void Constructor_WithVerticalAndHorizontal_SetsSidesCorrectly()
    {
        var margins = new Margins(1, 0.5);

        margins.Top.Should().Be(1);
        margins.Bottom.Should().Be(1);
        margins.Right.Should().Be(0.5);
        margins.Left.Should().Be(0.5);
    }

    [Fact]
    public void Constructor_WithIndividualValues_SetsSidesCorrectly()
    {
        var margins = new Margins(1, 2, 3, 4);

        margins.Top.Should().Be(1);
        margins.Right.Should().Be(2);
        margins.Bottom.Should().Be(3);
        margins.Left.Should().Be(4);
    }

    [Fact]
    public void FromMillimeters_ConvertsCorrectly()
    {
        var margins = Margins.FromMillimeters(25.4, 25.4, 25.4, 25.4);

        margins.Top.Should().BeApproximately(1, 0.01);
        margins.Right.Should().BeApproximately(1, 0.01);
        margins.Bottom.Should().BeApproximately(1, 0.01);
        margins.Left.Should().BeApproximately(1, 0.01);
    }

    [Fact]
    public void FromCentimeters_ConvertsCorrectly()
    {
        var margins = Margins.FromCentimeters(2.54);

        margins.Top.Should().BeApproximately(1, 0.01);
        margins.Right.Should().BeApproximately(1, 0.01);
        margins.Bottom.Should().BeApproximately(1, 0.01);
        margins.Left.Should().BeApproximately(1, 0.01);
    }

    [Fact]
    public void Equals_ReturnsTrue_ForSameMargins()
    {
        var margins1 = new Margins(1, 2, 3, 4);
        var margins2 = new Margins(1, 2, 3, 4);

        margins1.Equals(margins2).Should().BeTrue();
        (margins1 == margins2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ReturnsFalse_ForDifferentMargins()
    {
        var margins1 = Margins.Normal;
        var margins2 = Margins.Narrow;

        margins1.Equals(margins2).Should().BeFalse();
        (margins1 != margins2).Should().BeTrue();
    }
}