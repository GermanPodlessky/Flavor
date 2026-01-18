using Flavor.Options;
using Microsoft.AspNetCore.Mvc;

namespace Flavor.AspNetCore.Extensions;

/// <summary>
///     Extension methods for controllers to easily return PDF results.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    ///     Creates a <see cref="PdfResult" /> that renders HTML content as a PDF file.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="html">The HTML content to convert.</param>
    /// <param name="fileName">The filename for the PDF download. If null, displays inline.</param>
    /// <returns>A <see cref="PdfResult" /> instance.</returns>
    public static PdfResult Pdf(this ControllerBase controller, string html, string? fileName = null)
    {
        return new PdfResult(html, fileName);
    }

    /// <summary>
    ///     Creates a <see cref="PdfResult" /> that renders HTML content as a PDF file with options.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="html">The HTML content to convert.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <returns>A <see cref="PdfResult" /> instance.</returns>
    public static PdfResult Pdf(this ControllerBase controller, string html, string? fileName, Action<PdfOptionsBuilder> configure)
    {
        return new PdfResult(html, fileName, configure);
    }

    /// <summary>
    ///     Creates a <see cref="UrlPdfResult" /> that renders a URL as a PDF file.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="url">The URL to convert.</param>
    /// <param name="fileName">The filename for the PDF download. If null, displays inline.</param>
    /// <returns>A <see cref="UrlPdfResult" /> instance.</returns>
    public static UrlPdfResult PdfFromUrl(this ControllerBase controller, string url, string? fileName = null)
    {
        return new UrlPdfResult(url, fileName);
    }

    /// <summary>
    ///     Creates a <see cref="UrlPdfResult" /> that renders a URL as a PDF file with options.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="url">The URL to convert.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <returns>A <see cref="UrlPdfResult" /> instance.</returns>
    public static UrlPdfResult PdfFromUrl(this ControllerBase controller, string url, string? fileName, Action<PdfOptionsBuilder> configure)
    {
        return new UrlPdfResult(url, fileName, configure);
    }

    /// <summary>
    ///     Creates a <see cref="ViewPdfResult" /> that renders a Razor view as a PDF file.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="viewName">The name or path of the Razor view.</param>
    /// <param name="fileName">The filename for the PDF download. If null, displays inline.</param>
    /// <returns>A <see cref="ViewPdfResult" /> instance.</returns>
    public static ViewPdfResult PdfFromView(this ControllerBase controller, string viewName, string? fileName = null)
    {
        return new ViewPdfResult(viewName, fileName);
    }

    /// <summary>
    ///     Creates a <see cref="ViewPdfResult" /> that renders a Razor view as a PDF file with a model.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="viewName">The name or path of the Razor view.</param>
    /// <param name="model">The model to pass to the view.</param>
    /// <param name="fileName">The filename for the PDF download. If null, displays inline.</param>
    /// <returns>A <see cref="ViewPdfResult" /> instance.</returns>
    public static ViewPdfResult PdfFromView(this ControllerBase controller, string viewName, object? model, string? fileName = null)
    {
        return new ViewPdfResult(viewName, model, fileName);
    }

    /// <summary>
    ///     Creates a <see cref="ViewPdfResult" /> that renders a Razor view as a PDF file with a model and options.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="viewName">The name or path of the Razor view.</param>
    /// <param name="model">The model to pass to the view.</param>
    /// <param name="fileName">The filename for the PDF download.</param>
    /// <param name="configure">An action to configure PDF options.</param>
    /// <returns>A <see cref="ViewPdfResult" /> instance.</returns>
    public static ViewPdfResult PdfFromView(this ControllerBase controller, string viewName, object? model, string? fileName,
        Action<PdfOptionsBuilder> configure)
    {
        return new ViewPdfResult(viewName, model, fileName, configure);
    }
}