using Kessler.Domain.Entities;
using Kessler.Domain.Interfaces;
using Kessler.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kessler.Infrastructure.Repositories;

public class ReuseMaterialRepository : BaseRepository<ReuseMaterialEstimate>, IReuseMaterialRepository
{
    public ReuseMaterialRepository(KesslerDbContext context) : base(context) { }

    public async Task<IEnumerable<ReuseMaterialEstimate>> GetByOrbitalObjectIdAsync(int objectId, CancellationToken ct = default)
        => await _context.ReuseMaterialEstimates
            .Include(r => r.OrbitalObject)
            .Where(r => r.OrbitalObjectId == objectId)
            .ToListAsync(ct);
}
