using IISGoogleEvents.Infrastructure.Entities;
using IISGoogleEvents.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace IISGoogleEvents.Infrastructure;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, Func<string, string> hashPassword)
    {
        await SeedUsersAsync(context, hashPassword);
        await SeedCalendarEventsAsync(context);
    }

    private static async Task SeedUsersAsync(AppDbContext context, Func<string, string> hashPassword)
    {
        var seedUsers = new[]
        {
            (Username: "admin", Password: "admin", Role: Roles.Admin),
            (Username: "user", Password: "user", Role: Roles.User)
        };

        foreach (var seed in seedUsers)
        {
            if (await context.Users.AnyAsync(u => u.Username == seed.Username))
                continue;

            context.Users.Add(new User
            {
                Username = seed.Username,
                PasswordHash = hashPassword(seed.Password),
                Role = seed.Role
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedCalendarEventsAsync(AppDbContext context)
    {
        if (await context.CalendarEvents.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var todayUtc = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);

        var seedEvents = new[]
        {
            (Summary: "Demo za obranu IIS-a", Description: "Prolaz kroz integraciju s Google Calendarom za obranu.", DaysFromNow: 7),
            (Summary: "Sastanak tima", Description: "Tjedni pregled statusa projekta.", DaysFromNow: 1),
            (Summary: "Planiranje sprinta", Description: "Planiranje sljedeće iteracije.", DaysFromNow: 3)
        };

        foreach (var seed in seedEvents)
        {
            var start = todayUtc.AddDays(seed.DaysFromNow).AddHours(10);

            context.CalendarEvents.Add(new CalendarEvent
            {
                GoogleEventId = $"local-{Guid.NewGuid()}",
                Summary = seed.Summary,
                Description = seed.Description,
                Location = null,
                Start = start,
                End = start.AddHours(1),
                IsAllDay = false,
                Status = "confirmed",
                HtmlLink = "local://newevent",
                Created = now,
                Updated = now
            });
        }

        await context.SaveChangesAsync();
    }
}
