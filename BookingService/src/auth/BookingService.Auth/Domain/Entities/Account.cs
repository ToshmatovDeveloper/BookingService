using Microsoft.AspNetCore.Identity;

namespace BookingService.Auth.Domain.Entities;

public sealed class Account : IdentityUser<Guid>
{
    private Account() { }

    public Account(string email, string userName)
    {
        Id = Guid.CreateVersion7();
        Email = email;
        UserName = userName;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
}