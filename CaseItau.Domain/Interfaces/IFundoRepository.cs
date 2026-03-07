using CaseItau.Domain.Entities;

namespace CaseItau.Domain.Interfaces;

/// <summary>
/// Defines data access operations for <see cref="Fundo"/>.
/// </summary>
public interface IFundoRepository
{
    /// <summary>Returns all funds including their type name.</summary>
    Task<IEnumerable<Fundo>> GetAllAsync();

    /// <summary>Returns a fund by its unique code, or <c>null</c> if not found.</summary>
    /// <param name="codigo">The unique code of the fund.</param>
    Task<Fundo?> GetByCodigoAsync(string codigo);

    /// <summary>Adds a new fund to the data store.</summary>
    /// <param name="fundo">The fund to add.</param>
    Task AddAsync(Fundo fundo);

    /// <summary>Updates an existing fund in the data store.</summary>
    /// <param name="fundo">The fund with updated values.</param>
    Task UpdateAsync(Fundo fundo);

    /// <summary>Removes a fund from the data store.</summary>
    /// <param name="fundo">The fund to remove.</param>
    Task DeleteAsync(Fundo fundo);

    /// <summary>Adds the given amount to the fund's patrimônio.</summary>
    /// <param name="fundo">The fund to update.</param>
    /// <param name="valor">The amount to add (can be negative).</param>
    Task MovimentarPatrimonioAsync(Fundo fundo, decimal valor);
}
