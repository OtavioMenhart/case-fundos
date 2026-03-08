using CaseItau.Domain.Entities;
using CaseItau.Domain.Interfaces;
using CaseItau.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace CaseItau.Infra.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="IFundoRepository"/>.
/// </summary>
public class FundoRepository(AppDbContext context) : IFundoRepository
{
    private readonly AppDbContext _context = context;

    /// <inheritdoc/>
    public async Task<IEnumerable<Fundo>> GetAllAsync()
        => await _context.Fundos.Include(f => f.TipoFundo).ToListAsync();

    /// <inheritdoc/>
    public async Task<Fundo?> GetByCodigoAsync(string codigo)
        => await _context.Fundos.Include(f => f.TipoFundo).FirstOrDefaultAsync(f => f.Codigo == codigo);

    /// <inheritdoc/>
    public async Task AddAsync(Fundo fundo)
    {
        await _context.Fundos.AddAsync(fundo);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Fundo fundo)
    {
        _context.Fundos.Update(fundo);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Fundo fundo)
    {
        _context.Fundos.Remove(fundo);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateFundAssetsAsync(Fundo fundo, decimal valor)
    {
        fundo.Patrimonio = valor;
        _context.Fundos.Update(fundo);
        await _context.SaveChangesAsync();
    }
}
