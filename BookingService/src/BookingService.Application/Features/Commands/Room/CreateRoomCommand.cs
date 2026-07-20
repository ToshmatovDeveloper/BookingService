using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Commands.Room;

public record CreateRoomCommand(RoomDto Dto) : IRequest<RoomDto>;

public class  CreateRoomCommandHandler(
    ApplicationDbContext dbContext,
    ILogger<CreateRoomCommandHandler> logger) : IRequestHandler<CreateRoomCommand, RoomDto>
{
    public async Task<RoomDto> Handle(CreateRoomCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Started creating room");
        var dto = command.Dto;

        var room = new Domain.Entities.Room(
            dto.RoomNumber, dto.FloorNumber, dto.HotelId, dto.RoomType);
        
        try
        {
            await dbContext.Rooms.AddAsync(room, cancellationToken);
            
            logger.LogInformation($"Room created successfully. Room id : {room.Id}");
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            throw;
        }
        
        return new RoomDto(room.HotelId, room.RoomNumber, room.FloorNumber, room.RoomType);
    }
}