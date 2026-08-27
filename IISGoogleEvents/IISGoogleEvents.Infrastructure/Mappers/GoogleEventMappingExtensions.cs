using IISGoogleEvents.Application.DTOs.Events;

namespace IISGoogleEvents.Infrastructure.Mappers;

/// <summary>
/// Upstream-shape-specific: unwraps Google's start/end "date vs dateTime"
/// ambiguity into our flat Start/End + IsAllDay fields.
/// </summary>
public static class GoogleEventMappingExtensions
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
