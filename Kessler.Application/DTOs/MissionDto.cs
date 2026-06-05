using System.ComponentModel.DataAnnotations;
using Kessler.Domain.Enums;

namespace Kessler.Application.DTOs;

public record MissionDto(
    int Id,
    string Name,
    int OrbitalObjectId,
    string? OrbitalObjectName,
    string Objective,
    string Type,
    string RiskLevel,
    int EstimatedDurationDays,
    double? EstimatedDeltaVMps,
    string DataConfidence,
    string? Summary,
    DateTime? PlannedStartUtc,
    DateTime? CompletedAt,
    DateTime RegisteredAt,
    bool IsOverdue
);

public record CreateMissionRequest(
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [MaxLength(200, ErrorMessage = "Nome não pode exceder 200 caracteres.")]
    [MinLength(2, ErrorMessage = "Nome deve ter pelo menos 2 caracteres.")]
    string Name,

    [Range(1, int.MaxValue, ErrorMessage = "OrbitalObjectId deve ser um ID válido.")]
    int OrbitalObjectId,

    [Required(ErrorMessage = "Objetivo é obrigatório.")]
    [MaxLength(500, ErrorMessage = "Objetivo não pode exceder 500 caracteres.")]
    [MinLength(5, ErrorMessage = "Objetivo deve ter pelo menos 5 caracteres.")]
    string Objective,

    [Required] MissionType Type,
    [Required] RiskLevel RiskLevel,

    [Range(1, 3650, ErrorMessage = "Duração estimada deve ser entre 1 e 3650 dias.")]
    int EstimatedDurationDays,

    [Required] DataConfidence DataConfidence,

    [Range(0, 100_000, ErrorMessage = "Delta-V estimado deve ser positivo.")]
    double? EstimatedDeltaVMps,

    DateTime? PlannedStartUtc,

    [MaxLength(1000, ErrorMessage = "Resumo não pode exceder 1000 caracteres.")]
    string? Summary
);
