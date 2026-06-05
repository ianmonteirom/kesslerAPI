namespace Kessler.Domain.Constants;

/// <summary>
/// Pesos e limiares do sistema de pontuação orbital.
/// </summary>
public static class ScoringConstants
{
    // Limiares de nível de risco
    public const int HighRiskThreshold   = 70;
    public const int MediumRiskThreshold = 40;

    // Limiar de valor de reaproveitamento
    public const int HighForgeValueThreshold   = 66;
    public const int MediumForgeValueThreshold = 33;

    // Pesos do score de prioridade composta
    public const double PriorityRiskWeight       = 0.58;
    public const double PriorityForgeValueWeight = 0.24;
    public const double PriorityFeasibilityWeight = 0.18;

    // Viabilidade de missão por região (usado no cálculo de prioridade)
    public const double FeasibilityLeo = 80;
    public const double FeasibilityMeo = 60;
    public const double FeasibilityDefault = 40;

    // Limiar para remoção imediata
    public const int ImmediateRemovalThreshold = 76;
}
