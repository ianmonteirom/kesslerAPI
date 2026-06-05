using Kessler.Application.DTOs;
using Kessler.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kessler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class MissionsController : ControllerBase
{
    private readonly IMissionService _service;

    public MissionsController(IMissionService service) => _service = service;

    /// <summary>Lista todas as missões cadastradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    /// <summary>Retorna uma missão pelo ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var m = await _service.GetByIdAsync(id, ct);
        return m is null ? NotFound() : Ok(m);
    }

    /// <summary>Lista missões vinculadas a um objeto orbital.</summary>
    [HttpGet("by-object/{objectId:int}")]
    [ProducesResponseType(typeof(IEnumerable<MissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByObject(int objectId, CancellationToken ct)
        => Ok(await _service.GetByOrbitalObjectAsync(objectId, ct));

    /// <summary>Retorna missões com prazo vencido.</summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IEnumerable<MissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdue(CancellationToken ct)
        => Ok(await _service.GetOverdueAsync(ct));

    /// <summary>Cria uma nova missão.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MissionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMissionRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Marca uma missão como concluída.</summary>
    [HttpPatch("{id:int}/complete")]
    [ProducesResponseType(typeof(MissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(int id, CancellationToken ct)
        => Ok(await _service.CompleteAsync(id, ct));

    /// <summary>Retorna os checkpoints de progresso de uma missão.</summary>
    [HttpGet("{id:int}/checkpoints")]
    [ProducesResponseType(typeof(IEnumerable<DateTime>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCheckpoints(
        int id,
        [FromQuery] int intervalDays = 7,
        CancellationToken ct = default)
        => Ok(await _service.GetCheckpointsAsync(id, intervalDays, ct));

    /// <summary>Remove uma missão.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
