using System.Net.Http.Headers;
using System.Net.Http.Json;
using Google.Apis.Auth.OAuth2;
using IISGoogleEvents.Application.Configurations;
using IISGoogleEvents.Application.DTOs.Events;
using Microsoft.Extensions.Options;

namespace IISGoogleEvents.Infrastructure.Services;

/// <summary>
/// Hand-written HttpClient against the real Google Calendar REST API, plus
/// Google.Apis.Auth purely to mint the bearer token from the service-account
/// key file. Deliberately not the generated Google.Apis.Calendar.v3 client -
/// same lesson the reference learned and then undid with its abandoned Kiota
/// client for Notion.
/// </summary>
public class GoogleCalendarHttpClient
{
    private const string CalendarScope = "https://www.googleapis.com/auth/calendar";

    private readonly HttpClient _httpClient;
    private readonly GoogleConfig _config;
    private readonly ServiceAccountCredential _credential;

    public GoogleCalendarHttpClient(HttpClient httpClient, IOptions<GoogleConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;

        var keyPath = ResolveKeyPath(_config.ServiceAccountKeyPath);
        if (!File.Exists(keyPath))
        {
            throw new InvalidOperationException(
                $"Missing Google service-account key at '{keyPath}'. " +
                "See the README for the GCP setup steps. Only needed while AppConfig:DataSource is External.");
        }

        // CredentialFactory.FromFile<T>, not the obsolete GoogleCredential.FromFile -
        // the latter is flagged as a security risk in recent Google.Apis.Auth versions.
        _credential = CredentialFactory.FromFile<ServiceAccountCredential>(keyPath);
        _credential.Scopes = [CalendarScope];
    }

    /// <summary>
    /// GoogleConfig__ServiceAccountKeyPath is a relative path in .env (e.g.
    /// "secrets/google-service-account.json"), but "dotnet run"'s working
    /// directory is the API project folder, not the repo root the secret
    /// actually lives under. Resolve it the same way DotEnv locates the repo
    /// root - walk up from the running assembly until a ".git" directory turns up.
    /// </summary>
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
        // PUT, not PATCH - requirement 5 spells out GET/POST/PUT/DELETE literally.
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

        // StatusCode set explicitly (not just embedded in the message) so callers can
        // tell "not found" apart from a real failure without parsing response text.
        throw new HttpRequestException($"Google Calendar API returned {response.StatusCode}: {body}", null, response.StatusCode);
    }

    #endregion
}
