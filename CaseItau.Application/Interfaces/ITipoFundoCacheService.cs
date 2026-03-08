namespace CaseItau.Application.Interfaces;

/// <summary>
/// Provides cached access to fund type data for validation and lookup.
/// </summary>
public interface ITipoFundoCacheService
{
    /// <summary>
    /// Determines whether a fund type with the given code exists.
    /// </summary>
    /// <param name="codigo">The fund type code to look up.</param>
    /// <returns><c>true</c> if the fund type exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(int codigo);
}
