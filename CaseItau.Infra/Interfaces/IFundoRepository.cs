using CaseItau.Domain.Entities;
using CaseItau.Infra.Interfaces;

namespace CaseItau.Domain.Interfaces;

/// <summary>
/// Defines data access operations for <see cref="Fundo"/>.
/// </summary>
public interface IFundoRepository : IBaseRepository<Fundo>
{
    /// <summary>Returns the fund matching the given code, or <c>null</c> if not found.</summary>
    /// <param name="codigo">The unique code of the fund.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<Fundo?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken);

    /// <summary>
    /// Checks in a single query whether <paramref name="codigo"/> or <paramref name="cnpj"/>
    /// are already taken by any existing fund.
    /// </summary>
    /// <param name="codigo">The fund code to check.</param>
    /// <param name="cnpj">The raw 14-digit CNPJ to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A tuple where <c>CodigoExists</c> is <c>true</c> if the code is already in use,
    /// and <c>CnpjExists</c> is <c>true</c> if the CNPJ is already in use.
    /// </returns>
    Task<(bool CodigoExists, bool CnpjExists)> CheckDuplicatedKeysAsync(
        string codigo, string cnpj, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the fund by <paramref name="codigo"/> and simultaneously checks whether
    /// <paramref name="cnpj"/> is already taken by a <em>different</em> fund — all in a single query.
    /// </summary>
    /// <param name="codigo">The code of the fund to retrieve.</param>
    /// <param name="cnpj">The raw 14-digit CNPJ to check against other funds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A tuple where <c>Fundo</c> is the matched fund (or <c>null</c> if not found)
    /// and <c>CnpjTakenByOther</c> is <c>true</c> if another fund already owns the given CNPJ.
    /// </returns>
    Task<(Fundo? Fundo, bool CnpjTakenByOther)> FindForUpdateAsync(
        string codigo, string cnpj, CancellationToken cancellationToken);
}
