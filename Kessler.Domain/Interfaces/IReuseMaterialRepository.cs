using Kessler.Domain.Entities;

namespace Kessler.Domain.Interfaces;

public interface IReuseMaterialRepository : IRepository<ReuseMaterialEstimate>
{
    Task<IEnumerable<ReuseMaterialEstimate>> GetByOrbitalObjectIdAsync(int objectId, CancellationToken ct = default);
}
