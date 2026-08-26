using IISGoogleEvents.Application.Interfaces.Security;
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
}
