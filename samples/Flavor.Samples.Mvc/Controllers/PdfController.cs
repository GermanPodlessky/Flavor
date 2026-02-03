using Flavor.AspNetCore;
using Flavor.AspNetCore.Extensions;
using Flavor.Options;
using Microsoft.AspNetCore.Mvc;

namespace Flavor.Samples.Mvc.Controllers;

public class PdfController : Controller
{
    private readonly IFlavorConverter _converter;

    public PdfController(IFlavorConverter converter)
    {
        _converter = converter;
    }

    // ===========================================
    // Example 1: Simple HTML to PDF using extension method
    // ===========================================
    [HttpGet]
    public IActionResult Simple()
    {
        var html = "<h1>Hello from Flavor!</h1><p>This is a simple PDF.</p>";
        return this.Pdf(html, "simple.pdf");
    }

    // ===========================================
    // Example 2: PDF with custom options
    // ===========================================
    [HttpGet]
    public IActionResult CustomOptions()
    {
        var html = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 50px; }
                    h1 { font-size: 48px; }
                </style>
            </head>
            <body>
                <h1>Custom PDF</h1>
                <p>This PDF has custom options: A4 landscape with background.</p>
            </body>
            </html>";

        return this.Pdf(html, "custom.pdf", options => options
            .WithPageSize(PageSize.A4)
            .WithLandscape()
            .WithBackground()
            .WithMargins(0.5));
    }

    // ===========================================
    // Example 3: URL to PDF
    // ===========================================
    [HttpGet]
    public IActionResult FromUrl(string url = "https://example.com")
    {
        return this.PdfFromUrl(url, "webpage.pdf");
    }

    // ===========================================
    // Example 4: Inline PDF (displayed in browser)
    // ===========================================
    [HttpGet]
    public IActionResult Preview()
    {
        var html = "<h1>PDF Preview</h1><p>This PDF is displayed inline in the browser.</p>";
        return new PdfResult(html); // No filename = inline
    }

    // ===========================================
    // Example 5: Using PdfResult directly
    // ===========================================
    [HttpGet]
    public IActionResult Report()
    {
        var html = GenerateReportHtml();
        return new PdfResult(html, "report.pdf", options => options
            .WithPageSize(PageSize.A4)
            .WithPageNumbers("Page {page} of {pages}")
            .WithMargins(1, 0.75, 1, 0.75));
    }

    // ===========================================
    // Example 6: Using IFlavorConverter directly
    // ===========================================
    [HttpGet]
    public async Task<IActionResult> Advanced(CancellationToken ct)
    {
        var html = "<h1>Advanced PDF</h1><p>Generated using IFlavorConverter directly.</p>";

        var options = new PdfOptionsBuilder()
            .WithPageSize(PageSize.Letter)
            .WithBackground()
            .Build();

        var pdf = await _converter.ConvertHtmlAsync(html, options, ct);

        // You can process the PDF here (e.g., add watermark, merge, etc.)

        return File(pdf.ToBytes(), "application/pdf", "advanced.pdf");
    }

    // ===========================================
    // Example 7: Invoice generation
    // ===========================================
    [HttpGet]
    public IActionResult Invoice(string id = "INV-001")
    {
        var html = GenerateInvoiceHtml(id);
        return this.Pdf(html, $"invoice-{id}.pdf", options => options
            .WithPageSize(PageSize.A4)
            .WithBackground());
    }

    // ===========================================
    // Helper methods
    // ===========================================
    private static string GenerateReportHtml()
    {
        return @$"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; margin: 40px; color: #333; }}
        h1 {{ color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }}
        .section {{ margin: 30px 0; }}
        .metric {{ display: inline-block; width: 200px; padding: 20px; background: #f8f9fa; border-radius: 8px; margin: 10px; text-align: center; }}
        .metric-value {{ font-size: 32px; font-weight: bold; color: #3498db; }}
        .metric-label {{ color: #666; margin-top: 5px; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th {{ background: #3498db; color: white; padding: 12px; text-align: left; }}
        td {{ padding: 12px; border-bottom: 1px solid #ddd; }}
        tr:hover {{ background: #f5f5f5; }}
    </style>
</head>
<body>
    <h1>Monthly Report</h1>
    <p>Generated on {DateTime.Now:MMMM dd, yyyy}</p>
    <div class=""section"">
        <h2>Key Metrics</h2>
        <div class=""metric""><div class=""metric-value"">1,234</div><div class=""metric-label"">Total Users</div></div>
        <div class=""metric""><div class=""metric-value"">$45,678</div><div class=""metric-label"">Revenue</div></div>
        <div class=""metric""><div class=""metric-value"">89%</div><div class=""metric-label"">Satisfaction</div></div>
    </div>
    <div class=""section"">
        <h2>Top Products</h2>
        <table>
            <tr><th>Product</th><th>Sales</th><th>Revenue</th></tr>
            <tr><td>Product A</td><td>523</td><td>$15,690</td></tr>
            <tr><td>Product B</td><td>412</td><td>$12,360</td></tr>
            <tr><td>Product C</td><td>298</td><td>$8,940</td></tr>
        </table>
    </div>
</body>
</html>";
    }

    private static string GenerateInvoiceHtml(string invoiceNumber)
    {
        return @$"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        .header {{ display: flex; justify-content: space-between; border-bottom: 2px solid #333; padding-bottom: 20px; }}
        .logo {{ font-size: 32px; font-weight: bold; color: #3498db; }}
        .invoice-info {{ text-align: right; }}
        .invoice-number {{ font-size: 24px; color: #333; }}
        .addresses {{ display: flex; justify-content: space-between; margin: 30px 0; }}
        .address-block {{ width: 45%; }}
        .address-block h3 {{ color: #666; margin-bottom: 10px; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 30px; }}
        th {{ background: #3498db; color: white; padding: 12px; text-align: left; }}
        td {{ padding: 12px; border-bottom: 1px solid #ddd; }}
        .totals {{ margin-top: 30px; text-align: right; }}
        .total-row {{ padding: 10px 0; }}
        .grand-total {{ font-size: 24px; font-weight: bold; color: #3498db; }}
    </style>
</head>
<body>
    <div class=""header"">
        <div class=""logo"">ACME Corp</div>
        <div class=""invoice-info"">
            <div class=""invoice-number"">INVOICE {invoiceNumber}</div>
            <div>Date: {DateTime.Now:yyyy-MM-dd}</div>
            <div>Due: {DateTime.Now.AddDays(30):yyyy-MM-dd}</div>
        </div>
    </div>
    <div class=""addresses"">
        <div class=""address-block""><h3>From:</h3><p>ACME Corporation<br>123 Business St<br>New York, NY 10001</p></div>
        <div class=""address-block""><h3>Bill To:</h3><p>John Doe<br>456 Customer Ave<br>Los Angeles, CA 90001</p></div>
    </div>
    <table>
        <tr><th>Description</th><th>Qty</th><th>Unit Price</th><th>Total</th></tr>
        <tr><td>Web Development Services</td><td>40</td><td>$150.00</td><td>$6,000.00</td></tr>
        <tr><td>UI/UX Design</td><td>20</td><td>$125.00</td><td>$2,500.00</td></tr>
        <tr><td>Hosting (Annual)</td><td>1</td><td>$500.00</td><td>$500.00</td></tr>
    </table>
    <div class=""totals"">
        <div class=""total-row"">Subtotal: $9,000.00</div>
        <div class=""total-row"">Tax (10%): $900.00</div>
        <div class=""total-row grand-total"">Total: $9,900.00</div>
    </div>
</body>
</html>";
    }
}
