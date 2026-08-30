using IISGoogleEvents.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace IISGoogleEvents.Infrastructure.Repositories;

public class RefreshTokenRepository(AppDbContext context)
{
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash) =>
        await context.RefreshTokens.Include(rt => rt.User).FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

    public async Task<List<RefreshToken>> GetUnrevokedByUserIdAsync(int userId) =>
        await context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.Revoked == null)
            .ToListAsync();

    public async Task AddAsync(RefreshToken refreshToken) =>
        await context.RefreshTokens.AddAsync(refreshToken);

    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}
