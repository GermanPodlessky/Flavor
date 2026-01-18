using Flavor.Options;

namespace Flavor.Tests.Options;

public class PageSizeTests
{
    [Fact]
    public void A4_HasCorrectDimensions()
    {
        var a4 = PageSize.A4;

        a4.Width.Should().BeApproximately(8.27, 0.01);
        a4.Height.Should().BeApproximately(11.7, 0.01);
    }

    [Fact]
    public void Letter_HasCorrectDimensions()
    {
        var letter = PageSize.Letter;

        letter.Width.Should().Be(8.5);
        letter.Height.Should().Be(11);
    }

    [Fact]
    public void FromMillimeters_ConvertsCorrectly()
    {
        // A4 is 210mm x 297mm
        var pageSize = PageSize.FromMillimeters(210, 297);

        pageSize.Width.Should().BeApproximately(8.27, 0.01);
        pageSize.Height.Should().BeApproximately(11.69, 0.01);
    }

    [Fact]
    public void FromCentimeters_ConvertsCorrectly()
    {
        // A4 is 21cm x 29.7cm
        var pageSize = PageSize.FromCentimeters(21, 29.7);

        pageSize.Width.Should().BeApproximately(8.27, 0.01);
        pageSize.Height.Should().BeApproximately(11.69, 0.01);
    }

    [Fact]
    public void FromPixels_ConvertsCorrectly()
    {
        var pageSize = PageSize.FromPixels(96, 96);

        pageSize.Width.Should().Be(1);
        pageSize.Height.Should().Be(1);
    }

    [Fact]
    public void Landscape_SwapsWidthAndHeight()
    {
        var portrait = new PageSize(8.5, 11);
        var landscape = portrait.Landscape;

        landscape.Width.Should().Be(11);
        landscape.Height.Should().Be(8.5);
    }

    [Fact]
    public void Portrait_SwapsWidthAndHeight_WhenLandscape()
    {
        var landscape = new PageSize(11, 8.5);
        var portrait = landscape.Portrait;

        portrait.Width.Should().Be(8.5);
        portrait.Height.Should().Be(11);
    }

    [Fact]
    public void Equals_ReturnsTrue_ForSameDimensions()
    {
        var size1 = new PageSize(8.5, 11);
        var size2 = new PageSize(8.5, 11);

        size1.Equals(size2).Should().BeTrue();
        (size1 == size2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ReturnsFalse_ForDifferentDimensions()
    {
        var size1 = PageSize.A4;
        var size2 = PageSize.Letter;

        size1.Equals(size2).Should().BeFalse();
        (size1 != size2).Should().BeTrue();
    }

    [Fact]
    public void WidthMm_ReturnsCorrectValue()
    {
        var a4 = PageSize.A4;

        a4.WidthMm.Should().BeApproximately(210, 1);
        a4.HeightMm.Should().BeApproximately(297, 1);
    }
}