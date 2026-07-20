using System.Security.Claims;
using System.Text;
using BookingService.Auth.Application.CustomExceptions;
using BookingService.Auth.Application.Settings;
using BookingService.Auth.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BookingService.Auth.Application.Features;

public record UserLoginCommand(string Email, string Password) : IRequest<UserLoginResponse>;

public record UserLoginResponse(string AccessToken, Guid UserId, string Email);

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
        
        var credentials = new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256);
        
        List<Claim> claims =
        [                  
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email!),
            ..roles.Select(role => new Claim(ClaimTypes.Role, role))
        ];
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(options.CurrentValue.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = options.CurrentValue.Issuer,
            Audience = options.CurrentValue.Audience    
        };
        
        var tokenHandler = new JsonWebTokenHandler();
            
        string accessToken = tokenHandler.CreateToken(tokenDescriptor);
            
        return new UserLoginResponse(accessToken, user.Id, user.Email!);
    }
}