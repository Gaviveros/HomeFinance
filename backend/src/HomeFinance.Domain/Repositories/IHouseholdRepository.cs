using HomeFinance.Domain.Entities;

namespace HomeFinance.Domain.Repositories;

public interface IHouseholdRepository
{
    Task AddAsync(Household household, CancellationToken cancellationToken = default);
    Task<Household?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Household>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Household household, CancellationToken cancellationToken = default);
}
