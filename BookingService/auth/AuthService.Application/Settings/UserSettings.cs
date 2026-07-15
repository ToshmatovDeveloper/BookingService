namespace AuthService.Application.Settings;

public class UserSettings
{
    public const string SectionName = "UserSettings";
    
    public bool RequireUniqueEmail { get; set; } = true;
}