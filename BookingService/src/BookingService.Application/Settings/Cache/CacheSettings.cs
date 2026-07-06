namespace BookingService.Application.Settings.Cache;

public class CacheSettings
{
    public const string SectionName = "CacheSettings";

    public TimeSpan TimeToLive { get; set; }
}