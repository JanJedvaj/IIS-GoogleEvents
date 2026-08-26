using IISGoogleEvents.Domain.Abstractions;
using IISGoogleEvents.Domain.Entities;

namespace IISGoogleEvents.Domain.Interfaces;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
}
