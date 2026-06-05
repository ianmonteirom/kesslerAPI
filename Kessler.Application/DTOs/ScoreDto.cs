namespace Kessler.Application.DTOs;

public record ScoreFactorDto(string Name, int Points, string Description);

public record ScoreResultDto(
    int Score,
    string Level,
    string Summary,
    IEnumerable<ScoreFactorDto> Factors
);

public record OrbitalObjectScoresDto(
    int OrbitalObjectId,
    string OrbitalObjectName,
    ScoreResultDto RiskScore,
    ScoreResultDto ForgeValueScore,
    ScoreResultDto PriorityScore,
    string Recommendation
);
