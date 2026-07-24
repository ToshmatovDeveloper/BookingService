namespace BookingService.Auth.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    
    public string Token  { get; set; }
    
    public Guid AccountId { get; set; }
    
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresOnUtc { get; set; }
    
    public Account Account { get; set; }
    
}