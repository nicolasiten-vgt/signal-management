using Microsoft.AspNetCore.Diagnostics;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Exceptions;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Api;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is NotFoundException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(new { error = exception.Message }, cancellationToken: cancellationToken);
        }
        else if (exception is ValidationException validationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(
                new { errors = validationException.Errors }, cancellationToken: cancellationToken);
        }
        else
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                Error = "Unexpected error",
                Message = exception.Message
            });
        }

        return true;
    }
}