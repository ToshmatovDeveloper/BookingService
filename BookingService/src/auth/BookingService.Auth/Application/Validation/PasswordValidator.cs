using BookingService.Auth.Application.Features;
using BookingService.Auth.Application.Settings;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace BookingService.Auth.Application.Validation;

public class PasswordValidator 
    : AbstractValidator<UserRegisterCommand>
{
    public PasswordValidator(
        IOptionsMonitor<PasswordSettings> passwordOptions)
    {
        var passwordSettings = passwordOptions.CurrentValue;

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(passwordSettings.RequiredLength)
            .WithMessage(
                $"Password must be at least {passwordSettings.RequiredLength} characters long.");

        if (passwordSettings.RequireUppercase)
        {
            RuleFor(x => x.Password)
                .Matches("[A-Z]")
                .WithMessage(
                    "Password must contain at least one uppercase letter.");
        }

        if (passwordSettings.RequireLowercase)
        {
            RuleFor(x => x.Password)
                .Matches("[a-z]")
                .WithMessage(
                    "Password must contain at least one lowercase letter.");
        }

        if (passwordSettings.RequireDigit)
        {
            RuleFor(x => x.Password)
                .Matches("[0-9]")
                .WithMessage(
                    "Password must contain at least one digit.");
        }

        if (passwordSettings.RequireNonAlphanumeric)
        {
            RuleFor(x => x.Password)
                .Matches("[^a-zA-Z0-9]")
                .WithMessage(
                    "Password must contain at least one non-alphanumeric character.");
        }
    }
}