using IISGoogleEvents.Application.DTOs.Events;
using IISGoogleEvents.Application.Interfaces.Services;
using IISGoogleEvents.Application.Mappers;
using IISGoogleEvents.Application.Models;
using IISGoogleEvents.Domain.Entities;
using IISGoogleEvents.Domain.Interfaces;

namespace IISGoogleEvents.Application.Services;

/// <summary>
/// ICalendarEventService over the app's own Postgres mirror. Resolved when
/// AppConfig.DataSource == Local. "cancelled" is a soft delete - every read
/// filters it out, same trick as the reference's NotionObject.InTrash.
/// </summary>
public class LocalCalendarEventService : ICalendarEventService
{
    private const string CancelledStatus = "cancelled";

    private readonly ICalendarEventRepository _repository;

    public LocalCalendarEventService(ICalendarEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<StandardResponse<IEnumerable<CalendarEventDto>>> SearchAsync(string? query = null)
    {
        var results = string.IsNullOrWhiteSpace(query)
            ? await _repository.FindAsync(x => x.Status != CancelledStatus)
            : await _repository.FindAsync(x => x.Summary.Contains(query) && x.Status != CancelledStatus);

        var dtos = results.Select(r => r.ToDto());
        return StandardResponse<IEnumerable<CalendarEventDto>>.Create(ResultStatus.Ok, dtos);
    }

    public async Task<StandardResponse<CalendarEventDto>> GetAsync(string googleEventId)
    {
        var results = await _repository.FindAsync(x => x.GoogleEventId == googleEventId && x.Status != CancelledStatus);
        var calendarEvent = results.FirstOrDefault();

        if (calendarEvent == null)
            return StandardResponse<CalendarEventDto>.Create(ResultStatus.NotFound, message: "Event not found");

        return StandardResponse<CalendarEventDto>.Create(ResultStatus.Ok, calendarEvent.ToDto());
    }

    public async Task<StandardResponse<CalendarEventDto>> CreateAsync(CreateCalendarEventDto request)
    {
        var now = DateTimeOffset.UtcNow;

        var entity = new CalendarEvent
        {
            GoogleEventId = $"local-{Guid.NewGuid()}",
            Summary = request.Summary,
            Description = request.Description,
            Location = request.Location,
            // Postgres' "timestamp with time zone" column only accepts UTC (offset 0);
            // a client can submit any offset, so normalize before it ever reaches EF.
            Start = request.Start.ToUniversalTime(),
            End = request.End.ToUniversalTime(),
            IsAllDay = request.IsAllDay,
            Status = "confirmed",
            HtmlLink = "local://newevent",
            Created = now,
            Updated = now
        };

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return StandardResponse<CalendarEventDto>.Create(ResultStatus.Created, entity.ToDto());
    }

    public async Task<StandardResponse<CalendarEventDto>> UpdateAsync(string googleEventId, UpdateCalendarEventDto request)
    {
        var results = await _repository.FindAsync(x => x.GoogleEventId == googleEventId && x.Status != CancelledStatus);
        var calendarEvent = results.FirstOrDefault();

        if (calendarEvent == null)
            return StandardResponse<CalendarEventDto>.Create(ResultStatus.NotFound, message: "Event not found");

        if (!string.IsNullOrWhiteSpace(request.Summary))
            calendarEvent.Summary = request.Summary;

        if (request.Description != null)
            calendarEvent.Description = request.Description;

        if (request.Location != null)
            calendarEvent.Location = request.Location;

        if (request.Start.HasValue)
            calendarEvent.Start = request.Start.Value.ToUniversalTime();

        if (request.End.HasValue)
            calendarEvent.End = request.End.Value.ToUniversalTime();

        if (request.IsAllDay.HasValue)
            calendarEvent.IsAllDay = request.IsAllDay.Value;

        calendarEvent.Updated = DateTimeOffset.UtcNow;
        _repository.Update(calendarEvent);
        await _repository.SaveChangesAsync();

        return StandardResponse<CalendarEventDto>.Create(ResultStatus.Ok, calendarEvent.ToDto());
    }

    public async Task<StandardResponse<bool>> DeleteAsync(string googleEventId)
    {
        var results = await _repository.FindAsync(x => x.GoogleEventId == googleEventId && x.Status != CancelledStatus);
        var calendarEvent = results.FirstOrDefault();

        if (calendarEvent == null)
            return StandardResponse<bool>.Create(ResultStatus.NotFound, message: "Event not found");

        calendarEvent.Status = CancelledStatus;
        calendarEvent.Updated = DateTimeOffset.UtcNow;
        _repository.Update(calendarEvent);
        await _repository.SaveChangesAsync();

        return StandardResponse<bool>.Create(ResultStatus.Ok, true);
    }
}
