using IISGoogleEvents.Application.Dtos.Events;
using IISGoogleEvents.Application.Models;

namespace IISGoogleEvents.Application.Interfaces;

public interface ICalendarEventService
{
    CalendarCapabilitiesDto Capabilities { get; }

    Task<StandardResponse<IEnumerable<CalendarEventDto>>> SearchAsync(string? query = null);
    Task<StandardResponse<CalendarEventDto>> GetAsync(string googleEventId);
    Task<StandardResponse<CalendarEventDto>> CreateAsync(CreateCalendarEventDto request);
    Task<StandardResponse<CalendarEventDto>> UpdateAsync(string googleEventId, UpdateCalendarEventDto request);
    Task<StandardResponse<bool>> DeleteAsync(string googleEventId);
}
