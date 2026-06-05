using System.ComponentModel.DataAnnotations;
using Kessler.Domain.Enums;

namespace Kessler.Application.DTOs;

public record OrbitalObjectDto(
    int Id,
    string Name,
    string? NoradId,
    string Type,
    string Status,
    string OrbitRegion,
    double AltitudeKm,
    double InclinationDeg,
    int? LaunchYear,
    double? EstimatedMassKg,
    double? EstimatedSizeM,
    string DataConfidence,
    string? Summary,
    DateTime RegisteredAt,
    DateTime? LastUpdatedAt,
    string EquipmentCategory,
    bool IsHighRisk
);

public record CreateOrbitalObjectRequest(
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [MaxLength(200, ErrorMessage = "Nome não pode exceder 200 caracteres.")]
    [MinLength(2, ErrorMessage = "Nome deve ter pelo menos 2 caracteres.")]
    string Name,

    [MaxLength(20, ErrorMessage = "NORAD ID não pode exceder 20 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9\-]*$", ErrorMessage = "NORAD ID deve conter apenas letras, números e hífens.")]
    string? NoradId,

    [Required] OrbitalObjectType Type,
    [Required] ObjectStatus Status,
    [Required] OrbitRegion OrbitRegion,

    [Range(0, 1_000_000, ErrorMessage = "Altitude deve estar entre 0 e 1.000.000 km.")]
    double AltitudeKm,

    [Range(-180, 180, ErrorMessage = "Inclinação deve estar entre -180° e 180°.")]
    double InclinationDeg,

    [Required] DataConfidence DataConfidence,

    [Range(1957, 2100, ErrorMessage = "Ano de lançamento deve estar entre 1957 e 2100.")]
    int? LaunchYear,

    [Range(0, 1_000_000, ErrorMessage = "Massa estimada deve ser positiva.")]
    double? EstimatedMassKg,

    [Range(0, 10_000, ErrorMessage = "Tamanho estimado deve ser positivo.")]
    double? EstimatedSizeM,

    [MaxLength(1000, ErrorMessage = "Resumo não pode exceder 1000 caracteres.")]
    string? Summary
);

public record UpdateOrbitalObjectRequest(
    [Required] OrbitalObjectType Type,
    [Required] ObjectStatus Status,
    [Required] OrbitRegion OrbitRegion,

    [Range(0, 1_000_000, ErrorMessage = "Altitude deve estar entre 0 e 1.000.000 km.")]
    double AltitudeKm,

    [Range(-180, 180, ErrorMessage = "Inclinação deve estar entre -180° e 180°.")]
    double InclinationDeg,

    [Range(0, 1_000_000, ErrorMessage = "Massa estimada deve ser positiva.")]
    double? EstimatedMassKg,

    [Range(0, 10_000, ErrorMessage = "Tamanho estimado deve ser positivo.")]
    double? EstimatedSizeM,

    [MaxLength(1000, ErrorMessage = "Resumo não pode exceder 1000 caracteres.")]
    string? Summary
);
