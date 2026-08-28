using IISGoogleEvents.Application.Dtos.Events;
using IISGoogleEvents.Application.Interfaces;
using IISGoogleEvents.Application.Mappers;
using IISGoogleEvents.Application.Models;
using IISGoogleEvents.Infrastructure.Entities;
using IISGoogleEvents.Infrastructure.Repositories;

namespace IISGoogleEvents.Application.Services;

public class LocalCalendarEventService(CalendarEventRepository repository) : ICalendarEventService
{
    public CalendarCapabilitiesDto Capabilities { get; } = new(Source: "Local", SoftDeletes: true);

    public async Task<StandardResponse<IEnumerable<CalendarEventDto>>> SearchAsync(string? query = null)
    {
        var results = await repository.SearchAsync(query);
        var dtos = results.Select(r => r.ToDto());

        return StandardResponse<IEnumerable<CalendarEventDto>>.Create(ResultStatus.Ok, dtos);
    }

    public async Task<StandardResponse<CalendarEventDto>> GetAsync(string googleEventId)
    {
        var calendarEvent = await repository.GetByGoogleEventIdAsync(googleEventId);

        if (calendarEvent == null)
            return StandardResponse<CalendarEventDto>.Create(ResultStatus.NotFound, message: "Događaj nije nađen.");

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
            Start = request.Start.ToUniversalTime(),
            End = request.End.ToUniversalTime(),
            IsAllDay = request.IsAllDay,
            Status = "confirmed",
            HtmlLink = "local://newevent",
            Created = now,
            Updated = now
        };

        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        return StandardResponse<CalendarEventDto>.Create(ResultStatus.Created, entity.ToDto());
    }

    public async Task<StandardResponse<CalendarEventDto>> UpdateAsync(string googleEventId, UpdateCalendarEventDto request)
    {
        var calendarEvent = await repository.GetByGoogleEventIdAsync(googleEventId);

        if (calendarEvent == null)
            return StandardResponse<CalendarEventDto>.Create(ResultStatus.NotFound, message: "Događaj nije nađen.");

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

        await repository.SaveChangesAsync();

        return StandardResponse<CalendarEventDto>.Create(ResultStatus.Ok, calendarEvent.ToDto());
    }

    public async Task<StandardResponse<bool>> DeleteAsync(string googleEventId)
    {
        var calendarEvent = await repository.GetByGoogleEventIdAsync(googleEventId);

        if (calendarEvent == null)
            return StandardResponse<bool>.Create(ResultStatus.NotFound, message: "Događaj nije nađen.");

        calendarEvent.Status = CalendarEventRepository.CancelledStatus;
        calendarEvent.Updated = DateTimeOffset.UtcNow;
        await repository.SaveChangesAsync();

        return StandardResponse<bool>.Create(ResultStatus.Ok, true);
    }
}
