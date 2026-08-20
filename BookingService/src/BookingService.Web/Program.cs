using System.Text.Json;
using BookingService.Auth.Domain.Entities;
using BookingService.Auth.Infrastructure;
using BookingService.Auth.Application.Features.Tokens;
using BookingService.Auth.Application.BackgroundServices;
using BookingService.Infrastructure.Health;
using BookingService.Web.Extensions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

var authServiceUrl = builder.Configuration["GrpcSettings:AuthServiceUrl"] ?? "https://localhost:8139";

builder.Services.AddControllers();
builder.Services.AddCustomOpenApi();
builder.Services.AddMagicOnion();
builder.Services.AddProblemDetails();

builder.Services.AddCustomHealthChecks(builder.Configuration);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddMyCustomConfiguration(builder.Configuration);
builder.Services.AddCustomDatabases(builder.Configuration);
builder.Services.AddCustomMassTransit(builder.Configuration);
builder.Services.AddCustomValidators();
builder.Services.AddCustomRateLimiter();
builder.Services.AddCustomMediatR();
builder.Services.AddMyCustomMiddlewares();
builder.Services.AddAuthGrpcClient(authServiceUrl);
builder.Services.AddMyOpenTelemetry();

builder.Services.AddIdentity<Account, Role>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddCustomAuth(builder.Configuration);

builder.Services.AddMemoryCache();
builder.Services.AddScoped<TokenProvider>();
builder.Services.AddHostedService<RefreshToKenCleaner>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
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