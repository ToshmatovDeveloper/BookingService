using BookingService.Notification.Application.Features;
using BookingService.Notification.Domain.Enum;
using BookingService.Notification.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookingService.IntegrationTests.notificationService;

public class NotificationTests(
    NotificationServiceTestWebFactory factory)
    : IClassFixture<NotificationServiceTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase = factory.ResetDatabaseAsync;
    private IServiceProvider Services { get; set; } = factory.Services;

    [Fact]
    public async Task SendMail_with_valid_data_should_succeed_and_save_to_db_with_sent_status()
    {
        // Arrange
        await using var scope = Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var cancellationToken = CancellationToken.None;

        var receiver = "customer@mail.com";
        var subject = "Booking Confirmed";
        var text = "Your booking was successfully processed.";

        var command = new SendMailCommand(receiver, subject, text);

        // Act
        var response = await mediator.Send(command, cancellationToken);

        Assert.NotNull(response);
        Assert.Equal(SendMailStatus.Sent, response.Status);
        Assert.Equal(receiver, response.ReceiverAddress);

        var savedMail = await dbContext.Mails
            .FirstOrDefaultAsync(m => m.ReceiverAddress == receiver, cancellationToken);
            
        Assert.NotNull(savedMail);
        Assert.Equal(SendMailStatus.Sent, savedMail.Status);
        Assert.Equal(subject, savedMail.Subject);
        Assert.Equal(text, savedMail.Text);
        Assert.NotNull(savedMail.SentAt);
        Assert.Null(savedMail.ErrorMessage);
    }

    [Fact]
    public async Task SendMail_when_smtp_fails_should_throw_exception_and_save_to_db_with_failed_status()
    {
        await using var scope = Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var cancellationToken = CancellationToken.None;

        var receiver = "invalid-email-format"; 
        var command = new SendMailCommand(receiver, "Fail Test", "Should fail");

        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await mediator.Send(command, cancellationToken);
        });

        var savedMail = await dbContext.Mails
            .FirstOrDefaultAsync(m => m.ReceiverAddress == receiver, cancellationToken);

        Assert.NotNull(savedMail);
        Assert.Equal(SendMailStatus.Failed, savedMail.Status);
        Assert.False(string.IsNullOrWhiteSpace(savedMail.ErrorMessage));
        Assert.Null(savedMail.SentAt);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _resetDatabase();
}
