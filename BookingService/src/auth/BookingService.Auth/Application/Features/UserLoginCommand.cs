using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookingService.Auth.Application.BackgroundServices;
using BookingService.Auth.Application.CustomExceptions;
using BookingService.Auth.Application.Features.Tokens;
using BookingService.Auth.Application.Settings;
using BookingService.Auth.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BookingService.Auth.Application.Features;

public record UserLoginCommand(string Email, string Password) : IRequest<UserLoginResponse>;

public record UserLoginResponse(string AccessToken, string RefreshToken, Guid UserId, string Email);

public class UserLoginCommandHandler(
    UserManager<Account> userManager,
    IOptionsMonitor<JwtSettings> options) : IRequestHandler<UserLoginCommand, UserLoginResponse>
{
    public async Task<UserLoginResponse> Handle(UserLoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        
        if (user is null || !await userManager.CheckPasswordAsync(user, command.Password))
        {
            throw new UnauthorizedException("Invalid login or password.");
        }
        
        var roles = await userManager.GetRolesAsync(user);
        
        List<Claim> claims =
        [                  
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email!),
        ];
        
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenHandlers = new TokenHandlers(options);
        
        var accessToken = tokenHandlers.GenerateAccessToken(claims, options.CurrentValue.Secret);
        
        var refreshToken = tokenHandlers.GenerateRefreshToken();
        
        return new UserLoginResponse(accessToken, refreshToken, user.Id, user.Email!);
    }
}