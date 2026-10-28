using SGCAE.Application.Common.Exceptions;
using SGCAE.Domain.Exceptions;

namespace SGCAE.Api.Middleware;

/// <summary>
/// Traduce excepciones de dominio/aplicación a respuestas ProblemDetails (RFC 7807):
///  - DomainException                     → 400 Bad Request
///  - ValidacionException                 → 400 Bad Request con detalle por campo
///  - NotFoundException                   → 404 Not Found
///  - ConflictoException                  → 409 Conflict
///  - cualquier otra excepción            → 500 Internal Server Error
/// </summary>
public sealed class ManejadorExcepcionesMiddleware(RequestDelegate next, ILogger<ManejadorExcepcionesMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (estado, titulo) = exception switch
            {
                ValidacionException => (StatusCodes.Status400BadRequest, "Validación de entrada"),
                DomainException => (StatusCodes.Status400BadRequest, "Regla de negocio"),
                NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                ConflictoException => (StatusCodes.Status409Conflict, "Conflicto con el estado actual"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
            };

            if (estado is StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Excepción no controlada en {Ruta}", context.Request.Path);
            else
                logger.LogWarning("Solicitud rechazada en {Ruta}: {Mensaje}", context.Request.Path, exception.Message);

            await EscribirRespuestaAsync(context, exception, estado, titulo);
        }
    }

    private static async Task EscribirRespuestaAsync(
        HttpContext context, Exception exception, int estado, string titulo)
    {
        context.Response.StatusCode = estado;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            type = $"https://sgcae.ejemplo.com/errores/{estado}",
            title = titulo,
            status = estado,
            detail = exception.Message,
            instance = context.Request.Path.ToString(),
            errors = exception is ValidacionException validacion ? validacion.Errores : null
        });
    }
}
