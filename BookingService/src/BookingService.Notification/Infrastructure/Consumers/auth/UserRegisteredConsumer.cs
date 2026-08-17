using BookingService.Contracts.Events.auth;
using BookingService.Notification.Application.Features;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingService.Notification.Infrastructure.Consumers.auth;

public class UserRegisteredConsumer(
    IMediator mediator, 
    ILogger<UserRegisteredConsumer> logger) : IConsumer<UserRegisteredIntegrationEvent>
{
    public Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        var message = context.Message;
        
        logger.LogInformation("Processing user registered event for UserId: {UserId}, Email: {Email}", message.UserId, message.UserEmail);

        try
        {
            var command = new SendMailCommand(
                ReceiverAddress: message.UserEmail,
                Subject: "Welcome to Booking Service",
                Text: $"Hello {message.UserName}! Thank you for registering with our service."
            );

            return mediator.Send(command, context.CancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while processing user registered event for UserId: {UserId}", message.UserId);
            throw;
        }
    }
}