namespace CaseItau.Domain.Constants;

/// <summary>
/// Defines domain-level constraints for fund-related entities and value objects.
/// </summary>
public static class FundoConstants
{
    /// <summary>Maximum allowed length for a fund code.</summary>
    public const int CodigoMaxLength = 20;

    /// <summary>Maximum allowed length for a fund name.</summary>
    public const int NomeMaxLength = 100;

    /// <summary>Exact required length for a CNPJ value.</summary>
    public const int CnpjLength = 14;

    /// <summary>Regex pattern that validates a CNPJ contains only digits with the correct length.</summary>
    public const string CnpjPattern = @"^\d{14}$";

    /// <summary>Maximum allowed length for a fund type name.</summary>
    public const int TipoNomeMaxLength = 20;
}
