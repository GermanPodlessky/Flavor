using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Flavor.Options;

namespace Flavor.Benchmarks;

/// <summary>
///     Benchmarks for PDF generation performance.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[RankColumn]
public class PdfGenerationBenchmarks
{
    private const string SimpleHtml = "<html><body><h1>Hello World</h1><p>This is a simple test.</p></body></html>";

    private const string HtmlWithCss = """
                                       <!DOCTYPE html>
                                       <html>
                                       <head>
                                           <style>
                                               body { font-family: Arial, sans-serif; margin: 40px; background: #f5f5f5; }
                                               .container { max-width: 800px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
                                               h1 { color: #333; border-bottom: 2px solid #4CAF50; padding-bottom: 10px; }
                                               .grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin-top: 20px; }
                                               .card { background: #f9f9f9; padding: 15px; border-radius: 4px; }
                                               .card h3 { margin: 0 0 10px 0; color: #4CAF50; }
                                               table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                                               th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }
                                               th { background: #4CAF50; color: white; }
                                               tr:nth-child(even) { background: #f9f9f9; }
                                           </style>
                                       </head>
                                       <body>
                                           <div class="container">
                                               <h1>Performance Report</h1>
                                               <div class="grid">
                                                   <div class="card"><h3>Metric 1</h3><p>Value: 123</p></div>
                                                   <div class="card"><h3>Metric 2</h3><p>Value: 456</p></div>
                                                   <div class="card"><h3>Metric 3</h3><p>Value: 789</p></div>
                                               </div>
                                               <table>
                                                   <thead><tr><th>Item</th><th>Quantity</th><th>Price</th><th>Total</th></tr></thead>
                                                   <tbody>
                                                       <tr><td>Product A</td><td>10</td><td>$25.00</td><td>$250.00</td></tr>
                                                       <tr><td>Product B</td><td>5</td><td>$50.00</td><td>$250.00</td></tr>
                                                       <tr><td>Product C</td><td>20</td><td>$10.00</td><td>$200.00</td></tr>
                                                   </tbody>
                                               </table>
                                           </div>
                                       </body>
                                       </html>
                                       """;

    private const string HtmlWithImages = """
                                          <!DOCTYPE html>
                                          <html>
                                          <head>
                                              <style>
                                                  body { font-family: Arial, sans-serif; margin: 40px; }
                                                  .gallery { display: flex; flex-wrap: wrap; gap: 10px; }
                                                  .gallery img { width: 200px; height: 150px; object-fit: cover; border-radius: 4px; }
                                              </style>
                                          </head>
                                          <body>
                                              <h1>Image Gallery</h1>
                                              <div class="gallery">
                                                  <img src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='150'%3E%3Crect fill='%234CAF50' width='200' height='150'/%3E%3Ctext x='50%25' y='50%25' fill='white' text-anchor='middle' dy='.3em'%3EImage 1%3C/text%3E%3C/svg%3E" alt="Image 1">
                                                  <img src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='150'%3E%3Crect fill='%232196F3' width='200' height='150'/%3E%3Ctext x='50%25' y='50%25' fill='white' text-anchor='middle' dy='.3em'%3EImage 2%3C/text%3E%3C/svg%3E" alt="Image 2">
                                                  <img src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='150'%3E%3Crect fill='%23FF9800' width='200' height='150'/%3E%3Ctext x='50%25' y='50%25' fill='white' text-anchor='middle' dy='.3em'%3EImage 3%3C/text%3E%3C/svg%3E" alt="Image 3">
                                                  <img src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='150'%3E%3Crect fill='%239C27B0' width='200' height='150'/%3E%3Ctext x='50%25' y='50%25' fill='white' text-anchor='middle' dy='.3em'%3EImage 4%3C/text%3E%3C/svg%3E" alt="Image 4">
                                              </div>
                                          </body>
                                          </html>
                                          """;

    private readonly string _largeHtml = GenerateLargeHtml();
    private FlavorConverter _converter = null!;

    private static string GenerateLargeHtml()
    {
        var rows = string.Join("\n", Enumerable.Range(1, 100).Select(i =>
            $"<tr><td>Row {i}</td><td>Data {i * 10}</td><td>Value {i * 100}</td><td>${i * 25:F2}</td></tr>"));

        return $$"""
                 <!DOCTYPE html>
                 <html>
                 <head>
                     <style>
                         body { font-family: Arial, sans-serif; margin: 20px; }
                         table { width: 100%; border-collapse: collapse; }
                         th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                         th { background: #4CAF50; color: white; }
                         tr:nth-child(even) { background: #f9f9f9; }
                     </style>
                 </head>
                 <body>
                     <h1>Large Data Report</h1>
                     <table>
                         <thead><tr><th>Name</th><th>Quantity</th><th>Value</th><th>Price</th></tr></thead>
                         <tbody>{{rows}}</tbody>
                     </table>
                 </body>
                 </html>
                 """;
    }

    [GlobalSetup]
    public async Task Setup()
    {
        _converter = new FlavorConverter(options => { options.PoolSize = 1; });
        await _converter.WarmupAsync();
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _converter.DisposeAsync();
    }

    [Benchmark(Description = "Simple HTML")]
    public async Task<PdfDocument> SimpleHtmlToPdf()
    {
        return await _converter.ConvertHtmlAsync(SimpleHtml);
    }

    [Benchmark(Description = "HTML with CSS (Grid, Table)")]
    public async Task<PdfDocument> HtmlWithCssToPdf()
    {
        return await _converter.ConvertHtmlAsync(HtmlWithCss, options => options
            .WithBackground());
    }

    [Benchmark(Description = "HTML with Images")]
    public async Task<PdfDocument> HtmlWithImagesToPdf()
    {
        return await _converter.ConvertHtmlAsync(HtmlWithImages, options => options
            .WithBackground());
    }

    [Benchmark(Description = "Large Document (100 rows)")]
    public async Task<PdfDocument> LargeDocumentToPdf()
    {
        return await _converter.ConvertHtmlAsync(_largeHtml, options => options
            .WithPageSize(PageSize.A4)
            .WithMargins(Margins.Narrow));
    }

    [Benchmark(Description = "With Header/Footer")]
    public async Task<PdfDocument> WithHeaderFooterToPdf()
    {
        return await _converter.ConvertHtmlAsync(HtmlWithCss, options => options
            .WithPageSize(PageSize.A4)
            .WithHeader("<div style='font-size:10px;text-align:center;width:100%'>Header - <span class='title'></span></div>")
            .WithFooter(
                "<div style='font-size:10px;text-align:center;width:100%'>Page <span class='pageNumber'></span> of <span class='totalPages'></span></div>")
            .WithBackground());
    }
}