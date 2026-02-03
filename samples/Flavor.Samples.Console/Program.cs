using Flavor;
using Flavor.Operations;
using Flavor.Options;

Console.WriteLine("===========================================");
Console.WriteLine("  Flavor Console Sample");
Console.WriteLine("===========================================");
Console.WriteLine();

// Create converter instance
using var converter = new FlavorConverter();

// Warmup (optional, but improves first conversion speed)
Console.WriteLine("Warming up browser...");
await converter.WarmupAsync();
Console.WriteLine("Ready!");
Console.WriteLine();

// ===========================================
// Example 1: Simple HTML to PDF
// ===========================================
Console.WriteLine("1. Simple HTML to PDF");
var simpleHtml = "<h1>Hello, World!</h1><p>This is a simple PDF.</p>";
var simplePdf = await converter.ConvertHtmlAsync(simpleHtml);
await simplePdf.SaveAsync("output/simple.pdf");
Console.WriteLine("   Saved: output/simple.pdf");
Console.WriteLine();

// ===========================================
// Example 2: HTML with custom options
// ===========================================
Console.WriteLine("2. PDF with custom options");
var styledHtml = """
    <!DOCTYPE html>
    <html>
    <head>
        <style>
            body {
                font-family: Arial, sans-serif;
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                color: white;
                padding: 50px;
                min-height: 100vh;
                margin: 0;
            }
            h1 { font-size: 48px; margin-bottom: 20px; }
            p { font-size: 18px; line-height: 1.6; }
        </style>
    </head>
    <body>
        <h1>Styled PDF Document</h1>
        <p>This PDF was generated with custom options:</p>
        <ul>
            <li>A4 page size</li>
            <li>Landscape orientation</li>
            <li>Background graphics enabled</li>
            <li>Custom margins</li>
        </ul>
    </body>
    </html>
    """;

var options = new PdfOptionsBuilder()
    .WithPageSize(PageSize.A4)
    .WithLandscape()
    .WithBackground()
    .WithMargins(0.5)
    .Build();

var styledPdf = await converter.ConvertHtmlAsync(styledHtml, options);
await styledPdf.SaveAsync("output/styled.pdf");
Console.WriteLine($"   Saved: output/styled.pdf ({styledPdf.PageCount} pages)");
Console.WriteLine();

// ===========================================
// Example 3: URL to PDF
// ===========================================
Console.WriteLine("3. URL to PDF (capturing example.com)");
var urlPdf = await converter.ConvertUrlAsync("https://example.com");
await urlPdf.SaveAsync("output/webpage.pdf");
Console.WriteLine($"   Saved: output/webpage.pdf ({urlPdf.PageCount} pages)");
Console.WriteLine();

// ===========================================
// Example 4: Multi-page document with headers/footers
// ===========================================
Console.WriteLine("4. Multi-page document with page numbers");
var multiPageHtml = GenerateMultiPageHtml();
var multiPageOptions = new PdfOptionsBuilder()
    .WithPageSize(PageSize.A4)
    .WithMargins(1, 0.75, 1, 0.75)
    .WithPageNumbers("Page {page} of {pages}")
    .WithBackground()
    .Build();

var multiPagePdf = await converter.ConvertHtmlAsync(multiPageHtml, multiPageOptions);
await multiPagePdf.SaveAsync("output/multipage.pdf");
Console.WriteLine($"   Saved: output/multipage.pdf ({multiPagePdf.PageCount} pages)");
Console.WriteLine();

// ===========================================
// Example 5: PDF operations - Merge
// ===========================================
Console.WriteLine("5. Merging multiple PDFs");
var pdf1 = await converter.ConvertHtmlAsync("<h1>Document 1</h1><p>First document content.</p>");
var pdf2 = await converter.ConvertHtmlAsync("<h1>Document 2</h1><p>Second document content.</p>");
var pdf3 = await converter.ConvertHtmlAsync("<h1>Document 3</h1><p>Third document content.</p>");

var mergedPdf = PdfMerger.Merge(pdf1, pdf2, pdf3);
await mergedPdf.SaveAsync("output/merged.pdf");
Console.WriteLine($"   Saved: output/merged.pdf ({mergedPdf.PageCount} pages)");
Console.WriteLine();

// ===========================================
// Example 6: PDF operations - Add watermark
// ===========================================
Console.WriteLine("6. Adding watermark");
var documentPdf = await converter.ConvertHtmlAsync("""
    <h1>Confidential Document</h1>
    <p>This document contains sensitive information.</p>
    <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>
    """);

var watermarkOptions = new WatermarkOptions
{
    Text = "CONFIDENTIAL",
    FontSize = 60,
    Opacity = 0.3f,
    Rotation = -45,
    Color = "#FF0000"
};

var watermarkedPdf = PdfWatermark.AddText(documentPdf, watermarkOptions);
await watermarkedPdf.SaveAsync("output/watermarked.pdf");
Console.WriteLine("   Saved: output/watermarked.pdf");
Console.WriteLine();

// ===========================================
// Example 7: Different output formats
// ===========================================
Console.WriteLine("7. Different output formats");
var outputPdf = await converter.ConvertHtmlAsync("<h1>Output Formats</h1>");

// As bytes
var bytes = outputPdf.ToBytes();
Console.WriteLine($"   Bytes: {bytes.Length} bytes");

// As Base64
var base64 = outputPdf.ToBase64();
Console.WriteLine($"   Base64: {base64.Length} characters");

// As Data URI (useful for embedding in HTML)
var dataUri = outputPdf.ToDataUri();
Console.WriteLine($"   Data URI: {dataUri[..50]}...");

// As Stream
using var stream = outputPdf.ToStream();
Console.WriteLine($"   Stream: {stream.Length} bytes");
Console.WriteLine();

// ===========================================
// Example 8: Batch processing
// ===========================================
Console.WriteLine("8. Batch processing (5 documents)");
var tasks = Enumerable.Range(1, 5).Select(async i =>
{
    var html = $"<h1>Batch Document {i}</h1><p>Generated at {DateTime.Now:HH:mm:ss.fff}</p>";
    var pdf = await converter.ConvertHtmlAsync(html);
    await pdf.SaveAsync($"output/batch-{i}.pdf");
    return i;
});

var results = await Task.WhenAll(tasks);
Console.WriteLine($"   Generated {results.Length} documents");
Console.WriteLine();

// ===========================================
// Pool statistics
// ===========================================
Console.WriteLine("Browser Pool Statistics:");
var stats = converter.GetPoolStatistics();
Console.WriteLine($"   Active instances: {stats.ActiveInstances}");
Console.WriteLine($"   Available slots: {stats.AvailableSlots}");
Console.WriteLine($"   Pool size: {stats.PoolSize}");
Console.WriteLine();

Console.WriteLine("===========================================");
Console.WriteLine("  All examples completed successfully!");
Console.WriteLine("  Check the 'output' folder for results.");
Console.WriteLine("===========================================");

// ===========================================
// Helper methods
// ===========================================
static string GenerateMultiPageHtml()
{
    var chapters = new[]
    {
        ("Introduction", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."),
        ("Getting Started", "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur."),
        ("Advanced Topics", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium."),
        ("Best Practices", "Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit."),
        ("Conclusion", "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum.")
    };

    var chaptersHtml = string.Join("\n", chapters.Select((c, i) =>
        $@"<div class=""chapter"" style=""page-break-before: {(i > 0 ? "always" : "auto")}"">
            <h2>Chapter {i + 1}: {c.Item1}</h2>
            <p>{c.Item2}</p><p>{c.Item2}</p><p>{c.Item2}</p>
        </div>"));

    return @$"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Georgia, serif; margin: 0; padding: 40px; line-height: 1.8; }}
        h1 {{ text-align: center; color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 20px; }}
        h2 {{ color: #3498db; margin-top: 30px; }}
        p {{ text-align: justify; color: #333; }}
        .chapter {{ margin-bottom: 40px; }}
    </style>
</head>
<body>
    <h1>Sample Multi-Page Document</h1>
    <p style=""text-align: center; color: #666;"">Generated with Flavor</p>
    {chaptersHtml}
</body>
</html>";
}
