using BookingService.Domain.DTOs;
using CSharpFunctionalExtensions;
using MediatR;

namespace BookingService.Application.Booking.Get;

public record GetBookingByIdRequest(Guid Id) : IRequest<Result<BookingDto, string>>;