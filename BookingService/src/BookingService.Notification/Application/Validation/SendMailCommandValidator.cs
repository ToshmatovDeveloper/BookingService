using BookingService.Notification.Application.Features;
using FluentValidation;

namespace BookingService.Notification.Application.Validation;

public class SendMailCommandValidator : AbstractValidator<SendMailCommand>
{
    public SendMailCommandValidator()
    {
        RuleFor(x => x.ReceiverAddress)
            .NotEmpty().WithMessage("Receiver email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Email subject is required.");
    }
}