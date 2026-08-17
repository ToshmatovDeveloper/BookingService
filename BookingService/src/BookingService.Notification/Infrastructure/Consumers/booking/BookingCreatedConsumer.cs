using BookingService.Contracts.Events;
using BookingService.Notification.Application.Features;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingService.Notification.Infrastructure.Consumers.booking;

public class BookingCreatedConsumer(
    IMediator mediator, 
    ILogger<BookingCreatedConsumer> logger) : IConsumer<BookingCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<BookingCreatedIntegrationEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Processing booking created event for BookingId: {BookingId}, Email: {Email}", message.BookingId, message.UserEmail);

        try
        {
            var command = new SendMailCommand(
                ReceiverAddress: message.UserEmail,
                Subject: "Booking Confirmation",
                Text: $"Hello! Your booking #{message.BookingId} has been successfully created."
            );

            await mediator.Send(command, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process email sending for BookingId: {BookingId}. Message will be retried or moved to DLQ.", message.BookingId);
            throw; 
        }
    }
}