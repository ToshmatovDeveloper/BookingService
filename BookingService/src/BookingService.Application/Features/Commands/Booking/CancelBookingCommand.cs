using BookingService.Contracts.Events;
using BookingService.Domain.DTOs;
using BookingService.Domain.Enum;
using BookingService.Infrastructure;
using gRPC.Clients;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Commands.Booking;

public record CancelBookingCommand(Guid Id, Guid UserId) : IRequest<CancelBookingCommandResponse>;

public record CancelBookingCommandResponse(BookingDto? dto, string message);

public class CancelBookingCommandHandler(
    ApplicationDbContext dbContext,
    ILogger<CancelBookingCommandHandler> logger,
    IPublishEndpoint publishEndpoint,
    AuthGrpcClient grpcClient) : IRequestHandler<CancelBookingCommand, CancelBookingCommandResponse>
{
    public async Task<CancelBookingCommandResponse> Handle(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Started cancelling booking by id : {Id} by user: {UserId}", command.Id, command.UserId);

        var result = await dbContext.Bookings
            .FindAsync([command.Id], cancellationToken);

        if (result == null)
        {
            logger.LogError("Booking with id : {Id} not found", command.Id);
            return new CancelBookingCommandResponse(null, "Booking not found");
        }
        
        if (result.UserId != command.UserId)
        {
            logger.LogWarning("Security alert: User {UserId} tried to cancel booking {Id} belonging to another user!", command.UserId, command.Id);
            return new CancelBookingCommandResponse(null, "Access denied. You can only cancel your own bookings.");
        }
        
        var bookingDto = new BookingDto(result.HotelId, result.RoomId, result.StartDate, result.EndDate);
        
        if (result.StartDate < DateTime.UtcNow)
        {
            logger.LogWarning("Booking period is already started for booking id: {Id}", command.Id);
            return new CancelBookingCommandResponse(bookingDto, "Too late to cancel booking");
        }
        
        var user = await grpcClient.GetUserByIdAsync(command.UserId);

        result.Status = BookingStatus.Cancelled;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        await publishEndpoint.Publish(new BookingCancelledIntegrationEvent
        {
            BookingId = result.Id,
            UserName = user.Username,
            UserEmail = user.Email
        }, cancellationToken);
        
        logger.LogInformation("Booking cancelled with id {Id} and event published", result.Id);  
        
        return new CancelBookingCommandResponse(bookingDto, "Booking cancelled successfully");
    }
}