using IISGoogleEvents.Application.Configurations;
using IISGoogleEvents.Application.Interfaces.Security;
using IISGoogleEvents.Infrastructure.Grpc;
using IISGoogleEvents.Infrastructure.Security.Helpers;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}
