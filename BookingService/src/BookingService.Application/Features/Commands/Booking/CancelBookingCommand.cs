using BookingService.Domain.DTOs;
using BookingService.Domain.Enum;
using BookingService.Infrastructure;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Commands.Booking;

public record CancelBookingCommand(Guid Id) : IRequest<CancelBookingCommandResponse>;

public record CancelBookingCommandResponse(BookingDto? dto, string message);

public class CancelBookingCommandHandler(
    ApplicationDbContext dbContext,
    ILogger<CancelBookingCommandHandler> logger) : IRequestHandler<CancelBookingCommand, CancelBookingCommandResponse>
{
    public async Task<CancelBookingCommandResponse> Handle(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started cancelling booking by id : {command.Id}");

        var result = await dbContext.Bookings
            .FindAsync(command.Id, cancellationToken);

        if (result == null)
        {
            logger.LogError($"Booking with id : {command.Id} not found");

            return new CancelBookingCommandResponse(null, "Booking not found");
        }
        
        var bookingDto = new BookingDto(result.HotelId, result.RoomId, result.StartDate, result.EndDate);
        
        if (result.StartDate < DateTime.UtcNow)
        {
            logger.LogWarning("Booking period is already started!");
            
            return new CancelBookingCommandResponse(bookingDto, "Too late to cancel booking");
        }
        
        result.Status = BookingStatus.Cancelled;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation($"Booking with id : {command.Id} cancelled");
        
        return new CancelBookingCommandResponse(bookingDto, "Booking cancelled successfully");
    }
}