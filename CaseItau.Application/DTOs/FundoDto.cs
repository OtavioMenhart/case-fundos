namespace CaseItau.Application.DTOs;

/// <summary>
/// Data transfer object representing a fund with its type name.
/// </summary>
public class FundoDto
{
    /// <summary>Gets or sets the unique code of the fund.</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the fund.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Gets or sets the CNPJ of the fund.</summary>
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>Gets or sets the type code of the fund.</summary>
    public int CodigoTipo { get; set; }

    /// <summary>Gets or sets the name of the fund type.</summary>
    public string NomeTipo { get; set; } = string.Empty;

    /// <summary>Gets or sets the net asset value of the fund.</summary>
    public decimal? Patrimonio { get; set; }
}
