using Kessler.Application.DTOs;
using Kessler.Application.Interfaces;
using Kessler.Domain.Entities;
using Kessler.Domain.Enums;
using Kessler.Domain.Exceptions;
using Kessler.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Kessler.Services;

public class OrbitalObjectService : IOrbitalObjectService
{
    private readonly IOrbitalObjectRepository _repo;
    private readonly ILogger<OrbitalObjectService> _logger;

    public OrbitalObjectService(IOrbitalObjectRepository repo, ILogger<OrbitalObjectService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<OrbitalObjectDto>> GetAllAsync(CancellationToken ct = default)
    {
        var objects = await _repo.GetAllAsync(ct);
        return objects.Select(ToDto);
    }

    public async Task<OrbitalObjectDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var obj = await _repo.GetByIdAsync(id, ct);
        return obj is null ? null : ToDto(obj);
    }

    public async Task<IEnumerable<OrbitalObjectDto>> GetByRegionAsync(OrbitRegion region, CancellationToken ct = default)
    {
        var objects = await _repo.GetByRegionAsync(region, ct);
        return objects.Select(ToDto);
    }

    public async Task<IEnumerable<OrbitalObjectDto>> GetHighRiskAsync(CancellationToken ct = default)
    {
        var objects = await _repo.GetHighRiskAsync(ct);
        return objects.Select(ToDto);
    }

    public async Task<OrbitalObjectDto> CreateAsync(CreateOrbitalObjectRequest request, CancellationToken ct = default)
    {
        if (request.NoradId is not null)
        {
            var existing = await _repo.GetByNoradIdAsync(request.NoradId, ct);
            if (existing is not null)
                throw new DuplicateNoradIdException(request.NoradId);
        }

        var entity = OrbitalObject.Create(
            request.Name,
            request.Type,
            request.Status,
            request.OrbitRegion,
            request.AltitudeKm,
            request.InclinationDeg,
            request.DataConfidence,
            request.NoradId,
            request.LaunchYear,
            request.EstimatedMassKg,
            request.EstimatedSizeM,
            request.Summary
        );

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Objeto orbital criado: {Name} (ID={Id})", entity.Name, entity.Id);
        return ToDto(entity);
    }

    public async Task<OrbitalObjectDto> UpdateAsync(int id, UpdateOrbitalObjectRequest request, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new OrbitalObjectNotFoundException(id);

        entity.Update(
            request.Type,
            request.Status,
            request.OrbitRegion,
            request.AltitudeKm,
            request.InclinationDeg,
            request.EstimatedMassKg,
            request.EstimatedSizeM,
            request.Summary
        );

        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Objeto orbital atualizado: ID={Id}", id);
        return ToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new OrbitalObjectNotFoundException(id);

        _repo.Delete(entity);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Objeto orbital removido: ID={Id}", id);
    }

    public async Task<OrbitalObjectScoresDto> GetScoresAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new OrbitalObjectNotFoundException(id);

        return ScoringService.Calculate(entity);
    }

    private static OrbitalObjectDto ToDto(OrbitalObject o) => new(
        o.Id,
        o.Name,
        o.NoradId,
        o.Type.ToString(),
        o.Status.ToString(),
        o.OrbitRegion.ToString(),
        o.AltitudeKm,
        o.InclinationDeg,
        o.LaunchYear,
        o.EstimatedMassKg,
        o.EstimatedSizeM,
        o.DataConfidence.ToString(),
        o.Summary,
        o.RegisteredAt,
        o.LastUpdatedAt,
        o.GetEquipmentCategory(),
        o.IsHighRisk()
    );
}
