using BookingService.Notification.Domain.Enum;

namespace BookingService.Notification.Domain;

public class Mail
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ReceiverAddress { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Text { get; set; } = default!; // или Body
    public SendMailStatus Status { get; set; } = SendMailStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
}