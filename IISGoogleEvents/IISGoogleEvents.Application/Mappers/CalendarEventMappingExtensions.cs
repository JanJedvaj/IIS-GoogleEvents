using IISGoogleEvents.Application.DTOs.Events;
using IISGoogleEvents.Domain.Entities;

namespace IISGoogleEvents.Application.Mappers;

public static class CalendarEventMappingExtensions
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

    public static CalendarEvent ToEntity(this CalendarEventDto dto)
    {
        return new CalendarEvent
        {
            GoogleEventId = dto.GoogleEventId,
            Summary = dto.Summary,
            Description = dto.Description,
            Location = dto.Location,
            Start = dto.Start,
            End = dto.End,
            IsAllDay = dto.IsAllDay,
            Status = dto.Status,
            HtmlLink = dto.HtmlLink,
            Created = dto.Created,
            Updated = dto.Updated
        };
    }
}
