using Flavor;
using Flavor.AspNetCore.Extensions;
using Flavor.AspNetCore.MinimalApi;
using Flavor.Extensions;
using Flavor.Options;

var builder = WebApplication.CreateBuilder(args);

// Add Flavor services with Minimal API configuration
builder.Services.AddFlavor();
builder.Services.AddFlavorAspNetCore(options =>
{
    options.DefaultFileName = "document.pdf";
    options.IncludePageCountHeader = true;
    options.CachePolicy = new PdfCachePolicy
    {
        Enabled = true,
        MaxAgeSeconds = 300,
        Private = true
    };
    options.CustomHeaders["X-Generator"] = "Flavor";
});

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ===========================================
// Example 1: Simple HTML to PDF endpoint
// ===========================================
app.MapPost("/api/pdf/simple", (string html) =>
    PdfResults.FromHtml(html, "simple.pdf"))
    .WithName("SimplePdf")
    .WithOpenApi();

// ===========================================
// Example 2: Using typed request model
// ===========================================
app.MapPost("/api/pdf/html", (HtmlToPdfRequest request) =>
    PdfResults.FromHtml(request.Html, request.FileName ?? "document.pdf"))
    .WithPdfValidation()
    .WithPdfLogging()
    .WithName("HtmlToPdf")
    .WithOpenApi();

// ===========================================
// Example 3: URL to PDF capture
// ===========================================
app.MapPost("/api/pdf/url", (UrlToPdfRequest request) =>
    PdfResults.FromUrl(request.Url, request.FileName ?? "page.pdf"))
    .WithPdfValidation()
    .WithPdfLogging()
    .WithName("UrlToPdf")
    .WithOpenApi();

// =========================================== 
// Example 4: Inline PDF display (in browser)
// ===========================================
app.MapPost("/api/pdf/preview", (string html) =>
    PdfResults.Inline(html))
    .WithName("PreviewPdf")
    .WithOpenApi();

// ===========================================
// Example 5: With custom PDF options
// ===========================================
app.MapPost("/api/pdf/custom", (HtmlToPdfRequest request) =>
    PdfResults.FromHtml(request.Html, "custom.pdf", options => options
        .WithPageSize(PageSize.A4)
        .WithLandscape()
        .WithMargins(0.5)
        .WithBackground()
        .WithPageNumbers("Page {page} of {pages}")))
    .WithPdfFilters()
    .WithName("CustomPdf")
    .WithOpenApi();

// ===========================================
// Example 6: PDF Group with shared configuration
// ===========================================
var invoiceGroup = app.MapFlavorPdfGroup("/api/invoices")
    .WithTags("Invoices");

invoiceGroup.MapPost("/generate", (InvoiceRequest request) =>
{
    var html = GenerateInvoiceHtml(request);
    return PdfResults.FromHtml(html, $"invoice-{request.InvoiceNumber}.pdf");
}).WithName("GenerateInvoice");

invoiceGroup.MapPost("/preview", (InvoiceRequest request) =>
{
    var html = GenerateInvoiceHtml(request);
    return PdfResults.Inline(html);
}).WithName("PreviewInvoice");

// ===========================================
// Example 7: Using IFlavorConverter directly
// ===========================================
app.MapPost("/api/pdf/advanced", async (
    HtmlToPdfRequest request,
    IFlavorConverter converter,
    CancellationToken ct) =>
{
    var options = new PdfOptionsBuilder()
        .WithPageSize(PageSize.A4)
        .WithBackground()
        .Build();

    var pdf = await converter.ConvertHtmlAsync(request.Html, options, ct);

    // You can do additional processing here...

    return PdfResults.FromDocument(pdf, "advanced.pdf");
})
.WithName("AdvancedPdf")
.WithOpenApi();

// ===========================================
// Example 8: Rate-limited endpoint
// ===========================================
app.MapPost("/api/pdf/limited", (HtmlToPdfRequest request) =>
    PdfResults.FromHtml(request.Html, "limited.pdf"))
    .WithPdfRateLimiting(maxConcurrentRequests: 5)
    .WithName("LimitedPdf")
    .WithOpenApi();

app.Run();

// ===========================================
// Helper methods and models
// ===========================================

static string GenerateInvoiceHtml(InvoiceRequest request)
{
    var itemsHtml = string.Join("\n", request.Items.Select(i =>
        $"<tr><td>{i.Name}</td><td>{i.Quantity}</td><td>${i.Price:F2}</td><td>${i.Quantity * i.Price:F2}</td></tr>"));

    var total = request.Items.Sum(i => i.Quantity * i.Price);

    return @$"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        .header {{ border-bottom: 2px solid #333; padding-bottom: 20px; }}
        .invoice-number {{ color: #666; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 10px; text-align: left; }}
        th {{ background-color: #f5f5f5; }}
        .total {{ font-weight: bold; font-size: 1.2em; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>INVOICE</h1>
        <p class=""invoice-number"">#{request.InvoiceNumber}</p>
        <p>Date: {DateTime.Now:yyyy-MM-dd}</p>
    </div>
    <p><strong>Customer:</strong> {request.CustomerName}</p>
    <table>
        <tr>
            <th>Item</th>
            <th>Quantity</th>
            <th>Price</th>
            <th>Total</th>
        </tr>
        {itemsHtml}
        <tr class=""total"">
            <td colspan=""3"">Total</td>
            <td>${total:F2}</td>
        </tr>
    </table>
</body>
</html>";
}

public record InvoiceRequest(
    string InvoiceNumber,
    string CustomerName,
    List<InvoiceItem> Items);

public record InvoiceItem(
    string Name,
    int Quantity,
    decimal Price);
