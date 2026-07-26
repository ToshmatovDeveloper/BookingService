using BookingService.Application.Features.Commands.Hotel;
using BookingService.Application.Validation;
using BookingService.Auth.Application.BackgroundServices;
using BookingService.Auth.Application.Features;
using BookingService.Auth.Application.Features.Tokens;
using BookingService.Auth.Application.Validation;
using BookingService.Auth.Domain.Entities;
using BookingService.Auth.Infrastructure;
using BookingService.Infrastructure;
using BookingService.Web.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var authServiceConnectionString = builder.Configuration.GetConnectionString("AuthServiceConnection");

builder.Services.AddControllers();

builder.Services.AddCustomOpenApi(); 

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(authServiceConnectionString));

builder.Services.AddValidatorsFromAssemblies([
    typeof(CreateHotelRequestValidator).Assembly,
    typeof(PasswordValidator).Assembly
]);

builder.Services.AddMyCustomMiddlewares()
    .AddMyCustomConfiguration(builder.Configuration)
    .AddProblemDetails();

builder.Services.AddIdentity<Account, Role>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddCustomAuth(builder.Configuration);

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

app.UseAuthentication();
app.UseAuthorization();

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

app.Run();
