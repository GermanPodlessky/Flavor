namespace Flavor.Exceptions;

/// <summary>
///     Base exception for all Flavor library errors.
/// </summary>
public class FlavorException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorException" /> class.
    /// </summary>
    public FlavorException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public FlavorException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlavorException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public FlavorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}