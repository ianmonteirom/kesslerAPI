namespace Kessler.Domain.Constants;

/// <summary>
/// Limiares de altitude (km) para classificação de regiões orbitais.
/// </summary>
public static class OrbitConstants
{
    public const double LeoMaxAltitudeKm = 2_000;
    public const double MeoMaxAltitudeKm = 34_000;
    public const double GeoAltitudeKm    = 35_786;
    public const double GeoMaxAltitudeKm = 38_000;

    /// <summary>Altitude crítica dentro da LEO — objetos abaixo deste limite têm risco elevado de colisão.</summary>
    public const double LeoCriticalAltitudeKm = 600;

    /// <summary>Inclinação em graus de uma órbita equatorial.</summary>
    public const double EquatorialInclinationDeg = 0;

    /// <summary>Inclinação típica de órbita polar.</summary>
    public const double PolarInclinationDeg = 90;
}
