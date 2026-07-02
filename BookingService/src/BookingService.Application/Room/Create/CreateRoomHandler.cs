using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Room.Create;

public record CreateRoomRequest(RoomDto Dto) : IRequest<Guid>;

public class CreateRoomHandler(
    ApplicationDbContext dbContext,
    ILogger<CreateRoomHandler> logger) : IRequestHandler<CreateRoomRequest, Guid>
{
    public async Task<Guid> Handle(CreateRoomRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Started creating room");
        var dto = request.Dto;

        var room = new Domain.Entities.Room(
            Guid.NewGuid(), dto.RoomNumber, dto.FloorNumber, dto.HotelId, dto.RoomType);
        
        try
        {
            await dbContext.Rooms.AddAsync(room, cancellationToken);
            
            logger.LogInformation($"Room created successfully. Room id : {room.Id}");
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
        
        return  room.Id;
    }
}