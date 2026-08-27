using System.Net;
using IISGoogleEvents.Application.DTOs.Events;
using IISGoogleEvents.Application.Interfaces.Services;
using IISGoogleEvents.Application.Models;
using IISGoogleEvents.Infrastructure.Mappers;

namespace IISGoogleEvents.Infrastructure.Services;

/// <summary>
/// ICalendarEventService over the live Google Calendar API. Resolved when
/// AppConfig.DataSource == External. Request bodies here are upstream-shaped,
/// which is why the request-building lives in this layer rather than Local's.
/// </summary>
public class ExternalCalendarEventService : ICalendarEventService
{
    private readonly GoogleCalendarHttpClient _httpClient;

    public ExternalCalendarEventService(GoogleCalendarHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<StandardResponse<IEnumerable<CalendarEventDto>>> SearchAsync(string? query = null)
    {
        try
        {
            var response = await _httpClient.ListEventsAsync(query);
            var dtos = response?.Items.Select(i => i.ToDto()) ?? [];
            return StandardResponse<IEnumerable<CalendarEventDto>>.Create(ResultStatus.Ok, dtos);
        }
        catch (Exception ex)
        {
            return StandardResponse<IEnumerable<CalendarEventDto>>.Create(ResultStatus.InternalError, message: ex.Message);
        }
    }

    public async Task<StandardResponse<CalendarEventDto>> GetAsync(string googleEventId)
    {
        try
        {
            var response = await _httpClient.GetEventAsync(googleEventId);

            // Google keeps a deleted event as a "cancelled" tombstone for a grace
            // period rather than purging it immediately - events.list already
            // excludes those by default, but a direct events.get does not. Filter
            // it here too, so Get behaves like Search and like Local's soft delete:
            // once deleted, every read 404s, regardless of which source is live.
            if (response == null || response.Status == "cancelled")
                return StandardResponse<CalendarEventDto>.Create(ResultStatus.NotFound, message: "Event not found");

            return StandardResponse<CalendarEventDto>.Create(ResultStatus.Ok, response.ToDto());
        }
        catch (Exception ex) when (IsNotFound(ex))
        {
            return StandardResponse<CalendarEventDto>.Create(ResultStatus.NotFound, message: "Event not found");
        }
        catch (Exception ex)
        {
            return StandardResponse<CalendarEventDto>.Create(ResultStatus.InternalError, message: ex.Message);
        }
    }

    public async Task<StandardResponse<CalendarEventDto>> CreateAsync(CreateCalendarEventDto request)
    {
        try
        {
            var body = BuildEventBody(request.Summary, request.Description, request.Location, request.Start, request.End, request.IsAllDay);
            var response = await _httpClient.CreateEventAsync(body);

            return response == null
                ? StandardResponse<CalendarEventDto>.Create(ResultStatus.InternalError, message: "Failed to parse Google Calendar API response")
                : StandardResponse<CalendarEventDto>.Create(ResultStatus.Created, response.ToDto());
        }
        catch (Exception ex)
        {
            return StandardResponse<CalendarEventDto>.Create(ResultStatus.InternalError, message: ex.Message);
        }
    }

    public async Task<StandardResponse<CalendarEventDto>> UpdateAsync(string googleEventId, UpdateCalendarEventDto request)
    {
        try
        {
            // Google's events.update is a full replace, so fetch the current resource
            // first and patch it - the upstream-shape equivalent of Local's partial update.
            var current = await _httpClient.GetEventAsync(googleEventId);

            // Same tombstone gap as GetAsync: without this check, updating a deleted
            // event's id would "resurrect" its cancelled tombstone with new data.
            if (current == null || current.Status == "cancelled")
                return StandardResponse<CalendarEventDto>.Create(ResultStatus.NotFound, message: "Event not found");

            var isAllDay = request.IsAllDay ?? current.Start.Date != null;
            var start = request.Start ?? ResolveDateTime(current.Start);
            var end = request.End ?? ResolveDateTime(current.End);

            var body = BuildEventBody(
                request.Summary ?? current.Summary ?? string.Empty,
                request.Description ?? current.Description,
                request.Location ?? current.Location,
                start, end, isAllDay);

            var response = await _httpClient.UpdateEventAsync(googleEventId, body);

            return response == null
                ? StandardResponse<CalendarEventDto>.Create(ResultStatus.InternalError, message: "Failed to parse Google Calendar API response")
                : StandardResponse<CalendarEventDto>.Create(ResultStatus.Ok, response.ToDto());
        }
        catch (Exception ex) when (IsNotFound(ex))
        {
            return StandardResponse<CalendarEventDto>.Create(ResultStatus.NotFound, message: "Event not found");
        }
        catch (Exception ex)
        {
            return StandardResponse<CalendarEventDto>.Create(ResultStatus.InternalError, message: ex.Message);
        }
    }

    public async Task<StandardResponse<bool>> DeleteAsync(string googleEventId)
    {
        try
        {
            // A real upstream delete, not a soft one - Google Calendar has no
            // "trash" flag equivalent to Notion's in_trash to PATCH instead.
            await _httpClient.DeleteEventAsync(googleEventId);
            return StandardResponse<bool>.Create(ResultStatus.Ok, true);
        }
        catch (Exception ex) when (IsNotFound(ex))
        {
            // Deleting an id that's already gone (or never existed) - Google
            // returns 404/410 here, not the tombstone shape Get/Update see.
            return StandardResponse<bool>.Create(ResultStatus.NotFound, false, message: "Event not found");
        }
        catch (Exception ex)
        {
            return StandardResponse<bool>.Create(ResultStatus.InternalError, false, message: ex.Message);
        }
    }

    #region Private methods

    private static bool IsNotFound(Exception ex) =>
        ex is HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.Gone };

    private static Dictionary<string, object?> BuildEventBody(
        string summary, string? description, string? location, DateTimeOffset start, DateTimeOffset end, bool isAllDay)
    {
        return new Dictionary<string, object?>
        {
            ["summary"] = summary,
            ["description"] = description,
            ["location"] = location,
            ["start"] = BuildEventDateTime(start, isAllDay),
            ["end"] = BuildEventDateTime(end, isAllDay)
        };
    }

    private static object BuildEventDateTime(DateTimeOffset value, bool isAllDay) =>
        isAllDay ? new { date = value.ToString("yyyy-MM-dd") } : new { dateTime = value.ToString("O") };

    private static DateTimeOffset ResolveDateTime(GoogleEventDateTimeDto value) =>
        value.DateTimeValue ?? (value.Date != null ? DateTimeOffset.Parse(value.Date) : DateTimeOffset.UtcNow);

    #endregion
}
