using IISGoogleEvents.Infrastructure.Entities;
using IISGoogleEvents.Infrastructure.Enums;

namespace IISGoogleEvents.Infrastructure.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public Roles Role { get; set; } = Roles.User;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
