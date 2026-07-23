using BookingService.Application.Features.Commands.Hotel;
using BookingService.Application.Validation;
using BookingService.Auth.Application.BackgroundServices;
using BookingService.Auth.Application.Features;
using BookingService.Auth.Application.Features.Tokens;
using BookingService.Auth.Application.Interfaces;
using BookingService.Auth.Application.Validation;
using BookingService.Auth.Domain.Entities;
using BookingService.Auth.Infrastructure;
using BookingService.Infrastructure;
using BookingService.Web;
using BookingService.Web.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var authServiceConnectionString = builder.Configuration.GetConnectionString("AuthServiceConnection");

builder.Services.AddControllers();
builder.Services.AddHostedService<GenerateAccessTokenBackgroundService>();
builder.Services.AddScoped<ITokenHandlers, TokenHandlers>();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();             
    app.MapScalarApiReference();  
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
