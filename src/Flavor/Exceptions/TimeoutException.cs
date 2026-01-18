namespace Flavor.Exceptions;

/// <summary>
///     Exception thrown when an operation times out.
/// </summary>
public class FlavorTimeoutException : FlavorException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorTimeoutException" /> class.
    /// </summary>
    public FlavorTimeoutException() : this(TimeSpan.Zero)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorTimeoutException" /> class with timeout value.
    /// </summary>
    /// <param name="timeout">The timeout duration that was exceeded.</param>
    public FlavorTimeoutException(TimeSpan timeout)
        : base($"Operation timed out after {timeout.TotalMilliseconds}ms")
    {
        Timeout = timeout;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorTimeoutException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="timeout">The timeout duration that was exceeded.</param>
    public FlavorTimeoutException(string message, TimeSpan timeout) : base(message)
    {
        Timeout = timeout;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorTimeoutException" /> class with detailed context.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="timeout">The timeout duration that was exceeded.</param>
    /// <param name="operation">The name of the operation that timed out.</param>
    public FlavorTimeoutException(string message, TimeSpan timeout, string? operation) : base(message)
    {
        Timeout = timeout;
        Operation = operation;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorTimeoutException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="timeout">The timeout duration that was exceeded.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public FlavorTimeoutException(string message, TimeSpan timeout, Exception innerException)
        : base(message, innerException)
    {
        Timeout = timeout;
    }

    /// <summary>
    ///     Gets the timeout duration that was exceeded.
    /// </summary>
    public TimeSpan Timeout { get; }

    /// <summary>
    ///     Gets the operation that timed out.
    /// </summary>
    public string? Operation { get; }
}