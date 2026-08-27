using IISGoogleEvents.Application.Configurations;

namespace IISGoogleEvents.API.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppConfig>(configuration.GetSection(nameof(AppConfig)));
        services.Configure<JwtConfig>(configuration.GetSection(nameof(JwtConfig)));
        services.Configure<CorsConfig>(configuration.GetSection(nameof(CorsConfig)));

        return services;
    }

    public static string GetDbConnectionString(this IConfiguration configuration) =>
        Required(configuration.GetConnectionString("Db"), "ConnectionStrings__Db");

    /// <summary>
    /// Secrets come from .env, so a missing one should name the variable and the file
    /// rather than surfacing later as a null reference deep inside EF or JWT setup.
    /// </summary>
    internal static string Required(string? value, string variableName) =>
        !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException(
                $"Missing configuration '{variableName}'. Copy .env.example to .env in the repository root and set it.");
}
