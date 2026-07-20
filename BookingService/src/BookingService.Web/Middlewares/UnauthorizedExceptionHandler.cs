using BookingService.Application.CustomExceptions;
using BookingService.Auth.Application.CustomExceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Web.Middlewares;

internal sealed class UnauthorizedExceptionHandler(ILogger<UnauthorizedExceptionHandler> logger) : IExceptionHandler    
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var unauthorizedException = exception as UnauthorizedException 
                                    ?? exception.InnerException as UnauthorizedException;

        if (unauthorizedException is null)
        {
            return false; 
        }

        logger.LogError(
            unauthorizedException,
            "Unauthorized exception occurred: {Message}",
            unauthorizedException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = unauthorizedException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}