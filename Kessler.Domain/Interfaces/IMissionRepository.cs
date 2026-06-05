using Kessler.Domain.Entities;

namespace Kessler.Domain.Interfaces;

public interface IMissionRepository : IRepository<MissionScenario>
{
    Task<IEnumerable<MissionScenario>> GetByOrbitalObjectIdAsync(int objectId, CancellationToken ct = default);
    Task<IEnumerable<MissionScenario>> GetOverdueAsync(CancellationToken ct = default);
}
