using BookingService.Domain.DTOs;
using BookingService.Domain.Enum;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Commands.Booking;

public record CreateBookingCommand(BookingDto BookingDto) : IRequest<BookingDto>;

public class CreateBookingCommandHandler(
    ApplicationDbContext dbContext,
    ILogger<CreateBookingCommandHandler> logger) : IRequestHandler<CreateBookingCommand, BookingDto>
{
    public async Task<BookingDto> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Started creating booking");
        var dto = command.BookingDto;

        var isAvailable = await CheckAvailability(dto.RoomId, dto.StartDate, dto.EndDate, cancellationToken);
        
        if (!isAvailable)
        {
            throw new Exception("Room is not available for booking in current time range");
        }
        
        BookingStatus status = BookingStatus.Confirmed;
        
        var booking = new Domain.Entities.Booking(
            dto.HotelId, dto.RoomId, dto.StartDate, dto.EndDate, status);
        
        try
        {
            await dbContext.Bookings.AddAsync(booking, cancellationToken);
            
            logger.LogInformation("Booking created with id {id}", booking.Id);
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            throw;
        }
        
        return new BookingDto(booking.HotelId, booking.RoomId, booking.StartDate, booking.EndDate);
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