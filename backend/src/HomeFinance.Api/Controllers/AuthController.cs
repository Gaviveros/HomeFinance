using HomeFinance.Application.Services;
using HomeFinance.Api.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinance.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly AccessTokenService _accessTokenService;

    public AuthController(AuthService authService, AccessTokenService accessTokenService)
    {
        _authService = authService;
        _accessTokenService = accessTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "La petición es obligatoria." });
        }

        var result = await _authService.RegisterAsync(request.Email, request.Password, cancellationToken);
        if (!result.Success)
        {
            return result.DuplicateEmail
                ? Conflict(new { message = result.Error })
                : BadRequest(new { message = result.Error });
        }

        return Ok(new
        {
            message = "Cuenta creada correctamente.",
            userId = result.User!.Id,
            email = result.User.Email
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "La petición es obligatoria." });
        }

        var result = await _authService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        var (token, expiresAt) = _accessTokenService.CreateToken(result.User!);
        return Ok(new
        {
            message = "Inicio de sesión correcto.",
            userId = result.User!.Id,
            email = result.User.Email,
            accessToken = token,
            expiresAt
        });
    }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
