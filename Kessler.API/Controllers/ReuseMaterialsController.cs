using Kessler.Application.DTOs;
using Kessler.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kessler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ReuseMaterialsController : ControllerBase
{
    private readonly IReuseMaterialService _service;

    public ReuseMaterialsController(IReuseMaterialService service) => _service = service;

    /// <summary>Lista todas as estimativas de reaproveitamento.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReuseMaterialDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    /// <summary>Retorna uma estimativa pelo ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReuseMaterialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Lista estimativas de um objeto orbital específico.</summary>
    [HttpGet("by-object/{objectId:int}")]
    [ProducesResponseType(typeof(IEnumerable<ReuseMaterialDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByObject(int objectId, CancellationToken ct)
        => Ok(await _service.GetByOrbitalObjectAsync(objectId, ct));

    /// <summary>Cria uma nova estimativa de reaproveitamento.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReuseMaterialDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateReuseMaterialRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Remove uma estimativa.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
