using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace IISGoogleEvents.Application.Dtos.Import;

[XmlRoot("calendarEvents", Namespace = "http://iis.algebra.hr/calendar")]
public class ImportCalendarEventsDto
{
    [JsonPropertyName("calendarEvents")]
    [XmlElement("calendarEvent")]
    public List<ImportCalendarEventDto> CalendarEvents { get; set; } = [];
}
