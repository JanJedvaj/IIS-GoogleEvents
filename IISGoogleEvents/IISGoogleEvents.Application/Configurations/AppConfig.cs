namespace IISGoogleEvents.Application.Configurations;

public enum DataSourceType
{
    Local,
    External
}

/// <summary>
/// The "prekidac" from requirement 5: decides whether the application serves
/// its own database or proxies the upstream Google Calendar API.
/// </summary>
public class AppConfig
{
    public DataSourceType DataSource { get; set; } = DataSourceType.Local;
}
