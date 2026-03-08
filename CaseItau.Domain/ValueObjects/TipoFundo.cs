using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaseItau.Domain.ValueObjects;

/// <summary>
/// Represents a fund type classification.
/// </summary>
[Table("TIPO_FUNDO")]
public record TipoFundo
{
    /// <summary>Gets the unique code of the fund type.</summary>
    [Key]
    [Column("CODIGO")]
    public int Codigo { get; init; }

    /// <summary>Gets the name of the fund type.</summary>
    [Column("NOME")]
    [MaxLength(20)]
    [Required]
    public string Nome { get; init; } = string.Empty;
}
