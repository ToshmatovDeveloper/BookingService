using AuthService.Web;
using BookingService.Application.Features.Commands.Hotel;
using BookingService.Application.Settings.Cache;
using BookingService.Application.Validation;
using BookingService.Auth.Application.Settings;
using BookingService.Auth.Domain.Entities;
using BookingService.Auth.Infrastructure;
using BookingService.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var authServiceConnectionString = builder.Configuration.GetConnectionString("AuthServiceConnection");

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(authServiceConnectionString,
        x => x.MigrationsHistoryTable("__ef_migrations_history")));

builder.Services.AddValidatorsFromAssembly(
    typeof(CreateHotelRequestValidator).Assembly);

builder.Services.AddMyCustomMiddlewares();

builder.Services.AddProblemDetails();

builder.Services.Configure<PasswordSettings>(
    builder.Configuration.GetSection("PasswordSettings"));

builder.Services.Configure<LockoutSettings>(
    builder.Configuration.GetSection("LockoutSettings"));

builder.Services.Configure<UserSettings>(
    builder.Configuration.GetSection("UserSettings"));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection("CacheSettings"));


builder.Services.AddIdentity<Account, Role>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();


builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateHotelCommand).Assembly);

    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddMemoryCache();


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();