using BookingService.Contracts.Events.auth;
using BookingService.Notification.Application.Features;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingService.Notification.Infrastructure.Consumers.auth;

public class UserLoggedInConsumer(
    IMediator mediator,
    ILogger<UserLoggedInConsumer>  logger) : IConsumer<UserLoggedInIntegrationEvent>
{
    public Task Consume(ConsumeContext<UserLoggedInIntegrationEvent> context)
    {
        var message = context.Message;
        
        logger.LogInformation("Processing user logged in event for UserId: {UserId}, Email: {Email}", message.UserId, message.UserEmail);

        try
        {
            var command = new SendMailCommand(
                ReceiverAddress: message.UserEmail,
                Subject: "Login Notification",
                Text: $"Hello {message.UserName}! You have successfully logged in."
            );

            return mediator.Send(command, context.CancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while processing user logged in event for UserId: {UserId}", message.UserId);
            throw;
        }
    }
}