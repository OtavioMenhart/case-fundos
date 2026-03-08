using CaseItau.Application.DTOs;
using CaseItau.Application.Interfaces;
using CaseItau.Application.Services;
using CaseItau.Domain.Entities;
using CaseItau.Domain.Exceptions;
using CaseItau.Domain.Interfaces;
using Moq;

namespace CaseItau.Tests.Application.Services;

public class FundoServiceTests
{
    private readonly Mock<IFundoRepository> _repositoryMock = new();
    private readonly Mock<ITipoFundoCacheService> _tipoFundoCacheServiceMock = new();
    private readonly FundoService _service;

    public FundoServiceTests()
    {
        _service = new FundoService(_repositoryMock.Object, _tipoFundoCacheServiceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenFundosExist_ReturnsMappedDtos()
    {
        // Arrange
        var fundos = new List<Fundo>
        {
            new() { Codigo = "ITAUTESTE01", Nome = "Fundo Teste", Cnpj = "00.000.000/0001-00", CodigoTipo = 1, TipoFundo = new TipoFundo { Codigo = 1, Nome = "Renda Fixa" } }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(fundos);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        var dto = Assert.Single(result);
        Assert.Equal("ITAUTESTE01", dto.Codigo);
        Assert.Equal("Renda Fixa", dto.NomeTipo);
    }

    [Fact]
    public async Task GetByCodigoAsync_WhenFundoExists_ReturnsFundoDto()
    {
        // Arrange
        var fundo = new Fundo { Codigo = "ITAUTESTE01", Nome = "Fundo Teste", Cnpj = "00.000.000/0001-00", CodigoTipo = 1, TipoFundo = new TipoFundo { Codigo = 1, Nome = "Renda Fixa" } };
        _repositoryMock.Setup(r => r.GetByCodigoAsync("ITAUTESTE01")).ReturnsAsync(fundo);

        // Act
        var result = await _service.GetByCodigoAsync("ITAUTESTE01");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ITAUTESTE01", result.Codigo);
    }

    [Fact]
    public async Task GetByCodigoAsync_WhenFundoDoesNotExist_ReturnsNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByCodigoAsync("NOTFOUND")).ReturnsAsync((Fundo?)null);

        // Act
        var result = await _service.GetByCodigoAsync("NOTFOUND");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WhenCodigoTipoExists_CallsRepositoryAddAsync()
    {
        // Arrange
        var dto = new CreateFundoDto { Codigo = "ITAUTESTE01", Nome = "Fundo Teste", Cnpj = "00.000.000/0001-00", CodigoTipo = 1 };
        _tipoFundoCacheServiceMock.Setup(c => c.ExistsAsync(1)).ReturnsAsync(true);

        // Act
        await _service.CreateAsync(dto);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Fundo>(f => f.Codigo == "ITAUTESTE01")), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenCodigoTipoDoesNotExist_ThrowsDomainException()
    {
        // Arrange
        var dto = new CreateFundoDto { Codigo = "ITAUTESTE01", Nome = "Fundo Teste", Cnpj = "00.000.000/0001-00", CodigoTipo = 99 };
        _tipoFundoCacheServiceMock.Setup(c => c.ExistsAsync(99)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(dto));
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Fundo>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenFundoExists_UpdatesAndReturnsTrue()
    {
        // Arrange
        var fundo = new Fundo { Codigo = "ITAUTESTE01", Nome = "Antigo", Cnpj = "00.000.000/0001-00", CodigoTipo = 1, TipoFundo = new TipoFundo { Codigo = 1, Nome = "Renda Fixa" } };
        var dto = new UpdateFundoDto { Nome = "Novo Nome", Cnpj = "11.111.111/0001-11", CodigoTipo = 2 };
        _repositoryMock.Setup(r => r.GetByCodigoAsync("ITAUTESTE01")).ReturnsAsync(fundo);
        _tipoFundoCacheServiceMock.Setup(c => c.ExistsAsync(2)).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateAsync("ITAUTESTE01", dto);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Fundo>(f => f.Nome == "Novo Nome")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenCodigoTipoDoesNotExist_ThrowsDomainException()
    {
        // Arrange
        var fundo = new Fundo { Codigo = "ITAUTESTE01", Nome = "Antigo", Cnpj = "00.000.000/0001-00", CodigoTipo = 1, TipoFundo = new TipoFundo { Codigo = 1, Nome = "Renda Fixa" } };
        var dto = new UpdateFundoDto { Nome = "Novo Nome", Cnpj = "11.111.111/0001-11", CodigoTipo = 99 };
        _repositoryMock.Setup(r => r.GetByCodigoAsync("ITAUTESTE01")).ReturnsAsync(fundo);
        _tipoFundoCacheServiceMock.Setup(c => c.ExistsAsync(99)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _service.UpdateAsync("ITAUTESTE01", dto));
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Fundo>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenFundoDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByCodigoAsync("NOTFOUND")).ReturnsAsync((Fundo?)null);
        var dto = new UpdateFundoDto { Nome = "Novo", Cnpj = "00.000.000/0001-00", CodigoTipo = 1 };

        // Act
        var result = await _service.UpdateAsync("NOTFOUND", dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenFundoExists_DeletesAndReturnsTrue()
    {
        // Arrange
        var fundo = new Fundo { Codigo = "ITAUTESTE01", TipoFundo = new TipoFundo() };
        _repositoryMock.Setup(r => r.GetByCodigoAsync("ITAUTESTE01")).ReturnsAsync(fundo);

        // Act
        var result = await _service.DeleteAsync("ITAUTESTE01");

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(fundo), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenFundoDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByCodigoAsync("NOTFOUND")).ReturnsAsync((Fundo?)null);

        // Act
        var result = await _service.DeleteAsync("NOTFOUND");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task MovimentarPatrimonioAsync_WhenFundoExists_UpdatesPatrimonioAndReturnsTrue()
    {
        // Arrange
        var fundo = new Fundo { Codigo = "ITAUTESTE01", Patrimonio = 1000m, TipoFundo = new TipoFundo() };
        _repositoryMock.Setup(r => r.GetByCodigoAsync("ITAUTESTE01")).ReturnsAsync(fundo);

        // Act
        var result = await _service.MovimentarPatrimonioAsync("ITAUTESTE01", 500m);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.MovimentarPatrimonioAsync(fundo, 500m), Times.Once);
    }

    [Fact]
    public async Task MovimentarPatrimonioAsync_WhenFundoDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByCodigoAsync("NOTFOUND")).ReturnsAsync((Fundo?)null);

        // Act
        var result = await _service.MovimentarPatrimonioAsync("NOTFOUND", 100m);

        // Assert
        Assert.False(result);
    }
}
