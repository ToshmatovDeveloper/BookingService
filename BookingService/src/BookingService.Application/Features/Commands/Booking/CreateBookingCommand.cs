using BookingService.Contracts.Events;
using BookingService.Domain.DTOs;
using BookingService.Domain.Enum;
using BookingService.Infrastructure;
using gRPC.Clients;
using MassTransit; 
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Commands.Booking;

public record CreateBookingCommand(BookingDto BookingDto, Guid UserId) : IRequest<BookingDto>;

public class CreateBookingCommandHandler(
    ApplicationDbContext dbContext,
    ILogger<CreateBookingCommandHandler> logger,
    IPublishEndpoint publishEndpoint,
    AuthGrpcClient authGrpcClient) : IRequestHandler<CreateBookingCommand, BookingDto> 
{
    public async Task<BookingDto> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Started creating booking for user: {UserId}", command.UserId);
        var dto = command.BookingDto;
        
        var userInfo = await authGrpcClient.GetUserByIdAsync(command.UserId);
        var userEmail = userInfo?.Email ?? "unknown@example.com"; 

        var isAvailable = await CheckAvailability(dto.RoomId, dto.StartDate, dto.EndDate, cancellationToken);
        
        if (!isAvailable)
        {
            throw new BadHttpRequestException("Room is not available for booking in current time range");
        }
        
        BookingStatus status = BookingStatus.Confirmed;
        
        var booking = new Domain.Entities.Booking(
            dto.HotelId, dto.RoomId, command.UserId, dto.StartDate, dto.EndDate, status);
        
        await dbContext.Bookings.AddAsync(booking, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish<BookingCancelledIntegrationEvent>(new BookingCancelledIntegrationEvent
        {
            BookingId = booking.Id,
            UserName = userInfo.Username,
            UserEmail = userEmail, 
        }, cancellationToken);

        logger.LogInformation("Booking created with id {id} and event published", booking.Id);
        
        return new BookingDto(booking.HotelId, booking.RoomId, booking.StartDate, booking.EndDate);
    }
    
    private async Task<bool> CheckAvailability(Guid roomId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms
            .Include(r => r.Bookings) 
            .Where(x => x.Id == roomId)
            .FirstOrDefaultAsync(cancellationToken);

        if (room == null) 
            throw new BadHttpRequestException($"Room with ID {roomId} was not found.");
            
        if (room.Bookings == null) return true; 

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