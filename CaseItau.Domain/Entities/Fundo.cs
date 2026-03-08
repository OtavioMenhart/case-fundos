using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CaseItau.Domain.ValueObjects;

namespace CaseItau.Domain.Entities;

/// <summary>
/// Represents an investment fund.
/// </summary>
[Table("FUNDO")]
public class Fundo
{
    /// <summary>Gets or sets the unique code of the fund.</summary>
    [Key]
    [Column("CODIGO")]
    [MaxLength(20)]
    [Required]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the fund.</summary>
    [Column("NOME")]
    [MaxLength(100)]
    [Required]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Gets or sets the CNPJ of the fund.</summary>
    [Column("CNPJ")]
    public Cnpj Cnpj { get; set; } = null!;

    /// <summary>Gets or sets the fund type code (foreign key).</summary>
    [Column("CODIGO_TIPO")]
    [Required]
    public int CodigoTipo { get; set; }

    /// <summary>Gets or sets the net asset value of the fund.</summary>
    [Column("PATRIMONIO", TypeName = "decimal(18,2)")]
    public decimal? Patrimonio { get; set; }

    /// <summary>Gets or sets the fund type navigation property.</summary>
    [ForeignKey(nameof(CodigoTipo))]
    public TipoFundo TipoFundo { get; set; } = null!;
}
