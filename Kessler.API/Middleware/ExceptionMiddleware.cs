using System.Net;
using System.Text.Json;
using Kessler.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Kessler.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OrbitalObjectNotFoundException ex)
        {
            _logger.LogWarning("Objeto orbital não encontrado: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (MissionNotFoundException ex)
        {
            _logger.LogWarning("Missão não encontrada: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ReuseMaterialNotFoundException ex)
        {
            _logger.LogWarning("Estimativa não encontrada: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (DuplicateNoradIdException ex)
        {
            _logger.LogWarning("NORAD ID duplicado: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (MissionAlreadyCompletedException ex)
        {
            _logger.LogWarning("Missão já concluída: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Violação de regra de domínio: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.UnprocessableEntity, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Recurso não encontrado: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning("Valor fora do intervalo permitido: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning("Argumento nulo não permitido: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Argumento inválido: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Operação inválida: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Acesso não autorizado: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao salvar no banco de dados.");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError,
                "Erro ao persistir dados. Verifique as informações enviadas.");
        }
        catch (FormatException ex)
        {
            _logger.LogWarning("Formato inválido: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, "Formato de dado inválido.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não esperado na requisição.");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError,
                "Erro interno do servidor. Por favor, tente novamente.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new
        {
            status = (int)statusCode,
            error = message,
            timestamp = DateTime.UtcNow
        });

        await context.Response.WriteAsync(body);
    }
}
