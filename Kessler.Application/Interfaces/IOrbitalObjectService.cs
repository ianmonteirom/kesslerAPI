using Kessler.Application.DTOs;
using Kessler.Domain.Enums;

namespace Kessler.Application.Interfaces;

public interface IOrbitalObjectService
{
    Task<IEnumerable<OrbitalObjectDto>> GetAllAsync(CancellationToken ct = default);
    Task<OrbitalObjectDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<OrbitalObjectDto>> GetByRegionAsync(OrbitRegion region, CancellationToken ct = default);
    Task<IEnumerable<OrbitalObjectDto>> GetHighRiskAsync(CancellationToken ct = default);
    Task<OrbitalObjectDto> CreateAsync(CreateOrbitalObjectRequest request, CancellationToken ct = default);
    Task<OrbitalObjectDto> UpdateAsync(int id, UpdateOrbitalObjectRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<OrbitalObjectScoresDto> GetScoresAsync(int id, CancellationToken ct = default);
}
