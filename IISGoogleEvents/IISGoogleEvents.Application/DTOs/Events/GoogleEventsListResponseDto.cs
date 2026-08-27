using System.Text.Json.Serialization;

namespace IISGoogleEvents.Application.DTOs.Events;

public class GoogleEventsListResponseDto
{
    [JsonPropertyName("items")]
    public List<GoogleEventResponseDto> Items { get; set; } = [];

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}
