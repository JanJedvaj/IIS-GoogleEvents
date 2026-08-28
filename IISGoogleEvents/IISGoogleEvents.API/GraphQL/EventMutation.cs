using HotChocolate;
using IISGoogleEvents.Application.Dtos.Events;
using IISGoogleEvents.Application.Interfaces;
using IISGoogleEvents.Infrastructure.Enums;

namespace IISGoogleEvents.API.GraphQL;

public class EventMutation
{
    [AuthorizeMinRole(Roles.Admin)]
    public async Task<CalendarEventDto> CreateEvent(
        [Service] ICalendarEventService calendarEventService,
        CreateEventInput input) =>
        (await calendarEventService.CreateAsync(input.ToDto())).UnwrapOrThrow();

    [AuthorizeMinRole(Roles.Admin)]
    public async Task<CalendarEventDto> UpdateEvent(
        [Service] ICalendarEventService calendarEventService,
        string id,
        UpdateEventInput input) =>
        (await calendarEventService.UpdateAsync(id, input.ToDto())).UnwrapOrThrow();

    [AuthorizeMinRole(Roles.Admin)]
    public async Task<bool> DeleteEvent(
        [Service] ICalendarEventService calendarEventService,
        string id) =>
        (await calendarEventService.DeleteAsync(id)).UnwrapOrThrow();
}
