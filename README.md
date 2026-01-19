# Flavor

[![NuGet](https://img.shields.io/nuget/v/Flavor.svg)](https://www.nuget.org/packages/Flavor)
[![License](https://img.shields.io/badge/license-Apache-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%2B-512BD4)](https://dotnet.microsoft.com/)

**HTML to PDF for .NET** — free, fast, cross-platform.

```csharp
var pdf = await new FlavorConverter().ConvertHtmlAsync("<h1>Hello</h1>");
await pdf.SaveAsync("hello.pdf");
```

## Contents

- [Install](#install) · [Why Flavor?](#why-flavor) · [Quick Start](#quick-start) · [Options](#options)
- [ASP.NET Core](#aspnet-core) · [PDF Operations](#pdf-operations) · [Security](#security)
- [Playwright](#playwright) · [Performance](#performance) · [Deployment](#deployment)

**[Full Documentation](docs/)** — detailed guides, API reference, deployment examples

---

## Install

```bash
dotnet add package Flavor
dotnet add package Flavor.AspNetCore    # optional
dotnet add package Flavor.Playwright    # optional
```

First run downloads Chromium (~150MB). Use `BrowserExecutablePath` to skip.

---

## Why Flavor?

You can use PuppeteerSharp and PdfSharpCore directly. Flavor adds:

**Without Flavor:**
```csharp
await new BrowserFetcher().DownloadAsync();
var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
var page = await browser.NewPageAsync();
await page.SetContentAsync(html);
await page.PdfAsync("output.pdf", new PdfOptions { Format = PaperFormat.A4 });
await browser.CloseAsync();
// + separate code for merge, watermarks, encryption...
```

**With Flavor:**
```csharp
var pdf = await new FlavorConverter().ConvertHtmlAsync("<h1>Hello</h1>");
```

| | Direct libs | Flavor |
|-|-------------|--------|
| Browser lifecycle | Manual | Managed |
| Browser reuse | DIY | Built-in pool |
| ASP.NET Core | Boilerplate | `this.Pdf()` |
| Merge/Split/Watermark | Different APIs | Unified |
| Resource cleanup | Easy to leak | Automatic |

**When to skip Flavor:** single simple use-case, need full browser control, already have your own wrapper.

---

## Quick Start

```csharp
// HTML
var pdf = await converter.ConvertHtmlAsync("<h1>Hello</h1>");

// URL
var pdf = await converter.ConvertUrlAsync("https://example.com");

// With options
var pdf = await converter.ConvertHtmlAsync(html, o => o
    .WithPageSize(PageSize.A4)
    .WithMargins(Margins.Normal)
    .WithBackground());
```

---

## Options

```csharp
// Page sizes
.WithPageSize(PageSize.A4)
.WithPageSize(new PageSize(8.5, 11))  // custom inches

// Margins
.WithMargins(Margins.None)
.WithMargins(new Margins(1, 0.5, 1, 0.5))  // top, right, bottom, left

// Headers/Footers
.WithHeader("<div style='font-size:10px'><span class='title'></span></div>")
.WithFooter("<div>Page <span class='pageNumber'></span> of <span class='totalPages'></span></div>")

// Wait for JS
.WithWaitCondition(WaitCondition.NetworkIdle0)
.WithWaitDelay(TimeSpan.FromSeconds(1))
```

---

## ASP.NET Core

```csharp
// Program.cs
builder.Services.AddFlavorAspNetCore();
builder.Services.AddFlavorWarmup();  // optional, reduces first-request latency
```

```csharp
// Controller
[HttpGet("invoice")]
public IActionResult GetInvoice()
{
    return this.Pdf("<h1>Invoice</h1>", "invoice.pdf");
}

[HttpGet("report")]
public IActionResult GetReport()
{
    return this.PdfFromView("Reports/Template", model, "report.pdf");
}
```

---

## PDF Operations

```csharp
using Flavor.Operations;

// Merge
var merged = PdfMerger.Merge(pdf1, pdf2, pdf3);

// Split
var pages = PdfSplitter.SplitToPages(document);
var extracted = PdfSplitter.ExtractPages(document, 1, 3, 5);

// Watermark
var marked = PdfWatermark.AddText(document, "DRAFT");
var marked = PdfWatermark.AddText(document, o => o
    .WithText("CONFIDENTIAL")
    .WithOpacity(0.3)
    .WithRotation(-45));

// Page numbers
var numbered = PdfPageNumbers.Add(document);
```

---

## Security

```csharp
using Flavor.Security;

// Password protect
var encrypted = PdfSecurity.Encrypt(document, "secret");

// With permissions
var encrypted = PdfSecurity.Encrypt(document, new PdfSecurityOptions
{
    UserPassword = "view",
    OwnerPassword = "edit",
    AllowPrinting = false
});
```

---

## Playwright

Alternative to PuppeteerSharp. Supports Firefox and WebKit.

```bash
dotnet add package Flavor.Playwright
pwsh bin/Debug/net8.0/playwright.ps1 install
```

```csharp
builder.Services.AddFlavorWithPlaywright(PlaywrightBrowserType.Firefox);
```

---

## Performance

Browser pool for concurrent requests:

```csharp
builder.Services.AddFlavor(o => o.PoolSize = 4);
```

| Pool | Throughput |
|------|------------|
| 1 | ~8 PDF/sec |
| 2 | ~12 PDF/sec |
| 4 | ~18 PDF/sec |

---

## System Requirements

Uses real browser engine = heavy dependencies.

| | |
|-|-|
| Browser | ~150MB, auto-downloaded |
| Memory | ~150MB per instance |
| Linux | Needs system libs (see Docker) |

---

## Deployment

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y chromium \
    fonts-liberation libasound2 libatk-bridge2.0-0 libcups2 \
    libdrm2 libgbm1 libgtk-3-0 libnspr4 libnss3 \
    libxcomposite1 libxdamage1 libxfixes3 libxrandr2 \
    && rm -rf /var/lib/apt/lists/*

ENV FLAVOR_BROWSER_PATH=/usr/bin/chromium
```

### Kubernetes

```yaml
resources:
  limits:
    memory: "1Gi"
env:
  - name: Flavor__PoolSize
    value: "2"
```

Scale pods, not pool size.

### Azure

```csharp
// App Service
options.BrowserArgs = ["--no-sandbox", "--disable-dev-shm-usage"];

// Functions: use Premium Plan or Container Apps
```

### AWS Lambda

250MB limit. Use Lambda Layer with Chrome, or better — Fargate.

---


## Contributing
I appreciate feedback and contribution to this repo!


## License

[Apache 2.0](LICENSE)

---

Built on [PuppeteerSharp](https://github.com/hardkoded/puppeteer-sharp), [Playwright](https://playwright.dev/dotnet/), [PdfSharpCore](https://github.com/ststeiger/PdfSharpCore).
