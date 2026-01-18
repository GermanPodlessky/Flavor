# ASP.NET Core Integration

## Setup

```csharp
// Program.cs
builder.Services.AddFlavorAspNetCore(options =>
{
    options.PoolSize = Environment.ProcessorCount;
});

// Optional: warmup browser on startup (reduces first-request latency)
builder.Services.AddFlavorWarmup();
```

## Controller Extensions

```csharp
using Flavor.AspNetCore.Extensions;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    [HttpGet("invoice")]
    public IActionResult GetInvoice()
    {
        var html = "<h1>Invoice #123</h1>";
        return this.Pdf(html, "invoice.pdf");
    }

    [HttpGet("webpage")]
    public IActionResult GetWebpage()
    {
        return this.PdfFromUrl("https://example.com", "page.pdf");
    }

    [HttpGet("report")]
    public IActionResult GetReport()
    {
        var model = new ReportViewModel { Title = "Annual Report" };
        return this.PdfFromView("Reports/Annual", model, "report.pdf");
    }
}
```

## With Options

```csharp
return this.Pdf(html, "report.pdf", options => options
    .WithPageSize(PageSize.A4)
    .WithMargins(Margins.Normal)
    .WithBackground());
```

## Manual Usage

```csharp
public class ReportsController : ControllerBase
{
    private readonly IFlavorConverter _converter;

    public ReportsController(IFlavorConverter converter)
    {
        _converter = converter;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReport(int id)
    {
        var html = await GenerateReportHtml(id);
        var pdf = await _converter.ConvertHtmlAsync(html, options => options
            .WithPageSize(PageSize.A4)
            .WithBackground());

        return File(pdf.ToBytes(), "application/pdf", $"report-{id}.pdf");
    }
}
```

## Razor View to PDF

```csharp
public class PdfService
{
    private readonly IFlavorConverter _converter;
    private readonly IRazorViewRenderer _viewRenderer;

    public PdfService(IFlavorConverter converter, IRazorViewRenderer viewRenderer)
    {
        _converter = converter;
        _viewRenderer = viewRenderer;
    }

    public async Task<PdfDocument> RenderViewToPdfAsync<TModel>(string viewName, TModel model)
    {
        var html = await _viewRenderer.RenderViewToStringAsync(viewName, model);
        return await _converter.ConvertHtmlAsync(html);
    }
}
```

## Available Extension Methods

| Method | Description |
|--------|-------------|
| `this.Pdf(html, filename)` | HTML string to PDF |
| `this.Pdf(html, filename, options)` | HTML with options |
| `this.PdfFromUrl(url, filename)` | URL to PDF |
| `this.PdfFromUrl(url, filename, options)` | URL with options |
| `this.PdfFromView(viewName, model, filename)` | Razor view to PDF |
| `this.PdfFromView(viewName, model, filename, options)` | Razor view with options |
