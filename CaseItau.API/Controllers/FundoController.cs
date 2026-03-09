using CaseItau.Application.DTOs;
using CaseItau.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CaseItau.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FundoController(IFundoService fundoService, ILogger<FundoController> logger) : ControllerBase
{
    private readonly IFundoService _fundoService = fundoService;
    private readonly ILogger<FundoController> _logger = logger;

    // GET: api/Fundo
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FundoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all funds.");
        var fundos = await _fundoService.GetAllAsync(cancellationToken);

        if (fundos == null || !fundos.Any())
        {
            _logger.LogWarning("No funds found.");
            return NotFound();
        }

        return Ok(fundos);
    }

    // GET: api/Fundo/ITAUTESTE01
    [HttpGet("{codigo}")]
    [ProducesResponseType(typeof(FundoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(string codigo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching fund with codigo '{Codigo}'.", codigo);
        var fundo = await _fundoService.GetByCodigoAsync(codigo, cancellationToken);

        if (fundo is null)
        {
            _logger.LogWarning("Fund with codigo '{Codigo}' not found.", codigo);
            return NotFound();
        }

        return Ok(fundo);
    }

    // POST: api/Fundo
    [HttpPost]
    [ProducesResponseType(typeof(FundoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Post([FromBody] CreateFundoDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating fund with codigo '{Codigo}'.", dto.Codigo);
        await _fundoService.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Fund with codigo '{Codigo}' created successfully.", dto.Codigo);
        return CreatedAtAction(nameof(Get), new { codigo = dto.Codigo }, dto);
    }

    // PUT: api/Fundo/ITAUTESTE01
    [HttpPut("{codigo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Put(string codigo, [FromBody] UpdateFundoDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating fund with codigo '{Codigo}'.", codigo);
        var updated = await _fundoService.UpdateAsync(codigo, dto, cancellationToken);

        if (!updated)
        {
            _logger.LogWarning("Fund with codigo '{Codigo}' not found for update.", codigo);
            return NotFound();
        }

        _logger.LogInformation("Fund with codigo '{Codigo}' updated successfully.", codigo);
        return Ok();
    }

    // DELETE: api/Fundo/ITAUTESTE01
    [HttpDelete("{codigo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(string codigo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting fund with codigo '{Codigo}'.", codigo);
        var deleted = await _fundoService.DeleteAsync(codigo, cancellationToken);

        if (!deleted)
        {
            _logger.LogWarning("Fund with codigo '{Codigo}' not found for deletion.", codigo);
            return NotFound();
        }

        _logger.LogInformation("Fund with codigo '{Codigo}' deleted successfully.", codigo);
        return Ok();
    }

    // PUT: api/Fundo/ITAUTESTE01/patrimonio
    [HttpPut("{codigo}/patrimonio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateFundAssets(string codigo, [FromBody] decimal valor, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating fund assets for codigo '{Codigo}' with valor {Valor}.", codigo, valor);
        var updated = await _fundoService.UpdateFundAssetsAsync(codigo, valor, cancellationToken);

        if (!updated)
        {
            _logger.LogWarning("Fund with codigo '{Codigo}' not found for assets update.", codigo);
            return NotFound();
        }

        _logger.LogInformation("Fund assets for codigo '{Codigo}' updated successfully.", codigo);
        return Ok();
    }
}

