using CaseItau.Domain.Entities;
using CaseItau.Domain.Interfaces;
using CaseItau.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace CaseItau.Infra.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="IFundoRepository"/>.
/// </summary>
public class FundoRepository : BaseRepository<Fundo>, IFundoRepository
{
    private DbSet<Fundo> _dataSet;

    public FundoRepository(AppDbContext context) : base(context)
    {
        _dataSet = context.Set<Fundo>();
    }

    /// <inheritdoc/>
    public async Task<Fundo?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken)
        => await _dataSet.Include(f => f.TipoFundo).SingleOrDefaultAsync(f => f.Codigo == codigo, cancellationToken);
}
