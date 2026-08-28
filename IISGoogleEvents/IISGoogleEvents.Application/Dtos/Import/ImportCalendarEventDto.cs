using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace IISGoogleEvents.Application.Dtos.Import;

public class ImportCalendarEventDto
{
    [JsonPropertyName("googleEventId")]
    [XmlElement("googleEventId")]
    public string GoogleEventId { get; set; } = "";

    [JsonPropertyName("summary")]
    [XmlElement("summary")]
    public string Summary { get; set; } = "";

    [JsonPropertyName("description")]
    [XmlElement("description", IsNullable = true)]
    public string? Description { get; set; }

    [JsonPropertyName("location")]
    [XmlElement("location", IsNullable = true)]
    public string? Location { get; set; }

    [JsonPropertyName("start")]
    [XmlElement("start")]
    public DateTimeOffset Start { get; set; }

    [JsonPropertyName("end")]
    [XmlElement("end")]
    public DateTimeOffset End { get; set; }

    [JsonPropertyName("isAllDay")]
    [XmlElement("isAllDay")]
    public bool IsAllDay { get; set; }

    [JsonPropertyName("status")]
    [XmlElement("status")]
    public string? Status { get; set; }

    [JsonPropertyName("htmlLink")]
    [XmlElement("htmlLink")]
    public string? HtmlLink { get; set; }

    [JsonPropertyName("created")]
    [XmlElement("created")]
    public DateTimeOffset? Created { get; set; }

    [JsonPropertyName("updated")]
    [XmlElement("updated")]
    public DateTimeOffset? Updated { get; set; }
}
