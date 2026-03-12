using Bogus;
using CaseItau.Application.DTOs;
using CaseItau.Application.Interfaces;
using CaseItau.Application.Services;
using CaseItau.Domain.Entities;
using CaseItau.Domain.Exceptions;
using CaseItau.Domain.Interfaces;
using CaseItau.Domain.ValueObjects;
using CaseItau.Infra.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace CaseItau.Tests.UnitTests.Services;


/// <summary>
/// Tests for the FundoService.DeleteAsync method.
/// </summary>
public partial class FundoServiceTests
{
    private readonly Mock<IFundoRepository> _repositoryMock;
    private readonly Mock<ITipoFundoCacheService> _tipoFundoCacheServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<FundoService>> _loggerMock;
    private readonly FundoService _service;
    private readonly Faker _faker;

    public FundoServiceTests()
    {
        _repositoryMock = new Mock<IFundoRepository>();
        _tipoFundoCacheServiceMock = new Mock<ITipoFundoCacheService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<FundoService>>();
        _service = new FundoService(_repositoryMock.Object, _tipoFundoCacheServiceMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
        _faker = new Faker();
    }

    /// <summary>
    /// Tests that DeleteAsync returns true when the fund exists and is successfully deleted.
    /// Input: Valid codigo, fund exists.
    /// Expected: Returns true, calls repository DeleteAsync and unit of work CommitAsync, logs information messages.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenFundExists_ReturnsTrue()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Fundo fundo = new Fundo { Codigo = codigo, Nome = _faker.Company.CompanyName() };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);
        _repositoryMock
            .Setup(r => r.DeleteAsync(fundo, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.DeleteAsync(codigo, CancellationToken.None);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(fundo, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Deleting fund with codigo '{codigo}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fund with codigo '{codigo}' deleted successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteAsync returns false when the fund does not exist.
    /// Input: Valid codigo, fund does not exist (GetByCodigoAsync returns null).
    /// Expected: Returns false, does not call DeleteAsync or CommitAsync, logs warning message.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenFundDoesNotExist_ReturnsFalse()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Fundo?)null);

        // Act
        bool result = await _service.DeleteAsync(codigo, CancellationToken.None);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Fundo>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Deleting fund with codigo '{codigo}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fund with codigo '{codigo}' was not found for deletion")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteAsync passes the correct CancellationToken to repository and unit of work.
    /// Input: Valid codigo with custom CancellationToken.
    /// Expected: GetByCodigoAsync, DeleteAsync, and CommitAsync are called with the provided token.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_PassesCancellationTokenToRepositoryAndUnitOfWork()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Fundo fundo = new Fundo { Codigo = codigo, Nome = _faker.Company.CompanyName() };
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, token))
            .ReturnsAsync(fundo);
        _repositoryMock
            .Setup(r => r.DeleteAsync(fundo, token))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(token))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(codigo, token);

        // Assert
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, token), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(fundo, token), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(token), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteAsync handles various edge case codigo values correctly when fund exists.
    /// Input: Edge case codigo values (null, empty, whitespace, special characters, very long string).
    /// Expected: Returns true for all cases when fund is found, proper repository and unit of work calls.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!@#$%^&*()")]
    [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890")]
    public async Task DeleteAsync_WithEdgeCaseCodigoValues_WhenFundExists_ReturnsTrue(string? codigo)
    {
        // Arrange
        Fundo fundo = new Fundo { Codigo = codigo ?? string.Empty, Nome = _faker.Company.CompanyName() };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);
        _repositoryMock
            .Setup(r => r.DeleteAsync(fundo, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.DeleteAsync(codigo!, CancellationToken.None);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo!, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(fundo, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteAsync handles various edge case codigo values correctly when fund does not exist.
    /// Input: Edge case codigo values (null, empty, whitespace, special characters, very long string), fund not found.
    /// Expected: Returns false for all cases, no DeleteAsync or CommitAsync calls.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!@#$%^&*()")]
    [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890")]
    public async Task DeleteAsync_WithEdgeCaseCodigoValues_WhenFundDoesNotExist_ReturnsFalse(string? codigo)
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo!, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Fundo?)null);

        // Act
        bool result = await _service.DeleteAsync(codigo!, CancellationToken.None);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo!, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Fundo>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that DeleteAsync propagates exceptions thrown by GetByCodigoAsync.
    /// Input: Valid codigo, GetByCodigoAsync throws exception.
    /// Expected: Exception is propagated to caller.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenGetByCodigoAsyncThrowsException_PropagatesException()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        InvalidOperationException expectedException = new("Repository error");
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(codigo, CancellationToken.None));
        Assert.Same(expectedException, actualException);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Fundo>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that DeleteAsync propagates exceptions thrown by DeleteAsync on repository.
    /// Input: Valid codigo, fund exists, DeleteAsync throws exception.
    /// Expected: Exception is propagated to caller, CommitAsync is not called.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenRepositoryDeleteAsyncThrowsException_PropagatesException()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Fundo fundo = new Fundo { Codigo = codigo, Nome = _faker.Company.CompanyName() };
        InvalidOperationException expectedException = new("Delete error");
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);
        _repositoryMock
            .Setup(r => r.DeleteAsync(fundo, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(codigo, CancellationToken.None));
        Assert.Same(expectedException, actualException);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(fundo, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that DeleteAsync propagates exceptions thrown by CommitAsync on unit of work.
    /// Input: Valid codigo, fund exists, CommitAsync throws exception.
    /// Expected: Exception is propagated to caller after DeleteAsync is called.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenCommitAsyncThrowsException_PropagatesException()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Fundo fundo = new Fundo { Codigo = codigo, Nome = _faker.Company.CompanyName() };
        InvalidOperationException expectedException = new("Commit error");
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);
        _repositoryMock
            .Setup(r => r.DeleteAsync(fundo, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(codigo, CancellationToken.None));
        Assert.Same(expectedException, actualException);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(fundo, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteAsync respects a cancelled CancellationToken.
    /// Input: Valid codigo with already cancelled CancellationToken.
    /// Expected: OperationCanceledException is thrown.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenCancellationTokenIsCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        CancellationTokenSource cts = new();
        cts.Cancel();
        CancellationToken token = cts.Token;
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, token))
            .ThrowsAsync(new OperationCanceledException(token));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _service.DeleteAsync(codigo, token));
    }

    /// <summary>
    /// Tests that DeleteAsync calls repository and unit of work methods in the correct order.
    /// Input: Valid codigo, fund exists.
    /// Expected: GetByCodigoAsync is called first, then DeleteAsync, then CommitAsync.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenFundExists_CallsMethodsInCorrectOrder()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Fundo fundo = new Fundo { Codigo = codigo, Nome = _faker.Company.CompanyName() };
        var callOrder = new System.Collections.Generic.List<string>();
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo)
            .Callback(() => callOrder.Add("GetByCodigoAsync"));
        _repositoryMock
            .Setup(r => r.DeleteAsync(fundo, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() => callOrder.Add("DeleteAsync"));
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() => callOrder.Add("CommitAsync"));

        // Act
        await _service.DeleteAsync(codigo, CancellationToken.None);

        // Assert
        Assert.Equal(3, callOrder.Count);
        Assert.Equal("GetByCodigoAsync", callOrder[0]);
        Assert.Equal("DeleteAsync", callOrder[1]);
        Assert.Equal("CommitAsync", callOrder[2]);
    }

    /// <summary>
    /// Tests that CreateAsync successfully creates a fund when all validations pass.
    /// Input: Valid CreateFundoDto with no duplicateds and existing CodigoTipo.
    /// Expected: Fund is added to repository, unit of work is committed, and appropriate logs are written.
    /// </summary>
    [Fact]
    public async Task CreateAsync_WithValidDto_CreatesSuccessfully()
    {
        // Arrange
        CreateFundoDto dto = new()
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.ReplaceNumbers("##############"),
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = _faker.Finance.Amount()
        };

        _tipoFundoCacheServiceMock
            .Setup(s => s.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Fundo>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        _tipoFundoCacheServiceMock.Verify(s => s.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Fundo>(f =>
            f.Codigo == dto.Codigo &&
            f.Nome == dto.Nome &&
            f.Cnpj.Value == dto.Cnpj &&
            f.CodigoTipo == dto.CodigoTipo &&
            f.Patrimonio == dto.Patrimonio), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Creating fund with codigo '{dto.Codigo}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fund with codigo '{dto.Codigo}' created successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateAsync throws DomainException when CodigoTipo does not exist.
    /// Input: CreateFundoDto with non-existent CodigoTipo.
    /// Expected: DomainException is thrown with appropriate message and warning is logged.
    /// </summary>
    [Fact]
    public async Task CreateAsync_WhenCodigoTipoDoesNotExist_ThrowsDomainException()
    {
        // Arrange
        CreateFundoDto dto = new()
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.ReplaceNumbers("##############"),
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = _faker.Finance.Amount()
        };

        _tipoFundoCacheServiceMock
            .Setup(s => s.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync(dto, CancellationToken.None));

        Assert.Equal($"CodigoTipo '{dto.CodigoTipo}' does not exist.", exception.Message);
        _tipoFundoCacheServiceMock.Verify(s => s.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.CheckDuplicatedKeysAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Fundo>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"CodigoTipo '{dto.CodigoTipo}' does not exist")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateAsync throws DomainException when Codigo already exists.
    /// Input: CreateFundoDto with duplicated Codigo.
    /// Expected: DomainException is thrown with appropriate message and warning is logged.
    /// </summary>
    [Fact]
    public async Task CreateAsync_WhenCodigoAlreadyExists_ThrowsDomainException()
    {
        // Arrange
        CreateFundoDto dto = new()
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.ReplaceNumbers("##############"),
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = _faker.Finance.Amount()
        };

        _tipoFundoCacheServiceMock
            .Setup(s => s.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, false));

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync(dto, CancellationToken.None));

        Assert.Equal($"Codigo '{dto.Codigo}' already exists.", exception.Message);
        _repositoryMock.Verify(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Fundo>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Duplicated codigo '{dto.Codigo}' detected")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateAsync throws DomainException when CNPJ already exists.
    /// Input: CreateFundoDto with duplicated CNPJ.
    /// Expected: DomainException is thrown with appropriate message and warning is logged.
    /// </summary>
    [Fact]
    public async Task CreateAsync_WhenCnpjAlreadyExists_ThrowsDomainException()
    {
        // Arrange
        CreateFundoDto dto = new()
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.ReplaceNumbers("##############"),
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = _faker.Finance.Amount()
        };

        _tipoFundoCacheServiceMock
            .Setup(s => s.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, true));

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync(dto, CancellationToken.None));

        Assert.Equal($"Cnpj '{dto.Cnpj}' already exists.", exception.Message);
        _repositoryMock.Verify(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Fundo>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Duplicated CNPJ '{dto.Cnpj}' detected")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that CreateAsync throws DomainException when both Codigo and CNPJ already exist.
    /// Input: CreateFundoDto with duplicated Codigo and CNPJ.
    /// Expected: DomainException is thrown for Codigo (first check) and warning is logged.
    /// </summary>
    [Fact]
    public async Task CreateAsync_WhenBothCodigoAndCnpjExist_ThrowsDomainExceptionForCodigo()
    {
        // Arrange
        CreateFundoDto dto = new()
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.ReplaceNumbers("##############"),
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = _faker.Finance.Amount()
        };

        _tipoFundoCacheServiceMock
            .Setup(s => s.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, true));

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync(dto, CancellationToken.None));

        Assert.Equal($"Codigo '{dto.Codigo}' already exists.", exception.Message);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Fundo>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that CreateAsync passes the correct CancellationToken to all dependencies.
    /// Input: Valid CreateFundoDto with a custom CancellationToken.
    /// Expected: All async calls receive the provided CancellationToken.
    /// </summary>
    [Fact]
    public async Task CreateAsync_PassesCancellationTokenToAllDependencies()
    {
        // Arrange
        CreateFundoDto dto = new()
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.ReplaceNumbers("##############"),
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = _faker.Finance.Amount()
        };

        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        _tipoFundoCacheServiceMock
            .Setup(s => s.ExistsAsync(dto.CodigoTipo, token))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, token))
            .ReturnsAsync((false, false));

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Fundo>(), token))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(token))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateAsync(dto, token);

        // Assert
        _tipoFundoCacheServiceMock.Verify(s => s.ExistsAsync(dto.CodigoTipo, token), Times.Once);
        _repositoryMock.Verify(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, token), Times.Once);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Fundo>(), token), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(token), Times.Once);
    }

    /// <summary>
    /// Tests that CreateAsync throws DomainException when CNPJ is invalid.
    /// Input: CreateFundoDto with invalid CNPJ (not 14 digits).
    /// Expected: DomainException is thrown by Cnpj value object constructor.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("12345678901234567")]
    [InlineData("1234567890123A")]
    [InlineData("12345678 01234")]
    public async Task CreateAsync_WhenCnpjIsInvalid_ThrowsDomainException(string invalidCnpj)
    {
        // Arrange
        CreateFundoDto dto = new()
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = invalidCnpj,
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = _faker.Finance.Amount()
        };

        _tipoFundoCacheServiceMock
            .Setup(s => s.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync(dto, CancellationToken.None));

        Assert.Equal("Cnpj must be exactly 14 numeric digits.", exception.Message);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Fundo>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that CreateAsync successfully creates a fund with null Patrimonio.
    /// Input: Valid CreateFundoDto with null Patrimonio.
    /// Expected: Fund is created with null Patrimonio value.
    /// </summary>
    [Fact]
    public async Task CreateAsync_WithNullPatrimonio_CreatesSuccessfully()
    {
        // Arrange
        CreateFundoDto dto = new()
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.ReplaceNumbers("##############"),
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = null
        };

        _tipoFundoCacheServiceMock
            .Setup(s => s.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Fundo>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Fundo>(f =>
            f.Codigo == dto.Codigo &&
            f.Nome == dto.Nome &&
            f.Cnpj.Value == dto.Cnpj &&
            f.CodigoTipo == dto.CodigoTipo &&
            f.Patrimonio == null), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateAsync successfully creates a fund with extreme Patrimonio values.
    /// Input: Valid CreateFundoDto with zero, negative, or extreme Patrimonio values.
    /// Expected: Fund is created with the provided Patrimonio value.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1000.50)]
    [InlineData(999999999999.99)]
    public async Task CreateAsync_WithExtremePatrimonioValues_CreatesSuccessfully(decimal patrimonio)
    {
        // Arrange
        CreateFundoDto dto = new()
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.ReplaceNumbers("##############"),
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = patrimonio
        };

        _tipoFundoCacheServiceMock
            .Setup(s => s.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Fundo>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Fundo>(f =>
            f.Patrimonio == patrimonio), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateAsync successfully creates a fund with extreme CodigoTipo values.
    /// Input: Valid CreateFundoDto with zero, negative, or extreme CodigoTipo values.
    /// Expected: Fund is created with the provided CodigoTipo value.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task CreateAsync_WithExtremeCodigoTipoValues_CreatesSuccessfully(int codigoTipo)
    {
        // Arrange
        CreateFundoDto dto = new()
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.ReplaceNumbers("##############"),
            CodigoTipo = codigoTipo,
            Patrimonio = _faker.Finance.Amount()
        };

        _tipoFundoCacheServiceMock
            .Setup(s => s.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.CheckDuplicatedKeysAsync(dto.Codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Fundo>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Fundo>(f =>
            f.CodigoTipo == codigoTipo), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetAllAsync returns an empty collection when the repository returns no funds.
    /// Input: Repository returns an empty collection.
    /// Expected: Returns an empty IEnumerable of FundoDto and logs appropriate messages.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WhenRepositoryReturnsEmptyCollection_ReturnsEmptyCollection()
    {
        // Arrange
        IEnumerable<Fundo> emptyFundos = new List<Fundo>();
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyFundos);

        // Act
        IEnumerable<FundoDto> result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving all funds")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("0 fund(s) retrieved")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetAllAsync correctly passes the CancellationToken to the repository.
    /// Input: Custom CancellationToken.
    /// Expected: Repository's GetAllAsync is called with the provided token.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_PassesCancellationTokenToRepository()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;
        IEnumerable<Fundo> fundos = new List<Fundo>();
        _repositoryMock
            .Setup(r => r.GetAllAsync(token))
            .ReturnsAsync(fundos);

        // Act
        await _service.GetAllAsync(token);

        // Assert
        _repositoryMock.Verify(r => r.GetAllAsync(token), Times.Once);
    }

    /// <summary>
    /// Tests that GetAllAsync correctly maps funds with null TipoFundo.
    /// Input: Repository returns funds where TipoFundo is null.
    /// Expected: Returns DTOs with NomeTipo set to empty string.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WhenTipoFundoIsNull_MapsNomeTipoToEmptyString()
    {
        // Arrange
        Fundo fundo = new Fundo
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = new Cnpj(_faker.Random.Replace("##############")),
            CodigoTipo = _faker.Random.Int(1, 10),
            TipoFundo = null,
            Patrimonio = _faker.Finance.Amount()
        };
        IEnumerable<Fundo> fundos = new List<Fundo> { fundo };
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundos);

        // Act
        IEnumerable<FundoDto> result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        FundoDto dto = result.First();
        Assert.Equal(string.Empty, dto.NomeTipo);
    }

    /// <summary>
    /// Tests that GetAllAsync correctly maps funds with null Patrimonio.
    /// Input: Repository returns funds where Patrimonio is null.
    /// Expected: Returns DTOs with Patrimonio set to null.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WhenPatrimonioIsNull_MapsPatrimonioToNull()
    {
        // Arrange
        Fundo fundo = new Fundo
        {
            Codigo = "FUND001",
            Nome = "Test Fund",
            Cnpj = new Cnpj("12345678901234"),
            CodigoTipo = 1,
            TipoFundo = new TipoFundo
            {
                Codigo = 1,
                Nome = "Test Type"
            },
            Patrimonio = null
        };
        IEnumerable<Fundo> fundos = new List<Fundo> { fundo };
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundos);

        // Act
        IEnumerable<FundoDto> result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        FundoDto dto = result.First();
        Assert.Null(dto.Patrimonio);
    }

    /// <summary>
    /// Tests that GetAllAsync correctly maps funds with extreme Patrimonio values.
    /// Input: Repository returns funds with very large and very small Patrimonio values.
    /// Expected: Returns DTOs with correct Patrimonio values mapped.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(999999999999.99)]
    [InlineData(-999999999999.99)]
    public async Task GetAllAsync_WithExtremePatrimonioValues_MapsCorrectly(decimal patrimonioValue)
    {
        // Arrange
        Fundo fundo = new Fundo
        {
            Codigo = "TEST001",
            Nome = "Test Fund",
            Cnpj = new Cnpj("12345678901234"),
            CodigoTipo = 1,
            TipoFundo = new TipoFundo
            {
                Codigo = 1,
                Nome = "Test Type"
            },
            Patrimonio = patrimonioValue
        };
        IEnumerable<Fundo> fundos = new List<Fundo> { fundo };
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundos);

        // Act
        IEnumerable<FundoDto> result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        FundoDto dto = result.First();
        Assert.Equal(patrimonioValue, dto.Patrimonio);
    }

    /// <summary>
    /// Tests that GetAllAsync correctly maps funds with extreme CodigoTipo values.
    /// Input: Repository returns funds with minimum and maximum integer values for CodigoTipo.
    /// Expected: Returns DTOs with correct CodigoTipo values mapped.
    /// </summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    public async Task GetAllAsync_WithExtremeCodigoTipoValues_MapsCorrectly(int codigoTipo)
    {
        // Arrange
        Fundo fundo = new Fundo
        {
            Codigo = "TEST123",
            Nome = "Test Fund",
            Cnpj = new Cnpj("12345678901234"),
            CodigoTipo = codigoTipo,
            TipoFundo = new TipoFundo
            {
                Codigo = 1,
                Nome = "Test Type"
            },
            Patrimonio = 1000000m
        };
        IEnumerable<Fundo> fundos = new List<Fundo> { fundo };
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundos);

        // Act
        IEnumerable<FundoDto> result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        FundoDto dto = result.First();
        Assert.Equal(codigoTipo, dto.CodigoTipo);
    }

    /// <summary>
    /// Helper method to create a Fundo entity for testing purposes.
    /// </summary>
    private Fundo CreateFundo()
    {
        return new Fundo
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = new Cnpj(_faker.Random.Replace("##.###.###/####-##")),
            CodigoTipo = _faker.Random.Int(1, 10),
            TipoFundo = new TipoFundo
            {
                Codigo = _faker.Random.Int(1, 10),
                Nome = _faker.Random.Word()
            },
            Patrimonio = _faker.Finance.Amount()
        };
    }

    /// <summary>
    /// Tests that UpdateAsync returns true and updates the fund when all conditions are valid.
    /// Input: Valid codigo, existing fund, valid CodigoTipo, CNPJ not taken by others.
    /// Expected: Returns true, updates fund properties, commits changes, and logs success.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WhenAllConditionsAreValid_ReturnsTrueAndUpdatesFund()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        UpdateFundoDto dto = new()
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = "12345678901234",
            CodigoTipo = _faker.Random.Int(1, 100)
        };
        Fundo existingFundo = new()
        {
            Codigo = codigo,
            Nome = "Old Name",
            Cnpj = new Cnpj("98765432109876"),
            CodigoTipo = 1
        };

        _repositoryMock
            .Setup(r => r.FindForUpdateAsync(codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((existingFundo, false));
        _tipoFundoCacheServiceMock
            .Setup(t => t.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.UpdateAsync(codigo, dto, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(dto.Nome, existingFundo.Nome);
        Assert.Equal(dto.Cnpj, existingFundo.Cnpj.Value);
        Assert.Equal(dto.CodigoTipo, existingFundo.CodigoTipo);
        _repositoryMock.Verify(r => r.Update(existingFundo), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Updating fund with codigo '{codigo}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fund with codigo '{codigo}' updated successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateAsync returns false when the fund does not exist.
    /// Input: Valid codigo, but fund not found (FindForUpdateAsync returns null fund).
    /// Expected: Returns false, logs warning, does not update or commit.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WhenFundNotFound_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        UpdateFundoDto dto = new()
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = "12345678901234",
            CodigoTipo = _faker.Random.Int(1, 100)
        };

        _repositoryMock
            .Setup(r => r.FindForUpdateAsync(codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((Fundo?)null, false));

        // Act
        bool result = await _service.UpdateAsync(codigo, dto, CancellationToken.None);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.Update(It.IsAny<Fundo>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fund with codigo '{codigo}' was not found for update")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateAsync throws DomainException when CodigoTipo does not exist.
    /// Input: Valid fund exists, but CodigoTipo does not exist in cache.
    /// Expected: Throws DomainException with correct message, logs warning, does not update or commit.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WhenCodigoTipoDoesNotExist_ThrowsDomainExceptionAndLogsWarning()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        UpdateFundoDto dto = new()
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = "12345678901234",
            CodigoTipo = _faker.Random.Int(1, 100)
        };
        Fundo existingFundo = new()
        {
            Codigo = codigo,
            Nome = "Old Name",
            Cnpj = new Cnpj("98765432109876"),
            CodigoTipo = 1
        };

        _repositoryMock
            .Setup(r => r.FindForUpdateAsync(codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((existingFundo, false));
        _tipoFundoCacheServiceMock
            .Setup(t => t.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(() =>
            _service.UpdateAsync(codigo, dto, CancellationToken.None));

        Assert.Equal($"CodigoTipo '{dto.CodigoTipo}' does not exist.", exception.Message);
        _repositoryMock.Verify(r => r.Update(It.IsAny<Fundo>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"CodigoTipo '{dto.CodigoTipo}' does not exist")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateAsync throws DomainException when CNPJ is already taken by another fund.
    /// Input: Valid fund exists, valid CodigoTipo, but CNPJ is taken by another fund.
    /// Expected: Throws DomainException with correct message, logs warning, does not update or commit.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WhenCnpjTakenByOther_ThrowsDomainExceptionAndLogsWarning()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        UpdateFundoDto dto = new()
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = "12345678901234",
            CodigoTipo = _faker.Random.Int(1, 100)
        };
        Fundo existingFundo = new()
        {
            Codigo = codigo,
            Nome = "Old Name",
            Cnpj = new Cnpj("98765432109876"),
            CodigoTipo = 1
        };

        _repositoryMock
            .Setup(r => r.FindForUpdateAsync(codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((existingFundo, true));
        _tipoFundoCacheServiceMock
            .Setup(t => t.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(() =>
            _service.UpdateAsync(codigo, dto, CancellationToken.None));

        Assert.Equal($"Cnpj '{dto.Cnpj}' already exists.", exception.Message);
        _repositoryMock.Verify(r => r.Update(It.IsAny<Fundo>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Duplicated CNPJ '{dto.Cnpj}' detected on update for codigo '{codigo}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateAsync throws DomainException when dto.Cnpj has invalid format.
    /// Input: Valid fund, valid CodigoTipo, but dto.Cnpj is not 14 numeric digits.
    /// Expected: Throws DomainException when constructing Cnpj value object.
    /// </summary>
    [Theory]
    [InlineData("123456789012")] // Too short
    [InlineData("123456789012345")] // Too long
    [InlineData("1234567890123A")] // Contains non-digit
    [InlineData("")] // Empty
    [InlineData("   ")] // Whitespace
    public async Task UpdateAsync_WhenCnpjFormatInvalid_ThrowsDomainException(string invalidCnpj)
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        UpdateFundoDto dto = new()
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = invalidCnpj,
            CodigoTipo = _faker.Random.Int(1, 100)
        };
        Fundo existingFundo = new()
        {
            Codigo = codigo,
            Nome = "Old Name",
            Cnpj = new Cnpj("98765432109876"),
            CodigoTipo = 1
        };

        _repositoryMock
            .Setup(r => r.FindForUpdateAsync(codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((existingFundo, false));
        _tipoFundoCacheServiceMock
            .Setup(t => t.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(() =>
            _service.UpdateAsync(codigo, dto, CancellationToken.None));

        Assert.Equal("Cnpj must be exactly 14 numeric digits.", exception.Message);
        _repositoryMock.Verify(r => r.Update(It.IsAny<Fundo>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that UpdateAsync passes the correct CancellationToken to all async operations.
    /// Input: Valid codigo, dto, and a custom CancellationToken.
    /// Expected: All async methods receive the provided CancellationToken.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_PassesCancellationTokenToAllAsyncOperations()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        UpdateFundoDto dto = new()
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = "12345678901234",
            CodigoTipo = _faker.Random.Int(1, 100)
        };
        Fundo existingFundo = new()
        {
            Codigo = codigo,
            Nome = "Old Name",
            Cnpj = new Cnpj("98765432109876"),
            CodigoTipo = 1
        };
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        _repositoryMock
            .Setup(r => r.FindForUpdateAsync(codigo, dto.Cnpj, token))
            .ReturnsAsync((existingFundo, false));
        _tipoFundoCacheServiceMock
            .Setup(t => t.ExistsAsync(dto.CodigoTipo, token))
            .ReturnsAsync(true);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(token))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(codigo, dto, token);

        // Assert
        _repositoryMock.Verify(r => r.FindForUpdateAsync(codigo, dto.Cnpj, token), Times.Once);
        _tipoFundoCacheServiceMock.Verify(t => t.ExistsAsync(dto.CodigoTipo, token), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(token), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateAsync handles boundary values for CodigoTipo correctly.
    /// Input: Valid fund with CodigoTipo set to extreme integer values.
    /// Expected: Updates fund successfully when CodigoTipo exists in cache.
    /// </summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    public async Task UpdateAsync_WhenCodigoTipoHasBoundaryValues_UpdatesSuccessfully(int codigoTipo)
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        UpdateFundoDto dto = new()
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = "12345678901234",
            CodigoTipo = codigoTipo
        };
        Fundo existingFundo = new()
        {
            Codigo = codigo,
            Nome = "Old Name",
            Cnpj = new Cnpj("98765432109876"),
            CodigoTipo = 1
        };

        _repositoryMock
            .Setup(r => r.FindForUpdateAsync(codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((existingFundo, false));
        _tipoFundoCacheServiceMock
            .Setup(t => t.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.UpdateAsync(codigo, dto, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(codigoTipo, existingFundo.CodigoTipo);
    }

    /// <summary>
    /// Tests that UpdateAsync handles edge cases for string parameters correctly.
    /// Input: Valid fund with edge-case values for codigo and dto.Nome.
    /// Expected: Updates fund successfully with the provided values.
    /// </summary>
    [Theory]
    [InlineData("A", "Single Char Nome")]
    [InlineData("VeryLongCodigoWith20C", "VeryLongNomeStringThatCouldPotentiallyExceedMaxLengthButForTestingPurposesWeNeedToValidateTheBehavior")]
    [InlineData("Special!@#$%", "Nome with special chars: !@#$%^&*()")]
    public async Task UpdateAsync_WhenStringParametersHaveEdgeCaseValues_UpdatesSuccessfully(string codigo, string nome)
    {
        // Arrange
        UpdateFundoDto dto = new()
        {
            Nome = nome,
            Cnpj = "12345678901234",
            CodigoTipo = _faker.Random.Int(1, 100)
        };
        Fundo existingFundo = new()
        {
            Codigo = codigo,
            Nome = "Old Name",
            Cnpj = new Cnpj("98765432109876"),
            CodigoTipo = 1
        };

        _repositoryMock
            .Setup(r => r.FindForUpdateAsync(codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((existingFundo, false));
        _tipoFundoCacheServiceMock
            .Setup(t => t.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.UpdateAsync(codigo, dto, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(nome, existingFundo.Nome);
    }

    /// <summary>
    /// Tests that UpdateAsync checks CodigoTipo existence before checking CNPJ duplication.
    /// Input: Fund exists, CodigoTipo doesn't exist, and CNPJ is taken by another.
    /// Expected: Throws DomainException for CNPJ.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WhenCodigoTipoDoesNotExistAndCnpjTaken_ThrowsCnpjException()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        UpdateFundoDto dto = new()
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = "12345678901234",
            CodigoTipo = _faker.Random.Int(1, 100)
        };
        Fundo existingFundo = new()
        {
            Codigo = codigo,
            Nome = "Old Name",
            Cnpj = new Cnpj("98765432109876"),
            CodigoTipo = 1
        };

        _repositoryMock
            .Setup(r => r.FindForUpdateAsync(codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((existingFundo, true));
        _tipoFundoCacheServiceMock
            .Setup(t => t.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(() =>
            _service.UpdateAsync(codigo, dto, CancellationToken.None));

        Assert.Equal($"Cnpj '{dto.Cnpj}' already exists.", exception.Message);
    }

    /// <summary>
    /// Tests that UpdateAsync correctly updates all fund properties.
    /// Input: Valid fund with all properties changed in dto.
    /// Expected: All fund properties (Nome, Cnpj, CodigoTipo) are updated correctly.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WhenAllPropertiesChanged_UpdatesAllFundProperties()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        string newNome = _faker.Company.CompanyName();
        string newCnpj = "11111111111111";
        int newCodigoTipo = _faker.Random.Int(1, 100);
        UpdateFundoDto dto = new()
        {
            Nome = newNome,
            Cnpj = newCnpj,
            CodigoTipo = newCodigoTipo
        };
        Fundo existingFundo = new()
        {
            Codigo = codigo,
            Nome = "Old Name",
            Cnpj = new Cnpj("98765432109876"),
            CodigoTipo = 1
        };

        _repositoryMock
            .Setup(r => r.FindForUpdateAsync(codigo, dto.Cnpj, It.IsAny<CancellationToken>()))
            .ReturnsAsync((existingFundo, false));
        _tipoFundoCacheServiceMock
            .Setup(t => t.ExistsAsync(dto.CodigoTipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.UpdateAsync(codigo, dto, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(newNome, existingFundo.Nome);
        Assert.Equal(newCnpj, existingFundo.Cnpj.Value);
        Assert.Equal(newCodigoTipo, existingFundo.CodigoTipo);
    }

    /// <summary>
    /// Tests that GetByCodigoAsync returns a mapped FundoDto when the fund exists.
    /// Input: Valid codigo, repository returns a fund entity with all properties populated.
    /// Expected: Returns FundoDto with all properties correctly mapped from the entity.
    /// </summary>
    [Fact]
    public async Task GetByCodigoAsync_WhenFundExists_ReturnsMappedDto()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Fundo fundo = new()
        {
            Codigo = codigo,
            Nome = _faker.Company.CompanyName(),
            Cnpj = new Cnpj("12345678000190"),
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = _faker.Finance.Amount(1000, 1000000),
            TipoFundo = new TipoFundo
            {
                Codigo = _faker.Random.Int(1, 100),
                Nome = _faker.Finance.AccountName()
            }
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);

        // Act
        FundoDto? result = await _service.GetByCodigoAsync(codigo, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fundo.Codigo, result.Codigo);
        Assert.Equal(fundo.Nome, result.Nome);
        Assert.Equal(fundo.Cnpj.Value, result.Cnpj);
        Assert.Equal(fundo.CodigoTipo, result.CodigoTipo);
        Assert.Equal(fundo.TipoFundo.Nome, result.NomeTipo);
        Assert.Equal(fundo.Patrimonio, result.Patrimonio);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Retrieving fund with codigo '{codigo}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that GetByCodigoAsync correctly maps a fund with null TipoFundo navigation property.
    /// Input: Valid codigo, fund entity with TipoFundo set to null.
    /// Expected: Returns FundoDto with NomeTipo set to empty string.
    /// </summary>
    [Fact]
    public async Task GetByCodigoAsync_WhenFundExistsWithNullTipoFundo_ReturnsDtoWithEmptyNomeTipo()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Fundo fundo = new()
        {
            Codigo = codigo,
            Nome = _faker.Company.CompanyName(),
            Cnpj = new Cnpj("12345678000190"),
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = _faker.Finance.Amount(1000, 1000000),
            TipoFundo = null!
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);

        // Act
        FundoDto? result = await _service.GetByCodigoAsync(codigo, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.NomeTipo);
    }

    /// <summary>
    /// Tests that GetByCodigoAsync correctly maps a fund with null Patrimonio.
    /// Input: Valid codigo, fund entity with Patrimonio set to null.
    /// Expected: Returns FundoDto with Patrimonio set to null.
    /// </summary>
    [Fact]
    public async Task GetByCodigoAsync_WhenFundExistsWithNullPatrimonio_ReturnsDtoWithNullPatrimonio()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Fundo fundo = new()
        {
            Codigo = codigo,
            Nome = _faker.Company.CompanyName(),
            Cnpj = new Cnpj("12345678000190"),
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = null,
            TipoFundo = new TipoFundo
            {
                Codigo = _faker.Random.Int(1, 100),
                Nome = _faker.Finance.AccountName()
            }
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);

        // Act
        FundoDto? result = await _service.GetByCodigoAsync(codigo, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Patrimonio);
    }

    /// <summary>
    /// Tests that GetByCodigoAsync returns null when the fund does not exist.
    /// Input: Valid codigo, repository returns null.
    /// Expected: Returns null and logs a warning message.
    /// </summary>
    [Fact]
    public async Task GetByCodigoAsync_WhenFundDoesNotExist_ReturnsNull()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Fundo?)null);

        // Act
        FundoDto? result = await _service.GetByCodigoAsync(codigo, CancellationToken.None);

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Retrieving fund with codigo '{codigo}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fund with codigo '{codigo}' was not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetByCodigoAsync passes the correct CancellationToken to the repository.
    /// Input: Valid codigo with a custom CancellationToken.
    /// Expected: Repository method is called with the provided token.
    /// </summary>
    [Fact]
    public async Task GetByCodigoAsync_PassesCancellationTokenToRepository()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;
        Fundo fundo = new()
        {
            Codigo = codigo,
            Nome = _faker.Company.CompanyName(),
            Cnpj = new Cnpj("12345678000190"),
            CodigoTipo = _faker.Random.Int(1, 100),
            TipoFundo = new TipoFundo
            {
                Codigo = _faker.Random.Int(1, 100),
                Nome = _faker.Finance.AccountName()
            }
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, token))
            .ReturnsAsync(fundo);

        // Act
        await _service.GetByCodigoAsync(codigo, token);

        // Assert
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, token), Times.Once);
    }

    /// <summary>
    /// Tests that GetByCodigoAsync handles various edge case codigo values.
    /// Input: Edge case codigo values (empty string, whitespace, very long string, special characters).
    /// Expected: Repository is called with the provided codigo value without modification.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")]
    [InlineData("!@#$%^&*()_+-=[]{}|;':\",./<>?")]
    [InlineData("código-com-acentos-ção-ã-õ")]
    public async Task GetByCodigoAsync_WithEdgeCaseCodigoValues_PassesValueToRepository(string codigo)
    {
        // Arrange
        Fundo fundo = new()
        {
            Codigo = codigo,
            Nome = _faker.Company.CompanyName(),
            Cnpj = new Cnpj("12345678000190"),
            CodigoTipo = _faker.Random.Int(1, 100),
            TipoFundo = new TipoFundo
            {
                Codigo = _faker.Random.Int(1, 100),
                Nome = _faker.Finance.AccountName()
            }
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);

        // Act
        FundoDto? result = await _service.GetByCodigoAsync(codigo, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetByCodigoAsync propagates exceptions thrown by the repository.
    /// Input: Valid codigo, repository throws an exception.
    /// Expected: Exception is propagated to the caller.
    /// </summary>
    [Fact]
    public async Task GetByCodigoAsync_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Exception expectedException = new InvalidOperationException("Database error");
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        Exception actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetByCodigoAsync(codigo, CancellationToken.None));
        Assert.Equal(expectedException.Message, actualException.Message);
    }

    /// <summary>
    /// Tests that GetByCodigoAsync respects cancellation requests.
    /// Input: Valid codigo with a cancelled CancellationToken.
    /// Expected: OperationCanceledException is thrown.
    /// </summary>
    [Fact]
    public async Task GetByCodigoAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        CancellationTokenSource cts = new();
        cts.Cancel();
        CancellationToken cancelledToken = cts.Token;
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, cancelledToken))
            .ThrowsAsync(new OperationCanceledException(cancelledToken));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.GetByCodigoAsync(codigo, cancelledToken));
    }

    /// <summary>
    /// Tests that GetByCodigoAsync correctly handles boundary values for Patrimonio.
    /// Input: Valid codigo, fund with extreme Patrimonio values.
    /// Expected: Returns FundoDto with Patrimonio values correctly mapped.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(999999999999999.99)]
    [InlineData(-0.01)]
    public async Task GetByCodigoAsync_WithBoundaryPatrimonioValues_ReturnsDtoWithCorrectValue(decimal patrimonioValue)
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Fundo fundo = new()
        {
            Codigo = codigo,
            Nome = _faker.Company.CompanyName(),
            Cnpj = new Cnpj("12345678000190"),
            CodigoTipo = _faker.Random.Int(1, 100),
            Patrimonio = patrimonioValue,
            TipoFundo = new TipoFundo
            {
                Codigo = _faker.Random.Int(1, 100),
                Nome = _faker.Finance.AccountName()
            }
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);

        // Act
        FundoDto? result = await _service.GetByCodigoAsync(codigo, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(patrimonioValue, result.Patrimonio);
    }

    /// <summary>
    /// Tests that GetByCodigoAsync correctly handles boundary values for CodigoTipo.
    /// Input: Valid codigo, fund with extreme CodigoTipo values.
    /// Expected: Returns FundoDto with CodigoTipo values correctly mapped.
    /// </summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public async Task GetByCodigoAsync_WithBoundaryCodigoTipoValues_ReturnsDtoWithCorrectValue(int codigoTipo)
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Fundo fundo = new()
        {
            Codigo = codigo,
            Nome = _faker.Company.CompanyName(),
            Cnpj = new Cnpj("12345678000190"),
            CodigoTipo = codigoTipo,
            TipoFundo = new TipoFundo
            {
                Codigo = codigoTipo,
                Nome = _faker.Finance.AccountName()
            }
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);

        // Act
        FundoDto? result = await _service.GetByCodigoAsync(codigo, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(codigoTipo, result.CodigoTipo);
    }

    /// <summary>
    /// Tests that UpdateFundAssetsAsync successfully updates fund assets and returns true when the fund exists and valor is valid.
    /// Input: Valid codigo, existing fund, and valid non-negative valor values (0, positive, decimal.MaxValue).
    /// Expected: Returns true, updates Patrimonio, calls Update and CommitAsync, logs appropriate messages.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100.50)]
    [InlineData(999999999.99)]
    public async Task UpdateFundAssetsAsync_WhenFundExistsAndValorIsValid_UpdatesAssetsAndReturnsTrue(decimal valor)
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Fundo fundo = new()
        {
            Codigo = codigo,
            Patrimonio = 1000m
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.UpdateFundAssetsAsync(codigo, valor, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(valor, fundo.Patrimonio);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.Update(fundo), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Updating fund assets for codigo '{codigo}'") && v.ToString().Contains("valor")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Fund assets for codigo '{codigo}' updated successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssetsAsync successfully updates fund assets with decimal.MaxValue.
    /// Input: Valid codigo, existing fund, and decimal.MaxValue as valor.
    /// Expected: Returns true, updates Patrimonio to decimal.MaxValue, calls Update and CommitAsync.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssetsAsync_WhenFundExistsAndValorIsMaxValue_UpdatesAssetsAndReturnsTrue()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        decimal valor = decimal.MaxValue;
        Fundo fundo = new()
        {
            Codigo = codigo,
            Patrimonio = 1000m
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.UpdateFundAssetsAsync(codigo, valor, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(decimal.MaxValue, fundo.Patrimonio);
        _repositoryMock.Verify(r => r.Update(fundo), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssetsAsync returns false when the fund is not found.
    /// Input: Valid codigo, but repository returns null.
    /// Expected: Returns false, does not call Update or CommitAsync, logs warning message.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssetsAsync_WhenFundNotFound_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        decimal valor = 1000m;
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Fundo?)null);

        // Act
        bool result = await _service.UpdateFundAssetsAsync(codigo, valor, CancellationToken.None);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.Update(It.IsAny<Fundo>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Updating fund assets for codigo '{codigo}' with valor {valor}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fund with codigo '{codigo}' was not found for assets update")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssetsAsync throws DomainException when valor is negative.
    /// Input: Valid codigo, existing fund, and negative valor values.
    /// Expected: Throws DomainException with message "Valor must be non-negative.", does not call Update or CommitAsync, logs warning.
    /// </summary>
    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    [InlineData(-999999999.99)]
    public async Task UpdateFundAssetsAsync_WhenValorIsNegative_ThrowsDomainException(decimal valor)
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        Fundo fundo = new()
        {
            Codigo = codigo,
            Patrimonio = 1000m
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(
            async () => await _service.UpdateFundAssetsAsync(codigo, valor, CancellationToken.None));

        Assert.Equal("Valor must be non-negative.", exception.Message);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.Update(It.IsAny<Fundo>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Negative valor") && v.ToString()!.Contains("provided for assets update") && v.ToString()!.Contains(codigo)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssetsAsync throws DomainException when valor is decimal.MinValue.
    /// Input: Valid codigo, existing fund, and decimal.MinValue as valor.
    /// Expected: Throws DomainException with message "Valor must be non-negative.", does not call Update or CommitAsync.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssetsAsync_WhenValorIsMinValue_ThrowsDomainException()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        decimal valor = decimal.MinValue;
        Fundo fundo = new()
        {
            Codigo = codigo,
            Patrimonio = 1000m
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(
            async () => await _service.UpdateFundAssetsAsync(codigo, valor, CancellationToken.None));

        Assert.Equal("Valor must be non-negative.", exception.Message);
        _repositoryMock.Verify(r => r.Update(It.IsAny<Fundo>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that UpdateFundAssetsAsync correctly passes the CancellationToken to repository and unit of work.
    /// Input: Valid codigo, existing fund, valid valor, and a custom CancellationToken.
    /// Expected: GetByCodigoAsync and CommitAsync are called with the provided CancellationToken.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssetsAsync_PassesCancellationTokenToRepositoryAndUnitOfWork()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        decimal valor = 5000m;
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;
        Fundo fundo = new()
        {
            Codigo = codigo,
            Patrimonio = 1000m
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, token))
            .ReturnsAsync(fundo);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(token))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateFundAssetsAsync(codigo, valor, token);

        // Assert
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, token), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(token), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssetsAsync works correctly with empty string codigo.
    /// Input: Empty string as codigo, existing fund, valid valor.
    /// Expected: Returns true and updates fund assets successfully.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssetsAsync_WhenCodigoIsEmpty_UpdatesAssetsAndReturnsTrue()
    {
        // Arrange
        string codigo = string.Empty;
        decimal valor = 2000m;
        Fundo fundo = new()
        {
            Codigo = codigo,
            Patrimonio = 1000m
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.UpdateFundAssetsAsync(codigo, valor, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(valor, fundo.Patrimonio);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssetsAsync works correctly with whitespace-only codigo.
    /// Input: Whitespace string as codigo, existing fund, valid valor.
    /// Expected: Returns true and updates fund assets successfully.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssetsAsync_WhenCodigoIsWhitespace_UpdatesAssetsAndReturnsTrue()
    {
        // Arrange
        string codigo = "   ";
        decimal valor = 3000m;
        Fundo fundo = new()
        {
            Codigo = codigo,
            Patrimonio = 1000m
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.UpdateFundAssetsAsync(codigo, valor, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(valor, fundo.Patrimonio);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssetsAsync works correctly with codigo containing special characters.
    /// Input: Codigo with special characters, existing fund, valid valor.
    /// Expected: Returns true and updates fund assets successfully.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssetsAsync_WhenCodigoHasSpecialCharacters_UpdatesAssetsAndReturnsTrue()
    {
        // Arrange
        string codigo = "!@#$%^&*()";
        decimal valor = 4000m;
        Fundo fundo = new()
        {
            Codigo = codigo,
            Patrimonio = 1000m
        };
        _repositoryMock
            .Setup(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundo);
        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.UpdateFundAssetsAsync(codigo, valor, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(valor, fundo.Patrimonio);
        _repositoryMock.Verify(r => r.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }
}