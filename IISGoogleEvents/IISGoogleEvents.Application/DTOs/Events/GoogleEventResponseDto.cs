using System.Text.Json.Serialization;

namespace IISGoogleEvents.Application.DTOs.Events;

/// <summary>
/// Wire shape of a Google Calendar API Event resource (the subset we surface).
/// Upstream-specific, but placed in Application to match the Infrastructure ->
/// Application dependency direction - Infrastructure's HttpClient and mapper
/// both need this type without Application needing to know about Infrastructure.
/// </summary>
public class GoogleEventResponseDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "confirmed";

    [JsonPropertyName("htmlLink")]
    public string? HtmlLink { get; set; }

    [JsonPropertyName("created")]
    public DateTimeOffset Created { get; set; }

    [JsonPropertyName("updated")]
    public DateTimeOffset Updated { get; set; }

    [JsonPropertyName("start")]
    public GoogleEventDateTimeDto Start { get; set; } = new();

    [JsonPropertyName("end")]
    public GoogleEventDateTimeDto End { get; set; } = new();
}

/// <summary>
/// Google represents an all-day event with "date" (yyyy-MM-dd) and a timed
/// event with "dateTime" - never both. Which one is present is exactly what
/// maps to CalendarEvent.IsAllDay.
/// </summary>
public class GoogleEventDateTimeDto
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("dateTime")]
    public DateTimeOffset? DateTimeValue { get; set; }

    [JsonPropertyName("timeZone")]
    public string? TimeZone { get; set; }
}
