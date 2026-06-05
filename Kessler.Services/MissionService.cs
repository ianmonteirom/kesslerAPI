using Kessler.Application.DTOs;
using Kessler.Application.Interfaces;
using Kessler.Domain.Entities;
using Kessler.Domain.Exceptions;
using Kessler.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Kessler.Services;

public class MissionService : IMissionService
{
    private readonly IMissionRepository _repo;
    private readonly IOrbitalObjectRepository _orbitalRepo;
    private readonly ILogger<MissionService> _logger;

    public MissionService(
        IMissionRepository repo,
        IOrbitalObjectRepository orbitalRepo,
        ILogger<MissionService> logger)
    {
        _repo = repo;
        _orbitalRepo = orbitalRepo;
        _logger = logger;
    }

    public async Task<IEnumerable<MissionDto>> GetAllAsync(CancellationToken ct = default)
    {
        var missions = await _repo.GetAllAsync(ct);
        return missions.Select(ToDto);
    }

    public async Task<MissionDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var m = await _repo.GetByIdAsync(id, ct);
        return m is null ? null : ToDto(m);
    }

    public async Task<IEnumerable<MissionDto>> GetByOrbitalObjectAsync(int objectId, CancellationToken ct = default)
    {
        var missions = await _repo.GetByOrbitalObjectIdAsync(objectId, ct);
        return missions.Select(ToDto);
    }

    public async Task<IEnumerable<MissionDto>> GetOverdueAsync(CancellationToken ct = default)
    {
        var missions = await _repo.GetOverdueAsync(ct);
        return missions.Select(ToDto);
    }

    public async Task<MissionDto> CreateAsync(CreateMissionRequest request, CancellationToken ct = default)
    {
        var orbitalObject = await _orbitalRepo.GetByIdAsync(request.OrbitalObjectId, ct)
            ?? throw new OrbitalObjectNotFoundException(request.OrbitalObjectId);

        var entity = MissionScenario.Create(
            request.Name,
            request.OrbitalObjectId,
            request.Objective,
            request.Type,
            request.RiskLevel,
            request.EstimatedDurationDays,
            request.DataConfidence,
            request.EstimatedDeltaVMps,
            request.PlannedStartUtc,
            request.Summary
        );

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Missão criada: {Name} para objeto {ObjectName}", entity.Name, orbitalObject.Name);
        return ToDto(entity);
    }

    public async Task<MissionDto> CompleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new MissionNotFoundException(id);

        if (entity.CompletedAt is not null)
            throw new MissionAlreadyCompletedException(id);

        entity.Complete();
        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Missão concluída: ID={Id}", id);
        return ToDto(entity);
    }

    public async Task<IEnumerable<DateTime>> GetCheckpointsAsync(int id, int intervalDays = 7, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new MissionNotFoundException(id);

        return entity.GetProgressCheckpoints(intervalDays);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new MissionNotFoundException(id);

        _repo.Delete(entity);
        await _repo.SaveChangesAsync(ct);
    }

    private static MissionDto ToDto(MissionScenario m) => new(
        m.Id,
        m.Name,
        m.OrbitalObjectId,
        m.OrbitalObject?.Name,
        m.Objective,
        m.Type.ToString(),
        m.RiskLevel.ToString(),
        m.EstimatedDurationDays,
        m.EstimatedDeltaVMps,
        m.DataConfidence.ToString(),
        m.Summary,
        m.PlannedStartUtc,
        m.CompletedAt,
        m.RegisteredAt,
        m.IsOverdue()
    );
}
