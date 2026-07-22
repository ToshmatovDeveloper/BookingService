using System.Text;
using BookingService.Auth.Application.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookingService.Web.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddCustomAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(); 

        services.AddAuthorization();

        return services;
    }
}

public class ConfigureJwtBearerOptions(IOptionsMonitor<JwtSettings> jwtMonitor)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            Configure(options);
        }
    }

    public void Configure(JwtBearerOptions options)
    {
        var settings = jwtMonitor.CurrentValue;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)),
            
            ValidateIssuer = !string.IsNullOrEmpty(settings.Issuer),
            ValidIssuer = settings.Issuer,
            
            ValidateAudience = !string.IsNullOrEmpty(settings.Audience),
            ValidAudience = settings.Audience,
            
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}
