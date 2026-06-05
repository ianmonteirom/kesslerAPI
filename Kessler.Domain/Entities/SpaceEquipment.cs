using Kessler.Domain.Enums;

namespace Kessler.Domain.Entities;

/// <summary>
/// Classe base abstrata para todo equipamento/objeto rastreado no espaço.
/// </summary>
public abstract class SpaceEquipment
{
    public int Id { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string? Summary { get; protected set; }
    public DataConfidence DataConfidence { get; protected set; }
    public DateTime RegisteredAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? LastUpdatedAt { get; protected set; }

    public abstract string GetEquipmentCategory();

    public void UpdateTimestamp()
    {
        LastUpdatedAt = DateTime.UtcNow;
    }
}
