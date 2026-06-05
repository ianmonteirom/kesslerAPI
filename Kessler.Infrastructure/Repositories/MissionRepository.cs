using Kessler.Domain.Entities;
using Kessler.Domain.Interfaces;
using Kessler.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kessler.Infrastructure.Repositories;

public class MissionRepository : BaseRepository<MissionScenario>, IMissionRepository
{
    public MissionRepository(KesslerDbContext context) : base(context) { }

    public async Task<IEnumerable<MissionScenario>> GetByOrbitalObjectIdAsync(int objectId, CancellationToken ct = default)
        => await _context.MissionScenarios
            .Include(m => m.OrbitalObject)
            .Where(m => m.OrbitalObjectId == objectId)
            .ToListAsync(ct);

    public async Task<IEnumerable<MissionScenario>> GetOverdueAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.MissionScenarios
            .Include(m => m.OrbitalObject)
            .Where(m => m.PlannedStartUtc != null && m.CompletedAt == null)
            .ToListAsync(ct)
            .ContinueWith(t => t.Result.Where(m => m.IsOverdue()), ct);
    }
}
