namespace IISGoogleEvents.Application.Configurations;

public class CorsConfig
{
    public string[] AllowedOrigins { get; set; } = [];
    public bool AllowCredentials { get; set; }
}
