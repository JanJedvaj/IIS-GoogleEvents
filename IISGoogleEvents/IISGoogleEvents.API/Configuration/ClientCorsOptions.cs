namespace IISGoogleEvents.API.Configuration;

public class ClientCorsOptions
{
    public const string SectionName = nameof(ClientCorsOptions);

    public string[] AllowedOrigins { get; set; } = [];
    public bool AllowCredentials { get; set; }
}
