using Kessler.Application.DTOs;
using Kessler.Application.Interfaces;
using Kessler.Domain.Entities;
using Kessler.Domain.Exceptions;
using Kessler.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Kessler.Services;

public class ReuseMaterialService : IReuseMaterialService
{
    private readonly IReuseMaterialRepository _repo;
    private readonly IOrbitalObjectRepository _orbitalRepo;
    private readonly ILogger<ReuseMaterialService> _logger;

    public ReuseMaterialService(
        IReuseMaterialRepository repo,
        IOrbitalObjectRepository orbitalRepo,
        ILogger<ReuseMaterialService> logger)
    {
        _repo = repo;
        _orbitalRepo = orbitalRepo;
        _logger = logger;
    }

    public async Task<IEnumerable<ReuseMaterialDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repo.GetAllAsync(ct);
        return items.Select(ToDto);
    }

    public async Task<ReuseMaterialDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var item = await _repo.GetByIdAsync(id, ct);
        return item is null ? null : ToDto(item);
    }

    public async Task<IEnumerable<ReuseMaterialDto>> GetByOrbitalObjectAsync(int objectId, CancellationToken ct = default)
    {
        var items = await _repo.GetByOrbitalObjectIdAsync(objectId, ct);
        return items.Select(ToDto);
    }

    public async Task<ReuseMaterialDto> CreateAsync(CreateReuseMaterialRequest request, CancellationToken ct = default)
    {
        _ = await _orbitalRepo.GetByIdAsync(request.OrbitalObjectId, ct)
            ?? throw new OrbitalObjectNotFoundException(request.OrbitalObjectId);

        var entity = ReuseMaterialEstimate.Create(
            request.Name,
            request.OrbitalObjectId,
            request.Material,
            request.RecoveryPotential,
            request.PreferredPath,
            request.EstimatedSharePct,
            request.DataConfidence,
            request.Notes
        );

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Estimativa de reaproveitamento criada: {Name}", entity.Name);
        return ToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new ReuseMaterialNotFoundException(id);

        _repo.Delete(entity);
        await _repo.SaveChangesAsync(ct);
    }

    private static ReuseMaterialDto ToDto(ReuseMaterialEstimate r) => new(
        r.Id,
        r.Name,
        r.OrbitalObjectId,
        r.OrbitalObject?.Name,
        r.Material.ToString(),
        r.RecoveryPotential.ToString(),
        r.PreferredPath.ToString(),
        r.EstimatedSharePct,
        r.DataConfidence.ToString(),
        r.Notes,
        r.RegisteredAt
    );
}
