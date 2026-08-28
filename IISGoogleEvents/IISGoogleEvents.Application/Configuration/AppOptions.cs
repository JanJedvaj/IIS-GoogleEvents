namespace IISGoogleEvents.Application.Configuration;

public enum DataSourceType
{
    Local,
    External
}

public class AppOptions
{
    public const string SectionName = nameof(AppOptions);

    public DataSourceType DataSource { get; set; } = DataSourceType.Local;
}
