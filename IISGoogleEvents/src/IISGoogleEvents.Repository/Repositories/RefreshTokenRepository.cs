using IISGoogleEvents.Domain.Entities;
using IISGoogleEvents.Domain.Interfaces;
using IISGoogleEvents.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IISGoogleEvents.Repository.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Includes the owning user, because token rotation needs it to mint the next access token.
    /// </summary>
    public async Task<RefreshToken?> GetByTokenAsync(string token) =>
        await DbSet.Include(rt => rt.User).FirstOrDefaultAsync(rt => rt.Token == token);
}
