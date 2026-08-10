namespace BookingService.Domain.Interfaces;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime ModifiedAt { get; set; }
}