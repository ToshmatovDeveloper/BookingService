using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Commands.Room;

public record DeleteRoomCommand(Guid Id) : IRequest<bool>;

public class DeleteRoomCommandHandler(
    ApplicationDbContext dbContext, 
    ILogger<DeleteRoomCommandHandler> logger) : IRequestHandler<DeleteRoomCommand, bool>
{
    public async Task<bool> Handle(DeleteRoomCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started deleting room with id : {command.Id}");

        var room = await dbContext.Rooms
            .Where(x => x.Id == command.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (room == null)
        {
            logger.LogWarning($"Room with id : {command.Id} not found");
            
            return false;
        }
        
        dbContext.Rooms.Remove(room);
        
        logger.LogInformation($"Room with id : {command.Id} deleted");

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}