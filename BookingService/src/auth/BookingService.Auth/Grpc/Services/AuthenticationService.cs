using System.Text;
using BookingService.Auth.Application.Settings;
using BookingService.Auth.Infrastructure;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BookingService.Auth.Grpc.Services;

[MessagePackObject]
public record Response(
    [property: Key(0)] bool IsAuthentificated, 
    [property: Key(1)] string UserId
);

[MessagePackObject]
public record UserResponse(
    [property: Key(0)] bool Success,
    [property: Key(1)] string Email,
    [property: Key(2)] string Username
);

public class AuthenticationService(
    IOptionsMonitor<JwtSettings> optionsMonitor,
    ILogger<AuthenticationService> logger,
    AuthDbContext dbContext) : ServiceBase<IAuthenticationService>, IAuthenticationService
{
    public async UnaryResult<Response> CheckAsync(string token)
    {
        logger.LogInformation("gRPC Server: Received request for checking token");
        
        var settings = optionsMonitor.CurrentValue;
        
        var key = Encoding.UTF8.GetBytes(settings.Secret);
        
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            
            ValidateIssuer = true, 
            ValidIssuer = settings.Issuer,
            
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            
            ClockSkew = TimeSpan.Zero 
        };

        if (string.IsNullOrEmpty(token))
        {
            return new Response(false, string.Empty);
        }

        var handler = new JsonWebTokenHandler();
        var result = await handler.ValidateTokenAsync(token, validationParameters);

        if (result.IsValid)
        {
            var userId = result.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                         ?? result.ClaimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            logger.LogInformation("gRPC Server: Token is valid. UserId: {UserId}", userId);
            
            return new Response(true, userId ?? string.Empty);
        }
        
        logger.LogWarning("gRPC Server: JWT validation failed. Reason: {Reason}", result.Exception?.Message);
        
        return new Response(false, string.Empty);
    }
    
    public async UnaryResult<UserResponse> GetUserByIdAsync(Guid userId)
    {
        logger.LogInformation("gRPC Server: Received request for getting user by id: {UserId}", userId);

        var user = await dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.Email, u.UserName })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            logger.LogWarning("gRPC Server: User with id {UserId} not found", userId);
            return new UserResponse(false, string.Empty, string.Empty);
        }

        return new UserResponse(true, user.Email, user.UserName);
    }
}
