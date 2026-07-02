using BookingService.Application.Hotel.Delete;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Room.Delete;

public record DeleteRoomRequest(Guid Id) : IRequest<bool>;

public class DeleteRoomHandler(
    ApplicationDbContext dbContext, 
    ILogger<DeleteRoomHandler> logger) : IRequestHandler<DeleteRoomRequest, bool>
{
    public async Task<bool> Handle(DeleteRoomRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started deleting room with id : {request.Id}");

        var room = await dbContext.Rooms
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (room == null)
        {
            logger.LogWarning($"Room with id : {request.Id} not found");
            
            return false;
        }
        
        dbContext.Rooms.Remove(room);
        
        logger.LogInformation($"Room with id : {request.Id} deleted");

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}