namespace IISGoogleEvents.Infrastructure.Configuration;

public class DhmzOptions
{
    public const string SectionName = nameof(DhmzOptions);

    public string BaseUrl { get; set; } = string.Empty;
    public string XmlPath { get; set; } = string.Empty;
    public double TimeoutSeconds { get; set; }

    public double CacheMinutes { get; set; } = 15.0;
}
