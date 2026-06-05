using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Kessler.API.Middleware;

/// <summary>
/// Verifica a integridade do payload via HMAC-SHA256.
/// Quando habilitado (PayloadIntegrity:Enabled = true), requisições POST/PUT/PATCH
/// devem incluir o header X-Payload-Signature com o valor HMAC-SHA256 do corpo.
/// </summary>
public class PayloadIntegrityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PayloadIntegrityMiddleware> _logger;
    private readonly bool _enabled;
    private readonly string? _secret;

    private static readonly HashSet<string> MethodsToVerify = ["POST", "PUT", "PATCH"];

    public PayloadIntegrityMiddleware(
        RequestDelegate next,
        ILogger<PayloadIntegrityMiddleware> logger,
        IConfiguration config)
    {
        _next = next;
        _logger = logger;
        _enabled = config.GetValue<bool>("PayloadIntegrity:Enabled");
        _secret = config["PayloadIntegrity:Secret"];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_enabled && MethodsToVerify.Contains(context.Request.Method.ToUpperInvariant()))
        {
            var signatureHeader = context.Request.Headers["X-Payload-Signature"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(signatureHeader))
            {
                _logger.LogWarning("[INTEGRITY] Header X-Payload-Signature ausente — IP: {IP} | Path: {Path}",
                    context.Connection.RemoteIpAddress, context.Request.Path);
                await RejectAsync(context, "Header X-Payload-Signature obrigatório.");
                return;
            }

            context.Request.EnableBuffering();
            var body = await ReadBodyAsync(context.Request);
            context.Request.Body.Position = 0;

            var expectedSignature = ComputeHmacSha256(body, _secret ?? string.Empty);

            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(signatureHeader),
                    Encoding.UTF8.GetBytes(expectedSignature)))
            {
                _logger.LogWarning("[INTEGRITY] Assinatura inválida — IP: {IP} | Path: {Path}",
                    context.Connection.RemoteIpAddress, context.Request.Path);
                await RejectAsync(context, "Assinatura do payload inválida.");
                return;
            }
        }

        await _next(context);
    }

    private static async Task<string> ReadBodyAsync(HttpRequest request)
    {
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static string ComputeHmacSha256(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static async Task RejectAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = 400,
            error = message,
            timestamp = DateTime.UtcNow
        }));
    }
}
