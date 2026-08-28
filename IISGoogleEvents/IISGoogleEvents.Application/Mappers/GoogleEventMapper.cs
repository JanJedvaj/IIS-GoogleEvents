using IISGoogleEvents.Application.Dtos.Events;
using IISGoogleEvents.Infrastructure.Clients.Google;

namespace IISGoogleEvents.Application.Mappers;

public static class GoogleEventMapper
{
    public static CalendarEventDto ToDto(this GoogleEventResponseDto response)
    {
        return new CalendarEventDto
        {
            GoogleEventId = response.Id,
            Summary = response.Summary ?? "(no title)",
            Description = response.Description,
            Location = response.Location,
            Start = ResolveDateTime(response.Start),
            End = ResolveDateTime(response.End),
            IsAllDay = response.Start.Date != null,
            Status = response.Status,
            HtmlLink = response.HtmlLink ?? string.Empty,
            Created = response.Created,
            Updated = response.Updated
        };
    }

    private static DateTimeOffset ResolveDateTime(GoogleEventDateTimeDto value) =>
        value.DateTimeValue ?? (value.Date != null ? DateTimeOffset.Parse(value.Date) : DateTimeOffset.UtcNow);
}
