namespace IISGoogleEvents.Application.DTOs.Events;

public class CalendarEventDto
{
    public string GoogleEventId { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public bool IsAllDay { get; set; }
    public string Status { get; set; } = "confirmed";
    public string HtmlLink { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;
}
