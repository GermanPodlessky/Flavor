# Playwright Engine

Alternative to PuppeteerSharp. Supports Chromium, Firefox, and WebKit.

## Installation

```bash
dotnet add package Flavor.Playwright

# Install browsers (first time)
pwsh bin/Debug/net8.0/playwright.ps1 install
```

## Usage with DI

```csharp
using Flavor.Playwright;

// Chromium (default)
builder.Services.AddFlavorWithPlaywright();

// Firefox
builder.Services.AddFlavorWithPlaywright(PlaywrightBrowserType.Firefox);

// WebKit (Safari engine)
builder.Services.AddFlavorWithPlaywright(PlaywrightBrowserType.WebKit);

// With options
builder.Services.AddFlavorWithPlaywright(options =>
{
    options.Headless = true;
    options.ViewportWidth = 1920;
    options.PoolSize = 2;
}, PlaywrightBrowserType.Chromium);

// With warmup
builder.Services.AddFlavorWithPlaywrightAndWarmup();
```

## Direct Usage

```csharp
using Flavor.Playwright;

var engine = new PlaywrightEngine(
    new FlavorConverterOptions { PoolSize = 2 },
    browserType: PlaywrightBrowserType.Firefox);

await using var converter = new FlavorConverter(engine);
var pdf = await converter.ConvertHtmlAsync("<h1>Hello from Firefox!</h1>");
```

## Browser Types

| Type | Engine | Use Case |
|------|--------|----------|
| `Chromium` | Chrome/Edge | Default, best compatibility |
| `Firefox` | Gecko | Different rendering |
| `WebKit` | Safari | macOS/iOS testing |

## When to Use Playwright

- Need Firefox or WebKit rendering
- Already using Playwright in your project
- Want Microsoft's active development
- Need better cross-browser testing

## PuppeteerSharp vs Playwright

| | PuppeteerSharp | Playwright |
|---|---|---|
| Browsers | Chromium only | Chromium, Firefox, WebKit |
| Maintainer | Community | Microsoft |
| API style | Puppeteer port | Own design |
| Package size | Smaller | Larger |
