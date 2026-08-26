using IISGoogleEvents.Application.Interfaces.Services;
using IISGoogleEvents.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IISGoogleEvents.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
