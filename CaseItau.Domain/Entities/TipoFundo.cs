using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaseItau.Domain.Entities;

/// <summary>
/// Represents a fund type classification.
/// </summary>
[Table("TIPO_FUNDO")]
public class TipoFundo
{
    /// <summary>Gets or sets the unique code of the fund type.</summary>
    [Key]
    [Column("CODIGO")]
    public int Codigo { get; set; }

    /// <summary>Gets or sets the name of the fund type.</summary>
    [Column("NOME")]
    [MaxLength(20)]
    [Required]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Gets or sets the funds associated with this type.</summary>
    public ICollection<Fundo> Fundos { get; set; } = [];
}
