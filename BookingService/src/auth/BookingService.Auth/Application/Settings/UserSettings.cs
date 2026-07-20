namespace BookingService.Auth.Application.Settings;

public class UserSettings
{
    public const string SectionName = "UserSettings";
    
    public bool RequireUniqueUserName { get; set; } = true;
    public bool RequireUniqueEmail { get; set; } = true;
}