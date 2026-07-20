using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Commands.Hotel;

public record DeleteHotelCommand(Guid Id) : IRequest<bool>;

public class DeleteHotelCommandHandler(
    ApplicationDbContext dbContext, 
    ILogger<DeleteHotelCommandHandler> logger) : IRequestHandler<DeleteHotelCommand, bool>
{
    public async Task<bool> Handle(DeleteHotelCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started deleting hotel with id : {command.Id}");

        var hotel = await dbContext.Hotels
            .Where(x => x.Id == command.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (hotel == null)
        {
            logger.LogWarning($"Hotel with id : {command.Id} not found");
            
            return false;
        }
        
        dbContext.Hotels.Remove(hotel);
        
        logger.LogInformation($"Hotel with id : {command.Id} deleted");

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}