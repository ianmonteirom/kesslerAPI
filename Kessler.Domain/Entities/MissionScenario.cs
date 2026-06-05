using Kessler.Domain.Constants;
using Kessler.Domain.Enums;
using Kessler.Domain.Exceptions;

namespace Kessler.Domain.Entities;

public class MissionScenario : SpaceEquipment
{
    public int OrbitalObjectId { get; private set; }
    public OrbitalObject? OrbitalObject { get; private set; }
    public string Objective { get; private set; } = string.Empty;
    public MissionType Type { get; private set; }
    public RiskLevel RiskLevel { get; private set; }
    public int EstimatedDurationDays { get; private set; }
    public double? EstimatedDeltaVMps { get; private set; }
    public DateTime? PlannedStartUtc { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private MissionScenario() { }

    public static MissionScenario Create(
        string name,
        int orbitalObjectId,
        string objective,
        MissionType type,
        RiskLevel riskLevel,
        int estimatedDurationDays,
        DataConfidence confidence,
        double? deltaVMps = null,
        DateTime? plannedStart = null,
        string? summary = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOrbitalDataException(nameof(name), "Nome da missão não pode ser vazio.");

        if (estimatedDurationDays < MissionConstants.MinDurationDays)
            throw new InvalidOrbitalDataException(nameof(estimatedDurationDays),
                $"Duração deve ser de pelo menos {MissionConstants.MinDurationDays} dia.");

        if (estimatedDurationDays > MissionConstants.MaxDurationDays)
            throw new InvalidOrbitalDataException(nameof(estimatedDurationDays),
                $"Duração não pode exceder {MissionConstants.MaxDurationDays} dias.");

        return new MissionScenario
        {
            Name = name,
            OrbitalObjectId = orbitalObjectId,
            Objective = objective,
            Type = type,
            RiskLevel = riskLevel,
            EstimatedDurationDays = estimatedDurationDays,
            EstimatedDeltaVMps = deltaVMps,
            DataConfidence = confidence,
            PlannedStartUtc = plannedStart?.ToUniversalTime(),
            Summary = summary,
            RegisteredAt = DateTime.UtcNow
        };
    }

    public void Complete()
    {
        CompletedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public bool IsOverdue()
    {
        if (PlannedStartUtc is null) return false;
        var deadline = PlannedStartUtc.Value.AddDays(EstimatedDurationDays);
        return DateTime.UtcNow > deadline && CompletedAt is null;
    }

    /// <summary>
    /// Calcula os checkpoints de progresso da missão em intervalos regulares.
    /// Usa while para avançar pela linha do tempo até o prazo final.
    /// </summary>
    public IEnumerable<DateTime> GetProgressCheckpoints(int checkpointIntervalDays = MissionConstants.DefaultCheckpointIntervalDays)
    {
        if (PlannedStartUtc is null)
            return Enumerable.Empty<DateTime>();

        var checkpoints = new List<DateTime>();
        var current = PlannedStartUtc.Value;
        var deadline = current.AddDays(EstimatedDurationDays);

        while (current <= deadline)
        {
            checkpoints.Add(current);
            current = current.AddDays(checkpointIntervalDays);
        }

        return checkpoints;
    }

    public int DaysRemainingUntilDeadline()
    {
        if (PlannedStartUtc is null || CompletedAt is not null) return 0;
        var deadline = PlannedStartUtc.Value.AddDays(EstimatedDurationDays);
        var remaining = (deadline - DateTime.UtcNow).Days;
        return remaining < 0 ? 0 : remaining;
    }

    public override string GetEquipmentCategory() => "Missão Espacial";
}
