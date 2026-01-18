# PDF Operations

## Merge

```csharp
using Flavor.Operations;

// From params
var merged = PdfMerger.Merge(pdf1, pdf2, pdf3);

// From collection
var documents = new List<PdfDocument> { pdf1, pdf2, pdf3 };
var merged = PdfMerger.Merge(documents);

await merged.SaveAsync("merged.pdf");
```

## Split

```csharp
// Split into individual pages
var pages = PdfSplitter.SplitToPages(document);
for (int i = 0; i < pages.Count; i++)
{
    await pages[i].SaveAsync($"page-{i + 1}.pdf");
}

// Extract specific pages (1-based)
var extracted = PdfSplitter.ExtractPages(document, 1, 3, 5);

// Extract page range
var range = PdfSplitter.ExtractRange(document, 2, 5);  // pages 2-5

// Remove pages
var withoutPages = PdfSplitter.RemovePages(document, 2, 4);

// Split into chunks
var chunks = PdfSplitter.SplitByPageCount(document, 3);  // 3 pages per chunk
```

## Watermarks

```csharp
// Simple
var watermarked = PdfWatermark.AddText(document, "CONFIDENTIAL");

// With options
var watermarked = PdfWatermark.AddText(document, new WatermarkOptions
{
    Text = "DRAFT",
    FontSize = 72,
    Color = "#FF0000",
    Opacity = 0.3,
    Rotation = -45,
    Position = WatermarkPosition.Center
});

// Fluent builder
var watermarked = PdfWatermark.AddText(document, opt => opt
    .WithText("CONFIDENTIAL")
    .WithFontSize(60)
    .WithColor("#0000FF")
    .WithOpacity(0.2)
    .WithRotation(-30)
    .AtPosition(WatermarkPosition.DiagonalAscending));
```

### Watermark Positions

```csharp
WatermarkPosition.Center
WatermarkPosition.TopLeft
WatermarkPosition.TopRight
WatermarkPosition.BottomLeft
WatermarkPosition.BottomRight
WatermarkPosition.DiagonalAscending
WatermarkPosition.DiagonalDescending
```

## Page Numbers

```csharp
// Simple
var numbered = PdfPageNumbers.Add(document);

// With options
var numbered = PdfPageNumbers.Add(document, new PageNumberOptions
{
    Format = "Page {0} of {1}",
    Position = PageNumberPosition.BottomCenter,
    FontSize = 10,
    Margin = 30,
    StartNumber = 1
});

// Fluent builder
var numbered = PdfPageNumbers.Add(document, opt => opt
    .WithFormat("{0} / {1}")
    .AtPosition(PageNumberPosition.BottomRight)
    .WithFontSize(12)
    .WithMargin(25)
    .StartingFrom(1));
```

### Page Number Positions

```csharp
PageNumberPosition.BottomLeft
PageNumberPosition.BottomCenter
PageNumberPosition.BottomRight
PageNumberPosition.TopLeft
PageNumberPosition.TopCenter
PageNumberPosition.TopRight
```

## Table of Contents

```csharp
var entries = new List<TocEntry>
{
    new("Introduction", 1),
    new("Chapter 1", 2) { Level = 1 },
    new("  1.1 Section", 3) { Level = 2 },
    new("Chapter 2", 5) { Level = 1 },
    new("Conclusion", 10)
};

var toc = PdfTableOfContents.Generate(entries);
var withToc = PdfMerger.Merge(toc, document);
```
