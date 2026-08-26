using IISGoogleEvents.Application.Interfaces.Security;

namespace IISGoogleEvents.Infrastructure.Security.Helpers;

/// <summary>
/// BCrypt generates a salt and embeds it in the resulting hash, so no separate
/// salt column is needed - the hash string is self-describing.
/// </summary>
public class PasswordHelper : IPasswordHelper
{
    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
