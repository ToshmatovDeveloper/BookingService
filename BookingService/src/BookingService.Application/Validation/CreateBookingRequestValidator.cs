using BookingService.Application.Booking.Create;
using FluentValidation;

namespace BookingService.Application.Validation;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(request => request.BookingDto.HotelId)
            .NotEmpty()
            .WithMessage("HotelId is required.");
        
        RuleFor(request => request.BookingDto.RoomId)
            .NotEmpty()
            .WithMessage("RoomId is required.");
        
        RuleFor(request => request.BookingDto.StartDate)
            .NotEmpty()
            .WithMessage("StartDate is required.");
        
        RuleFor(request => request.BookingDto.EndDate)
            .NotEmpty()
            .WithMessage("EndDate is required.");
    }
}