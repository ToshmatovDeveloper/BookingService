using BookingService.Application.Room.Create;
using FluentValidation;

namespace BookingService.Application.Validation;

public class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(request => request.Dto.HotelId)
            .NotEmpty()
            .WithMessage("HotelId is required.");
        
        RuleFor(request => request.Dto.FloorNumber)
            .NotEmpty()
            .WithMessage("FloorNumber is required.");
        
        RuleFor(request => request.Dto.RoomNumber)
            .NotEmpty()
            .WithMessage("RoomNumber is required.");
        
        RuleFor(request => request.Dto.RoomType)
            .NotEmpty()
            .WithMessage("RoomType is required.");
    }
}