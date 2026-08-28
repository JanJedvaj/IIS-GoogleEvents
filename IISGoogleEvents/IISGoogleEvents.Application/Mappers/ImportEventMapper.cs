using IISGoogleEvents.Application.Dtos.Import;
using IISGoogleEvents.Infrastructure.Entities;

namespace IISGoogleEvents.Application.Mappers;

public static class ImportEventMapper
{
    private const string DefaultStatus = "confirmed";
    private const string DefaultHtmlLink = "local://import";

    public static CalendarEvent ToEntity(this ImportCalendarEventDto dto)
    {
        var now = DateTimeOffset.UtcNow;

        return new CalendarEvent
        {
            GoogleEventId = dto.GoogleEventId.Trim(),
            Summary = dto.Summary.Trim(),
            Description = NormalizeToNull(dto.Description),
            Location = NormalizeToNull(dto.Location),
            Start = dto.Start.ToUniversalTime(),
            End = dto.End.ToUniversalTime(),
            IsAllDay = dto.IsAllDay,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? DefaultStatus : dto.Status.Trim(),
            HtmlLink = string.IsNullOrWhiteSpace(dto.HtmlLink) ? DefaultHtmlLink : dto.HtmlLink.Trim(),
            Created = (dto.Created ?? now).ToUniversalTime(),
            Updated = (dto.Updated ?? now).ToUniversalTime()
        };
    }

    private static string? NormalizeToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
