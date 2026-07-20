using BookingService.Auth.Application.CustomExceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Web.Middlewares;

internal sealed class EmailIsAlreadyInUseExceptionHandler(ILogger<EmailIsAlreadyInUseExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var emailIsAlreadyInUseException = exception as EmailIsAlreadyInUseException 
                                           ?? exception.InnerException as EmailIsAlreadyInUseException;

        if (emailIsAlreadyInUseException is null)
        {
            return false;
        }

        logger.LogError(
            emailIsAlreadyInUseException,
            "EmailIsAlreadyInUse exception occurred: {Message}",
            emailIsAlreadyInUseException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = emailIsAlreadyInUseException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}