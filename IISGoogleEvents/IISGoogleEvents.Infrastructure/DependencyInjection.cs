using IISGoogleEvents.Application.Configurations;
using IISGoogleEvents.Application.Interfaces.Security;
using IISGoogleEvents.Infrastructure.Grpc;
using IISGoogleEvents.Infrastructure.Security.Helpers;
using IISGoogleEvents.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IISGoogleEvents.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHelper, PasswordHelper>();
        services.AddScoped<ITokenHelper, TokenHelper>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, DhmzConfig dhmzConfig)
    {
        services.AddInfrastructure();

        services.Configure<DhmzConfig>(opts =>
        {
            opts.BaseUrl = dhmzConfig.BaseUrl;
            opts.XmlPath = dhmzConfig.XmlPath;
            opts.TimeoutSeconds = dhmzConfig.TimeoutSeconds;
        });

        services.AddHttpClient<DhmzHttpClient>(client =>
            {
                client.BaseAddress = new Uri(dhmzConfig.BaseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("IISGoogleEvents/1.0");
                client.DefaultRequestHeaders.Accept.ParseAdd("text/xml");
            })
            .ConfigurePrimaryHttpMessageHandler(DhmzHttpClient.CreateIPv4OnlyHandler);

        services.AddScoped<DhmzGrpcService>();

        // GoogleConfig resolves lazily via IOptions inside the factory delegate below,
        // so - unlike DhmzConfig above - it needs no plain-parameter overload of its own.
        services.AddHttpClient<GoogleCalendarHttpClient>((provider, client) =>
        {
            var googleConfig = provider.GetRequiredService<IOptions<GoogleConfig>>().Value;
            client.BaseAddress = new Uri(googleConfig.BaseUrl);
        });

        services.AddScoped<ExternalCalendarEventService>();

        return services;
    }
}
