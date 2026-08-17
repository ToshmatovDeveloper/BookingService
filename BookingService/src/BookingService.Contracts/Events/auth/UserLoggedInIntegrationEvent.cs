namespace BookingService.Contracts.Events.auth;

public record UserLoggedInIntegrationEvent
{
    public Guid UserId { get; init; } 
    
    public string UserEmail { get; init; } = default!;
    
    public string UserName { get; init; } = default!;
}