using HotChocolate;
using IISGoogleEvents.Application.Dtos.Events;
using IISGoogleEvents.Application.Interfaces;
using IISGoogleEvents.Infrastructure.Enums;

namespace IISGoogleEvents.API.GraphQL;

public class EventQuery
{
    [AuthorizeMinRole(Roles.User)]
    public async Task<IEnumerable<CalendarEventDto>> Events(
        [Service] ICalendarEventService calendarEventService,
        string? query = null) =>
        (await calendarEventService.SearchAsync(query)).UnwrapOrThrow();

    [AuthorizeMinRole(Roles.User)]
    public async Task<CalendarEventDto?> Event(
        [Service] ICalendarEventService calendarEventService,
        string id) =>
        (await calendarEventService.GetAsync(id)).UnwrapOrDefault();

    [AuthorizeMinRole(Roles.User)]
    public CalendarCapabilitiesDto Capabilities(
        [Service] ICalendarEventService calendarEventService) =>
        calendarEventService.Capabilities;
}
