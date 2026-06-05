using Kessler.Domain.Enums;

namespace Kessler.Domain.Entities;

public class ReuseMaterialEstimate : SpaceEquipment
{
    public int OrbitalObjectId { get; private set; }
    public OrbitalObject? OrbitalObject { get; private set; }
    public MaterialType Material { get; private set; }
    public RiskLevel RecoveryPotential { get; private set; }
    public RecoveryPath PreferredPath { get; private set; }
    public double EstimatedSharePct { get; private set; }
    public string? Notes { get; private set; }

    private ReuseMaterialEstimate() { }

    public static ReuseMaterialEstimate Create(
        string name,
        int orbitalObjectId,
        MaterialType material,
        RiskLevel recoveryPotential,
        RecoveryPath preferredPath,
        double estimatedSharePct,
        DataConfidence confidence,
        string? notes = null)
    {
        if (estimatedSharePct is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(estimatedSharePct), "Porcentagem deve estar entre 0 e 100.");

        return new ReuseMaterialEstimate
        {
            Name = name,
            OrbitalObjectId = orbitalObjectId,
            Material = material,
            RecoveryPotential = recoveryPotential,
            PreferredPath = preferredPath,
            EstimatedSharePct = estimatedSharePct,
            DataConfidence = confidence,
            Notes = notes,
            RegisteredAt = DateTime.UtcNow
        };
    }

    public override string GetEquipmentCategory() => "Estimativa de Reaproveitamento";
}
