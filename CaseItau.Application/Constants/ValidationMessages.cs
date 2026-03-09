namespace CaseItau.Application.Constants;

/// <summary>
/// Defines all validation error messages used by FluentValidation validators.
/// </summary>
public static class ValidationMessages
{
    /// <summary>Messages for the <c>Codigo</c> field.</summary>
    public static class Codigo
    {
        /// <summary>Error message when <c>Codigo</c> is empty.</summary>
        public const string Required = "Codigo is required.";

        /// <summary>Error message when <c>Codigo</c> exceeds the maximum length.</summary>
        public const string MaxLength = "Codigo must not exceed 20 characters.";
    }

    /// <summary>Messages for the <c>Nome</c> field.</summary>
    public static class Nome
    {
        /// <summary>Error message when <c>Nome</c> is empty.</summary>
        public const string Required = "Nome is required.";

        /// <summary>Error message when <c>Nome</c> exceeds the maximum length.</summary>
        public const string MaxLength = "Nome must not exceed 100 characters.";
    }

    /// <summary>Messages for the <c>Cnpj</c> field.</summary>
    public static class Cnpj
    {
        /// <summary>Error message when <c>Cnpj</c> is empty.</summary>
        public const string Required = "Cnpj is required.";

        /// <summary>Error message when <c>Cnpj</c> does not have the required length.</summary>
        public const string Length = "Cnpj must be exactly 14 characters.";

        /// <summary>Error message when <c>Cnpj</c> contains non-digit characters.</summary>
        public const string OnlyDigits = "Cnpj must contain only digits.";
    }

    /// <summary>Messages for the <c>CodigoTipo</c> field.</summary>
    public static class CodigoTipo
    {
        /// <summary>Error message when <c>CodigoTipo</c> is not greater than zero.</summary>
        public const string GreaterThanZero = "CodigoTipo must be greater than zero.";
    }

    /// <summary>Messages for the <c>Patrimonio</c> field.</summary>
    public static class Patrimonio
    {
        /// <summary>Error message when <c>Patrimonio</c> is negative.</summary>
        public const string GreaterThanOrEqualToZero = "Patrimonio must be greater than or equal to zero.";
    }
}
