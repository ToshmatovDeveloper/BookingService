using System.Threading.RateLimiting;
using BookingService.Application.Features.Commands.Hotel;
using BookingService.Application.Validation;
using BookingService.Application.Settings.Cache;
using BookingService.Auth.Application.Features;
using BookingService.Auth.Application.Settings;
using BookingService.Auth.Application.Validation;
using BookingService.Auth.Infrastructure;
using BookingService.Infrastructure;
using BookingService.Infrastructure.Interceptors;
using BookingService.Notification.Application.Features;
using BookingService.Notification.Application.Settings;
using BookingService.Notification.Application.Validation;
using BookingService.Notification.Infrastructure;
using BookingService.Web.Middlewares.Auth;
using BookingService.Web.Middlewares.Exception;
using FluentValidation;
using gRPC.Clients;
using Grpc.Net.Client;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

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
        services.Configure<PasswordSettings>(configuration.GetSection("PasswordSettings"));
        services.Configure<LockoutSettings>(configuration.GetSection("LockoutSettings"));
        services.Configure<UserSettings>(configuration.GetSection("UserSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));

        return services;
    }
    
    public static IApplicationBuilder AddMyCustomAuth(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CustomAuthMiddleware>();
    }
    
    public static IServiceCollection AddAuthGrpcClient(this IServiceCollection services, string serverUrl)
    {
        services.AddSingleton(GrpcChannel.ForAddress(serverUrl, new GrpcChannelOptions()));
        services.AddScoped<AuthGrpcClient>();

        return services;
    }

    public static IServiceCollection AddCustomDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var authServiceConnectionString = configuration.GetConnectionString("AuthServiceConnection");
        var notificationConnectionString = configuration.GetConnectionString("NotificationConnection");

        services.AddSingleton<UpdateAuditableEntitiesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var auditableInterceptor = sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>();
            options.UseNpgsql(connectionString).AddInterceptors(auditableInterceptor);
        });

        services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(authServiceConnectionString));
        services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(notificationConnectionString));

        return services;
    }

    public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<BookingCreatedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                cfg.UseInMemoryOutbox();
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static IServiceCollection AddCustomValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblies([
            typeof(CreateHotelRequestValidator).Assembly,
            typeof(PasswordValidator).Assembly,
            typeof(SendMailCommandValidator).Assembly 
        ]);

        return services;
    }

    public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                double retrySeconds = 60; 
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    retrySeconds = retryAfter.TotalSeconds;
                    context.HttpContext.Response.Headers.RetryAfter = retrySeconds.ToString();
                }

                var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                var problemDetails = problemDetailsFactory.CreateProblemDetails(
                    context.HttpContext,
                    statusCode: StatusCodes.Status429TooManyRequests,
                    title: "Too Many Requests",
                    detail: $"You have exceeded the rate limit. Please try again after {retrySeconds} seconds."
                );

                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
            };
            
            options.AddFixedWindowLimiter("fixed", cfg =>
            {
                cfg.PermitLimit = 5;
                cfg.Window = TimeSpan.FromMinutes(1);
            });

            options.AddPolicy("per-user", httpContext =>
            {
                string? userId = httpContext.User?.FindFirst("sub")?.Value;

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    return RateLimitPartition.GetTokenBucketLimiter(userId, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 5,
                        TokensPerPeriod = 2,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    });
                }
                
                return RateLimitPartition.GetFixedWindowLimiter("anonymous", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1)
                });
            });

            options.AddPolicy("auth-limit", httpContext =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
            });
        });

        return services;
    }

    public static IServiceCollection AddCustomMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateHotelCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(UserRegisterCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(SendMailCommand).Assembly);

            cfg.AddOpenBehavior(typeof(Application.Validation.ValidationBehavior<,>));
        });

        return services;
    }
}