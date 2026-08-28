using System.Text.Json.Serialization;

namespace IISGoogleEvents.Infrastructure.Clients.Google;

public class GoogleEventsListResponseDto
{
    [JsonPropertyName("items")]
    public List<GoogleEventResponseDto> Items { get; set; } = [];

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}
