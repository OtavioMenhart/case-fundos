using CaseItau.Domain.Entities;
using CaseItau.Domain.Interfaces;
using CaseItau.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace CaseItau.Infra.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="ITipoFundoRepository"/>.
/// </summary>
public class TipoFundoRepository(AppDbContext context) : ITipoFundoRepository
{
    private readonly AppDbContext _context = context;

    /// <inheritdoc/>
    public async Task<IEnumerable<TipoFundo>> GetAllAsync()
        => await _context.TiposFundo.ToListAsync();
}
