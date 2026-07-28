using System.Text;
using BookingService.Auth.Application.Settings;
using Grpc.Core;
using gRPC.Contracts;
using gRPC.Contracts.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BookingService.Auth.Grpc.Services;

public class AuthenticationService(
    IOptionsMonitor<JwtSettings> optionsMonitor,
    ILogger<AuthenticationService> logger) : IsAuthenticated.IsAuthenticatedBase
{
    public override async Task<Response> Check(Request request, ServerCallContext context)
    {
        logger.LogInformation("gRPC Сервер: Получен запрос Check от клиента {Peer}", context.Peer);
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
        
        var token = request.Token;

        if (string.IsNullOrEmpty(token))
        {
            return new Response
            {
                IsAuthentificated = false,
                UserId = string.Empty
            };
        }

        var handler = new JsonWebTokenHandler();
        var result = await handler.ValidateTokenAsync(token, validationParameters);

        if (result.IsValid)
        {
            var userId = result.ClaimsIdentity
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            logger.LogInformation("gRPC Сервер: Токен валиден. Пользователь: {UserId}", userId);
            
            return new Response
            {
                IsAuthentificated = true, 
                UserId = userId ?? string.Empty 
            };
        }
        
        logger.LogWarning("gRPC Сервер: Токен не прошел валидацию JWT");
        
        return new Response
        {
            IsAuthentificated = false,
            UserId = string.Empty
        };
    }
}
