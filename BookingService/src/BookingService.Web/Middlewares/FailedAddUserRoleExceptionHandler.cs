using BookingService.Auth.Application.CustomExceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Web.Middlewares;

internal sealed class FailedAddUserRoleExceptionHandler(ILogger<FailedAddUserRoleExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var failedAddUserRoleException = exception as FailedAddUserRoleException 
                                         ?? exception.InnerException as FailedAddUserRoleException;

        if (failedAddUserRoleException is null)
        {
            return false;
        }

        logger.LogError(
            failedAddUserRoleException,
            "FailedAddUserRole exception occurred: {Message}",
            failedAddUserRoleException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = failedAddUserRoleException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}