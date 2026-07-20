using AuthService.Web.Middlewares;
using BookingService.Web.Middlewares;
using Microsoft.AspNetCore.Diagnostics;

namespace AuthService.Web;

public static class AddMiddlewareHandler
{
    public static IServiceCollection AddMyCustomMiddlewares(this IServiceCollection services)
    {
        services.AddExceptionHandler<UserNameIsAlreadyInUseExceptionHandler>();
        services.AddExceptionHandler<EmailIsAlreadyInUseExceptionHandler>();
        services.AddExceptionHandler<BadRequestExceptionHandler>();
        services.AddExceptionHandler<FailedAddUserRoleExceptionHandler>();
        services.AddExceptionHandler<UserCreateFailedExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        return services;
    }
}