using System.ComponentModel.DataAnnotations;

namespace Kessler.Application.DTOs;

public record RegisterRequest(
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [MaxLength(100, ErrorMessage = "Nome não pode exceder 100 caracteres.")]
    [MinLength(2, ErrorMessage = "Nome deve ter pelo menos 2 caracteres.")]
    string Name,

    [Required(ErrorMessage = "E-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
    [MaxLength(200, ErrorMessage = "E-mail não pode exceder 200 caracteres.")]
    string Email,

    [Required(ErrorMessage = "Senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres.")]
    [MaxLength(128, ErrorMessage = "Senha não pode exceder 128 caracteres.")]
    string Password,

    [RegularExpression("^(Admin|Operator)$", ErrorMessage = "Role deve ser 'Admin' ou 'Operator'.")]
    string Role = "Operator"
);

public record LoginRequest(
    [Required(ErrorMessage = "E-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
    [MaxLength(200)]
    string Email,

    [Required(ErrorMessage = "Senha é obrigatória.")]
    [MaxLength(128)]
    string Password
);

public record AuthResponse(string Token, string Email, string Name, string Role, DateTime ExpiresAt);
