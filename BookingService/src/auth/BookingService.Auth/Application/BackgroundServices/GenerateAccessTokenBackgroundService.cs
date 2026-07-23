using BookingService.Auth.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingService.Auth.Application.BackgroundServices;

public record GenerateAccessTokenBackgroundServiceResponse(string AccessToken);

public class GenerateAccessTokenBackgroundService(
    ILogger<GenerateAccessTokenBackgroundService> logger,
    IServiceScopeFactory scopeFactory,
    string refreshToken, string secretKey) : BackgroundService
{
    protected override async Task<GenerateAccessTokenBackgroundServiceResponse> ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Generating access tokens.");
        var newAccessToken = string.Empty;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateAsyncScope();
            var accessTokenGenerator = scope.ServiceProvider.GetRequiredService<ITokenHandlers>();
            
            newAccessToken = accessTokenGenerator.GenerateAccessTokenFromRefreshToken(refreshToken, secretKey);
        }
        
        return new  GenerateAccessTokenBackgroundServiceResponse(newAccessToken);
    }
}