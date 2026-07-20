using BookingService.Auth.Application.CustomExceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Web.Middlewares;

internal sealed class UserCreateFailedExceptionHandler(ILogger<UserCreateFailedExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var userCreateFailedException = exception as UserCreateFailedException 
                                        ?? exception.InnerException as UserCreateFailedException;

        if (userCreateFailedException is null)
        {
            return false;
        }

        logger.LogError(
            userCreateFailedException,
            "UserCreateFailed exception occurred: {Message}",
            userCreateFailedException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = userCreateFailedException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}