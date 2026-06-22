using BookingService.Domain.DTOs;
using MediatR;

namespace BookingService.Application.Hotel.Create;

public record CreateHotelRequest(HotelDto Dto) : IRequest<Guid>;