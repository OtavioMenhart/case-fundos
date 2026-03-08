using CaseItau.Domain.Exceptions;

namespace CaseItau.Domain.ValueObjects;

/// <summary>
/// Represents a Brazilian company registration number (CNPJ).
/// </summary>
public record Cnpj
{
    /// <summary>Gets the raw CNPJ value (14 digits).</summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="Cnpj"/>.
    /// </summary>
    /// <param name="value">The 14-digit CNPJ string.</param>
    /// <exception cref="DomainException">Thrown when the value is not a valid CNPJ.</exception>
    public Cnpj(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 14 || !value.All(char.IsDigit))
            throw new DomainException("Cnpj must be exactly 14 numeric digits.");

        Value = value;
    }

    /// <summary>Implicitly converts a <see cref="Cnpj"/> to its string representation.</summary>
    /// <param name="cnpj">The <see cref="Cnpj"/> instance to convert.</param>
    /// <returns>The raw string value of the CNPJ.</returns>
    public static implicit operator string(Cnpj cnpj) => cnpj.Value;

    /// <inheritdoc/>
    public override string ToString() => Value;
}
