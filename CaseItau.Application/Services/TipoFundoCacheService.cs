using CaseItau.Application.Interfaces;
using CaseItau.Domain.ValueObjects;
using CaseItau.Infra.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CaseItau.Application.Services;

/// <summary>
/// Provides a memory-cached implementation of <see cref="ITipoFundoCacheService"/>.
/// Fund types are static data and are cached with a sliding expiration to avoid
/// redundant database queries on every fund creation request.
/// </summary>
public class TipoFundoCacheService(IBaseRepository<TipoFundo> repository, IMemoryCache cache, ILogger<TipoFundoCacheService> logger) : ITipoFundoCacheService
{
    private readonly IBaseRepository<TipoFundo> _repository = repository;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<TipoFundoCacheService> _logger = logger;

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
        {
            _logger.LogDebug("TipoFundo list served from cache.");
            return cached;
        }

        _logger.LogDebug("TipoFundo cache miss. Fetching from database.");
        var tipos = (await _repository.GetAllAsync(cancellationToken)).ToList();

        _cache.Set(CacheKey, tipos, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheExpiration
        });

        _logger.LogDebug("{Count} TipoFundo(s) loaded into cache.", tipos.Count);
        return tipos;
    }
}
