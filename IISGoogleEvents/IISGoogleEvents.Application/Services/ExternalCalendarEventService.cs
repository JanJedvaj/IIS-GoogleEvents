using System.Net;
using IISGoogleEvents.Application.Dtos.Events;
using IISGoogleEvents.Infrastructure.Clients.Google;
using IISGoogleEvents.Application.Interfaces;
using IISGoogleEvents.Application.Models;
using IISGoogleEvents.Application.Mappers;

namespace IISGoogleEvents.Application.Services;

public class ExternalCalendarEventService : ICalendarEventService
{
    public CalendarCapabilitiesDto Capabilities { get; } = new(Source: "External", SoftDeletes: false);

    private readonly GoogleCalendarClient _httpClient;

    public ExternalCalendarEventService(GoogleCalendarClient httpClient)
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

            if (response == null || response.Status == "cancelled")
                return StandardResponse<CalendarEventDto>.Create(ResultStatus.NotFound, message: "Događaj nije nađen.");

            return StandardResponse<CalendarEventDto>.Create(ResultStatus.Ok, response.ToDto());
        }
        catch (Exception ex) when (IsNotFound(ex))
        {
            return StandardResponse<CalendarEventDto>.Create(ResultStatus.NotFound, message: "Događaj nije nađen.");
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
                ? StandardResponse<CalendarEventDto>.Create(ResultStatus.InternalError, message: "Nije moguće protumačiti odgovor Google Calendar API-ja.")
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
            var current = await _httpClient.GetEventAsync(googleEventId);

            if (current == null || current.Status == "cancelled")
                return StandardResponse<CalendarEventDto>.Create(ResultStatus.NotFound, message: "Događaj nije nađen.");

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
                ? StandardResponse<CalendarEventDto>.Create(ResultStatus.InternalError, message: "Nije moguće protumačiti odgovor Google Calendar API-ja.")
                : StandardResponse<CalendarEventDto>.Create(ResultStatus.Ok, response.ToDto());
        }
        catch (Exception ex) when (IsNotFound(ex))
        {
            return StandardResponse<CalendarEventDto>.Create(ResultStatus.NotFound, message: "Događaj nije nađen.");
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
            await _httpClient.DeleteEventAsync(googleEventId);
            return StandardResponse<bool>.Create(ResultStatus.Ok, true);
        }
        catch (Exception ex) when (IsNotFound(ex))
        {
            return StandardResponse<bool>.Create(ResultStatus.NotFound, false, message: "Događaj nije nađen.");
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
