using Inventario.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Common;

/// <summary>Traduce las excepciones de la capa de aplicación a respuestas ProblemDetails.</summary>
public sealed class ManejadorExcepciones(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        var (status, titulo) = exception switch
        {
            NoEncontradoException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ConflictoException => (StatusCodes.Status409Conflict, "Conflicto"),
            ValidacionException => (StatusCodes.Status400BadRequest, "Datos inválidos"),
            AutenticacionException => (StatusCodes.Status401Unauthorized, "No autenticado"),
            _ => (0, string.Empty),
        };

        if (status == 0)
        {
            return false;
        }

        httpContext.Response.StatusCode = status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = exception.Message,
            },
        });
    }
}
