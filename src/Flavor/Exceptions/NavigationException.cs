namespace Flavor.Exceptions;

/// <summary>
///     Exception thrown when URL navigation fails.
/// </summary>
public class NavigationException : FlavorException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NavigationException" /> class.
    /// </summary>
    public NavigationException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NavigationException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NavigationException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NavigationException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public NavigationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NavigationException" /> class with URL context.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="url">The URL that failed to load.</param>
    /// <param name="statusCode">The HTTP status code if available.</param>
    public NavigationException(string message, string? url, int? statusCode = null)
        : base(message)
    {
        Url = url;
        StatusCode = statusCode;
    }

    /// <summary>
    ///     Gets the URL that failed to load.
    /// </summary>
    public string? Url { get; }

    /// <summary>
    ///     Gets the HTTP status code if available.
    /// </summary>
    public int? StatusCode { get; }
}