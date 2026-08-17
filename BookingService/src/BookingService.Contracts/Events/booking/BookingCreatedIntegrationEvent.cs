namespace BookingService.Contracts.Events;

public record BookingCreatedIntegrationEvent
{
    public Guid BookingId { get; init; }
    public string UserEmail { get; init; } = default!;
    public string UserName { get; init; } = default!;
}