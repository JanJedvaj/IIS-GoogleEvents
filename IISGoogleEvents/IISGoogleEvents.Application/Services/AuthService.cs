using IISGoogleEvents.Application.Configuration;
using IISGoogleEvents.Application.Dtos.Auth;
using IISGoogleEvents.Application.Models;
using IISGoogleEvents.Application.Security;
using IISGoogleEvents.Infrastructure.Entities;
using IISGoogleEvents.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace IISGoogleEvents.Application.Services;

public class AuthService
{
    private readonly UserRepository _userRepository;
    private readonly RefreshTokenRepository _refreshTokenRepository;
    private readonly PasswordHelper _passwordHelper;
    private readonly TokenHelper _tokenHelper;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserRepository userRepository,
        RefreshTokenRepository refreshTokenRepository,
        PasswordHelper passwordHelper,
        TokenHelper tokenHelper,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHelper = passwordHelper;
        _tokenHelper = tokenHelper;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<StandardResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var existing = await _userRepository.GetByUsernameAsync(request.Username);
        if (existing != null)
            return StandardResponse<AuthResponseDto>.Create(ResultStatus.Conflict, message: "Korisničko ime je već zauzeto.");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHelper.HashPassword(request.Password)
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var response = await IssueTokensAsync(user);
        return StandardResponse<AuthResponseDto>.Create(ResultStatus.Created, response, "Registracija je uspješna.");
    }

    public async Task<StandardResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null || !_passwordHelper.VerifyPassword(request.Password, user.PasswordHash))
            return StandardResponse<AuthResponseDto>.Create(ResultStatus.Unauthorized, message: "Neispravno korisničko ime ili lozinka.");

        var response = await IssueTokensAsync(user);
        return StandardResponse<AuthResponseDto>.Create(ResultStatus.Ok, response, "Prijava je uspješna.");
    }

    public async Task<StandardResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(_tokenHelper.HashRefreshToken(refreshToken));

        if (stored == null)
            return StandardResponse<AuthResponseDto>.Create(ResultStatus.Unauthorized, message: "Neispravan ili istekao refresh token.");

        if (stored.Revoked != null)
        {
            var revokedAt = DateTime.UtcNow;

            foreach (var token in await _refreshTokenRepository.GetUnrevokedByUserIdAsync(stored.UserId))
                token.Revoked = revokedAt;

            await _refreshTokenRepository.SaveChangesAsync();

            return StandardResponse<AuthResponseDto>.Create(
                ResultStatus.Unauthorized,
                message: "Refresh token je već iskorišten. Sve sesije su odjavljene, prijavite se ponovno.");
        }

        if (!stored.IsActive)
            return StandardResponse<AuthResponseDto>.Create(ResultStatus.Unauthorized, message: "Neispravan ili istekao refresh token.");

        stored.Revoked = DateTime.UtcNow;

        var response = await IssueTokensAsync(stored.User);
        return StandardResponse<AuthResponseDto>.Create(ResultStatus.Ok, response, "Token je osvježen.");
    }

    public async Task<StandardResponse<bool>> SignOutAsync(string refreshToken)
    {
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(_tokenHelper.HashRefreshToken(refreshToken));

        if (stored == null)
            return StandardResponse<bool>.Create(ResultStatus.NotFound, false, "Refresh token nije nađen.");

        if (stored.Revoked != null)
            return StandardResponse<bool>.Create(ResultStatus.Conflict, false, "Refresh token je već povučen.");

        stored.Revoked = DateTime.UtcNow;
        await _refreshTokenRepository.SaveChangesAsync();

        return StandardResponse<bool>.Create(ResultStatus.Ok, true, "Odjava je uspješna.");
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user)
    {
        var accessToken = _tokenHelper.GenerateAccessToken(user);
        var refreshToken = _tokenHelper.GenerateRefreshToken();

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            TokenHash = _tokenHelper.HashRefreshToken(refreshToken),
            UserId = user.Id,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.RefreshTokenExpirationInMinutes)
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
