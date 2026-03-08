using CaseItau.Application.DTOs;
using CaseItau.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CaseItau.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FundoController(IFundoService fundoService) : ControllerBase
{
    private readonly IFundoService _fundoService = fundoService;

    // GET: api/Fundo
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FundoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get()
    {
        var fundos = await _fundoService.GetAllAsync();

        if (fundos == null || !fundos.Any())
            return NotFound();

        return Ok(fundos);
    }

    // GET: api/Fundo/ITAUTESTE01
    [HttpGet("{codigo}")]
    [ProducesResponseType(typeof(FundoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(string codigo)
    {
        var fundo = await _fundoService.GetByCodigoAsync(codigo);
        return fundo is null ? NotFound() : Ok(fundo);
    }

    // POST: api/Fundo
    [HttpPost]
    [ProducesResponseType(typeof(FundoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Post([FromBody] CreateFundoDto dto)
    {
        await _fundoService.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { codigo = dto.Codigo }, dto);
    }

    // PUT: api/Fundo/ITAUTESTE01
    [HttpPut("{codigo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Put(string codigo, [FromBody] UpdateFundoDto dto)
    {
        var updated = await _fundoService.UpdateAsync(codigo, dto);
        return updated ? Ok() : NotFound();
    }

    // DELETE: api/Fundo/ITAUTESTE01
    [HttpDelete("{codigo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(string codigo)
    {
        var deleted = await _fundoService.DeleteAsync(codigo);
        return deleted ? Ok() : NotFound();
    }

    // PUT: api/Fundo/ITAUTESTE01/patrimonio
    [HttpPut("{codigo}/patrimonio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MovimentarPatrimonio(string codigo, [FromBody] decimal valor)
    {
        var updated = await _fundoService.MovimentarPatrimonioAsync(codigo, valor);
        return updated ? Ok() : NotFound();
    }
}

