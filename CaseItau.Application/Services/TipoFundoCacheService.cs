using CaseItau.Application.Interfaces;
using CaseItau.Domain.ValueObjects;
using CaseItau.Infra.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CaseItau.Application.Services;

/// <summary>
/// Provides a memory-cached implementation of <see cref="ITipoFundoCacheService"/>.
/// Fund types are static data and are cached with a sliding expiration to avoid
/// redundant database queries on every fund creation request.
/// </summary>
public class TipoFundoCacheService(IBaseRepository<TipoFundo> repository, IMemoryCache cache) : ITipoFundoCacheService
{
    private readonly IBaseRepository<TipoFundo> _repository = repository;
    private readonly IMemoryCache _cache = cache;

    private const string CacheKey = "tipo_fundo_all";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(int codigo, CancellationToken cancellationToken)
    {
        var tipos = await GetAllCachedAsync(cancellationToken);
        return tipos.Any(t => t.Codigo == codigo);
    }

    private async Task<IEnumerable<TipoFundo>> GetAllCachedAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out IEnumerable<TipoFundo>? cached) && cached is not null)
            return cached;

        var tipos = (await _repository.GetAllAsync(cancellationToken)).ToList();

        _cache.Set(CacheKey, tipos, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheExpiration
        });

        return tipos;
    }
}
