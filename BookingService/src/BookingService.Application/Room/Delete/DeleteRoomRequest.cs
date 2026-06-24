using MediatR;

namespace BookingService.Application.Room.Delete;

public record DeleteRoomRequest(Guid Id) : IRequest<bool>;