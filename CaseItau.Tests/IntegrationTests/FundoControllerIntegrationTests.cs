using System.Net;
using System.Net.Http.Json;
using CaseItau.Application.DTOs;
using CaseItau.Domain.Entities;
using CaseItau.Domain.ValueObjects;
using CaseItau.Infra.Data;
using Microsoft.Extensions.DependencyInjection;

namespace CaseItau.Tests.IntegrationTests;

/// <summary>
/// Integration tests for <see cref="CaseItau.API.Controllers.FundoController"/>.
/// Covers all endpoints and every documented HTTP response code using a real SQL Server instance
/// provided by Testcontainers.
/// </summary>
public class FundoControllerIntegrationTests : IClassFixture<FundoApiFactory>, IAsyncLifetime
{
    private readonly FundoApiFactory _factory;
    private readonly HttpClient _client;

    public FundoControllerIntegrationTests(FundoApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>Clears the Fundos table before each test to ensure isolation.</summary>
    public async ValueTask InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Fundos.RemoveRange(db.Fundos);
        await db.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/Fundo
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET all returns 200 and a non-empty list when funds exist.</summary>
    [Fact]
    public async Task GetAll_WhenFundsExist_ReturnsOkWithFunds()
    {
        // Arrange
        await SeedFundoAsync("ITAUTESTE01", "00000000000191", 1);

        // Act
        var response = await _client.GetAsync("/api/Fundo", CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fundos = await response.Content.ReadFromJsonAsync<IEnumerable<FundoDto>>();
        Assert.NotNull(fundos);
        Assert.NotEmpty(fundos);
    }

    /// <summary>GET all returns 404 when no funds exist in the database.</summary>
    [Fact]
    public async Task GetAll_WhenNoFundsExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/Fundo", CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/Fundo/{codigo}
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET by codigo returns 200 and the matching fund when it exists.</summary>
    [Fact]
    public async Task GetByCodigo_WhenFundExists_ReturnsOkWithFund()
    {
        // Arrange
        await SeedFundoAsync("ITAUTESTE01", "00000000000191", 1, "Fundo de Renda Fixa");

        // Act
        var response = await _client.GetAsync("/api/Fundo/ITAUTESTE01", CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fundo = await response.Content.ReadFromJsonAsync<FundoDto>();
        Assert.NotNull(fundo);
        Assert.Equal("ITAUTESTE01", fundo.Codigo);
        Assert.Equal("Fundo de Renda Fixa", fundo.Nome);
    }

    /// <summary>GET by codigo returns 404 when no fund matches the given codigo.</summary>
    [Fact]
    public async Task GetByCodigo_WhenFundDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/Fundo/NONEXISTENT", CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/Fundo
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>POST returns 201 Created when the payload is valid.</summary>
    [Fact]
    public async Task Post_WithValidData_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = "ITAUTESTE01",
            Nome = "Fundo de Renda Fixa",
            Cnpj = "00000000000191",
            CodigoTipo = 1,
            Patrimonio = 1_000_000m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Fundo", dto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    /// <summary>POST returns 400 when required string fields are empty.</summary>
    [Fact]
    public async Task Post_WithMissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange – Codigo, Nome and Cnpj are empty, violating [Required]
        var dto = new { Codigo = "", Nome = "", Cnpj = "", CodigoTipo = 1 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Fundo", dto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>POST returns 422 when the Codigo already belongs to another fund.</summary>
    [Fact]
    public async Task Post_WhenCodigoAlreadyExists_ReturnsUnprocessableEntity()
    {
        // Arrange
        await SeedFundoAsync("ITAUTESTE01", "00000000000191", 1);
        var dto = new CreateFundoDto
        {
            Codigo = "ITAUTESTE01",
            Nome = "Outro Fundo",
            Cnpj = "11111111000191",
            CodigoTipo = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Fundo", dto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>POST returns 422 when the CNPJ already belongs to another fund.</summary>
    [Fact]
    public async Task Post_WhenCnpjAlreadyExists_ReturnsUnprocessableEntity()
    {
        // Arrange
        await SeedFundoAsync("ITAUTESTE01", "00000000000191", 1);
        var dto = new CreateFundoDto
        {
            Codigo = "ITAUTESTE02",
            Nome = "Outro Fundo",
            Cnpj = "00000000000191",
            CodigoTipo = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Fundo", dto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>POST returns 422 when the CodigoTipo does not exist.</summary>
    [Fact]
    public async Task Post_WhenCodigoTipoDoesNotExist_ReturnsUnprocessableEntity()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = "ITAUTESTE01",
            Nome = "Fundo Teste",
            Cnpj = "00000000000191",
            CodigoTipo = 999
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Fundo", dto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUT /api/Fundo/{codigo}
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>PUT returns 200 when the fund exists and the payload is valid.</summary>
    [Fact]
    public async Task Put_WhenFundExistsAndDataIsValid_ReturnsOk()
    {
        // Arrange
        await SeedFundoAsync("ITAUTESTE01", "00000000000191", 1);
        var dto = new UpdateFundoDto { Nome = "Fundo Atualizado", Cnpj = "00000000000191", CodigoTipo = 2 };

        // Act
        var response = await _client.PutAsJsonAsync("/api/Fundo/ITAUTESTE01", dto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>PUT returns 404 when no fund matches the given codigo.</summary>
    [Fact]
    public async Task Put_WhenFundDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var dto = new UpdateFundoDto { Nome = "Fundo", Cnpj = "00000000000191", CodigoTipo = 1 };

        // Act
        var response = await _client.PutAsJsonAsync("/api/Fundo/NONEXISTENT", dto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>PUT returns 400 when required string fields are empty.</summary>
    [Fact]
    public async Task Put_WithMissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange – Nome and Cnpj are empty, violating [Required]
        var dto = new { Nome = "", Cnpj = "", CodigoTipo = 1 };

        // Act
        var response = await _client.PutAsJsonAsync("/api/Fundo/ITAUTESTE01", dto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>PUT returns 422 when the new CNPJ is already used by a different fund.</summary>
    [Fact]
    public async Task Put_WhenCnpjBelongsToAnotherFund_ReturnsUnprocessableEntity()
    {
        // Arrange
        await SeedFundoAsync("ITAUTESTE01", "00000000000191", 1);
        await SeedFundoAsync("ITAUTESTE02", "11111111000191", 1);
        var dto = new UpdateFundoDto { Nome = "Atualizado", Cnpj = "11111111000191", CodigoTipo = 1 };

        // Act
        var response = await _client.PutAsJsonAsync("/api/Fundo/ITAUTESTE01", dto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>PUT returns 422 when the CodigoTipo does not exist.</summary>
    [Fact]
    public async Task Put_WhenCodigoTipoDoesNotExist_ReturnsUnprocessableEntity()
    {
        // Arrange
        await SeedFundoAsync("ITAUTESTE01", "00000000000191", 1);
        var dto = new UpdateFundoDto { Nome = "Atualizado", Cnpj = "00000000000191", CodigoTipo = 999 };

        // Act
        var response = await _client.PutAsJsonAsync("/api/Fundo/ITAUTESTE01", dto, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE /api/Fundo/{codigo}
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>DELETE returns 200 when the fund exists and is successfully removed.</summary>
    [Fact]
    public async Task Delete_WhenFundExists_ReturnsOk()
    {
        // Arrange
        await SeedFundoAsync("ITAUTESTE01", "00000000000191", 1);

        // Act
        var response = await _client.DeleteAsync("/api/Fundo/ITAUTESTE01", CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>DELETE returns 404 when no fund matches the given codigo.</summary>
    [Fact]
    public async Task Delete_WhenFundDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/Fundo/NONEXISTENT", CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUT /api/Fundo/{codigo}/patrimonio
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>UpdateFundAssets returns 200 when the fund exists and valor is positive.</summary>
    [Fact]
    public async Task UpdateFundAssets_WhenFundExistsAndValorIsPositive_ReturnsOk()
    {
        // Arrange
        await SeedFundoAsync("ITAUTESTE01", "00000000000191", 1);

        // Act
        var response = await _client.PutAsJsonAsync("/api/Fundo/ITAUTESTE01/patrimonio", 500_000.00m, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>UpdateFundAssets returns 200 when valor is zero (boundary value).</summary>
    [Fact]
    public async Task UpdateFundAssets_WhenValorIsZero_ReturnsOk()
    {
        // Arrange
        await SeedFundoAsync("ITAUTESTE01", "00000000000191", 1);

        // Act
        var response = await _client.PutAsJsonAsync("/api/Fundo/ITAUTESTE01/patrimonio", 0m, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>UpdateFundAssets returns 404 when no fund matches the given codigo.</summary>
    [Fact]
    public async Task UpdateFundAssets_WhenFundDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.PutAsJsonAsync("/api/Fundo/NONEXISTENT/patrimonio", 1_000m, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>UpdateFundAssets returns 422 when valor is negative.</summary>
    [Fact]
    public async Task UpdateFundAssets_WhenValorIsNegative_ReturnsUnprocessableEntity()
    {
        // Arrange
        await SeedFundoAsync("ITAUTESTE01", "00000000000191", 1);

        // Act
        var response = await _client.PutAsJsonAsync("/api/Fundo/ITAUTESTE01/patrimonio", -1m, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private async Task SeedFundoAsync(string codigo, string cnpj, int codigoTipo, string? nome = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Fundos.Add(new Fundo
        {
            Codigo = codigo,
            Nome = nome ?? $"Fundo {codigo}",
            Cnpj = new Cnpj(cnpj),
            CodigoTipo = codigoTipo
        });
        await db.SaveChangesAsync();
    }
}
