using BookingService.Auth.Application.CustomExceptions;
using BookingService.Auth.Domain.Entities;
using BookingService.Auth.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Auth.Application.Features;

public record UserRegisterCommand(string UserName, string Email, string Password) : IRequest<bool>;

public class UserRegisterCommandHandler(
    UserManager<Account> userManager,
    RoleManager<Role> roleManager,
    AuthDbContext  dbContext,
    ILogger<UserRegisterCommandHandler> logger) : IRequestHandler<UserRegisterCommand, bool>
{
    public async Task<bool> Handle(
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

        if (!roleManager.RoleExistsAsync(role.Name).Result)
        {
            await roleManager.CreateAsync(role);
        }
        
        var roleAddResult = await userManager.AddToRoleAsync(user, role.Name);

        if (!roleAddResult.Succeeded)
        {
            logger.LogError("Failed to add role to user.");
            throw new FailedAddUserRoleException("Failed to add role to user.");
        }
        
        logger.LogInformation("User successfully registered.");
        
        return true;
    }
}