using Kessler.Application.DTOs;
using Kessler.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kessler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    /// <summary>Relatório consolidado por região orbital (usa foreach internamente).</summary>
    [HttpGet("region-summary")]
    [ProducesResponseType(typeof(OrbitalRegionReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegionReport(CancellationToken ct)
        => Ok(await _service.GetRegionReportAsync(ct));

    /// <summary>Gera janelas temporais de alerta para um objeto (usa for internamente).</summary>
    [HttpGet("alert-windows/{objectId:int}")]
    [ProducesResponseType(typeof(IEnumerable<AlertWindowDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAlertWindows(
        int objectId,
        [FromQuery] int windowsCount = 6,
        [FromQuery] int intervalHours = 4,
        CancellationToken ct = default)
        => Ok(await _service.GetAlertWindowsAsync(objectId, windowsCount, intervalHours, ct));

    /// <summary>Score médio de risco de toda a frota orbital.</summary>
    [HttpGet("fleet-risk-score")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFleetRiskScore(CancellationToken ct)
    {
        var avgScore = await _service.GetFleetRiskScoreAsync(ct);
        return Ok(new { AverageRiskScore = avgScore, GeneratedAt = DateTime.UtcNow });
    }
}
