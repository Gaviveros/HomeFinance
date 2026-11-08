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

    public async Task<HouseholdResult> CreateHouseholdAsync(User? owner, string name, CancellationToken cancellationToken = default)
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

        await _householdRepository.AddAsync(household, cancellationToken);

        return new HouseholdResult { Success = true, Household = household };
    }

    public Task<Household?> GetHouseholdAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default) =>
        _householdRepository.GetByIdForUserAsync(id, ownerId, cancellationToken);

    public async Task<HouseholdResult> UpdateSettingsAsync(
        Guid householdId,
        Guid ownerId,
        string? name,
        string? currency,
        string? timeZone,
        CancellationToken cancellationToken = default)
    {
        var household = await _householdRepository.GetByIdForUserAsync(householdId, ownerId, cancellationToken);
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
        await _householdRepository.UpdateAsync(household, cancellationToken);

        return new HouseholdResult { Success = true, Household = household };
    }

    public Task<IReadOnlyCollection<Household>> GetHouseholdsForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _householdRepository.GetForUserAsync(userId, cancellationToken);
}
