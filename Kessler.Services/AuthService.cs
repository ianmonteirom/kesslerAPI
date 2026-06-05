using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Kessler.Application.DTOs;
using Kessler.Application.Interfaces;
using Kessler.Domain.Constants;
using Kessler.Domain.Entities;
using Kessler.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Kessler.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository repo, IConfiguration config, ILogger<AuthService> logger)
    {
        _repo = repo;
        _config = config;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        ValidatePasswordComplexity(request.Password);

        var existing = await _repo.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            throw new InvalidOperationException("E-mail já cadastrado.");

        string hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.Name, request.Email, hash, request.Role);

        await _repo.AddAsync(user, ct);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("[AUTH] Novo usuário registrado — Email: {Email} | Role: {Role}",
            MaskEmail(user.Email), user.Role);

        return GenerateToken(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _repo.GetByEmailAsync(request.Email, ct);

        if (user is null)
        {
            _logger.LogWarning("[AUTH] Tentativa de login com e-mail inexistente — Email: {Email}",
                MaskEmail(request.Email));
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("[AUTH] Tentativa de login em conta desativada — Email: {Email}",
                MaskEmail(request.Email));
            throw new UnauthorizedAccessException("Conta desativada.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("[AUTH] Falha de autenticação — senha incorreta — Email: {Email}",
                MaskEmail(request.Email));
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        user.RecordLogin();
        _repo.Update(user);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("[AUTH] Login bem-sucedido — Email: {Email} | Role: {Role} | Horário: {Time}",
            MaskEmail(user.Email), user.Role, DateTime.UtcNow);

        return GenerateToken(user);
    }

    private static void ValidatePasswordComplexity(string password)
    {
        if (password.Length < SecurityConstants.MinPasswordLength)
            throw new ArgumentException($"Senha deve ter no mínimo {SecurityConstants.MinPasswordLength} caracteres.");
        if (!password.Any(char.IsUpper))
            throw new ArgumentException("Senha deve conter pelo menos uma letra maiúscula.");
        if (!password.Any(char.IsLower))
            throw new ArgumentException("Senha deve conter pelo menos uma letra minúscula.");
        if (!password.Any(char.IsDigit))
            throw new ArgumentException("Senha deve conter pelo menos um número.");
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]"))
            throw new ArgumentException("Senha deve conter pelo menos um caractere especial.");
    }

    /// <summary>Mascara e-mail para logs — ex: ian@gmail.com → i**@gmail.com</summary>
    internal static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return "***@***";
        return email[0] + new string('*', at - 1) + email[at..];
    }

    private AuthResponse GenerateToken(User user)
    {
        var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada.");
        var issuer = _config["Jwt:Issuer"] ?? "kesslerAPI";
        var audience = _config["Jwt:Audience"] ?? "kesslerAPI";
        int expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out int m) ? m : 60;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Email,
            user.Name,
            user.Role,
            expires
        );
    }
}
