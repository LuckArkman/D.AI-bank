using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Fintech.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Fintech.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            DomainException => (StatusCodes.Status422UnprocessableEntity, exception.Message),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflito de estado ou concorrência."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado."),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor.")
        };

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = exception.GetType().Name,
            Detail = exception.Message
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}