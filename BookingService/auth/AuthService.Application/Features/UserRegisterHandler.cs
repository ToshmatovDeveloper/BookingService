using AuthService.Domain.Entities;
using AuthService.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features;

public record UserRegisterRequest(string UserName, string Email, string Password) : IRequest<bool>;

public class UserRegisterHandler(
    UserManager<Account> userManager,
    RoleManager<Role> roleManager,
    AuthDbContext  dbContext,
    ILogger<UserRegisterHandler> logger,
    IValidator<UserRegisterRequest> validator) : IRequestHandler<UserRegisterRequest, bool>
{
    public async Task<bool> Handle(
        UserRegisterRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling user registration request.");

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            logger.LogWarning("User registration request is invalid.");
            return false;
        }

        var user = new Account(request.Email, request.UserName);
        
        var emailIsNotValid = await dbContext.Accounts.AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (emailIsNotValid)
        {
            logger.LogWarning("Email is already in use.");
            return false;
        }
        
        var result = await userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create user.");
            return false;
        }
        
        var role = new Role("User");

        if (!roleManager.RoleExistsAsync(role.Name).Result)
        {
            await roleManager.CreateAsync(role);
        }
        
        var roleAddResult = await userManager.AddToRoleAsync(user, role.Name);

        if (!roleAddResult.Succeeded)
        {
            logger.LogError("Failed to add role to user.");
            return false;
        }
        
        logger.LogInformation("User successfully registered.");
        
        return true;
    }
}