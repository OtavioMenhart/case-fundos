using CaseItau.Application.DTOs;

namespace CaseItau.Application.Interfaces;

/// <summary>
/// Defines use-case operations for investment funds.
/// </summary>
public interface IFundoService
{
    /// <summary>Returns all funds.</summary>
    Task<IEnumerable<FundoDto>> GetAllAsync();

    /// <summary>
    /// Returns a fund by its unique code, or <c>null</c> if not found.
    /// </summary>
    /// <param name="codigo">The unique code of the fund.</param>
    Task<FundoDto?> GetByCodigoAsync(string codigo);

    /// <summary>Creates a new fund.</summary>
    /// <param name="dto">The data for the new fund.</param>
    Task CreateAsync(CreateFundoDto dto);

    /// <summary>
    /// Updates an existing fund.
    /// </summary>
    /// <param name="codigo">The code of the fund to update.</param>
    /// <param name="dto">The updated fund data.</param>
    /// <returns><c>true</c> if updated; <c>false</c> if not found.</returns>
    Task<bool> UpdateAsync(string codigo, UpdateFundoDto dto);

    /// <summary>
    /// Deletes a fund by its unique code.
    /// </summary>
    /// <param name="codigo">The code of the fund to delete.</param>
    /// <returns><c>true</c> if deleted; <c>false</c> if not found.</returns>
    Task<bool> DeleteAsync(string codigo);

    /// <summary>
    /// Adds the given amount to the fund's patrimônio.
    /// </summary>
    /// <param name="codigo">The code of the fund to update.</param>
    /// <param name="valor">The amount to add (can be negative).</param>
    /// <returns><c>true</c> if updated; <c>false</c> if not found.</returns>
    Task<bool> MovimentarPatrimonioAsync(string codigo, decimal valor);
}
