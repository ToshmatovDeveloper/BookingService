using AuthService.Application.Features;
using AuthService.Application.Settings;
using AuthService.Application.Validation;
using AuthService.Domain.Entities;
using AuthService.Infrastructure;
using AuthService.Presenters.Controllers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddApplicationPart(typeof(UserRegisterController).Assembly);

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseSnakeCaseNamingConvention();  
});

builder.Services.AddValidatorsFromAssembly(
    typeof(RegisterUserRequestValidator).Assembly);

builder.Services.Configure<PasswordSettings>(
    builder.Configuration.GetSection("PasswordSettings"));

builder.Services.Configure<LockoutSettings>(
    builder.Configuration.GetSection("LockoutSettings"));

builder.Services.Configure<UserSettings>(
    builder.Configuration.GetSection("UserSettings"));

builder.Services.AddIdentity<Account, Role>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(UserRegisterHandler).Assembly);
    
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

