using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BookingService.Auth.Application.CustomExceptions;
using BookingService.Auth.Application.Features.Tokens;
using BookingService.Auth.Application.Settings;
using BookingService.Auth.Domain.Entities;
using BookingService.Auth.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingService.Auth.Application.Features;

public record UserRegisterCommand(string UserName, string Email, string Password) : IRequest<UserRegisterResponse>;

public record UserRegisterResponse(string accessToken, string refreshToken, string message);

public class UserRegisterCommandHandler(
    UserManager<Account> userManager,
    RoleManager<Role> roleManager,
    AuthDbContext  dbContext,
    IOptionsMonitor<JwtSettings> options,
    TokenProvider tokenProvider,
    ILogger<UserRegisterCommandHandler> logger) : IRequestHandler<UserRegisterCommand, UserRegisterResponse>
{
    public async Task<UserRegisterResponse> Handle(
        UserRegisterCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling user registration request.");
        
        var user = new Account(command.Email, command.UserName);
        
        var userNameIsNotValid = await dbContext.Accounts.AnyAsync(x => x.UserName == command.UserName, cancellationToken);

        if (userNameIsNotValid)
        {
            logger.LogWarning("Username is already in use.");
            throw new UserNameIsAlreadyInUseException("Username is already in use.");
        }
        
        var emailIsNotValid = await dbContext.Accounts.AnyAsync(x => x.Email == command.Email, cancellationToken);

        if (emailIsNotValid)
        {
            logger.LogWarning("Email is already in use.");
            throw new EmailIsAlreadyInUseException("Email is already in use.");
        }
        
        var result = await userManager.CreateAsync(user, command.Password);
        
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create user.");
            throw new UserCreateFailedException("Failed to create user.");
        }
        
        var role = new Role("User");

        if (!await roleManager.RoleExistsAsync(role.Name))
        {
            await roleManager.CreateAsync(role);
        }
        
        var roleAddResult = await userManager.AddToRoleAsync(user, role.Name);

        if (!roleAddResult.Succeeded)
        {
            logger.LogError("Failed to add role to user.");
            throw new FailedAddUserRoleException("Failed to add role to user.");
        }

        var roles = await userManager.GetRolesAsync(user);
        
        List<Claim> claims =
        [                  
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email!),
        ];
        
        foreach (var userRole in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
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
        
        logger.LogInformation("User successfully registered.");
        
        return new UserRegisterResponse(accessToken, refreshToken.Token, "Welcome to Booking Service");
    }
}