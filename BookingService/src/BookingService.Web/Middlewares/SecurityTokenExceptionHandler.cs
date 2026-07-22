using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BookingService.Web.Middlewares;

internal sealed class SecurityTokenExceptionHandler(ILogger<SecurityTokenExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var securityTokenException = exception as SecurityTokenException 
                                     ?? exception.InnerException as SecurityTokenException;

        if (securityTokenException is null)
        {
            return false; 
        }

        logger.LogError(
            securityTokenException,
            "Security token exception occurred: {Message}",
            securityTokenException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = securityTokenException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}