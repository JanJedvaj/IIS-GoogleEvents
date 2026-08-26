using IISGoogleEvents.Application.DTOs.Auth;
using IISGoogleEvents.Application.Models;

namespace IISGoogleEvents.Application.Interfaces.Services;

public interface IAuthService
{
    Task<StandardResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<StandardResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<StandardResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<StandardResponse<bool>> SignOutAsync(string refreshToken);
}
