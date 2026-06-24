using CSharpFunctionalExtensions;
using MediatR;

namespace BookingService.Application.Booking.Cancel;

public record CancelBookingRequest(Guid Id) : IRequest<Result<bool, string>>;