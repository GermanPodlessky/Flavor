namespace Flavor.Exceptions;

/// <summary>
///     Exception thrown when browser operations fail (launch, navigation, etc.).
/// </summary>
public class BrowserException : FlavorException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BrowserException" /> class.
    /// </summary>
    public BrowserException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BrowserException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BrowserException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BrowserException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public BrowserException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BrowserException" /> class with browser path context.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="browserPath">The path to the browser executable.</param>
    public BrowserException(string message, Exception innerException, string? browserPath)
        : base(message, innerException)
    {
        BrowserPath = browserPath;
    }

    /// <summary>
    ///     Gets the browser executable path if available.
    /// </summary>
    public string? BrowserPath { get; }
}