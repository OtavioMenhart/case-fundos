using CaseItau.Domain.Entities;

namespace CaseItau.Domain.Interfaces;

/// <summary>
/// Defines data access operations for <see cref="TipoFundo"/>.
/// </summary>
public interface ITipoFundoRepository
{
    /// <summary>Returns all fund types from the data store.</summary>
    Task<IEnumerable<TipoFundo>> GetAllAsync();
}
