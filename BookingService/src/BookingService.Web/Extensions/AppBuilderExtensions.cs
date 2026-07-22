using AuthService.Web.Middlewares;
using BookingService.Application.Settings.Cache;
using BookingService.Auth.Application.Settings;
using BookingService.Web.Middlewares;

namespace BookingService.Web.Extensions;

public static class AppBuilderExtensions
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
    
    public static IServiceCollection AddMyCustomConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PasswordSettings>(
            configuration.GetSection("PasswordSettings"));

        services.Configure<LockoutSettings>(
            configuration.GetSection("LockoutSettings"));

        services.Configure<UserSettings>(
            configuration.GetSection("UserSettings"));

        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        services.Configure<CacheSettings>(
            configuration.GetSection("CacheSettings"));

        return services;
    }
}