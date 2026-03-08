using CaseItau.Domain.Entities;
using CaseItau.Domain.Interfaces;
using CaseItau.Domain.ValueObjects;
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
    public override async Task<IEnumerable<Fundo>> GetAllAsync(CancellationToken cancellationToken)
        => await _dataSet.Include(f => f.TipoFundo).ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<Fundo?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken)
        => await _dataSet.Include(f => f.TipoFundo).SingleOrDefaultAsync(f => f.Codigo == codigo, cancellationToken);

    /// <inheritdoc/>
    public async Task<(bool CodigoExists, bool CnpjExists)> CheckDuplicateKeysAsync(
        string codigo, string cnpj, CancellationToken cancellationToken)
    {
        var cnpjVo = new Cnpj(cnpj);
        var matches = await _dataSet
            .AsNoTracking()
            .Where(f => f.Codigo == codigo || f.Cnpj == cnpjVo)
            .Select(f => new { f.Codigo, HasCnpjMatch = f.Cnpj == cnpjVo })
            .ToListAsync(cancellationToken);

        return (matches.Any(f => f.Codigo == codigo), matches.Any(f => f.HasCnpjMatch));
    }

    /// <inheritdoc/>
    public async Task<(Fundo? Fundo, bool CnpjTakenByOther)> FindForUpdateAsync(
        string codigo, string cnpj, CancellationToken cancellationToken)
    {
        var cnpjVo = new Cnpj(cnpj);
        var matches = await _dataSet
            .Include(f => f.TipoFundo)
            .Where(f => f.Codigo == codigo || f.Cnpj == cnpjVo)
            .ToListAsync(cancellationToken);

        // Find the fundo by codigo, and check if any other fundo has the same CNPJ
        var fundo = matches.FirstOrDefault(f => f.Codigo == codigo);
        var cnpjTakenByOther = matches.Any(f => f.Codigo != codigo && f.Cnpj == cnpjVo);
        return (fundo, cnpjTakenByOther);
    }
}
