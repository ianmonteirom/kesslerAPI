using System.Text;
using System.Text.Json;

namespace Kessler.API.Middleware;

/// <summary>
/// Detecta padrões de ataque (XSS, SQL Injection, command injection) no corpo da requisição
/// antes que o payload chegue aos controllers.
/// </summary>
public class SanitizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SanitizationMiddleware> _logger;

    private static readonly string[] XssPatterns =
    [
        "<script", "</script>", "javascript:", "vbscript:", "onload=", "onerror=",
        "onclick=", "onmouseover=", "<iframe", "<object", "<embed", "eval(",
        "document.cookie", "document.write", "window.location", "<img src="
    ];

    private static readonly string[] SqlInjectionPatterns =
    [
        "'; --", "\"; --", "' OR '1'='1", "\" OR \"1\"=\"1",
        "UNION SELECT", "DROP TABLE", "DROP DATABASE", "TRUNCATE TABLE",
        "INSERT INTO", "DELETE FROM", "UPDATE SET", "xp_cmdshell",
        "EXEC(", "EXECUTE(", "EXEC (", "EXECUTE (", "sp_executesql",
        "INFORMATION_SCHEMA", "sysobjects", "syscolumns"
    ];

    private static readonly string[] CommandInjectionPatterns =
    [
        "; rm -", "; del ", "| /bin/sh", "| /bin/bash", "&& curl ",
        "&& wget ", "$(curl", "$(wget", "`curl", "`wget", "; cat /etc/passwd",
        "| cat /etc/", "/etc/shadow", "/proc/self"
    ];

    public SanitizationMiddleware(RequestDelegate next, ILogger<SanitizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsJsonRequest(context) && HasBody(context))
        {
            context.Request.EnableBuffering();

            var body = await ReadBodyAsync(context.Request);

            if (!string.IsNullOrWhiteSpace(body))
            {
                var attackType = DetectAttack(body);
                if (attackType is not null)
                {
                    _logger.LogWarning(
                        "Padrão de ataque detectado [{AttackType}] — IP: {IP} | Path: {Path}",
                        attackType,
                        context.Connection.RemoteIpAddress,
                        context.Request.Path);

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        status = 400,
                        error = "Entrada inválida detectada. Requisição rejeitada.",
                        timestamp = DateTime.UtcNow
                    }));
                    return;
                }
            }

            context.Request.Body.Position = 0;
        }

        await _next(context);
    }

    private static bool IsJsonRequest(HttpContext context) =>
        context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;

    private static bool HasBody(HttpContext context) =>
        context.Request.ContentLength is > 0 || context.Request.Headers.TransferEncoding == "chunked";

    private static async Task<string> ReadBodyAsync(HttpRequest request)
    {
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static string? DetectAttack(string body)
    {
        var upper = body.ToUpperInvariant();

        foreach (var pattern in XssPatterns)
            if (upper.Contains(pattern.ToUpperInvariant()))
                return "XSS";

        foreach (var pattern in SqlInjectionPatterns)
            if (upper.Contains(pattern.ToUpperInvariant()))
                return "SQL_INJECTION";

        foreach (var pattern in CommandInjectionPatterns)
            if (body.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return "COMMAND_INJECTION";

        return null;
    }
}
