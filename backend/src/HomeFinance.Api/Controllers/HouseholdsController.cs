using System.Security.Claims;
using HomeFinance.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinance.Api.Controllers;

[ApiController]
[Route("api/households")]
public class HouseholdsController : ControllerBase
{
    private readonly HouseholdService _householdService;
    private readonly AuthService _authService;

    public HouseholdsController(HouseholdService householdService, AuthService authService)
    {
        _householdService = householdService;
        _authService = authService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new { message = "La identidad autenticada no es válida." });
        }

        var households = (await _householdService.GetHouseholdsForUserAsync(userId, cancellationToken))
            .Select(h => new
            {
                id = h.Id,
                name = h.Name,
                ownerId = h.OwnerId,
                currency = h.Currency,
                timeZone = h.TimeZone
            });

        return Ok(households);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateHouseholdRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "La petición es obligatoria." });
        }

        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new { message = "La identidad autenticada no es válida." });
        }

        var user = await _authService.GetUserAsync(userId, cancellationToken);
        if (user is null)
        {
            return Unauthorized(new { message = "El usuario no existe o no está autenticado." });
        }

        var result = await _householdService.CreateHouseholdAsync(user, request.Name, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new
        {
            message = "Hogar creado correctamente.",
            household = result.Household
        });
    }

    [HttpGet("{id}/settings")]
    [Authorize]
    public async Task<IActionResult> GetSettings(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new { message = "La identidad autenticada no es válida." });
        }

        var household = await _householdService.GetHouseholdAsync(id, userId, cancellationToken);
        if (household is null)
        {
            return NotFound(new { message = "El hogar no existe." });
        }

        return Ok(new
        {
            id = household.Id,
            name = household.Name,
            currency = household.Currency,
            timeZone = household.TimeZone,
            ownerId = household.OwnerId
        });
    }

    [HttpPut("{id}/settings")]
    [Authorize]
    public async Task<IActionResult> UpdateSettings(
        Guid id,
        [FromBody] UpdateHouseholdSettingsRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new { message = "La identidad autenticada no es válida." });
        }

        var result = await _householdService.UpdateSettingsAsync(
            id, userId, request.Name, request.Currency, request.TimeZone, cancellationToken);
        if (!result.Success)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(new
        {
            message = "Configuración actualizada.",
            household = result.Household
        });
    }

    private bool TryGetAuthenticatedUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}

public class CreateHouseholdRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateHouseholdSettingsRequest
{
    public string? Name { get; set; }
    public string? Currency { get; set; }
    public string? TimeZone { get; set; }
}
