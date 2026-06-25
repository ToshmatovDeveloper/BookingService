using BookingService.Domain.Enum;
using BookingService.Infrastructure;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Booking.Cancel;

public class CancelBookingHandler(
    ApplicationDbContext dbContext,
    ILogger<CancelBookingHandler> logger) : IRequestHandler<CancelBookingRequest, Result<bool, string>>
{
    public async Task<Result<bool, string>> Handle(CancelBookingRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started cancelling booking by id : {request.Id}");

        var result = await dbContext.Bookings
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            logger.LogError($"Booking with id : {request.Id} not found");

            return "Booking not found";
        }

        if (result.StartDate < DateTime.UtcNow)
        {
            logger.LogWarning("Booking period is already started!");
            
            return "Too late to cancel booking";
        }
        
        result.Status = BookingStatus.Cancelled;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation($"Booking with id : {request.Id} cancelled");
        
        return true;
    }
}