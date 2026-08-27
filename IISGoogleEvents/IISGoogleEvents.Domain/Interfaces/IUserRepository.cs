using IISGoogleEvents.Domain.Abstractions;
using IISGoogleEvents.Domain.Entities;

namespace IISGoogleEvents.Domain.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
}
