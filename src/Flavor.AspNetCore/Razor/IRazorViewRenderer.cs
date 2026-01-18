namespace Flavor.AspNetCore.Razor;

/// <summary>
///     Renders Razor views to HTML strings.
/// </summary>
public interface IRazorViewRenderer
{
    /// <summary>
    ///     Renders a Razor view to an HTML string.
    /// </summary>
    /// <param name="viewName">The name or path of the view to render.</param>
    /// <param name="model">The model to pass to the view.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The rendered HTML string.</returns>
    Task<string> RenderViewToStringAsync(string viewName, object? model = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Renders a Razor view to an HTML string with a typed model.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="viewName">The name or path of the view to render.</param>
    /// <param name="model">The model to pass to the view.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The rendered HTML string.</returns>
    Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model, CancellationToken cancellationToken = default);
}