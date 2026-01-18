using Flavor.AspNetCore.Razor;
using Flavor.Options;
using Microsoft.AspNetCore.Mvc;

namespace Flavor.AspNetCore;

/// <summary>
///     An <see cref="IActionResult" /> that renders a Razor view as a PDF file.
/// </summary>
/// <example>
///     <code>
/// public IActionResult GetInvoice(int id)
/// {
///     var model = new InvoiceModel { Id = id, ... };
///     return new ViewPdfResult("Invoice", model, "invoice.pdf");
/// }
/// </code>
/// </example>
public class ViewPdfResult : IActionResult
{
    private readonly string? _fileName;
    private readonly bool _inline;
    private readonly object? _model;
    private readonly PdfOptions _options;
    private readonly string _viewName;

    /// <summary>
    ///     Initializes a new instance of <see cref="ViewPdfResult" /> with a view name.
    /// </summary>
    /// <param name="viewName">The name or path of the Razor view to render.</param>
    /// <param name="fileName">The filename for the PDF download. If null, displays inline.</param>
    public ViewPdfResult(string viewName, string? fileName = null)
        : this(viewName, null, fileName, new PdfOptions())
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ViewPdfResult" /> with a view name and model.
    /// </summary>
    /// <param name="viewName">The name or path of the Razor view to render.</param>
    /// <param name="model">The model to pass to the view.</param>
    /// <param name="fileName">The filename for the PDF download. If null, displays inline.</param>
    public ViewPdfResult(string viewName, object? model, string? fileName = null)
        : this(viewName, model, fileName, new PdfOptions())
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ViewPdfResult" /> with a view name, model, and options.
    /// </summary>
    /// <param name="viewName">The name or path of the Razor view to render.</param>
    /// <param name="model">The model to pass to the view.</param>
    /// <param name="fileName">The filename for the PDF download. If null, displays inline.</param>
    /// <param name="options">The PDF generation options.</param>
    public ViewPdfResult(string viewName, object? model, string? fileName, PdfOptions options)
    {
        _viewName = viewName ?? throw new ArgumentNullException(nameof(viewName));
        _model = model;
        _fileName = fileName;
        _options = options ?? new PdfOptions();
        _inline = fileName == null;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ViewPdfResult" /> with a view name, model, and a builder action.
    /// </summary>
    /// <param name="viewName">The name or path of the Razor view to render.</param>
    /// <param name="model">The model to pass to the view.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    public ViewPdfResult(string viewName, object? model, string? fileName, Action<PdfOptionsBuilder> configure)
    {
        _viewName = viewName ?? throw new ArgumentNullException(nameof(viewName));
        _model = model;
        _fileName = fileName;
        _inline = fileName == null;

        var builder = new PdfOptionsBuilder();
        configure?.Invoke(builder);
        _options = builder.Build();
    }

    /// <inheritdoc />
    public async Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var viewRenderer = context.HttpContext.RequestServices.GetService(typeof(IRazorViewRenderer)) as IRazorViewRenderer
                           ?? throw new InvalidOperationException(
                               "IRazorViewRenderer is not registered. Call services.AddFlavorAspNetCore() in your DI configuration.");

        var converter = context.HttpContext.RequestServices.GetService(typeof(IFlavorConverter)) as IFlavorConverter
                        ?? throw new InvalidOperationException(
                            "IFlavorConverter is not registered. Call services.AddFlavor() in your DI configuration.");

        var html = await viewRenderer.RenderViewToStringAsync(_viewName, _model, context.HttpContext.RequestAborted);
        var pdf = await converter.ConvertHtmlAsync(html, _options, context.HttpContext.RequestAborted);
        var pdfBytes = pdf.ToBytes();

        var response = context.HttpContext.Response;
        response.ContentType = "application/pdf";
        response.ContentLength = pdfBytes.Length;

        var contentDisposition = _inline
            ? "inline"
            : $"attachment; filename=\"{_fileName}\"";
        response.Headers.ContentDisposition = contentDisposition;

        await response.Body.WriteAsync(pdfBytes, context.HttpContext.RequestAborted);
    }
}