using System.ComponentModel.DataAnnotations;

namespace CaseItau.Application.DTOs;

/// <summary>
/// Data transfer object for creating a new fund.
/// </summary>
public class CreateFundoDto
{
    /// <summary>Gets or sets the unique code of the fund.</summary>
    [Required]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the fund.</summary>
    [Required]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Gets or sets the CNPJ of the fund.</summary>
    [Required]
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>Gets or sets the type code of the fund.</summary>
    [Required]
    public int CodigoTipo { get; set; }

    /// <summary>Gets or sets the initial net asset value of the fund.</summary>
    public decimal? Patrimonio { get; set; }
}
