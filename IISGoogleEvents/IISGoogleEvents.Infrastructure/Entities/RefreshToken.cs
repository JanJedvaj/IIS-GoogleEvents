using IISGoogleEvents.Infrastructure.Entities;

namespace IISGoogleEvents.Infrastructure.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = null!;
    public int UserId { get; set; }
    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
    public DateTime? Revoked { get; set; }

    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsActive => Revoked == null && !IsExpired;
}
