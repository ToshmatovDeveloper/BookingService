using AuthService.Application.Settings;
using AuthService.Domain.Entities;
using AuthService.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseSnakeCaseNamingConvention();  
});

builder.Services.Configure<PasswordSettings>(
    builder.Configuration.GetSection("PasswordSettings"));

builder.Services.Configure<LockoutSettings>(
    builder.Configuration.GetSection("LockoutSettings"));

builder.Services.Configure<UserSettings>(
    builder.Configuration.GetSection("UserSettings"));

builder.Services.AddIdentity<Account, Role>()
    .AddEntityFrameworkStores<AuthDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

