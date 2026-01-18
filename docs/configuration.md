# Configuration

## FlavorConverterOptions

```csharp
builder.Services.AddFlavor(options =>
{
    options.PoolSize = 4;
    options.Headless = true;
    options.BrowserExecutablePath = "/usr/bin/chromium";
});
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PoolSize` | int | 1 | Number of browser instances |
| `BrowserExecutablePath` | string? | null | Path to browser executable |
| `Headless` | bool | true | Run browser headless |
| `BrowserLaunchTimeout` | TimeSpan | 30s | Browser launch timeout |
| `ViewportWidth` | int | 1920 | Default viewport width |
| `ViewportHeight` | int | 1080 | Default viewport height |
| `IgnoreHttpsErrors` | bool | false | Ignore HTTPS errors |
| `BrowserArgs` | string[] | [] | Additional browser arguments |

## PdfOptions

```csharp
var pdf = await converter.ConvertHtmlAsync(html, options => options
    .WithPageSize(PageSize.A4)
    .WithMargins(Margins.Normal)
    .WithLandscape()
    .WithBackground()
    .WithHeader("<div>Header</div>")
    .WithFooter("<div>Page <span class='pageNumber'></span></div>")
    .WithWaitCondition(WaitCondition.NetworkIdle0)
    .WithTimeout(TimeSpan.FromSeconds(60)));
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PageSize` | PageSize | Letter | Page dimensions |
| `Margins` | Margins | Normal | Page margins |
| `Landscape` | bool | false | Landscape orientation |
| `PrintBackground` | bool | false | Print backgrounds |
| `Scale` | double | 1.0 | Content scale (0.1-2.0) |
| `WaitCondition` | WaitCondition | Load | When page is ready |
| `WaitDelay` | TimeSpan? | null | Additional wait after condition |
| `Timeout` | TimeSpan | 30s | Operation timeout |
| `HeaderTemplate` | string? | null | Header HTML |
| `FooterTemplate` | string? | null | Footer HTML |
| `PageRanges` | string? | null | Pages to print (e.g. "1-3,5") |

## Page Sizes

```csharp
PageSize.Letter    // 8.5 x 11 inches
PageSize.Legal     // 8.5 x 14 inches
PageSize.Tabloid   // 11 x 17 inches
PageSize.A3        // 297 x 420 mm
PageSize.A4        // 210 x 297 mm
PageSize.A5        // 148 x 210 mm

// Custom
new PageSize(8.5, 14)  // width x height in inches
```

## Margins

```csharp
Margins.None       // 0
Margins.Narrow     // 0.25" all sides
Margins.Normal     // 0.75" top/bottom, 0.7" left/right
Margins.Wide       // 1" all sides

// Custom (top, right, bottom, left in inches)
new Margins(1, 0.5, 1, 0.5)
```

## Wait Conditions

```csharp
WaitCondition.Load             // DOMContentLoaded
WaitCondition.DomContentLoaded // DOMContentLoaded event
WaitCondition.NetworkIdle0     // No network connections for 500ms
WaitCondition.NetworkIdle2     // Max 2 connections for 500ms
```

## Header/Footer Variables

Available in header and footer templates:

```html
<span class='date'></span>        <!-- Current date -->
<span class='title'></span>       <!-- Document title -->
<span class='url'></span>         <!-- Document URL -->
<span class='pageNumber'></span>  <!-- Current page -->
<span class='totalPages'></span>  <!-- Total pages -->
```
