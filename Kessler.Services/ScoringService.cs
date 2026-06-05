using Kessler.Application.DTOs;
using Kessler.Domain.Constants;
using Kessler.Domain.Entities;
using Kessler.Domain.Enums;

namespace Kessler.Services;

public static class ScoringService
{
    public static OrbitalObjectScoresDto Calculate(OrbitalObject obj)
    {
        var risk = CalculateRiskScore(obj);
        var forge = CalculateForgeValueScore(obj);
        var priority = CalculatePriorityScore(obj, risk.Score, forge.Score);
        var recommendation = GetRecommendation(risk.Score, forge.Score);

        return new OrbitalObjectScoresDto(
            obj.Id,
            obj.Name,
            risk,
            forge,
            priority,
            recommendation
        );
    }

    private static ScoreResultDto CalculateRiskScore(OrbitalObject obj)
    {
        var factors = new List<ScoreFactorDto>();
        int total = 0;

        int regionPoints = obj.OrbitRegion switch
        {
            OrbitRegion.LEO => 28,
            OrbitRegion.GEO => 18,
            OrbitRegion.MEO => 14,
            OrbitRegion.HEO => 12,
            _ => 0
        };
        factors.Add(new ScoreFactorDto("Congestionamento Orbital", regionPoints, $"Região: {obj.OrbitRegion}"));
        total += regionPoints;

        int statusPoints = obj.Status switch
        {
            ObjectStatus.Fragment => 22,
            ObjectStatus.Inactive => 18,
            ObjectStatus.Active => 5,
            _ => 10
        };
        factors.Add(new ScoreFactorDto("Status do Objeto", statusPoints, $"Status: {obj.Status}"));
        total += statusPoints;

        int typePoints = obj.Type switch
        {
            OrbitalObjectType.Debris => 20,
            OrbitalObjectType.Satellite => 8,
            OrbitalObjectType.RocketBody => 15,
            _ => 10
        };
        factors.Add(new ScoreFactorDto("Tipo de Objeto", typePoints, $"Tipo: {obj.Type}"));
        total += typePoints;

        int massPoints = obj.EstimatedMassKg switch
        {
            >= 5000 => 16,
            >= 1000 => 12,
            >= 100 => 8,
            >= 10 => 5,
            _ => 3
        };
        factors.Add(new ScoreFactorDto("Massa Estimada", massPoints, $"{obj.EstimatedMassKg ?? 0:F0} kg"));
        total += massPoints;

        int confidencePoints = obj.DataConfidence switch
        {
            DataConfidence.Unknown => 10,
            DataConfidence.Simulated => 7,
            DataConfidence.Estimated => 4,
            DataConfidence.Confirmed => 0,
            _ => 5
        };
        factors.Add(new ScoreFactorDto("Confiança nos Dados", confidencePoints, $"Nível: {obj.DataConfidence}"));
        total += confidencePoints;

        total = Math.Clamp(total, 0, 100);
        string level = total >= ScoringConstants.HighRiskThreshold ? "Alto"
                     : total >= ScoringConstants.MediumRiskThreshold ? "Médio"
                     : "Baixo";
        string summary = $"Risco {level.ToUpper()} — score {total}/100. " +
                         (total >= ScoringConstants.HighRiskThreshold ? "Prioridade crítica de intervenção." : "Monitoramento recomendado.");

        return new ScoreResultDto(total, level, summary, factors);
    }

    private static ScoreResultDto CalculateForgeValueScore(OrbitalObject obj)
    {
        var factors = new List<ScoreFactorDto>();
        int total = 0;

        int massPoints = obj.EstimatedMassKg switch
        {
            >= 5000 => 26,
            >= 1000 => 20,
            >= 100 => 14,
            _ => 6
        };
        factors.Add(new ScoreFactorDto("Massa Recuperável", massPoints, $"{obj.EstimatedMassKg ?? 0:F0} kg"));
        total += massPoints;

        int typePoints = obj.Type switch
        {
            OrbitalObjectType.Satellite => 20,
            OrbitalObjectType.RocketBody => 18,
            OrbitalObjectType.Debris => 8,
            _ => 5
        };
        factors.Add(new ScoreFactorDto("Tipo de Objeto", typePoints, $"{obj.Type}"));
        total += typePoints;

        int regionPoints = obj.OrbitRegion switch
        {
            OrbitRegion.LEO => 20,
            OrbitRegion.MEO => 14,
            OrbitRegion.GEO => 8,
            OrbitRegion.HEO => 5,
            _ => 5
        };
        factors.Add(new ScoreFactorDto("Acessibilidade Orbital", regionPoints, $"Região: {obj.OrbitRegion}"));
        total += regionPoints;

        int statusPoints = obj.Status switch
        {
            ObjectStatus.Inactive => 18,
            ObjectStatus.Fragment => 10,
            ObjectStatus.Active => 2,
            _ => 5
        };
        factors.Add(new ScoreFactorDto("Status de Recuperação", statusPoints, $"Status: {obj.Status}"));
        total += statusPoints;

        int confidencePoints = obj.DataConfidence switch
        {
            DataConfidence.Confirmed => 14,
            DataConfidence.Estimated => 9,
            DataConfidence.Simulated => 5,
            DataConfidence.Unknown => 3,
            _ => 3
        };
        factors.Add(new ScoreFactorDto("Confiança nos Dados", confidencePoints, $"{obj.DataConfidence}"));
        total += confidencePoints;

        int penalty = obj.OrbitRegion == OrbitRegion.GEO ? -12 : obj.OrbitRegion == OrbitRegion.HEO ? -8 : -3;
        factors.Add(new ScoreFactorDto("Penalidade de Manuseio", penalty, "Complexidade de acesso"));
        total += penalty;

        total = Math.Clamp(total, 0, 100);
        string level = total >= ScoringConstants.HighForgeValueThreshold ? "Alto"
                     : total >= ScoringConstants.MediumForgeValueThreshold ? "Médio"
                     : "Baixo";

        return new ScoreResultDto(total, level, $"Valor de reaproveitamento {level.ToUpper()}: {total}/100.", factors);
    }

    private static ScoreResultDto CalculatePriorityScore(OrbitalObject obj, int riskScore, int forgeScore)
    {
        double feasibility = obj.OrbitRegion == OrbitRegion.LEO ? ScoringConstants.FeasibilityLeo
                           : obj.OrbitRegion == OrbitRegion.MEO ? ScoringConstants.FeasibilityMeo
                           : ScoringConstants.FeasibilityDefault;
        double composite = (riskScore  * ScoringConstants.PriorityRiskWeight)
                         + (forgeScore * ScoringConstants.PriorityForgeValueWeight)
                         + (feasibility * ScoringConstants.PriorityFeasibilityWeight);
        int total = Math.Clamp((int)Math.Round(composite), 0, 100);

        string level = total >= ScoringConstants.HighRiskThreshold ? "Alta"
                     : total >= ScoringConstants.MediumRiskThreshold ? "Média"
                     : "Baixa";
        var factors = new List<ScoreFactorDto>
        {
            new("Risco Orbital", (int)(riskScore * ScoringConstants.PriorityRiskWeight), "Peso 58%"),
            new("Valor de Reaproveitamento", (int)(forgeScore * ScoringConstants.PriorityForgeValueWeight), "Peso 24%"),
            new("Viabilidade da Missão", (int)(feasibility * ScoringConstants.PriorityFeasibilityWeight), "Peso 18%")
        };

        return new ScoreResultDto(total, level, $"Prioridade {level.ToUpper()}: {total}/100.", factors);
    }

    private static string GetRecommendation(int riskScore, int forgeScore)
    {
        if (riskScore >= ScoringConstants.HighRiskThreshold && forgeScore >= ScoringConstants.HighForgeValueThreshold)
            return "Inspecionar antes da remoção — alto risco e alto valor de reaproveitamento.";

        if (riskScore >= ScoringConstants.ImmediateRemovalThreshold)
            return "Priorizar remoção imediata — risco crítico.";

        if (forgeScore >= ScoringConstants.HighForgeValueThreshold)
            return "Avaliar reaproveitamento — alto potencial de material recuperável.";

        return "Monitoramento contínuo recomendado.";
    }
}
