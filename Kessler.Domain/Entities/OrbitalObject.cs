using Kessler.Domain.Constants;
using Kessler.Domain.Enums;
using Kessler.Domain.Exceptions;

namespace Kessler.Domain.Entities;

public class OrbitalObject : SpaceEquipment
{
    public string? NoradId { get; private set; }
    public OrbitalObjectType Type { get; private set; }
    public ObjectStatus Status { get; private set; }
    public OrbitRegion OrbitRegion { get; private set; }
    public double AltitudeKm { get; private set; }
    public double InclinationDeg { get; private set; }
    public int? LaunchYear { get; private set; }
    public double? EstimatedMassKg { get; private set; }
    public double? EstimatedSizeM { get; private set; }

    public ICollection<MissionScenario> Missions { get; private set; } = new List<MissionScenario>();
    public ICollection<ReuseMaterialEstimate> MaterialEstimates { get; private set; } = new List<ReuseMaterialEstimate>();

    private OrbitalObject() { }

    public static OrbitalObject Create(
        string name,
        OrbitalObjectType type,
        ObjectStatus status,
        OrbitRegion orbitRegion,
        double altitudeKm,
        double inclinationDeg,
        DataConfidence confidence,
        string? noradId = null,
        int? launchYear = null,
        double? massKg = null,
        double? sizeM = null,
        string? summary = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOrbitalDataException(nameof(name), "Nome não pode ser vazio.");

        if (altitudeKm < 0)
            throw new InvalidOrbitalDataException(nameof(altitudeKm), "Altitude não pode ser negativa.");

        return new OrbitalObject
        {
            Name = name,
            NoradId = noradId,
            Type = type,
            Status = status,
            OrbitRegion = orbitRegion,
            AltitudeKm = altitudeKm,
            InclinationDeg = inclinationDeg,
            DataConfidence = confidence,
            LaunchYear = launchYear,
            EstimatedMassKg = massKg,
            EstimatedSizeM = sizeM,
            Summary = summary,
            RegisteredAt = DateTime.UtcNow
        };
    }

    public void Update(
        OrbitalObjectType type,
        ObjectStatus status,
        OrbitRegion orbitRegion,
        double altitudeKm,
        double inclinationDeg,
        double? massKg,
        double? sizeM,
        string? summary)
    {
        Type = type;
        Status = status;
        OrbitRegion = orbitRegion;
        AltitudeKm = altitudeKm;
        InclinationDeg = inclinationDeg;
        EstimatedMassKg = massKg;
        EstimatedSizeM = sizeM;
        Summary = summary;
        UpdateTimestamp();
    }

    public override string GetEquipmentCategory() => Type switch
    {
        OrbitalObjectType.Satellite => "Satélite Artificial",
        OrbitalObjectType.RocketBody => "Corpo de Foguete",
        OrbitalObjectType.Debris => "Detrito Orbital",
        _ => "Desconhecido"
    };

    public bool IsHighRisk()
    {
        return Status == ObjectStatus.Fragment
            || (OrbitRegion == OrbitRegion.LEO && AltitudeKm < OrbitConstants.LeoCriticalAltitudeKm)
            || Type == OrbitalObjectType.Debris;
    }
}
