using Kessler.Domain.Enums;

namespace Kessler.Application.DTOs;

public record OrbitalRegionReportDto(
    Dictionary<OrbitRegion, int> ObjectsPerRegion,
    IEnumerable<string> HighRiskObjects,
    int TotalDebris,
    int TotalSatellites,
    DateTime GeneratedAt
);

public record AlertWindowDto(
    int WindowIndex,
    DateTime StartUtc,
    DateTime EndUtc,
    string AlertLevel,
    string ObjectName,
    string Region
);
