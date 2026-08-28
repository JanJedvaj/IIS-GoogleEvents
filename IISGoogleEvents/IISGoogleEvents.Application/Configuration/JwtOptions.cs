namespace IISGoogleEvents.Application.Configuration;

public class JwtOptions
{
    public const string SectionName = nameof(JwtOptions);

    public string Key { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenExpirationInMinutes { get; set; }
    public int RefreshTokenExpirationInMinutes { get; set; }
}
