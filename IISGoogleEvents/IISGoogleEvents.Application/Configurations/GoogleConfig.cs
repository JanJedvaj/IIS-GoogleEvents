namespace IISGoogleEvents.Application.Configurations;

/// <summary>
/// Non-secret shape for the upstream Google Calendar API. The service-account
/// key file path is a secret and comes from .env (GoogleConfig__ServiceAccountKeyPath),
/// not from appsettings.json.
/// </summary>
public class GoogleConfig
{
    public string BaseUrl { get; set; } = "https://www.googleapis.com/calendar/v3/";
    public string CalendarId { get; set; } = "primary";
    public string ServiceAccountKeyPath { get; set; } = null!;
}
