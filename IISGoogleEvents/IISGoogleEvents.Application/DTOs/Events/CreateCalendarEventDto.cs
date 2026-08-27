using System.ComponentModel.DataAnnotations;

namespace IISGoogleEvents.Application.DTOs.Events;

public class CreateCalendarEventDto
{
    [Required]
    public string Summary { get; set; } = null!;

    public string? Description { get; set; }
    public string? Location { get; set; }

    [Required]
    public DateTimeOffset Start { get; set; }

    [Required]
    public DateTimeOffset End { get; set; }

    public bool IsAllDay { get; set; }
}
