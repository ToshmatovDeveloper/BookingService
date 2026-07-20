namespace BookingService.Auth.Application.Settings;

public class LockoutSettings
{
    public const string SectionName = "LockoutSettings";
    
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public TimeSpan DefaultLockoutTimeSpan { get; set; } = TimeSpan.FromMinutes(5);
}