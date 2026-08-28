namespace IISGoogleEvents.Infrastructure.Configuration;

public class GoogleOptions
{
    public const string SectionName = nameof(GoogleOptions);

    public string BaseUrl { get; set; } = "https://www.googleapis.com/calendar/v3/";
    public string CalendarId { get; set; } = "primary";
    public string ServiceAccountKeyPath { get; set; } = null!;
}
