using System.ComponentModel.DataAnnotations;

namespace CaseItau.Application.DTOs;

/// <summary>
/// Data transfer object for updating an existing fund.
/// </summary>
public class UpdateFundoDto
{
    /// <summary>Gets or sets the name of the fund.</summary>
    [Required]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Gets or sets the CNPJ of the fund.</summary>
    [Required]
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>Gets or sets the type code of the fund.</summary>
    [Required]
    public int CodigoTipo { get; set; }
}
