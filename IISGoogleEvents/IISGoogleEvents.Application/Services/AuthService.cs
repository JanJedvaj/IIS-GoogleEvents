using IISGoogleEvents.Application.Configurations;
using IISGoogleEvents.Application.DTOs.Auth;
using IISGoogleEvents.Application.Interfaces.Security;
using IISGoogleEvents.Application.Interfaces.Services;
using IISGoogleEvents.Application.Models;
using IISGoogleEvents.Domain.Entities;
using IISGoogleEvents.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace IISGoogleEvents.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHelper _passwordHelper;
    private readonly ITokenHelper _tokenHelper;
    private readonly JwtConfig _jwtConfig;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHelper passwordHelper,
        ITokenHelper tokenHelper,
        IOptions<JwtConfig> jwtConfig)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHelper = passwordHelper;
        _tokenHelper = tokenHelper;
        _jwtConfig = jwtConfig.Value;
    }

    public async Task<StandardResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var existing = await _userRepository.GetByUsernameAsync(request.Username);
        if (existing != null)
            return StandardResponse<AuthResponseDto>.Create(ResultStatus.Conflict, message: "Username is already taken.");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHelper.HashPassword(request.Password)
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var response = await IssueTokensAsync(user);
        return StandardResponse<AuthResponseDto>.Create(ResultStatus.Created, response, "Registration successful.");
    }

    public async Task<StandardResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null || !_passwordHelper.VerifyPassword(request.Password, user.PasswordHash))
            return StandardResponse<AuthResponseDto>.Create(ResultStatus.Unauthorized, message: "Invalid username or password.");

        var response = await IssueTokensAsync(user);
        return StandardResponse<AuthResponseDto>.Create(ResultStatus.Ok, response, "Login successful.");
    }

    public async Task<StandardResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (stored == null || !stored.IsActive)
            return StandardResponse<AuthResponseDto>.Create(ResultStatus.Unauthorized, message: "Invalid or expired refresh token.");

        // Rotation: the presented token is spent, and a fresh one is issued alongside
        // the new access token. A replayed token therefore fails the IsActive check.
        stored.Revoked = DateTime.UtcNow;
        _refreshTokenRepository.Update(stored);

        var response = await IssueTokensAsync(stored.User);
        return StandardResponse<AuthResponseDto>.Create(ResultStatus.Ok, response, "Token refreshed.");
    }

    public async Task<StandardResponse<bool>> SignOutAsync(string refreshToken)
    {
        var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (stored == null)
            return StandardResponse<bool>.Create(ResultStatus.NotFound, false, "Refresh token not found.");

        if (stored.Revoked != null)
            return StandardResponse<bool>.Create(ResultStatus.Conflict, false, "Refresh token is already revoked.");

        stored.Revoked = DateTime.UtcNow;
        _refreshTokenRepository.Update(stored);
        await _refreshTokenRepository.SaveChangesAsync();

        return StandardResponse<bool>.Create(ResultStatus.Ok, true, "Signed out.");
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user)
    {
        var accessToken = _tokenHelper.GenerateAccessToken(user);
        var refreshToken = _tokenHelper.GenerateRefreshToken();

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.RefreshTokenExpirationInMinutes)
        });
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Username = user.Username,
            Role = user.Role
        };
    }
}
