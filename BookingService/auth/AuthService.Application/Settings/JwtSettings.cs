namespace AuthService.Application.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpirationInMinutes { get; set; }
}