using BookingService.Application.Settings.Cache;
using BookingService.Auth.Application.Settings;
using BookingService.Web.Middlewares;
using BookingService.Web.Middlewares.Auth;
using BookingService.Web.Middlewares.Exception;
using gRPC.Clients;
using Grpc.Net.Client;

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
    public static IServiceCollection AddMyGrpcClients(this IServiceCollection services, IConfiguration configuration)
    {
        var authServiceUrl = configuration["GrpcSettings:AuthServiceUrl"];

        if (string.IsNullOrEmpty(authServiceUrl))
        {
            throw new InvalidOperationException("gRPC AuthServiceUrl is not configured in appsettings.json");
        }

        services.AddSingleton(sp => GrpcChannel.ForAddress(authServiceUrl));

        services.AddSingleton<AuthGrpcClient>();

        return services;
    }
    
    public static IApplicationBuilder AddMyCustomAuth(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CustomAuthMiddleware>();
    }
}