using System.ComponentModel.DataAnnotations;
using Kessler.Domain.Enums;

namespace Kessler.Application.DTOs;

public record ReuseMaterialDto(
    int Id,
    string Name,
    int OrbitalObjectId,
    string? OrbitalObjectName,
    string Material,
    string RecoveryPotential,
    string PreferredPath,
    double EstimatedSharePct,
    string DataConfidence,
    string? Notes,
    DateTime RegisteredAt
);

public record CreateReuseMaterialRequest(
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [MaxLength(200, ErrorMessage = "Nome não pode exceder 200 caracteres.")]
    [MinLength(2, ErrorMessage = "Nome deve ter pelo menos 2 caracteres.")]
    string Name,

    [Range(1, int.MaxValue, ErrorMessage = "OrbitalObjectId deve ser um ID válido.")]
    int OrbitalObjectId,

    [Required] MaterialType Material,
    [Required] RiskLevel RecoveryPotential,
    [Required] RecoveryPath PreferredPath,

    [Range(0.0, 100.0, ErrorMessage = "Percentual estimado deve estar entre 0 e 100.")]
    double EstimatedSharePct,

    [Required] DataConfidence DataConfidence,

    [MaxLength(500, ErrorMessage = "Notas não podem exceder 500 caracteres.")]
    string? Notes
);
