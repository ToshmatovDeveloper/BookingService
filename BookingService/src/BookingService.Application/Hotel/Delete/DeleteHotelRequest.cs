using CSharpFunctionalExtensions;
using MediatR;

namespace BookingService.Application.Hotel.Delete;

public record DeleteHotelRequest(Guid Id) : IRequest<bool>;