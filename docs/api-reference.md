# API Reference

## FlavorConverter

```csharp
public class FlavorConverter : IFlavorConverter, IAsyncDisposable
{
    // Constructors
    FlavorConverter();
    FlavorConverter(Action<FlavorConverterOptions> configure);
    FlavorConverter(IRenderEngine engine);

    // HTML to PDF
    Task<PdfDocument> ConvertHtmlAsync(string html, CancellationToken ct = default);
    Task<PdfDocument> ConvertHtmlAsync(string html, PdfOptions options, CancellationToken ct = default);
    Task<PdfDocument> ConvertHtmlAsync(string html, Action<PdfOptionsBuilder> configure, CancellationToken ct = default);

    // URL to PDF
    Task<PdfDocument> ConvertUrlAsync(string url, CancellationToken ct = default);
    Task<PdfDocument> ConvertUrlAsync(string url, PdfOptions options, CancellationToken ct = default);
    Task<PdfDocument> ConvertUrlAsync(string url, Action<PdfOptionsBuilder> configure, CancellationToken ct = default);

    // File to PDF
    Task<PdfDocument> ConvertFileAsync(string filePath, CancellationToken ct = default);
    Task<PdfDocument> ConvertFileAsync(string filePath, PdfOptions options, CancellationToken ct = default);

    // Lifecycle
    Task WarmupAsync(CancellationToken ct = default);
    BrowserPoolStatistics GetPoolStatistics();
    ValueTask DisposeAsync();
}
```

## PdfDocument

```csharp
public class PdfDocument
{
    int PageCount { get; }

    byte[] ToBytes();
    Stream ToStream();
    Task SaveAsync(string filePath, CancellationToken ct = default);
}
```

## PdfOptionsBuilder

```csharp
public class PdfOptionsBuilder
{
    PdfOptionsBuilder WithPageSize(PageSize size);
    PdfOptionsBuilder WithMargins(Margins margins);
    PdfOptionsBuilder WithLandscape(bool landscape = true);
    PdfOptionsBuilder WithBackground(bool print = true);
    PdfOptionsBuilder WithScale(double scale);
    PdfOptionsBuilder WithJavaScript(bool enabled = true);
    PdfOptionsBuilder WithWaitCondition(WaitCondition condition);
    PdfOptionsBuilder WithWaitDelay(TimeSpan delay);
    PdfOptionsBuilder WithTimeout(TimeSpan timeout);
    PdfOptionsBuilder WithHeader(string html);
    PdfOptionsBuilder WithFooter(string html);
    PdfOptionsBuilder WithPageRanges(string ranges);
    PdfOptionsBuilder WithOmitBackground(bool omit = true);

    PdfOptions Build();
}
```

## PDF Operations

### PdfMerger

```csharp
static PdfDocument Merge(params PdfDocument[] documents);
static PdfDocument Merge(IEnumerable<PdfDocument> documents);
```

### PdfSplitter

```csharp
static IReadOnlyList<PdfDocument> SplitToPages(PdfDocument document);
static PdfDocument ExtractPages(PdfDocument document, params int[] pageNumbers);
static PdfDocument ExtractRange(PdfDocument document, int startPage, int endPage);
static PdfDocument RemovePages(PdfDocument document, params int[] pageNumbers);
static IReadOnlyList<PdfDocument> SplitByPageCount(PdfDocument document, int pagesPerChunk);
```

### PdfWatermark

```csharp
static PdfDocument AddText(PdfDocument document, string text);
static PdfDocument AddText(PdfDocument document, WatermarkOptions options);
static PdfDocument AddText(PdfDocument document, Action<WatermarkOptionsBuilder> configure);
```

### PdfPageNumbers

```csharp
static PdfDocument Add(PdfDocument document);
static PdfDocument Add(PdfDocument document, PageNumberOptions options);
static PdfDocument Add(PdfDocument document, Action<PageNumberOptionsBuilder> configure);
```

### PdfTableOfContents

```csharp
static PdfDocument Generate(IEnumerable<TocEntry> entries);
static PdfDocument Generate(IEnumerable<TocEntry> entries, TocOptions options);
```

### PdfSecurity

```csharp
static PdfDocument Encrypt(PdfDocument document, string password);
static PdfDocument Encrypt(PdfDocument document, PdfSecurityOptions options);
static PdfDocument Encrypt(PdfDocument document, Action<PdfSecurityOptionsBuilder> configure);
static bool IsEncrypted(byte[] pdfBytes);
static PdfDocument RemoveSecurity(PdfDocument document);
```

## DI Extensions

```csharp
// Core
services.AddFlavor();
services.AddFlavor(Action<FlavorConverterOptions> configure);
services.AddFlavorWarmup();

// ASP.NET Core
services.AddFlavorAspNetCore();
services.AddFlavorAspNetCore(Action<FlavorConverterOptions> configure);

// Playwright
services.AddFlavorWithPlaywright();
services.AddFlavorWithPlaywright(PlaywrightBrowserType browserType);
services.AddFlavorWithPlaywright(Action<FlavorConverterOptions> configure, PlaywrightBrowserType browserType);
services.AddFlavorWithPlaywrightAndWarmup();
```
