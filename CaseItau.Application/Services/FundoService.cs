using CaseItau.Application.DTOs;
using CaseItau.Application.Interfaces;
using CaseItau.Domain.Entities;
using CaseItau.Domain.Exceptions;
using CaseItau.Domain.Interfaces;

namespace CaseItau.Application.Services;

/// <summary>
/// Implements use-case logic for investment funds.
/// </summary>
public class FundoService(IFundoRepository repository, ITipoFundoCacheService tipoFundoCacheService) : IFundoService
{
    private readonly IFundoRepository _repository = repository;
    private readonly ITipoFundoCacheService _tipoFundoCacheService = tipoFundoCacheService;

    /// <inheritdoc/>
    public async Task<IEnumerable<FundoDto>> GetAllAsync()
    {
        var fundos = await _repository.GetAllAsync();
        return fundos.Select(MapToDto);
    }

    /// <inheritdoc/>
    public async Task<FundoDto?> GetByCodigoAsync(string codigo)
    {
        var fundo = await _repository.GetByCodigoAsync(codigo);
        return fundo is null ? null : MapToDto(fundo);
    }

    /// <inheritdoc/>
    public async Task CreateAsync(CreateFundoDto dto)
    {
        if (!await _tipoFundoCacheService.ExistsAsync(dto.CodigoTipo))
            throw new DomainException($"CodigoTipo '{dto.CodigoTipo}' does not exist.");

        var fundo = new Fundo
        {
            Codigo = dto.Codigo,
            Nome = dto.Nome,
            Cnpj = dto.Cnpj,
            CodigoTipo = dto.CodigoTipo,
            Patrimonio = dto.Patrimonio
        };
        await _repository.AddAsync(fundo);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(string codigo, UpdateFundoDto dto)
    {
        var fundo = await _repository.GetByCodigoAsync(codigo);
        if (fundo is null) return false;

        if (!await _tipoFundoCacheService.ExistsAsync(dto.CodigoTipo))
            throw new DomainException($"CodigoTipo '{dto.CodigoTipo}' does not exist.");

        fundo.Nome = dto.Nome;
        fundo.Cnpj = dto.Cnpj;
        fundo.CodigoTipo = dto.CodigoTipo;

        await _repository.UpdateAsync(fundo);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string codigo)
    {
        var fundo = await _repository.GetByCodigoAsync(codigo);
        if (fundo is null) return false;

        await _repository.DeleteAsync(fundo);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> MovimentarPatrimonioAsync(string codigo, decimal valor)
    {
        var fundo = await _repository.GetByCodigoAsync(codigo);
        if (fundo is null) return false;

        await _repository.MovimentarPatrimonioAsync(fundo, valor);
        return true;
    }

    private static FundoDto MapToDto(Fundo fundo) => new()
    {
        Codigo = fundo.Codigo,
        Nome = fundo.Nome,
        Cnpj = fundo.Cnpj,
        CodigoTipo = fundo.CodigoTipo,
        NomeTipo = fundo.TipoFundo?.Nome ?? string.Empty,
        Patrimonio = fundo.Patrimonio
    };
}
