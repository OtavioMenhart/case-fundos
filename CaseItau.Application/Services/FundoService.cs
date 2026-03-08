using CaseItau.Application.DTOs;
using CaseItau.Application.Interfaces;
using CaseItau.Domain.Entities;
using CaseItau.Domain.Exceptions;
using CaseItau.Domain.Interfaces;
using CaseItau.Domain.ValueObjects;
using CaseItau.Infra.Interfaces;

namespace CaseItau.Application.Services;

/// <summary>
/// Implements use-case logic for investment funds.
/// </summary>
public class FundoService(IFundoRepository repository, ITipoFundoCacheService tipoFundoCacheService, IUnitOfWork unitOfWork) : IFundoService
{
    private readonly IFundoRepository _repository = repository;
    private readonly ITipoFundoCacheService _tipoFundoCacheService = tipoFundoCacheService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <inheritdoc/>
    public async Task<IEnumerable<FundoDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var fundos = await _repository.GetAllAsync(cancellationToken);
        return fundos.Select(MapToDto);
    }

    /// <inheritdoc/>
    public async Task<FundoDto?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken)
    {
        var fundo = await _repository.GetByCodigoAsync(codigo, cancellationToken);
        return fundo is null ? null : MapToDto(fundo);
    }

    /// <inheritdoc/>
    public async Task CreateAsync(CreateFundoDto dto, CancellationToken cancellationToken)
    {
        if (!await _tipoFundoCacheService.ExistsAsync(dto.CodigoTipo, cancellationToken))
            throw new DomainException($"CodigoTipo '{dto.CodigoTipo}' does not exist.");

        var (codigoExists, cnpjExists) = await _repository.CheckDuplicateKeysAsync(dto.Codigo, dto.Cnpj, cancellationToken);
        if (codigoExists)
            throw new DomainException($"Codigo '{dto.Codigo}' already exists.");
        if (cnpjExists)
            throw new DomainException($"Cnpj '{dto.Cnpj}' already exists.");

        var fundo = new Fundo
        {
            Codigo = dto.Codigo,
            Nome = dto.Nome,
            Cnpj = new Cnpj(dto.Cnpj),
            CodigoTipo = dto.CodigoTipo,
            Patrimonio = dto.Patrimonio
        };
        await _repository.AddAsync(fundo, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(string codigo, UpdateFundoDto dto, CancellationToken cancellationToken)
    {
        var (fundo, cnpjTakenByOther) = await _repository.FindForUpdateAsync(codigo, dto.Cnpj, cancellationToken);
        if (fundo is null) return false;

        if (!await _tipoFundoCacheService.ExistsAsync(dto.CodigoTipo, cancellationToken))
            throw new DomainException($"CodigoTipo '{dto.CodigoTipo}' does not exist.");

        if (cnpjTakenByOther)
            throw new DomainException($"Cnpj '{dto.Cnpj}' already exists.");

        fundo.Nome = dto.Nome;
        fundo.Cnpj = new Cnpj(dto.Cnpj);
        fundo.CodigoTipo = dto.CodigoTipo;

        _repository.Update(fundo);
        await _unitOfWork.CommitAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string codigo, CancellationToken cancellationToken)
    {
        var fundo = await _repository.GetByCodigoAsync(codigo, cancellationToken);
        if (fundo is null) return false;

        await _repository.DeleteAsync(fundo, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateFundAssetsAsync(string codigo, decimal valor, CancellationToken cancellationToken)
    {
        var fundo = await _repository.GetByCodigoAsync(codigo, cancellationToken);
        if (fundo is null) return false;

        if (valor < 0) throw new DomainException("Valor must be non-negative.");

        fundo.Patrimonio = valor;
        _repository.Update(fundo);
        await _unitOfWork.CommitAsync(cancellationToken);
        return true;
    }

    private static FundoDto MapToDto(Fundo fundo) => new()
    {
        Codigo = fundo.Codigo,
        Nome = fundo.Nome,
        Cnpj = fundo.Cnpj.Value,
        CodigoTipo = fundo.CodigoTipo,
        NomeTipo = fundo.TipoFundo?.Nome ?? string.Empty,
        Patrimonio = fundo.Patrimonio
    };
}
