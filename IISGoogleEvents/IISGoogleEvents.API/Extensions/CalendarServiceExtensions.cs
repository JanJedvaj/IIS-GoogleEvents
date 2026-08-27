using IISGoogleEvents.Application.Configurations;
using IISGoogleEvents.Application.Interfaces.Services;
using IISGoogleEvents.Application.Services;
using IISGoogleEvents.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace IISGoogleEvents.API.Extensions;

public static class CalendarServiceExtensions
{
    /// <summary>
    /// The switch requirement 5 asks for. Both concrete implementations are
    /// always registered under their own type; only the ICalendarEventService
    /// binding is decided, per scope, at first resolution. REST today and
    /// GraphQL later both depend on the interface only.
    /// </summary>
    public static IServiceCollection AddCalendarServiceSwitch(this IServiceCollection services)
    {
        services.AddScoped<ICalendarEventService>(provider =>
        {
            var appConfig = provider.GetRequiredService<IOptions<AppConfig>>().Value;
            return appConfig.DataSource == DataSourceType.Local
                ? provider.GetRequiredService<LocalCalendarEventService>()
                : provider.GetRequiredService<ExternalCalendarEventService>();
        });

        return services;
    }
}
