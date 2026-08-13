using BookingService.Notification.Application.Settings;
using BookingService.Notification.Domain;
using BookingService.Notification.Domain.Enum;
using BookingService.Notification.Infrastructure;
using MailKit.Net.Smtp;
using MediatR;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BookingService.Notification.Application.Features;

public record SendMailCommand(string ReceiverAddress, string Subject, string Text) : IRequest<SendMailResponse>;
public record SendMailResponse(SendMailStatus Status, string ReceiverAddress);

public class SendMailCommandHandler(
    NotificationDbContext dbContext,
    IOptions<SmtpSettings> smtpOptions) : IRequestHandler<SendMailCommand, SendMailResponse>
{
    public async Task<SendMailResponse> Handle(SendMailCommand command, CancellationToken cancellationToken)
    {
        var mail = new Mail
        {
            ReceiverAddress = command.ReceiverAddress,
            Subject = command.Subject,
            Text = command.Text,
            Status = SendMailStatus.Pending
        };

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Booking Service", smtpOptions.Value.User));
        message.To.Add(new MailboxAddress(string.Empty, command.ReceiverAddress));
        message.Subject = command.Subject;

        message.Body = new TextPart("plain")
        {
            Text = command.Text
        };

        try
        {
            using var client = new SmtpClient();
            
            await client.ConnectAsync(smtpOptions.Value.Host, smtpOptions.Value.Port, false, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            mail.Status = SendMailStatus.Sent;
            mail.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            mail.Status = SendMailStatus.Failed;
            mail.ErrorMessage = ex.Message;
            
            throw;
        }
        finally
        {
            await dbContext.Mails.AddAsync(mail, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new SendMailResponse(mail.Status, mail.ReceiverAddress);
    }
}