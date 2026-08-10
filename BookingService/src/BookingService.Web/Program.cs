using System.Threading.RateLimiting;
using BookingService.Application.Features.Commands.Hotel;
using BookingService.Application.Validation;
using BookingService.Auth.Application.BackgroundServices;
using BookingService.Auth.Application.Features;
using BookingService.Auth.Application.Features.Tokens;
using BookingService.Auth.Application.Validation;
using BookingService.Auth.Domain.Entities;
using BookingService.Auth.Infrastructure;
using BookingService.Infrastructure;
using BookingService.Infrastructure.Interceptors;
using BookingService.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMagicOnion();

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var authServiceConnectionString = builder.Configuration.GetConnectionString("AuthServiceConnection");
var authServiceUrl = builder.Configuration["GrpcSettings:AuthServiceUrl"] ?? "https://localhost:8139";

builder.Services.AddControllers();
builder.Services.AddCustomOpenApi(); 
 
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSingleton<UpdateAuditableEntitiesInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>(
    (sp, options) =>
    {
        var auditableInterceptor = sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>();
        
        options.UseNpgsql(connectionString)
            .AddInterceptors(auditableInterceptor);
    });

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(authServiceConnectionString));

builder.Services.AddValidatorsFromAssemblies([
    typeof(CreateHotelRequestValidator).Assembly,
    typeof(PasswordValidator).Assembly 
]);

builder.Services.AddMyCustomMiddlewares()
    .AddMyCustomConfiguration(builder.Configuration)
    .AddProblemDetails();

builder.Services.AddAuthGrpcClient(authServiceUrl);

builder.Services.AddIdentity<Account, Role>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddCustomAuth(builder.Configuration);

builder.Services.AddRateLimiter(options =>
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

        var problemDetailsFactory = context.HttpContext
            .RequestServices.GetRequiredService<ProblemDetailsFactory>();

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
            return RateLimitPartition.GetTokenBucketLimiter(
                userId,
                _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 5,
                    TokensPerPeriod = 2,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                });
        }
        
        return RateLimitPartition.GetFixedWindowLimiter(
            "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    options.AddPolicy("auth-limit", httpContext =>
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            ipAddress,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateHotelCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(UserRegisterCommand).Assembly);

    cfg.AddOpenBehavior(typeof(BookingService.Application.Validation.ValidationBehavior<,>));
});

builder.Services.AddMemoryCache();
builder.Services.AddScoped<TokenProvider>();
builder.Services.AddHostedService<RefreshToKenCleaner>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.MapReverseProxy();

app.AddMyCustomAuth(); 

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookingService API v1");
        c.RoutePrefix = "swagger"; 
    });
}

app.MapControllers();
app.MapMagicOnionService();

app.Run();