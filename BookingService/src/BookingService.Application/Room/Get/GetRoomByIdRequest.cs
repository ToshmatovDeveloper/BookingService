using BookingService.Domain.DTOs;
using CSharpFunctionalExtensions;
using MediatR;

namespace BookingService.Application.Room.Get;

public record GetRoomByIdRequest(Guid Id) : IRequest<RoomDto>;