namespace Kessler.API.Middleware;

/// <summary>
/// Adiciona cabeçalhos HTTP de segurança conforme boas práticas (OWASP / ISO 27001).
/// Em desenvolvimento o CSP é relaxado para permitir o Swagger UI funcionar.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _isDevelopment;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _isDevelopment = env.IsDevelopment();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["X-Content-Type-Options"]  = "nosniff";
        headers["X-Frame-Options"]         = "DENY";
        headers["X-XSS-Protection"]        = "1; mode=block";
        headers["Referrer-Policy"]         = "no-referrer";
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        headers["Permissions-Policy"]      = "geolocation=(), microphone=(), camera=()";

        // CSP relaxado em desenvolvimento para o Swagger UI (usa inline scripts/styles)
        // Em produção: CSP estrito sem inline
        headers["Content-Security-Policy"] = _isDevelopment
            ? "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:;"
            : "default-src 'self'";

        context.Response.Headers.Remove("Server");

        await _next(context);
    }
}
