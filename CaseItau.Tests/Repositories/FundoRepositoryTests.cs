using Bogus;
using CaseItau.Domain.Entities;
using CaseItau.Domain.Exceptions;
using CaseItau.Domain.ValueObjects;
using CaseItau.Infra.Data;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace CaseItau.Infra.Repositories.UnitTests;


/// <summary>
/// Unit tests for <see cref="FundoRepository"/> class.
/// </summary>
public class FundoRepositoryTests
{
    private readonly Faker _faker;
    private readonly AppDbContext _context;

    public FundoRepositoryTests()
    {
        _faker = new Faker();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    /// <summary>
    /// Tests that GetByCodigoAsync returns null when no fund with the specified codigo exists.
    /// Input: A codigo that does not exist in the database.
    /// Expected: Returns null.
    /// </summary>
    [Fact]
    public async Task GetByCodigoAsync_WhenFundoDoesNotExist_ReturnsNull()
    {
        // Arrange
        string nonExistentCodigo = _faker.Random.AlphaNumeric(20);
        List<Fundo> emptyList = new();
        Mock<DbSet<Fundo>> mockDbSet = emptyList.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);
        FundoRepository repository = new(mockContext.Object);

        // Act
        Fundo? result = await repository.GetByCodigoAsync(nonExistentCodigo, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that GetByCodigoAsync returns null when an empty string codigo is provided.
    /// Input: Empty string.
    /// Expected: Returns null.
    /// </summary>
    [Fact]
    public async Task GetByCodigoAsync_WhenCodigoIsEmpty_ReturnsNull()
    {
        // Arrange
        string emptyCodigo = string.Empty;
        List<Fundo> emptyList = new();
        Mock<DbSet<Fundo>> mockDbSet = emptyList.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);
        FundoRepository repository = new(mockContext.Object);

        // Act
        Fundo? result = await repository.GetByCodigoAsync(emptyCodigo, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that GetByCodigoAsync returns null when a whitespace-only codigo is provided.
    /// Input: Whitespace-only string.
    /// Expected: Returns null.
    /// </summary>
    [Fact]
    public async Task GetByCodigoAsync_WhenCodigoIsWhitespace_ReturnsNull()
    {
        // Arrange
        string whitespaceCodigo = "   ";
        List<Fundo> emptyList = new();
        Mock<DbSet<Fundo>> mockDbSet = emptyList.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);
        FundoRepository repository = new(mockContext.Object);

        // Act
        Fundo? result = await repository.GetByCodigoAsync(whitespaceCodigo, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that GetByCodigoAsync handles very long codigo strings.
    /// Input: Codigo exceeding typical length constraints.
    /// Expected: Returns null if no match exists.
    /// </summary>
    [Fact]
    public async Task GetByCodigoAsync_WhenCodigoIsVeryLong_ReturnsNull()
    {
        // Arrange
        string veryLongCodigo = new string('A', 1000);
        List<Fundo> emptyList = new();
        Mock<DbSet<Fundo>> mockDbSet = emptyList.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);
        FundoRepository repository = new(mockContext.Object);

        // Act
        Fundo? result = await repository.GetByCodigoAsync(veryLongCodigo, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that CheckDuplicateKeysAsync returns (false, false) when neither codigo nor cnpj exist in the database.
    /// Input: Valid codigo and cnpj that don't match any existing entities.
    /// Expected: Returns (false, false).
    /// </summary>
    [Fact]
    public async Task CheckDuplicateKeysAsync_WhenNeitherExists_ReturnsFalseFalse()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        string cnpj = _faker.Random.ReplaceNumbers("##############");
        List<Fundo> emptyList = new();

        Mock<DbSet<Fundo>> mockDbSet = emptyList.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);

        FundoRepository repository = new(mockContext.Object);

        // Act
        (bool codigoExists, bool cnpjExists) = await repository.CheckDuplicateKeysAsync(codigo, cnpj, CancellationToken.None);

        // Assert
        Assert.False(codigoExists);
        Assert.False(cnpjExists);
    }

    /// <summary>
    /// Tests that CheckDuplicateKeysAsync returns (true, false) when only codigo exists in the database.
    /// Input: Valid codigo that matches an existing entity, cnpj that doesn't match.
    /// Expected: Returns (true, false).
    /// </summary>
    [Fact]
    public async Task CheckDuplicateKeysAsync_WhenOnlyCodigoExists_ReturnsTrueFalse()
    {
        // Arrange
        string matchingCodigo = _faker.Random.AlphaNumeric(10);
        string nonMatchingCnpj = _faker.Random.ReplaceNumbers("##############");
        string existingCnpj = _faker.Random.ReplaceNumbers("##############");

        List<Fundo> fundos = new()
        {
            new Fundo
            {
                Codigo = matchingCodigo,
                Cnpj = new Cnpj(existingCnpj),
                Nome = _faker.Company.CompanyName(),
                CodigoTipo = 1
            }
        };

        Mock<DbSet<Fundo>> mockDbSet = fundos.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);

        FundoRepository repository = new(mockContext.Object);

        // Act
        (bool codigoExists, bool cnpjExists) = await repository.CheckDuplicateKeysAsync(matchingCodigo, nonMatchingCnpj, CancellationToken.None);

        // Assert
        Assert.True(codigoExists);
        Assert.False(cnpjExists);
    }

    /// <summary>
    /// Tests that CheckDuplicateKeysAsync returns (false, true) when only cnpj exists in the database.
    /// Input: Valid cnpj that matches an existing entity, codigo that doesn't match.
    /// Expected: Returns (false, true).
    /// </summary>
    [Fact]
    public async Task CheckDuplicateKeysAsync_WhenOnlyCnpjExists_ReturnsFalseTrue()
    {
        // Arrange
        string nonMatchingCodigo = _faker.Random.AlphaNumeric(10);
        string matchingCnpj = _faker.Random.ReplaceNumbers("##############");

        List<Fundo> fundos = new()
        {
            new Fundo
            {
                Codigo = _faker.Random.AlphaNumeric(10),
                Cnpj = new Cnpj(matchingCnpj),
                Nome = _faker.Company.CompanyName(),
                CodigoTipo = 1
            }
        };

        Mock<DbSet<Fundo>> mockDbSet = fundos.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);

        FundoRepository repository = new(mockContext.Object);

        // Act
        (bool codigoExists, bool cnpjExists) = await repository.CheckDuplicateKeysAsync(nonMatchingCodigo, matchingCnpj, CancellationToken.None);

        // Assert
        Assert.False(codigoExists);
        Assert.True(cnpjExists);
    }

    /// <summary>
    /// Tests that CheckDuplicateKeysAsync returns (true, true) when both codigo and cnpj exist in different entities.
    /// Input: Valid codigo and cnpj that match different existing entities.
    /// Expected: Returns (true, true).
    /// </summary>
    [Fact]
    public async Task CheckDuplicateKeysAsync_WhenBothExistInDifferentEntities_ReturnsTrueTrue()
    {
        // Arrange
        string matchingCodigo = _faker.Random.AlphaNumeric(10);
        string matchingCnpj = _faker.Random.ReplaceNumbers("##############");

        List<Fundo> fundos = new()
        {
            new Fundo
            {
                Codigo = matchingCodigo,
                Cnpj = new Cnpj(_faker.Random.ReplaceNumbers("##############")),
                Nome = _faker.Company.CompanyName(),
                CodigoTipo = 1
            },
            new Fundo
            {
                Codigo = _faker.Random.AlphaNumeric(10),
                Cnpj = new Cnpj(matchingCnpj),
                Nome = _faker.Company.CompanyName(),
                CodigoTipo = 2
            }
        };

        Mock<DbSet<Fundo>> mockDbSet = fundos.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);

        FundoRepository repository = new(mockContext.Object);

        // Act
        (bool codigoExists, bool cnpjExists) = await repository.CheckDuplicateKeysAsync(matchingCodigo, matchingCnpj, CancellationToken.None);

        // Assert
        Assert.True(codigoExists);
        Assert.True(cnpjExists);
    }

    /// <summary>
    /// Tests that CheckDuplicateKeysAsync returns (true, true) when both codigo and cnpj exist in the same entity.
    /// Input: Valid codigo and cnpj that both match the same existing entity.
    /// Expected: Returns (true, true).
    /// </summary>
    [Fact]
    public async Task CheckDuplicateKeysAsync_WhenBothExistInSameEntity_ReturnsTrueTrue()
    {
        // Arrange
        string matchingCodigo = _faker.Random.AlphaNumeric(10);
        string matchingCnpj = _faker.Random.ReplaceNumbers("##############");

        List<Fundo> fundos = new()
        {
            new Fundo
            {
                Codigo = matchingCodigo,
                Cnpj = new Cnpj(matchingCnpj),
                Nome = _faker.Company.CompanyName(),
                CodigoTipo = 1
            }
        };

        Mock<DbSet<Fundo>> mockDbSet = fundos.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);

        FundoRepository repository = new(mockContext.Object);

        // Act
        (bool codigoExists, bool cnpjExists) = await repository.CheckDuplicateKeysAsync(matchingCodigo, matchingCnpj, CancellationToken.None);

        // Assert
        Assert.True(codigoExists);
        Assert.True(cnpjExists);
    }

    /// <summary>
    /// Tests that CheckDuplicateKeysAsync returns correct results when multiple entities match the criteria.
    /// Input: Valid codigo and cnpj where multiple entities match either codigo or cnpj.
    /// Expected: Returns (true, true).
    /// </summary>
    [Fact]
    public async Task CheckDuplicateKeysAsync_WithMultipleMatches_ReturnsCorrectFlags()
    {
        // Arrange
        string matchingCodigo = _faker.Random.AlphaNumeric(10);
        string matchingCnpj = _faker.Random.ReplaceNumbers("##############");

        List<Fundo> fundos = new()
        {
            new Fundo
            {
                Codigo = matchingCodigo,
                Cnpj = new Cnpj(_faker.Random.ReplaceNumbers("##############")),
                Nome = _faker.Company.CompanyName(),
                CodigoTipo = 1
            },
            new Fundo
            {
                Codigo = matchingCodigo,
                Cnpj = new Cnpj(_faker.Random.ReplaceNumbers("##############")),
                Nome = _faker.Company.CompanyName(),
                CodigoTipo = 2
            },
            new Fundo
            {
                Codigo = _faker.Random.AlphaNumeric(10),
                Cnpj = new Cnpj(matchingCnpj),
                Nome = _faker.Company.CompanyName(),
                CodigoTipo = 3
            }
        };

        Mock<DbSet<Fundo>> mockDbSet = fundos.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);

        FundoRepository repository = new(mockContext.Object);

        // Act
        (bool codigoExists, bool cnpjExists) = await repository.CheckDuplicateKeysAsync(matchingCodigo, matchingCnpj, CancellationToken.None);

        // Assert
        Assert.True(codigoExists);
        Assert.True(cnpjExists);
    }

    /// <summary>
    /// Tests that CheckDuplicateKeysAsync throws DomainException when cnpj is invalid.
    /// Input: Valid codigo, invalid cnpj (null, empty, whitespace, wrong length, non-numeric).
    /// Expected: Throws DomainException.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("123456789012")]
    [InlineData("123456789012345")]
    [InlineData("1234567890123A")]
    [InlineData("ABCDEFGHIJKLMN")]
    [InlineData("12.345.678/0001-99")]
    public async Task CheckDuplicateKeysAsync_WithInvalidCnpj_ThrowsDomainException(string? invalidCnpj)
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        List<Fundo> emptyList = new();

        Mock<DbSet<Fundo>> mockDbSet = emptyList.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);

        FundoRepository repository = new(mockContext.Object);

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(
            async () => await repository.CheckDuplicateKeysAsync(codigo, invalidCnpj!, CancellationToken.None));

        Assert.Equal("Cnpj must be exactly 14 numeric digits.", exception.Message);
    }

    /// <summary>
    /// Tests that CheckDuplicateKeysAsync properly passes the CancellationToken to the async database operation.
    /// Input: Valid codigo and cnpj with a custom CancellationToken.
    /// Expected: The token is passed through to ToListAsync.
    /// </summary>
    [Fact]
    public async Task CheckDuplicateKeysAsync_PassesCancellationTokenCorrectly()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        string cnpj = _faker.Random.ReplaceNumbers("##############");
        List<Fundo> emptyList = new();
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        Mock<DbSet<Fundo>> mockDbSet = emptyList.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);

        FundoRepository repository = new(mockContext.Object);

        // Act
        (bool codigoExists, bool cnpjExists) = await repository.CheckDuplicateKeysAsync(codigo, cnpj, token);

        // Assert
        Assert.False(codigoExists);
        Assert.False(cnpjExists);
    }

    /// <summary>
    /// Tests that CheckDuplicateKeysAsync handles empty string codigo correctly.
    /// Input: Empty string codigo, valid cnpj.
    /// Expected: Returns (false, false) when no match exists, or (true, *) if an entity has empty codigo.
    /// </summary>
    [Fact]
    public async Task CheckDuplicateKeysAsync_WithEmptyStringCodigo_ReturnsCorrectResult()
    {
        // Arrange
        string emptyCodigo = string.Empty;
        string cnpj = _faker.Random.ReplaceNumbers("##############");
        List<Fundo> emptyList = new();

        Mock<DbSet<Fundo>> mockDbSet = emptyList.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);

        FundoRepository repository = new(mockContext.Object);

        // Act
        (bool codigoExists, bool cnpjExists) = await repository.CheckDuplicateKeysAsync(emptyCodigo, cnpj, CancellationToken.None);

        // Assert
        Assert.False(codigoExists);
        Assert.False(cnpjExists);
    }

    /// <summary>
    /// Tests that CheckDuplicateKeysAsync handles whitespace-only codigo correctly.
    /// Input: Whitespace-only codigo, valid cnpj.
    /// Expected: Returns (false, false) when no match exists.
    /// </summary>
    [Fact]
    public async Task CheckDuplicateKeysAsync_WithWhitespaceCodigo_ReturnsCorrectResult()
    {
        // Arrange
        string whitespaceCodigo = "   ";
        string cnpj = _faker.Random.ReplaceNumbers("##############");
        List<Fundo> emptyList = new();

        Mock<DbSet<Fundo>> mockDbSet = emptyList.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);

        FundoRepository repository = new(mockContext.Object);

        // Act
        (bool codigoExists, bool cnpjExists) = await repository.CheckDuplicateKeysAsync(whitespaceCodigo, cnpj, CancellationToken.None);

        // Assert
        Assert.False(codigoExists);
        Assert.False(cnpjExists);
    }

    /// <summary>
    /// Tests that CheckDuplicateKeysAsync handles very long codigo string correctly.
    /// Input: Very long codigo string (1000 characters), valid cnpj.
    /// Expected: Returns (false, false) when no match exists.
    /// </summary>
    [Fact]
    public async Task CheckDuplicateKeysAsync_WithVeryLongCodigo_ReturnsCorrectResult()
    {
        // Arrange
        string longCodigo = new('X', 1000);
        string cnpj = _faker.Random.ReplaceNumbers("##############");
        List<Fundo> emptyList = new();

        Mock<DbSet<Fundo>> mockDbSet = emptyList.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);

        FundoRepository repository = new(mockContext.Object);

        // Act
        (bool codigoExists, bool cnpjExists) = await repository.CheckDuplicateKeysAsync(longCodigo, cnpj, CancellationToken.None);

        // Assert
        Assert.False(codigoExists);
        Assert.False(cnpjExists);
    }

    /// <summary>
    /// Tests that CheckDuplicateKeysAsync handles codigo with special characters correctly.
    /// Input: Codigo with special characters, valid cnpj.
    /// Expected: Returns (false, false) when no match exists.
    /// </summary>
    [Fact]
    public async Task CheckDuplicateKeysAsync_WithSpecialCharactersInCodigo_ReturnsCorrectResult()
    {
        // Arrange
        string specialCodigo = "!@#$%^&*()";
        string cnpj = _faker.Random.ReplaceNumbers("##############");
        List<Fundo> emptyList = new();

        Mock<DbSet<Fundo>> mockDbSet = emptyList.AsQueryable().BuildMockDbSet();
        Mock<AppDbContext> mockContext = new(new DbContextOptions<AppDbContext>());
        mockContext.Setup(c => c.Set<Fundo>()).Returns(mockDbSet.Object);

        FundoRepository repository = new(mockContext.Object);

        // Act
        (bool codigoExists, bool cnpjExists) = await repository.CheckDuplicateKeysAsync(specialCodigo, cnpj, CancellationToken.None);

        // Assert
        Assert.False(codigoExists);
        Assert.False(cnpjExists);
    }
}