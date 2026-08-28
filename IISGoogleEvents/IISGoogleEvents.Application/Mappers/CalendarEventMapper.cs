using IISGoogleEvents.Application.Dtos.Events;
using IISGoogleEvents.Infrastructure.Entities;

namespace IISGoogleEvents.Application.Mappers;

public static class CalendarEventMapper
{
    public static CalendarEventDto ToDto(this CalendarEvent entity)
    {
        return new CalendarEventDto
        {
            GoogleEventId = entity.GoogleEventId,
            Summary = entity.Summary,
            Description = entity.Description,
            Location = entity.Location,
            Start = entity.Start,
            End = entity.End,
            IsAllDay = entity.IsAllDay,
            Status = entity.Status,
            HtmlLink = entity.HtmlLink,
            Created = entity.Created,
            Updated = entity.Updated
        };
    }
}
