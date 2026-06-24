using BookingService.Infrastructure;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Hotel.Delete;

public class DeleteHotelHandler(
    ApplicationDbContext dbContext, 
    ILogger<DeleteHotelHandler> logger) : IRequestHandler<DeleteHotelRequest, bool>
{
    public async Task<bool> Handle(DeleteHotelRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started deleting hotel with id : {request.Id}");

        var hotel = await dbContext.Hotels
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (hotel == null)
        {
            logger.LogWarning($"Hotel with id : {request.Id} not found");
            
            return false;
        }
        
        dbContext.Hotels.Remove(hotel);
        
        logger.LogInformation($"Hotel with id : {request.Id} deleted");

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}