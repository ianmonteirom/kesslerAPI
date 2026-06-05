using Kessler.Application.DTOs;
using Kessler.Application.Interfaces;
using Kessler.Domain.Exceptions;
using Kessler.Domain.Interfaces;

namespace Kessler.Services;

public class ReportService : IReportService
{
    private readonly IOrbitalObjectRepository _repo;

    public ReportService(IOrbitalObjectRepository repo) => _repo = repo;

    public async Task<OrbitalRegionReportDto> GetRegionReportAsync(CancellationToken ct = default)
    {
        var objects = await _repo.GetAllAsync(ct);
        return OrbitalReportService.GenerateRegionReport(objects);
    }

    public async Task<IEnumerable<AlertWindowDto>> GetAlertWindowsAsync(
        int objectId,
        int windowsCount = 6,
        int intervalHours = 4,
        CancellationToken ct = default)
    {
        var obj = await _repo.GetByIdAsync(objectId, ct)
            ?? throw new OrbitalObjectNotFoundException(objectId);

        return OrbitalReportService.GenerateAlertWindows(obj, DateTime.UtcNow, windowsCount, intervalHours);
    }

    public async Task<int> GetFleetRiskScoreAsync(CancellationToken ct = default)
    {
        var objects = await _repo.GetAllAsync(ct);
        return OrbitalReportService.CalculateTotalRiskScore(objects);
    }
}
