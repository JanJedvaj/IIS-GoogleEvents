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
    }
}
