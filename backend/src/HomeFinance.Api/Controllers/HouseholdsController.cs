using HomeFinance.Application.Services;
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
    public IActionResult GetAll([FromHeader(Name = "X-User-Id")] string? userIdHeader)
    {
        if (!Guid.TryParse(userIdHeader, out var userId))
        {
            return Unauthorized(new { message = "Debe indicar un usuario autenticado." });
        }

        var households = _householdService.GetHouseholdsForUser(userId)
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
    public IActionResult Create([FromBody] CreateHouseholdRequest request, [FromHeader(Name = "X-User-Id")] string? userIdHeader)
    {
        if (request is null)
        {
            return BadRequest(new { message = "La petición es obligatoria." });
        }

        var userId = userIdHeader ?? request.UserId;
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized(new { message = "Debe indicar el titular del hogar." });
        }

        var user = _authService.GetUser(parsedUserId);
        if (user is null)
        {
            return Unauthorized(new { message = "El usuario no existe o no está autenticado." });
        }

        var result = _householdService.CreateHousehold(user, request.Name);
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
    public IActionResult GetSettings(Guid id)
    {
        var household = _householdService.GetHousehold(id);
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
    public IActionResult UpdateSettings(Guid id, [FromBody] UpdateHouseholdSettingsRequest request)
    {
        var result = _householdService.UpdateSettings(id, request.Name, request.Currency, request.TimeZone);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new
        {
            message = "Configuración actualizada.",
            household = result.Household
        });
    }
}

public class CreateHouseholdRequest
{
    public string Name { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

public class UpdateHouseholdSettingsRequest
{
    public string? Name { get; set; }
    public string? Currency { get; set; }
    public string? TimeZone { get; set; }
}
