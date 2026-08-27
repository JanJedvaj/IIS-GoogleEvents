using IISGoogleEvents.Domain.Entities;
using IISGoogleEvents.Domain.Interfaces;
using IISGoogleEvents.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IISGoogleEvents.Repository.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username) =>
        await DbSet.FirstOrDefaultAsync(u => u.Username == username);
}
