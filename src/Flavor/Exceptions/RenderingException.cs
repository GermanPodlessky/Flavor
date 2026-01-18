namespace Flavor.Exceptions;

/// <summary>
///     Exception thrown when PDF rendering fails.
/// </summary>
public class RenderingException : FlavorException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderingException" /> class.
    /// </summary>
    public RenderingException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderingException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RenderingException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderingException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RenderingException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderingException" /> class with detailed context.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="htmlLength">The length of the HTML content being rendered.</param>
    /// <param name="timeout">The timeout value that was configured.</param>
    public RenderingException(string message, Exception innerException, int? htmlLength, TimeSpan? timeout)
        : base(message, innerException)
    {
        HtmlLength = htmlLength;
        Timeout = timeout;
    }

    /// <summary>
    ///     Gets the HTML content length that was being rendered when the error occurred.
    /// </summary>
    public int? HtmlLength { get; }

    /// <summary>
    ///     Gets the timeout value that was configured when the error occurred.
    /// </summary>
    public TimeSpan? Timeout { get; }
}