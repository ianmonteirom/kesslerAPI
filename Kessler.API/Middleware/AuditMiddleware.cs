using System.Security.Claims;

namespace Kessler.API.Middleware;

/// <summary>
/// Registra uma trilha de auditoria para todas as operações mutantes (POST, PUT, PATCH, DELETE).
/// Inclui identidade do usuário, IP, endpoint, método e status HTTP da resposta.
/// Dados sensíveis (senhas, tokens) nunca aparecem neste log.
/// </summary>
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    private static readonly HashSet<string> MutatingMethods = ["POST", "PUT", "PATCH", "DELETE"];

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!MutatingMethods.Contains(context.Request.Method.ToUpperInvariant()))
        {
            await _next(context);
            return;
        }

        var startTime = DateTime.UtcNow;

        await _next(context);

        var userId   = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var userRole = context.User.FindFirstValue(ClaimTypes.Role) ?? "none";
        var email    = MaskEmail(context.User.FindFirstValue(ClaimTypes.Email));
        var ip       = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var method   = context.Request.Method;
        var path     = context.Request.Path.Value ?? "/";
        var status   = context.Response.StatusCode;
        var elapsed  = (DateTime.UtcNow - startTime).TotalMilliseconds;

        var level = status >= 500 ? LogLevel.Error
                  : status >= 400 ? LogLevel.Warning
                  : LogLevel.Information;

        _logger.Log(level,
            "[AUDIT] {Method} {Path} → {Status} | UserId: {UserId} | Role: {Role} | Email: {Email} | IP: {IP} | {Elapsed}ms",
            method, path, status, userId, userRole, email, ip, elapsed);

        if (status == 401 || status == 403)
            _logger.LogWarning("[SECURITY] Acesso negado — {Method} {Path} | IP: {IP} | Email: {Email}",
                method, path, ip, email);
    }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email)) return "unknown";
        var at = email.IndexOf('@');
        if (at <= 1) return "***@***";
        return email[0] + new string('*', at - 1) + email[at..];
    }
}
