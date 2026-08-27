using IISGoogleEvents.Application.DTOs.Events;
using IISGoogleEvents.Application.Models;

namespace IISGoogleEvents.Application.Interfaces.Services;

/// <summary>
/// One interface, two implementations (Local over Postgres / External over
/// the live Google Calendar API), chosen by AppConfig.DataSource. No caller -
/// REST today, GraphQL later - ever knows which one is live.
/// </summary>
public interface ICalendarEventService
{
    Task<StandardResponse<IEnumerable<CalendarEventDto>>> SearchAsync(string? query = null);
    Task<StandardResponse<CalendarEventDto>> GetAsync(string googleEventId);
    Task<StandardResponse<CalendarEventDto>> CreateAsync(CreateCalendarEventDto request);
    Task<StandardResponse<CalendarEventDto>> UpdateAsync(string googleEventId, UpdateCalendarEventDto request);
    Task<StandardResponse<bool>> DeleteAsync(string googleEventId);
}
