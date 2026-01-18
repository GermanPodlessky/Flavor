using Flavor.Operations;

namespace Flavor.Tests.Operations;

public class PdfTableOfContentsTests
{
    [Fact]
    public void Generate_WithNullEntries_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => PdfTableOfContents.Generate(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Generate_WithEmptyEntries_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PdfTableOfContents.Generate([]);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one entry*");
    }

    [Fact]
    public void Generate_WithSingleEntry_ReturnsTocDocument()
    {
        // Arrange
        var entries = new[] { TocEntry.Create("Introduction", 1) };

        // Act
        var result = PdfTableOfContents.Generate(entries);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().BeGreaterThanOrEqualTo(1);
        result.ToBytes().Should().StartWith("%PDF"u8.ToArray());
    }

    [Fact]
    public void Generate_WithMultipleEntries_ReturnsTocDocument()
    {
        // Arrange
        var entries = new[]
        {
            TocEntry.Create("Introduction", 1),
            TocEntry.Create("Chapter 1: Getting Started", 3),
            TocEntry.Create("Chapter 2: Advanced Topics", 15),
            TocEntry.Create("Chapter 3: Best Practices", 25),
            TocEntry.Create("Conclusion", 30)
        };

        // Act
        var result = PdfTableOfContents.Generate(entries);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Generate_WithNestedEntries_ReturnsTocDocument()
    {
        // Arrange
        var entries = new[]
        {
            new TocEntry
            {
                Title = "Chapter 1",
                PageNumber = 1,
                Level = 1,
                Children =
                [
                    TocEntry.Create("Section 1.1", 2, 2),
                    TocEntry.Create("Section 1.2", 5, 2)
                ]
            },
            TocEntry.Create("Chapter 2", 10)
        };

        // Act
        var result = PdfTableOfContents.Generate(entries);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Generate_WithCustomOptions_ReturnsTocDocument()
    {
        // Arrange
        var entries = new[] { TocEntry.Create("Test", 1) };
        var options = new TableOfContentsOptions
        {
            Title = "Contents",
            TitleFontSize = 20,
            EntryFontSize = 10,
            ShowDottedLeaders = true,
            PageSize = TocPageSize.A4
        };

        // Act
        var result = PdfTableOfContents.Generate(entries, options);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Generate_WithBuilder_ReturnsTocDocument()
    {
        // Arrange
        var entries = new[] { TocEntry.Create("Test", 1) };

        // Act
        var result = PdfTableOfContents.Generate(entries, opt => opt
            .WithTitle("Table of Contents")
            .WithTitleFontSize(24)
            .WithDottedLeaders()
            .UseA4());

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void TocEntry_Create_SetsProperties()
    {
        // Act
        var entry = TocEntry.Create("Chapter 1", 5, 2);

        // Assert
        entry.Title.Should().Be("Chapter 1");
        entry.PageNumber.Should().Be(5);
        entry.Level.Should().Be(2);
    }

    [Fact]
    public void PageRange_Contains_ReturnsCorrectResults()
    {
        // Arrange
        var range = PageRange.FromTo(2, 5);

        // Act & Assert
        range.Contains(1, 10).Should().BeFalse();
        range.Contains(2, 10).Should().BeTrue();
        range.Contains(3, 10).Should().BeTrue();
        range.Contains(5, 10).Should().BeTrue();
        range.Contains(6, 10).Should().BeFalse();
    }

    [Fact]
    public void PageRange_Only_ReturnsCorrectResults()
    {
        // Arrange
        var range = PageRange.Only(1, 3, 5);

        // Act & Assert
        range.Contains(1, 10).Should().BeTrue();
        range.Contains(2, 10).Should().BeFalse();
        range.Contains(3, 10).Should().BeTrue();
        range.Contains(4, 10).Should().BeFalse();
        range.Contains(5, 10).Should().BeTrue();
    }
}