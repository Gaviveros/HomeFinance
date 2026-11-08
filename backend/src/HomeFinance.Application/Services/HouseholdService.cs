using HomeFinance.Domain.Entities;
using HomeFinance.Domain.Repositories;

namespace HomeFinance.Application.Services;

public class HouseholdService
{
    private readonly IHouseholdRepository _householdRepository;

    public HouseholdService(IHouseholdRepository householdRepository)
    {
        _householdRepository = householdRepository;
    }

    public HouseholdResult CreateHousehold(User? owner, string name)
    {
        if (owner is null || owner.Id == Guid.Empty)
        {
            return new HouseholdResult { Success = false, Error = "Debe existir un titular válido para crear el hogar." };
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return new HouseholdResult { Success = false, Error = "El nombre del hogar es obligatorio." };
        }

        var household = new Household
        {
            Name = name.Trim(),
            OwnerId = owner.Id,
            Currency = "ARS",
            TimeZone = "America/Argentina/Cordoba"
        };

        _householdRepository.Add(household);

        return new HouseholdResult { Success = true, Household = household };
    }

    public Household? GetHousehold(Guid id) => _householdRepository.GetById(id);

    public HouseholdResult UpdateSettings(Guid householdId, string? name, string? currency, string? timeZone)
    {
        var household = _householdRepository.GetById(householdId);
        if (household is null)
        {
            return new HouseholdResult { Success = false, Error = "El hogar no existe." };
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            household.Name = name.Trim();
        }

        household.Currency = string.IsNullOrWhiteSpace(currency) ? household.Currency : currency;
        household.TimeZone = string.IsNullOrWhiteSpace(timeZone) ? household.TimeZone : timeZone;

        return new HouseholdResult { Success = true, Household = household };
    }

    public IReadOnlyCollection<Household> GetHouseholdsForUser(Guid userId) => _householdRepository.GetForUser(userId);
}
