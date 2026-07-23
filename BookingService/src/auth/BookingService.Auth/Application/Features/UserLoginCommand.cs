using BookingService.Auth.Application.CustomExceptions;
using BookingService.Auth.Application.Features.Tokens;
using BookingService.Auth.Application.Settings;
using BookingService.Auth.Domain.Entities;
using BookingService.Auth.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BookingService.Auth.Application.Features;

public record UserLoginCommand(string Email, string Password) : IRequest<UserLoginResponse>;

public record UserLoginResponse(string AccessToken, string RefreshToken, Guid UserId, string Email);

public class UserLoginCommandHandler(
    AuthDbContext dbContext,
    UserManager<Account> userManager,
    IOptionsMonitor<JwtSettings> options,
    TokenProvider tokenProvider) : IRequestHandler<UserLoginCommand, UserLoginResponse>
{
    public async Task<UserLoginResponse> Handle(UserLoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        
        if (user is null || !await userManager.CheckPasswordAsync(user, command.Password))
        {
            throw new UnauthorizedException("Invalid login or password.");
        }
        
        var accessToken = tokenProvider.GenerateAccessToken(user);
        
        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            AccountId = user.Id,
            Token = tokenProvider.GenerateRefreshToken(),
            ExpiresOnUtc = DateTime.UtcNow.AddDays(options.CurrentValue.RefreshTokenExpirationInDays)
        };
        
        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken); 
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return new UserLoginResponse(accessToken, refreshToken.Token, user.Id, user.Email!);
    }
}