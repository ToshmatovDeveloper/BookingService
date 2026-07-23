using BookingService.Auth.Application.CustomExceptions;
using BookingService.Auth.Application.Features.Tokens;
using BookingService.Auth.Application.Settings;
using BookingService.Auth.Domain.Entities;
using BookingService.Auth.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BookingService.Auth.Application.Features;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponse>;

public record RefreshTokenResponse(string AccessToken, string RefreshToken);

public class RefreshTokenCommandHandler(
    AuthDbContext dbContext,
    TokenProvider provider,
    IOptionsMonitor<JwtSettings> options) : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var oldRefreshToken = await dbContext.RefreshTokens
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.Token == command.RefreshToken, cancellationToken);

        if (oldRefreshToken is null || oldRefreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Invalid or expired refresh token."); 
        }
    
        string accessToken = provider.GenerateAccessToken(oldRefreshToken.Account);
    
        var newRefreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            AccountId = oldRefreshToken.AccountId,
            Token = provider.GenerateRefreshToken(),
            ExpiresOnUtc = DateTime.UtcNow.AddDays(options.CurrentValue.RefreshTokenExpirationInDays)
        };
    
        dbContext.RefreshTokens.Remove(oldRefreshToken);
        await dbContext.RefreshTokens.AddAsync(newRefreshToken, cancellationToken); 
    
        await dbContext.SaveChangesAsync(cancellationToken);
    
        return new RefreshTokenResponse(accessToken, newRefreshToken.Token);
    }
}
