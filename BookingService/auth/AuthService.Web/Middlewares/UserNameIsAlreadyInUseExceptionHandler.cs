using AuthService.Application.CustomException;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Web.Middlewares;

internal sealed class UserNameIsAlreadyInUseExceptionHandler(ILogger<UserNameIsAlreadyInUseExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var userNameIsAlreadyInUseException = exception as UserNameIsAlreadyInUseException
                                              ?? exception.InnerException as UserNameIsAlreadyInUseException;

        if (userNameIsAlreadyInUseException is null)
        {
            return false;
        }

        logger.LogError(
            userNameIsAlreadyInUseException,
            "UserNameIsAlreadyInUse exception occurred: {Message}",
            userNameIsAlreadyInUseException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = userNameIsAlreadyInUseException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}