using HomeFinance.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinance.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { message = "La petición es obligatoria." });
        }

        var result = _authService.Register(request.Email, request.Password);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new
        {
            message = "Cuenta creada correctamente.",
            userId = result.User!.Id,
            email = result.User.Email
        });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { message = "La petición es obligatoria." });
        }

        var result = _authService.Login(request.Email, request.Password);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new
        {
            message = "Inicio de sesión correcto.",
            userId = result.User!.Id,
            email = result.User.Email
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
