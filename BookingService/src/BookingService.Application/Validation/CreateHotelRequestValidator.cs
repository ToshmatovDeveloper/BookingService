using BookingService.Application.Hotel.Create;
using FluentValidation;

namespace BookingService.Application.Validation;

public class CreateHotelRequestValidator : AbstractValidator<CreateHotelRequest>
{
    public CreateHotelRequestValidator()
    {
        RuleFor(request => request.Dto.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Name is required.");
        
        RuleFor(request => request.Dto.Address)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Address is required.");
        
        RuleFor(request => request.Dto.Floors)
            .NotEmpty()
            .WithMessage("Floors is required.");
        
        RuleFor(request => request.Dto.StarRating)
            .NotEmpty()
            .WithMessage("StarRating is required.");
    }
}