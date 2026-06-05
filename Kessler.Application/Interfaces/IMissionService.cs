using Kessler.Application.DTOs;

namespace Kessler.Application.Interfaces;

public interface IMissionService
{
    Task<IEnumerable<MissionDto>> GetAllAsync(CancellationToken ct = default);
    Task<MissionDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<MissionDto>> GetByOrbitalObjectAsync(int objectId, CancellationToken ct = default);
    Task<IEnumerable<MissionDto>> GetOverdueAsync(CancellationToken ct = default);
    Task<MissionDto> CreateAsync(CreateMissionRequest request, CancellationToken ct = default);
    Task<MissionDto> CompleteAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<DateTime>> GetCheckpointsAsync(int id, int intervalDays = 7, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
