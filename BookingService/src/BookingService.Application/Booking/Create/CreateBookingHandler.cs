using BookingService.Domain.DTOs;
using BookingService.Domain.Enum;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Booking.Create;

public record CreateBookingRequest(BookingDto BookingDto) : IRequest<Guid>;

public class CreateBookingHandler(
    ApplicationDbContext dbContext,
    ILogger<CreateBookingHandler> logger) : IRequestHandler<CreateBookingRequest, Guid>
{
    public async Task<Guid> Handle(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Started creating booking");
        var dto = request.BookingDto;

        var isAvailable = await CheckAvailability(dto.RoomId, dto.StartDate, dto.EndDate, cancellationToken);
        
        if (!isAvailable)
        {
            throw new Exception("Room is not available for booking in current time range");
        }
        
        BookingStatus status = BookingStatus.Confirmed;
        
        var booking = new Domain.Entities.Booking(
            Guid.NewGuid(), dto.HotelId, dto.RoomId, dto.StartDate, dto.EndDate, status);
        
        try
        {
            await dbContext.Bookings.AddAsync(booking, cancellationToken);
            
            logger.LogInformation("Booking created with id {id}", booking.Id);
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
        
        return booking.Id;
    }
    
    private async Task<bool> CheckAvailability(
        Guid roomId, 
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        logger.LogInformation($"Checking room with id : {roomId}");

        var room = await dbContext.Rooms
            .Include(r => r.Bookings) 
            .Where(x => x.Id == roomId)
            .FirstOrDefaultAsync(cancellationToken);

        if (room == null)
        {
            throw new Exception($"Room with ID {roomId} was not found.");
        }

        if (room.Bookings == null)
        {
            return true; 
        }

        foreach (var roomBooking in room.Bookings)
        {
            if (startDate <= roomBooking.EndDate && endDate >= roomBooking.StartDate)
            {
                return false;
            }
        }

        return true;
    }

}