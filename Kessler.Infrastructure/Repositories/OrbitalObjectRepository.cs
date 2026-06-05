using Kessler.Domain.Constants;
using Kessler.Domain.Entities;
using Kessler.Domain.Enums;
using Kessler.Domain.Interfaces;
using Kessler.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kessler.Infrastructure.Repositories;

public class OrbitalObjectRepository : BaseRepository<OrbitalObject>, IOrbitalObjectRepository
{
    public OrbitalObjectRepository(KesslerDbContext context) : base(context) { }

    public async Task<IEnumerable<OrbitalObject>> GetByRegionAsync(OrbitRegion region, CancellationToken ct = default)
        => await _context.OrbitalObjects
            .Where(o => o.OrbitRegion == region)
            .ToListAsync(ct);

    public async Task<IEnumerable<OrbitalObject>> GetByTypeAsync(OrbitalObjectType type, CancellationToken ct = default)
        => await _context.OrbitalObjects
            .Where(o => o.Type == type)
            .ToListAsync(ct);

    public async Task<IEnumerable<OrbitalObject>> GetHighRiskAsync(CancellationToken ct = default)
        => await _context.OrbitalObjects
            .Where(o => o.Status == ObjectStatus.Fragment
                     || o.Type == OrbitalObjectType.Debris
                     || (o.OrbitRegion == OrbitRegion.LEO && o.AltitudeKm < OrbitConstants.LeoCriticalAltitudeKm))
            .ToListAsync(ct);

    public async Task<OrbitalObject?> GetByNoradIdAsync(string noradId, CancellationToken ct = default)
        => await _context.OrbitalObjects
            .FirstOrDefaultAsync(o => o.NoradId == noradId, ct);
}
