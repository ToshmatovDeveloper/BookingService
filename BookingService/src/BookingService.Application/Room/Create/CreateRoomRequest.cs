using BookingService.Domain.DTOs;
using MediatR;

namespace BookingService.Application.Room.Create;

public record CreateRoomRequest(RoomDto Dto) : IRequest<Guid>;