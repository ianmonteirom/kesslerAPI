using Kessler.Domain.Entities;
using Kessler.Domain.Enums;

namespace Kessler.Domain.Interfaces;

public interface IOrbitalObjectRepository : IRepository<OrbitalObject>
{
    Task<IEnumerable<OrbitalObject>> GetByRegionAsync(OrbitRegion region, CancellationToken ct = default);
    Task<IEnumerable<OrbitalObject>> GetByTypeAsync(OrbitalObjectType type, CancellationToken ct = default);
    Task<IEnumerable<OrbitalObject>> GetHighRiskAsync(CancellationToken ct = default);
    Task<OrbitalObject?> GetByNoradIdAsync(string noradId, CancellationToken ct = default);
}
