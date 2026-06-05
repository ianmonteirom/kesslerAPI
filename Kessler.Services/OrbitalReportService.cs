using Kessler.Application.DTOs;
using Kessler.Domain.Constants;
using Kessler.Domain.Entities;
using Kessler.Domain.Enums;

namespace Kessler.Services;

public static class OrbitalReportService
{
    public static OrbitalRegionReportDto GenerateRegionReport(IEnumerable<OrbitalObject> objects)
    {
        var regionCounts = new Dictionary<OrbitRegion, int>();
        var highRiskList = new List<string>();
        int totalDebris = 0;
        int totalSatellites = 0;

        foreach (var obj in objects)
        {
            if (!regionCounts.ContainsKey(obj.OrbitRegion))
                regionCounts[obj.OrbitRegion] = 0;

            regionCounts[obj.OrbitRegion]++;

            switch (obj.Type)
            {
                case OrbitalObjectType.Debris:
                    totalDebris++;
                    break;
                case OrbitalObjectType.Satellite:
                    totalSatellites++;
                    break;
            }

            if (obj.IsHighRisk())
                highRiskList.Add($"{obj.Name} [{obj.OrbitRegion}]");
        }

        return new OrbitalRegionReportDto(
            regionCounts,
            highRiskList,
            totalDebris,
            totalSatellites,
            GeneratedAt: DateTime.UtcNow
        );
    }

    public static IEnumerable<AlertWindowDto> GenerateAlertWindows(
        OrbitalObject obj,
        DateTime fromUtc,
        int windowsCount = MissionConstants.DefaultAlertWindowsCount,
        int intervalHours = MissionConstants.DefaultAlertIntervalHours)
    {
        var windows = new List<AlertWindowDto>();

        for (int i = 0; i < windowsCount; i++)
        {
            var windowStart = fromUtc.AddHours(i * intervalHours);
            var windowEnd = windowStart.AddHours(intervalHours);
            bool isHighAttentionWindow = windowStart.Hour >= MissionConstants.HighAttentionWindowStartHour
                                      && windowStart.Hour < MissionConstants.HighAttentionWindowEndHour;

            string alertLevel = obj.IsHighRisk() && isHighAttentionWindow ? "CRÍTICO"
                              : obj.IsHighRisk() ? "ALTO"
                              : "NORMAL";

            windows.Add(new AlertWindowDto(
                WindowIndex: i + 1,
                StartUtc: windowStart,
                EndUtc: windowEnd,
                AlertLevel: alertLevel,
                ObjectName: obj.Name,
                Region: obj.OrbitRegion.ToString()
            ));
        }

        return windows;
    }

    public static int CalculateTotalRiskScore(IEnumerable<OrbitalObject> objects)
    {
        int accumulated = 0;
        int count = 0;

        foreach (var obj in objects)
        {
            var scores = ScoringService.Calculate(obj);
            accumulated += scores.RiskScore.Score;
            count++;
        }

        return count == 0 ? 0 : accumulated / count;
    }
}
