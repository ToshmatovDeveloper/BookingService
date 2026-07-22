using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookingService.Auth.Application.CustomExceptions;
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
         
        var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.CurrentValue.Secret));
        
        List<Claim> claims =
        [                  
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email!),
            ..roles.Select(role => new Claim(ClaimTypes.Role, role))
        ];
            
        var accessToken = GenerateAccessToken(claims, options.CurrentValue.Secret);
        
        var refreshToken = GenerateRefreshToken();
        
        return new UserLoginResponse(accessToken, refreshToken, user.Id, user.Email!);
    }

    private string GenerateAccessToken(List<Claim> claim, string secretKey)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claim),
            Expires = DateTime.UtcNow.AddMinutes(options.CurrentValue.ExpirationInMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    private string GenerateAccessTokenFromRefreshToken(string refreshToken, string secretKey)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddMinutes(options.CurrentValue.ExpirationInMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}