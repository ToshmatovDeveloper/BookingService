using BookingService.Contracts.Events;
using BookingService.Notification.Application.Features;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingService.Notification.Infrastructure;

public class BookingCancelledConsumer(
    IMediator mediator, 
    ILogger<BookingCancelledConsumer> logger) : IConsumer<BookingCancelledIntegrationEvent>
{
    public Task Consume(ConsumeContext<BookingCancelledIntegrationEvent> context)
    {
        var message = context.Message;
        
        logger.LogInformation("Processing booking cancelled event for BookingId: {BookingId}, Email: {Email}", message.BookingId, message.UserEmail);

        try
        {
            var command = new SendMailCommand(
                ReceiverAddress: message.UserEmail,
                Subject: "Booking Cancellation",
                Text: $"Hello! Your booking #{message.BookingId} has been cancelled."
            );

            return mediator.Send(command, context.CancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while processing booking cancelled event for BookingId: {BookingId}", message.BookingId);
            throw;
        }
    }
}