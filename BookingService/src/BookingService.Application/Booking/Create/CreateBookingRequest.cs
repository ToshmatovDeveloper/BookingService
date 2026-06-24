using BookingService.Domain.DTOs;
using MediatR;

namespace BookingService.Application.Booking.Create;

public record CreateBookingRequest(BookingDto BookingDto) : IRequest<Guid>;