using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Flavor.AspNetCore.Razor;

/// <summary>
///     Renders Razor views to HTML strings using the ASP.NET Core Razor engine.
/// </summary>
public class RazorViewRenderer : IRazorViewRenderer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IRazorViewEngine _viewEngine;

    /// <summary>
    ///     Initializes a new instance of <see cref="RazorViewRenderer" />.
    /// </summary>
    /// <param name="viewEngine">The Razor view engine.</param>
    /// <param name="tempDataProvider">The temp data provider.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public RazorViewRenderer(
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        _tempDataProvider = tempDataProvider ?? throw new ArgumentNullException(nameof(tempDataProvider));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public Task<string> RenderViewToStringAsync(string viewName, object? model = null, CancellationToken cancellationToken = default)
    {
        return RenderViewToStringInternalAsync(viewName, model, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model, CancellationToken cancellationToken = default)
    {
        return RenderViewToStringInternalAsync(viewName, model, cancellationToken);
    }

    private async Task<string> RenderViewToStringInternalAsync(string viewName, object? model, CancellationToken cancellationToken)
    {
        var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        var view = FindView(actionContext, viewName);

        await using var writer = new StringWriter();

        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };

        var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

        var viewContext = new ViewContext(
            actionContext,
            view,
            viewData,
            tempData,
            writer,
            new HtmlHelperOptions());

        await view.RenderAsync(viewContext);

        return writer.ToString();
    }

    private IView FindView(ActionContext actionContext, string viewName)
    {
        var getViewResult = _viewEngine.GetView(null, viewName, true);
        if (getViewResult.Success) return getViewResult.View;

        var findViewResult = _viewEngine.FindView(actionContext, viewName, true);
        if (findViewResult.Success) return findViewResult.View;

        var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }
                .Concat(searchedLocations));

        throw new InvalidOperationException(errorMessage);
    }
}