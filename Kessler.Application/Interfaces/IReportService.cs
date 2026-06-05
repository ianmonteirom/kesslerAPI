using Kessler.Application.DTOs;

namespace Kessler.Application.Interfaces;

public interface IReportService
{
    Task<OrbitalRegionReportDto> GetRegionReportAsync(CancellationToken ct = default);
    Task<IEnumerable<AlertWindowDto>> GetAlertWindowsAsync(int objectId, int windowsCount = 6, int intervalHours = 4, CancellationToken ct = default);
    Task<int> GetFleetRiskScoreAsync(CancellationToken ct = default);
}
