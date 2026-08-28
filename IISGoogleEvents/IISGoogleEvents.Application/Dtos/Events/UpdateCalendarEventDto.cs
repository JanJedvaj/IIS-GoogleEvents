namespace IISGoogleEvents.Application.Dtos.Events;

public class UpdateCalendarEventDto
{
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset? Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public bool? IsAllDay { get; set; }
}
