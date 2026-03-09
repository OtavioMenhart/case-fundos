using Bogus;
using CaseItau.Domain.ValueObjects;
using CaseItau.Infra.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;


namespace CaseItau.Application.Services.UnitTests;

/// <summary>
/// Unit tests for <see cref="TipoFundoCacheService"/>.
/// </summary>
public partial class TipoFundoCacheServiceTests
{
    private readonly Mock<IBaseRepository<TipoFundo>> _repositoryMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<ILogger<TipoFundoCacheService>> _loggerMock;
    private readonly TipoFundoCacheService _service;
    private readonly Faker _faker;

    public TipoFundoCacheServiceTests()
    {
        _repositoryMock = new Mock<IBaseRepository<TipoFundo>>();
        _cacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<TipoFundoCacheService>>();
        _service = new TipoFundoCacheService(_repositoryMock.Object, _cacheMock.Object, _loggerMock.Object);
        _faker = new Faker();
    }

    /// <summary>
    /// Tests that ExistsAsync returns true when the codigo exists in the cached collection.
    /// Input: Valid codigo that matches an entry in the cache.
    /// Expected: Returns true.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WhenCodigoExistsInCache_ReturnsTrue()
    {
        // Arrange
        int searchCodigo = _faker.Random.Int(1, 100);
        List<TipoFundo> cachedTipos = new()
        {
            new TipoFundo { Codigo = _faker.Random.Int(101, 200) },
            new TipoFundo { Codigo = searchCodigo },
            new TipoFundo { Codigo = _faker.Random.Int(201, 300) }
        };
        SetupCacheHit(cachedTipos);

        // Act
        bool result = await _service.ExistsAsync(searchCodigo, CancellationToken.None);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that ExistsAsync returns false when the codigo does not exist in the cached collection.
    /// Input: Valid codigo that does not match any entry in the cache.
    /// Expected: Returns false.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WhenCodigoNotExistsInCache_ReturnsFalse()
    {
        // Arrange
        int searchCodigo = 999;
        List<TipoFundo> cachedTipos = new()
        {
            new TipoFundo { Codigo = 1 },
            new TipoFundo { Codigo = 2 },
            new TipoFundo { Codigo = 3 }
        };
        SetupCacheHit(cachedTipos);

        // Act
        bool result = await _service.ExistsAsync(searchCodigo, CancellationToken.None);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that ExistsAsync returns false when the cache contains an empty collection.
    /// Input: Any codigo with empty cached collection.
    /// Expected: Returns false.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WhenCacheIsEmpty_ReturnsFalse()
    {
        // Arrange
        int searchCodigo = _faker.Random.Int(1, 100);
        List<TipoFundo> cachedTipos = new();
        SetupCacheHit(cachedTipos);

        // Act
        bool result = await _service.ExistsAsync(searchCodigo, CancellationToken.None);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that ExistsAsync returns true when cache misses and the codigo exists in repository.
    /// Input: Valid codigo that exists in repository after cache miss.
    /// Expected: Returns true and caches the data.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WhenCacheMissAndCodigoExistsInRepository_ReturnsTrue()
    {
        // Arrange
        int searchCodigo = _faker.Random.Int(1, 100);
        List<TipoFundo> repoTipos = new()
        {
            new TipoFundo { Codigo = _faker.Random.Int(101, 200) },
            new TipoFundo { Codigo = searchCodigo },
            new TipoFundo { Codigo = _faker.Random.Int(201, 300) }
        };
        SetupCacheMiss();
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoTipos);

        // Act
        bool result = await _service.ExistsAsync(searchCodigo, CancellationToken.None);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.CreateEntry(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that ExistsAsync returns false when cache misses and the codigo does not exist in repository.
    /// Input: Valid codigo that does not exist in repository after cache miss.
    /// Expected: Returns false and caches the data.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WhenCacheMissAndCodigoNotInRepository_ReturnsFalse()
    {
        // Arrange
        int searchCodigo = 999;
        List<TipoFundo> repoTipos = new()
        {
            new TipoFundo { Codigo = 1 },
            new TipoFundo { Codigo = 2 },
            new TipoFundo { Codigo = 3 }
        };
        SetupCacheMiss();
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoTipos);

        // Act
        bool result = await _service.ExistsAsync(searchCodigo, CancellationToken.None);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.CreateEntry(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that ExistsAsync returns false when cache misses and repository returns empty collection.
    /// Input: Any codigo with empty repository collection after cache miss.
    /// Expected: Returns false and caches the empty collection.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WhenCacheMissAndRepositoryReturnsEmpty_ReturnsFalse()
    {
        // Arrange
        int searchCodigo = _faker.Random.Int(1, 100);
        List<TipoFundo> repoTipos = new();
        SetupCacheMiss();
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoTipos);

        // Act
        bool result = await _service.ExistsAsync(searchCodigo, CancellationToken.None);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.CreateEntry(It.IsAny<object>()), Times.Once);
    }

    /// <summary>
    /// Tests that ExistsAsync handles various codigo boundary values correctly.
    /// Input: Various edge case codigo values (zero, negative, int.MinValue, int.MaxValue).
    /// Expected: Returns true if matching value exists in collection, false otherwise.
    /// </summary>
    [Theory]
    [InlineData(0, true)]
    [InlineData(0, false)]
    [InlineData(-1, true)]
    [InlineData(-1, false)]
    [InlineData(int.MinValue, true)]
    [InlineData(int.MinValue, false)]
    [InlineData(int.MaxValue, true)]
    [InlineData(int.MaxValue, false)]
    [InlineData(1, true)]
    [InlineData(1, false)]
    public async Task ExistsAsync_WithVariousCodigoValues_ReturnsExpectedResult(int codigo, bool shouldExist)
    {
        // Arrange
        List<TipoFundo> cachedTipos = shouldExist
            ? new List<TipoFundo> { new TipoFundo { Codigo = codigo } }
            : new List<TipoFundo> { new TipoFundo { Codigo = codigo + 1 } };
        SetupCacheHit(cachedTipos);

        // Act
        bool result = await _service.ExistsAsync(codigo, CancellationToken.None);

        // Assert
        Assert.Equal(shouldExist, result);
    }

    /// <summary>
    /// Tests that ExistsAsync passes the correct CancellationToken to the repository.
    /// Input: Valid codigo with a custom CancellationToken after cache miss.
    /// Expected: GetAllAsync is called with the provided token.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_PassesCancellationTokenToRepository()
    {
        // Arrange
        int searchCodigo = _faker.Random.Int(1, 100);
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;
        List<TipoFundo> repoTipos = new() { new TipoFundo { Codigo = searchCodigo } };
        SetupCacheMiss();
        _repositoryMock
            .Setup(r => r.GetAllAsync(token))
            .ReturnsAsync(repoTipos);

        // Act
        await _service.ExistsAsync(searchCodigo, token);

        // Assert
        _repositoryMock.Verify(r => r.GetAllAsync(token), Times.Once);
    }

    /// <summary>
    /// Tests that ExistsAsync returns false when cache contains null value.
    /// Input: Any codigo when TryGetValue returns true but with null cached value.
    /// Expected: Returns false by fetching from repository.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WhenCachedValueIsNull_FetchesFromRepository()
    {
        // Arrange
        int searchCodigo = _faker.Random.Int(1, 100);
        object? nullValue = null;
        _cacheMock
            .Setup(c => c.TryGetValue(It.IsAny<object>(), out nullValue))
            .Returns(true);
        List<TipoFundo> repoTipos = new() { new TipoFundo { Codigo = searchCodigo } };
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoTipos);
        Mock<ICacheEntry> cacheEntryMock = new();
        _cacheMock
            .Setup(c => c.CreateEntry(It.IsAny<object>()))
            .Returns(cacheEntryMock.Object);

        // Act
        bool result = await _service.ExistsAsync(searchCodigo, CancellationToken.None);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that ExistsAsync returns true when multiple items exist and one matches.
    /// Input: Codigo that matches one of many items in cache.
    /// Expected: Returns true.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WhenMultipleItemsAndOneMatches_ReturnsTrue()
    {
        // Arrange
        int searchCodigo = 50;
        List<TipoFundo> cachedTipos = Enumerable.Range(1, 100)
            .Select(i => new TipoFundo { Codigo = i })
            .ToList();
        SetupCacheHit(cachedTipos);

        // Act
        bool result = await _service.ExistsAsync(searchCodigo, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that ExistsAsync correctly handles duplicate codigo values in collection.
    /// Input: Codigo that has duplicates in cached collection.
    /// Expected: Returns true.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WhenDuplicateCodigosExist_ReturnsTrue()
    {
        // Arrange
        int searchCodigo = _faker.Random.Int(1, 100);
        List<TipoFundo> cachedTipos = new()
        {
            new TipoFundo { Codigo = searchCodigo },
            new TipoFundo { Codigo = searchCodigo },
            new TipoFundo { Codigo = _faker.Random.Int(101, 200) }
        };
        SetupCacheHit(cachedTipos);

        // Act
        bool result = await _service.ExistsAsync(searchCodigo, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    private void SetupCacheHit(IEnumerable<TipoFundo> cachedData)
    {
        object? cached = cachedData;
        _cacheMock
            .Setup(c => c.TryGetValue(It.IsAny<object>(), out cached))
            .Returns(true);
    }

    private void SetupCacheMiss()
    {
        object? nullValue = null;
        _cacheMock
            .Setup(c => c.TryGetValue(It.IsAny<object>(), out nullValue))
            .Returns(false);
        Mock<ICacheEntry> cacheEntryMock = new();
        _cacheMock
            .Setup(c => c.CreateEntry(It.IsAny<object>()))
            .Returns(cacheEntryMock.Object);
    }
}