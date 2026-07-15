using AuthService.Application.Features;
using AuthService.Application.Settings;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AuthService.Application.Validation;

public class RegisterUserRequestValidator 
    : AbstractValidator<UserRegisterRequest>
{
    public RegisterUserRequestValidator(
        IOptionsMonitor<PasswordSettings> passwordOptions, 
        IOptionsMonitor<UserSettings> userOptions)
    {
        var passwordSettings = passwordOptions.CurrentValue;
        var userSettings = userOptions.CurrentValue;

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(passwordSettings.RequiredLength)
            .WithMessage(
                $"Password must be at least {passwordSettings.RequiredLength} characters long.");

        if (passwordSettings.RequireUppercase)
        {
            RuleFor(x => x.Password)
                .Matches("[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.");
        }

        if (passwordSettings.RequireLowercase)
        {
            RuleFor(x => x.Password)
                .Matches("[a-z]")
                .WithMessage("Password must contain at least one lowercase letter.");
        }

        if (passwordSettings.RequireDigit)
        {
            RuleFor(x => x.Password)
                .Matches("[0-9]")
                .WithMessage("Password must contain at least one digit.");
        }

        if (passwordSettings.RequireNonAlphanumeric)
        {
            RuleFor(x => x.Password)
                .Matches("[^a-zA-Z0-9]")
                .WithMessage("Password must contain at least one non-alphanumeric character.");
        }

        if (userSettings.RequireUniqueEmail)
        {
            RuleFor(x => x.Email)
                .NotEqual(x => x.Password)
                .WithMessage("Email and password cannot be the same.");
        }
    }
}