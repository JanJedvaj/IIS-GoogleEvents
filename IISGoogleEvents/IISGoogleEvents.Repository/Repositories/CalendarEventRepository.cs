using IISGoogleEvents.Domain.Entities;
using IISGoogleEvents.Domain.Interfaces;
using IISGoogleEvents.Repository.Abstractions;

namespace IISGoogleEvents.Repository.Repositories;

public class CalendarEventRepository : BaseRepository<CalendarEvent>, ICalendarEventRepository
{
    public CalendarEventRepository(AppDbContext context) : base(context)
    {
    }
}
