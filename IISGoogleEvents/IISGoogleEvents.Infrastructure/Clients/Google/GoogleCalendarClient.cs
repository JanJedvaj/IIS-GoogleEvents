using System.Net.Http.Headers;
using System.Net.Http.Json;
using Google.Apis.Auth.OAuth2;
using IISGoogleEvents.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace IISGoogleEvents.Infrastructure.Clients.Google;

public class GoogleCalendarClient
{
    private const string CalendarScope = "https://www.googleapis.com/auth/calendar";

    private readonly HttpClient _httpClient;
    private readonly GoogleOptions _config;
    private readonly ServiceAccountCredential _credential;

    public GoogleCalendarClient(HttpClient httpClient, IOptions<GoogleOptions> config)
    {
        _httpClient = httpClient;
        _config = config.Value;

        var keyPath = ResolveKeyPath(_config.ServiceAccountKeyPath);
        if (!File.Exists(keyPath))
        {
            throw new InvalidOperationException(
                $"Nedostaje kljuc Google servisnog racuna na '{keyPath}'. " +
                "Koraci za GCP postavljanje su u READMEu. Potreban je samo dok je AppOptions:DataSource postavljen na External.");
        }

        _credential = CredentialFactory.FromFile<ServiceAccountCredential>(keyPath);
        _credential.Scopes = [CalendarScope];
    }

    private static string ResolveKeyPath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath) || Path.IsPathRooted(configuredPath))
            return configuredPath;

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
                return Path.Combine(directory.FullName, configuredPath);

            directory = directory.Parent;
        }

        return configuredPath;
    }

    public async Task<GoogleEventsListResponseDto?> ListEventsAsync(string? query, CancellationToken cancellationToken = default)
    {
        var path = $"calendars/{Uri.EscapeDataString(_config.CalendarId)}/events";
        if (!string.IsNullOrWhiteSpace(query))
            path += $"?q={Uri.EscapeDataString(query)}";

        var response = await SendAsync(HttpMethod.Get, path, null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<GoogleEventsListResponseDto>(cancellationToken: cancellationToken);
    }

    public async Task<GoogleEventResponseDto?> GetEventAsync(string eventId, CancellationToken cancellationToken = default)
    {
        var path = $"calendars/{Uri.EscapeDataString(_config.CalendarId)}/events/{Uri.EscapeDataString(eventId)}";
        var response = await SendAsync(HttpMethod.Get, path, null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<GoogleEventResponseDto>(cancellationToken: cancellationToken);
    }

    public async Task<GoogleEventResponseDto?> CreateEventAsync(object body, CancellationToken cancellationToken = default)
    {
        var path = $"calendars/{Uri.EscapeDataString(_config.CalendarId)}/events";
        var response = await SendAsync(HttpMethod.Post, path, body, cancellationToken);
        return await response.Content.ReadFromJsonAsync<GoogleEventResponseDto>(cancellationToken: cancellationToken);
    }

    public async Task<GoogleEventResponseDto?> UpdateEventAsync(string eventId, object body, CancellationToken cancellationToken = default)
    {
        var path = $"calendars/{Uri.EscapeDataString(_config.CalendarId)}/events/{Uri.EscapeDataString(eventId)}";
        var response = await SendAsync(HttpMethod.Put, path, body, cancellationToken);
        return await response.Content.ReadFromJsonAsync<GoogleEventResponseDto>(cancellationToken: cancellationToken);
    }

    public async Task DeleteEventAsync(string eventId, CancellationToken cancellationToken = default)
    {
        var path = $"calendars/{Uri.EscapeDataString(_config.CalendarId)}/events/{Uri.EscapeDataString(eventId)}";
        await SendAsync(HttpMethod.Delete, path, null, cancellationToken);
    }

    #region Private methods

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        var token = await _credential.GetAccessTokenForRequestAsync(cancellationToken: cancellationToken);

        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body != null)
            request.Content = JsonContent.Create(body);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessWithBodyAsync(response);
        return response;
    }

    private static async Task EnsureSuccessWithBodyAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();

        throw new HttpRequestException($"Google Calendar API je vratio {response.StatusCode}: {body}", null, response.StatusCode);
    }

    #endregion
}
