using IISGoogleEvents.Domain.Abstractions;

namespace IISGoogleEvents.Domain.Entities;

/// <summary>
/// Local mirror of a Google Calendar Event resource. Direct analogue of the
/// reference project's NotionObject - the one domain-specific entity.
/// </summary>
public class CalendarEvent : BaseEntity
{
    public string GoogleEventId { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public bool IsAllDay { get; set; }

    /// <summary>
    /// "confirmed" / "tentative" / "cancelled" - Google's own status values.
    /// "cancelled" doubles as our soft delete: DELETE sets it, every read filters it out.
    /// </summary>
    public string Status { get; set; } = "confirmed";

    public string HtmlLink { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Updated { get; set; }
}
