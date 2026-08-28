using System.Text.Json.Serialization;

namespace IISGoogleEvents.Infrastructure.Clients.Google;

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

public class GoogleEventDateTimeDto
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("dateTime")]
    public DateTimeOffset? DateTimeValue { get; set; }

    [JsonPropertyName("timeZone")]
    public string? TimeZone { get; set; }
}
