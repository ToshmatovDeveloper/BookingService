using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BookingService.Auth.Application.Features.Tokens;

namespace BookingService.Auth.Application.BackgroundServices;

public class RefreshToKenCleaner(
    ILogger<RefreshToKenCleaner> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromDays(1));

        while (!stoppingToken.IsCancellationRequested && 
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                logger.LogInformation("Starting background token cleaning.");

                using var scope = scopeFactory.CreateScope();
                
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                
                var deletedCount = await mediator
                    .Send(new TokenProvider.CleanOldTokensCommand(), stoppingToken);

                logger.LogInformation("Token cleanup completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during token cleanup command execution.");
            }
        }
    }
}