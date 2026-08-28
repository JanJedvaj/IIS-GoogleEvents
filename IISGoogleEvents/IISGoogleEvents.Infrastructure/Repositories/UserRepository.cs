using IISGoogleEvents.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace IISGoogleEvents.Infrastructure.Repositories;

public class UserRepository(AppDbContext context)
{
    public async Task<User?> GetByUsernameAsync(string username) =>
        await context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task AddAsync(User user) => await context.Users.AddAsync(user);

    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}
