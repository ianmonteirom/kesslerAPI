using Kessler.Application.DTOs;
using Kessler.Application.Interfaces;
using Kessler.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kessler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OrbitalObjectsController : ControllerBase
{
    private readonly IOrbitalObjectService _service;

    public OrbitalObjectsController(IOrbitalObjectService service) => _service = service;

    /// <summary>Lista todos os objetos orbitais cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrbitalObjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    /// <summary>Retorna um objeto orbital pelo ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrbitalObjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var obj = await _service.GetByIdAsync(id, ct);
        return obj is null ? NotFound() : Ok(obj);
    }

    /// <summary>Filtra objetos orbitais por região orbital (LEO, MEO, GEO, HEO).</summary>
    [HttpGet("region/{region}")]
    [ProducesResponseType(typeof(IEnumerable<OrbitalObjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRegion(OrbitRegion region, CancellationToken ct)
        => Ok(await _service.GetByRegionAsync(region, ct));

    /// <summary>Retorna objetos de alto risco orbital.</summary>
    [HttpGet("high-risk")]
    [ProducesResponseType(typeof(IEnumerable<OrbitalObjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHighRisk(CancellationToken ct)
        => Ok(await _service.GetHighRiskAsync(ct));

    /// <summary>Retorna os scores de risco, reaproveitamento e prioridade do objeto.</summary>
    [HttpGet("{id:int}/scores")]
    [ProducesResponseType(typeof(OrbitalObjectScoresDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScores(int id, CancellationToken ct)
        => Ok(await _service.GetScoresAsync(id, ct));

    /// <summary>Cadastra um novo objeto orbital.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrbitalObjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrbitalObjectRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza um objeto orbital existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(OrbitalObjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrbitalObjectRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    /// <summary>Remove um objeto orbital.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
