using CaseItau.Domain.Entities;
using CaseItau.Infra.Interfaces;

namespace CaseItau.Domain.Interfaces;

/// <summary>
/// Defines data access operations for <see cref="Fundo"/>.
/// </summary>
public interface IFundoRepository : IBaseRepository<Fundo>
{
    Task<Fundo?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken);
}
