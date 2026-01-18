using Flavor.Options;

namespace Flavor.Tests.Options;

public class PdfOptionsBuilderTests
{
    [Fact]
    public void Build_ReturnsDefaultOptions_WhenNotConfigured()
    {
        var builder = new PdfOptionsBuilder();
        var options = builder.Build();

        options.PageSize.Should().Be(PageSize.A4);
        options.Margins.Should().Be(Margins.Normal);
        options.Landscape.Should().BeFalse();
        options.PrintBackground.Should().BeTrue();
    }

    [Fact]
    public void WithPageSize_SetsPageSize()
    {
        var options = new PdfOptionsBuilder()
            .WithPageSize(PageSize.Letter)
            .Build();

        options.PageSize.Should().Be(PageSize.Letter);
    }

    [Fact]
    public void WithPageSize_WithCustomDimensions_SetsPageSize()
    {
        var options = new PdfOptionsBuilder()
            .WithPageSize(10, 15)
            .Build();

        options.PageSize.Width.Should().Be(10);
        options.PageSize.Height.Should().Be(15);
    }

    [Fact]
    public void WithMargins_SetsMargins()
    {
        var options = new PdfOptionsBuilder()
            .WithMargins(Margins.Narrow)
            .Build();

        options.Margins.Should().Be(Margins.Narrow);
    }

    [Fact]
    public void WithMargins_WithUniformValue_SetsMargins()
    {
        var options = new PdfOptionsBuilder()
            .WithMargins(0.5)
            .Build();

        options.Margins.Top.Should().Be(0.5);
        options.Margins.Right.Should().Be(0.5);
        options.Margins.Bottom.Should().Be(0.5);
        options.Margins.Left.Should().Be(0.5);
    }

    [Fact]
    public void WithMargins_WithIndividualValues_SetsMargins()
    {
        var options = new PdfOptionsBuilder()
            .WithMargins(1, 2, 3, 4)
            .Build();

        options.Margins.Top.Should().Be(1);
        options.Margins.Right.Should().Be(2);
        options.Margins.Bottom.Should().Be(3);
        options.Margins.Left.Should().Be(4);
    }

    [Fact]
    public void WithLandscape_SetsLandscape()
    {
        var options = new PdfOptionsBuilder()
            .WithLandscape()
            .Build();

        options.Landscape.Should().BeTrue();
    }

    [Fact]
    public void WithBackground_SetsBackground()
    {
        var options = new PdfOptionsBuilder()
            .WithBackground(false)
            .Build();

        options.PrintBackground.Should().BeFalse();
    }

    [Fact]
    public void WithScale_SetsScale()
    {
        var options = new PdfOptionsBuilder()
            .WithScale(0.5)
            .Build();

        options.Scale.Should().Be(0.5);
    }

    [Fact]
    public void WithScale_ThrowsForInvalidValue()
    {
        var builder = new PdfOptionsBuilder();

        builder.Invoking(b => b.WithScale(0.05))
            .Should().Throw<ArgumentOutOfRangeException>();

        builder.Invoking(b => b.WithScale(3))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WithHeader_SetsHeaderAndEnablesDisplay()
    {
        var options = new PdfOptionsBuilder()
            .WithHeader("<div>Header</div>")
            .Build();

        options.HeaderTemplate.Should().Be("<div>Header</div>");
        options.DisplayHeaderFooter.Should().BeTrue();
    }

    [Fact]
    public void WithFooter_SetsFooterAndEnablesDisplay()
    {
        var options = new PdfOptionsBuilder()
            .WithFooter("<div>Footer</div>")
            .Build();

        options.FooterTemplate.Should().Be("<div>Footer</div>");
        options.DisplayHeaderFooter.Should().BeTrue();
    }

    [Fact]
    public void WithPageNumbers_SetsFooterWithPageNumbers()
    {
        var options = new PdfOptionsBuilder()
            .WithPageNumbers()
            .Build();

        options.FooterTemplate.Should().Contain("pageNumber");
        options.FooterTemplate.Should().Contain("totalPages");
        options.DisplayHeaderFooter.Should().BeTrue();
    }

    [Fact]
    public void WithTimeout_SetsTimeout()
    {
        var options = new PdfOptionsBuilder()
            .WithTimeout(TimeSpan.FromMinutes(2))
            .Build();

        options.Timeout.Should().Be(TimeSpan.FromMinutes(2));
    }

    [Fact]
    public void WithWaitCondition_SetsWaitCondition()
    {
        var options = new PdfOptionsBuilder()
            .WithWaitCondition(WaitCondition.NetworkIdle0)
            .Build();

        options.WaitCondition.Should().Be(WaitCondition.NetworkIdle0);
    }

    [Fact]
    public void WithWaitDelay_SetsWaitDelay()
    {
        var options = new PdfOptionsBuilder()
            .WithWaitDelay(TimeSpan.FromSeconds(2))
            .Build();

        options.WaitDelay.Should().Be(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ImplicitConversion_ReturnsBuiltOptions()
    {
        PdfOptions options = new PdfOptionsBuilder()
            .WithPageSize(PageSize.Letter)
            .WithLandscape();

        options.PageSize.Should().Be(PageSize.Letter);
        options.Landscape.Should().BeTrue();
    }

    [Fact]
    public void FluentChaining_ConfiguresAllOptions()
    {
        var options = new PdfOptionsBuilder()
            .WithPageSize(PageSize.A4)
            .WithMargins(Margins.Narrow)
            .WithLandscape()
            .WithBackground()
            .WithScale(1.5)
            .WithHeader("<div>Header</div>")
            .WithFooter("<div>Footer</div>")
            .WithPageRanges("1-5")
            .WithTimeout(TimeSpan.FromMinutes(1))
            .WithWaitCondition(WaitCondition.NetworkIdle2)
            .WithJavaScript()
            .Build();

        options.PageSize.Should().Be(PageSize.A4);
        options.Margins.Should().Be(Margins.Narrow);
        options.Landscape.Should().BeTrue();
        options.PrintBackground.Should().BeTrue();
        options.Scale.Should().Be(1.5);
        options.HeaderTemplate.Should().Be("<div>Header</div>");
        options.FooterTemplate.Should().Be("<div>Footer</div>");
        options.DisplayHeaderFooter.Should().BeTrue();
        options.PageRanges.Should().Be("1-5");
        options.Timeout.Should().Be(TimeSpan.FromMinutes(1));
        options.WaitCondition.Should().Be(WaitCondition.NetworkIdle2);
        options.JavaScriptEnabled.Should().BeTrue();
    }
}