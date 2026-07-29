using System.Text;
using BookingService.Auth.Application.Settings;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BookingService.Auth.Grpc.Services;

public record Response(bool IsAuthentificated, string UserId);

public class AuthenticationService(
    IOptionsMonitor<JwtSettings> optionsMonitor,
    ILogger<AuthenticationService> logger) : ServiceBase<IAuthenticationService>, IAuthenticationService
{
    public async UnaryResult<Response> CheckAsync(string token)
    {
        logger.LogInformation("gRPC Server: Received request for checking token");
        var secretKey = optionsMonitor.CurrentValue.Secret;
        var key = Encoding.ASCII.GetBytes(secretKey);
        
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true, 
            ValidateAudience = true,
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
            var userId = result.ClaimsIdentity
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            logger.LogInformation("gRPC Server: Token is valid. UserId: {UserId}", userId);
            
            return new Response(true, userId!);
        }
        
        logger.LogWarning("gRPC Server: JWT validation failed");
        
        return new Response(false, string.Empty);
    }
}
