using AgriMarket.Modules.Users.Dtos.Auth;
using AgriMarket.Modules.Users.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AgriMarket.Modules.Users.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/auth")]
internal sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _config;

    public AuthController(IAuthService authService, IConfiguration config)
    {
        _authService = authService;
        _config = config;
    }

    [HttpPost("register")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            await _authService.RegisterAsync(request);
            return Created();
        }
        catch (InvalidOperationException ex) when (ex.Message == "Email already in use.")
        {
            return Problem(statusCode: 409, title: "Conflict", detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: 400, title: "Bad Request", detail: ex.Message);
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AccessTokenResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var tokens = await _authService.LoginAsync(request);
            SetRefreshTokenCookie(tokens.RefreshToken);
            return Ok(new AccessTokenResponse { AccessToken = tokens.AccessToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(statusCode: 401, title: "Unauthorized", detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: 400, title: "Bad Request", detail: ex.Message);
        }
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AccessTokenResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "No refresh token.");

        try
        {
            var tokens = await _authService.RefreshAsync(refreshToken);
            SetRefreshTokenCookie(tokens.RefreshToken);
            return Ok(new AccessTokenResponse { AccessToken = tokens.AccessToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            DeleteRefreshTokenCookie();
            return Problem(statusCode: 401, title: "Unauthorized", detail: ex.Message);
        }
    }

    [HttpPost("logout")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
            await _authService.LogoutAsync(refreshToken);

        DeleteRefreshTokenCookie();
        return NoContent();
    }

    private void SetRefreshTokenCookie(string token)
    {
        var expiryDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/v1/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(expiryDays),
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/v1/auth",
        });
    }
}
