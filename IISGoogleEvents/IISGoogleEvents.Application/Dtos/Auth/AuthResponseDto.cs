using IISGoogleEvents.Infrastructure.Enums;

namespace IISGoogleEvents.Application.Dtos.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string Username { get; set; } = null!;
    public Roles Role { get; set; }

    public string RefreshToken { get; set; } = null!;
}
