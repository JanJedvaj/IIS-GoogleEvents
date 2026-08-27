using IISGoogleEvents.Application.Interfaces.Security;
using IISGoogleEvents.Domain.Entities;
using IISGoogleEvents.Domain.Enums;
using IISGoogleEvents.Repository;
using Microsoft.EntityFrameworkCore;

namespace IISGoogleEvents.API.Extensions;

public static class DataSeeder
{
    /// <summary>
    /// Seeds the read-only / full-access pair that requirement 6 needs.
    /// </summary>
    public static async Task SeedDataAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHelper = scope.ServiceProvider.GetRequiredService<IPasswordHelper>();

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
                PasswordHash = passwordHelper.HashPassword(seed.Password),
                Role = seed.Role
            });
        }

        await context.SaveChangesAsync();
        await SeedCalendarEventsAsync(context);
    }

    /// <summary>
    /// Static demo events so Local mode has content to serve without waiting on
    /// a Google service-account key. A one-time pull from the upstream API is a
    /// follow-up once that key exists, not a Phase 3 blocker.
    /// </summary>
    private static async Task SeedCalendarEventsAsync(AppDbContext context)
    {
        if (await context.CalendarEvents.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        // Explicit zero offset: DateTimeOffset.Date narrows to a Kind=Unspecified
        // DateTime, and assigning that back to a DateTimeOffset field would silently
        // pick up the machine's local offset instead of UTC - which Npgsql's
        // "timestamp with time zone" column then rejects outright.
        var todayUtc = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);

        var seedEvents = new[]
        {
            (Summary: "IIS Defence Demo", Description: "Walkthrough of the Google Calendar integration for the defence.", DaysFromNow: 7),
            (Summary: "Team Sync", Description: "Weekly sync on project status.", DaysFromNow: 1),
            (Summary: "Sprint Planning", Description: "Plan the next iteration.", DaysFromNow: 3)
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
