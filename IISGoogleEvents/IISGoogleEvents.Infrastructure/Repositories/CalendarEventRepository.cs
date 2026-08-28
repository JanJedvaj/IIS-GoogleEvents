using IISGoogleEvents.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace IISGoogleEvents.Infrastructure.Repositories;

public class CalendarEventRepository(AppDbContext context)
{
    public const string CancelledStatus = "cancelled";

    public async Task<List<CalendarEvent>> SearchAsync(string? query = null)
    {
        var events = context.CalendarEvents.Where(e => e.Status != CancelledStatus);

        if (!string.IsNullOrWhiteSpace(query))
            events = events.Where(e => e.Summary.Contains(query));

        return await events.OrderBy(e => e.Start).ToListAsync();
    }

    public async Task<CalendarEvent?> GetByGoogleEventIdAsync(string googleEventId) =>
        await context.CalendarEvents.FirstOrDefaultAsync(
            e => e.GoogleEventId == googleEventId && e.Status != CancelledStatus);

    public async Task AddAsync(CalendarEvent calendarEvent) =>
        await context.CalendarEvents.AddAsync(calendarEvent);

    public async Task<List<string>> GetExistingGoogleEventIdsAsync(IEnumerable<string> googleEventIds) =>
        await context.CalendarEvents
            .Where(e => googleEventIds.Contains(e.GoogleEventId))
            .Select(e => e.GoogleEventId)
            .ToListAsync();

    public async Task AddRangeAsync(IEnumerable<CalendarEvent> calendarEvents) =>
        await context.CalendarEvents.AddRangeAsync(calendarEvents);

    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}
