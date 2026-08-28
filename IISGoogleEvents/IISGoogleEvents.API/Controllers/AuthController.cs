using IISGoogleEvents.Application.Configuration;
using IISGoogleEvents.Application.Dtos.Auth;
using IISGoogleEvents.Application.Models;
using IISGoogleEvents.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IISGoogleEvents.API.Controllers;

[Produces("application/json")]
public class AuthController : BaseController
{
    private const string RefreshTokenCookie = "refreshToken";

    private readonly AuthService _authService;
    private readonly JwtOptions _jwtOptions;

    public AuthController(AuthService authService, IOptions<JwtOptions> jwtOptions)
    {
        _authService = authService;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(StandardResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(StandardResponse<AuthResponseDto>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register([FromBody] RegisterRequestDto request) =>
        HandleResponse(WithRefreshCookie(await _authService.RegisterAsync(request)));

    [HttpPost("login")]
    [ProducesResponseType(typeof(StandardResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StandardResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Login([FromBody] LoginRequestDto request) =>
        HandleResponse(WithRefreshCookie(await _authService.LoginAsync(request)));

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(StandardResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StandardResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];

        if (string.IsNullOrEmpty(refreshToken))
            return HandleResponse(StandardResponse<AuthResponseDto>.Create(
                ResultStatus.Unauthorized, message: "Nedostaje cookie s refresh tokenom."));

        return HandleResponse(WithRefreshCookie(await _authService.RefreshTokenAsync(refreshToken)));
    }

    [HttpPost("signout")]
    [ProducesResponseType(typeof(StandardResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StandardResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(StandardResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SignOutUser()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];

        if (string.IsNullOrEmpty(refreshToken))
            return HandleResponse(StandardResponse<bool>.Create(
                ResultStatus.BadRequest, false, "Nedostaje cookie s refresh tokenom."));

        var response = await _authService.SignOutAsync(refreshToken);

        if (response.Success)
            Response.Cookies.Delete(RefreshTokenCookie);

        return HandleResponse(response);
    }

    private StandardResponse<AuthResponseDto> WithRefreshCookie(StandardResponse<AuthResponseDto> response)
    {
        if (!response.Success || response.Data == null)
            return response;

        Response.Cookies.Append(RefreshTokenCookie, response.Data.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.RefreshTokenExpirationInMinutes)
        });

        response.Data.RefreshToken = string.Empty;
        return response;
    }
}
