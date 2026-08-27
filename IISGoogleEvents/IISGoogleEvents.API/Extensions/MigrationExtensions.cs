using IISGoogleEvents.Repository;
using Microsoft.EntityFrameworkCore;

namespace IISGoogleEvents.API.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if ((await context.Database.GetPendingMigrationsAsync()).Any())
            await context.Database.MigrateAsync();
    }
}
