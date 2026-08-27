using IISGoogleEvents.API.Abstractions.Controllers;
using IISGoogleEvents.Application.Configurations;
using IISGoogleEvents.Application.DTOs.Auth;
using IISGoogleEvents.Application.Interfaces.Services;
using IISGoogleEvents.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IISGoogleEvents.API.Controllers;

public class AuthController : BaseController
{
    private const string RefreshTokenCookie = "refreshToken";

    private readonly IAuthService _authService;
    private readonly JwtConfig _jwtConfig;

    public AuthController(IAuthService authService, IOptions<JwtConfig> jwtConfig)
    {
        _authService = authService;
        _jwtConfig = jwtConfig.Value;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequestDto request) =>
        HandleResponse(WithRefreshCookie(await _authService.RegisterAsync(request)));

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequestDto request) =>
        HandleResponse(WithRefreshCookie(await _authService.LoginAsync(request)));

    [HttpPost("refresh")]
    public async Task<ActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];

        if (string.IsNullOrEmpty(refreshToken))
            return HandleResponse(StandardResponse<AuthResponseDto>.Create(
                ResultStatus.Unauthorized, message: "Refresh token cookie is missing."));

        return HandleResponse(WithRefreshCookie(await _authService.RefreshTokenAsync(refreshToken)));
    }

    [HttpPost("signout")]
    public async Task<ActionResult> SignOutUser()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];

        if (string.IsNullOrEmpty(refreshToken))
            return HandleResponse(StandardResponse<bool>.Create(
                ResultStatus.BadRequest, false, "Refresh token cookie is missing."));

        var response = await _authService.SignOutAsync(refreshToken);

        if (response.Success)
            Response.Cookies.Delete(RefreshTokenCookie);

        return HandleResponse(response);
    }

    /// <summary>
    /// Moves the refresh token out of the response body and into an HttpOnly cookie,
    /// so JavaScript can never read it. The access token stays in the body and is
    /// held in memory by the client.
    /// </summary>
    private StandardResponse<AuthResponseDto> WithRefreshCookie(StandardResponse<AuthResponseDto> response)
    {
        if (!response.Success || response.Data == null)
            return response;

        Response.Cookies.Append(RefreshTokenCookie, response.Data.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtConfig.RefreshTokenExpirationInMinutes)
        });

        response.Data.RefreshToken = string.Empty;
        return response;
    }
}
