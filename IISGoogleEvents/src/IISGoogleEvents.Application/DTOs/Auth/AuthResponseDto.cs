using IISGoogleEvents.Domain.Enums;

namespace IISGoogleEvents.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string Username { get; set; } = null!;
    public Roles Role { get; set; }

    /// <summary>
    /// Returned to the caller only so the controller can put it in an HttpOnly
    /// cookie. It is stripped before the response is serialised.
    /// </summary>
    public string RefreshToken { get; set; } = null!;
}
