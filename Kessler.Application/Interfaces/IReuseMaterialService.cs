using Kessler.Application.DTOs;

namespace Kessler.Application.Interfaces;

public interface IReuseMaterialService
{
    Task<IEnumerable<ReuseMaterialDto>> GetAllAsync(CancellationToken ct = default);
    Task<ReuseMaterialDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ReuseMaterialDto>> GetByOrbitalObjectAsync(int objectId, CancellationToken ct = default);
    Task<ReuseMaterialDto> CreateAsync(CreateReuseMaterialRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
