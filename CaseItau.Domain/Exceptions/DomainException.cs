namespace CaseItau.Domain.Exceptions;

/// <summary>
/// Represents a business rule violation in the domain.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="DomainException"/> with the specified message.
    /// </summary>
    /// <param name="message">The message that describes the violation.</param>
    public DomainException(string message) : base(message) { }
}
