using IISGoogleEvents.Domain.Entities;

namespace IISGoogleEvents.Application.Interfaces.Security;

public interface ITokenHelper
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
