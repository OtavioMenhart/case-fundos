using CaseItau.Application.DTOs;
using CaseItau.Application.Interfaces;
using CaseItau.Domain.Entities;
using CaseItau.Domain.Exceptions;
using CaseItau.Domain.Interfaces;
using CaseItau.Domain.ValueObjects;
using CaseItau.Infra.Interfaces;
using Microsoft.Extensions.Logging;

namespace CaseItau.Application.Services;

/// <summary>
/// Implements use-case logic for investment funds.
/// </summary>
public class FundoService(IFundoRepository repository, ITipoFundoCacheService tipoFundoCacheService, IUnitOfWork unitOfWork, ILogger<FundoService> logger) : IFundoService
{
    private readonly IFundoRepository _repository = repository;
    private readonly ITipoFundoCacheService _tipoFundoCacheService = tipoFundoCacheService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<FundoService> _logger = logger;

    /// <inheritdoc/>
    public async Task<IEnumerable<FundoDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all funds.");
        var fundos = await _repository.GetAllAsync(cancellationToken);
        _logger.LogInformation("{Count} fund(s) retrieved.", fundos.Count());
        return fundos.Select(MapToDto);
    }

    /// <inheritdoc/>
    public async Task<FundoDto?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving fund with codigo '{Codigo}'.", codigo);
        var fundo = await _repository.GetByCodigoAsync(codigo, cancellationToken);

        if (fundo is null)
        {
            _logger.LogWarning("Fund with codigo '{Codigo}' was not found.", codigo);
            return null;
        }

        return MapToDto(fundo);
    }

    /// <inheritdoc/>
    public async Task CreateAsync(CreateFundoDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating fund with codigo '{Codigo}'.", dto.Codigo);

        if (!await _tipoFundoCacheService.ExistsAsync(dto.CodigoTipo, cancellationToken))
        {
            _logger.LogWarning("CodigoTipo '{CodigoTipo}' does not exist.", dto.CodigoTipo);
            throw new DomainException($"CodigoTipo '{dto.CodigoTipo}' does not exist.");
        }

        var (codigoExists, cnpjExists) = await _repository.CheckDuplicateKeysAsync(dto.Codigo, dto.Cnpj, cancellationToken);
        if (codigoExists)
        {
            _logger.LogWarning("Duplicate codigo '{Codigo}' detected.", dto.Codigo);
            throw new DomainException($"Codigo '{dto.Codigo}' already exists.");
        }
        if (cnpjExists)
        {
            _logger.LogWarning("Duplicate CNPJ '{Cnpj}' detected.", dto.Cnpj);
            throw new DomainException($"Cnpj '{dto.Cnpj}' already exists.");
        }

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
        _logger.LogInformation("Fund with codigo '{Codigo}' created successfully.", dto.Codigo);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(string codigo, UpdateFundoDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating fund with codigo '{Codigo}'.", codigo);

        var (fundo, cnpjTakenByOther) = await _repository.FindForUpdateAsync(codigo, dto.Cnpj, cancellationToken);
        if (fundo is null)
        {
            _logger.LogWarning("Fund with codigo '{Codigo}' was not found for update.", codigo);
            return false;
        }

        if (!await _tipoFundoCacheService.ExistsAsync(dto.CodigoTipo, cancellationToken))
        {
            _logger.LogWarning("CodigoTipo '{CodigoTipo}' does not exist.", dto.CodigoTipo);
            throw new DomainException($"CodigoTipo '{dto.CodigoTipo}' does not exist.");
        }

        if (cnpjTakenByOther)
        {
            _logger.LogWarning("Duplicate CNPJ '{Cnpj}' detected on update for codigo '{Codigo}'.", dto.Cnpj, codigo);
            throw new DomainException($"Cnpj '{dto.Cnpj}' already exists.");
        }

        fundo.Nome = dto.Nome;
        fundo.Cnpj = new Cnpj(dto.Cnpj);
        fundo.CodigoTipo = dto.CodigoTipo;

        _repository.Update(fundo);
        await _unitOfWork.CommitAsync(cancellationToken);
        _logger.LogInformation("Fund with codigo '{Codigo}' updated successfully.", codigo);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string codigo, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting fund with codigo '{Codigo}'.", codigo);

        var fundo = await _repository.GetByCodigoAsync(codigo, cancellationToken);
        if (fundo is null)
        {
            _logger.LogWarning("Fund with codigo '{Codigo}' was not found for deletion.", codigo);
            return false;
        }

        await _repository.DeleteAsync(fundo, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        _logger.LogInformation("Fund with codigo '{Codigo}' deleted successfully.", codigo);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateFundAssetsAsync(string codigo, decimal valor, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating fund assets for codigo '{Codigo}' with valor {Valor}.", codigo, valor);

        var fundo = await _repository.GetByCodigoAsync(codigo, cancellationToken);
        if (fundo is null)
        {
            _logger.LogWarning("Fund with codigo '{Codigo}' was not found for assets update.", codigo);
            return false;
        }

        if (valor < 0)
        {
            _logger.LogWarning("Negative valor {Valor} provided for assets update of codigo '{Codigo}'.", valor, codigo);
            throw new DomainException("Valor must be non-negative.");
        }

        fundo.Patrimonio = valor;
        _repository.Update(fundo);
        await _unitOfWork.CommitAsync(cancellationToken);
        _logger.LogInformation("Fund assets for codigo '{Codigo}' updated successfully to {Valor}.", codigo, valor);
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
