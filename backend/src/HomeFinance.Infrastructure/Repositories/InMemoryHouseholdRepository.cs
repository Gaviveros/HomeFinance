using HomeFinance.Domain.Entities;
using HomeFinance.Domain.Repositories;

namespace HomeFinance.Infrastructure.Repositories;

public class InMemoryHouseholdRepository : IHouseholdRepository
{
    private readonly List<Household> _households = new();

    public Task AddAsync(Household household, CancellationToken cancellationToken = default)
    {
        _households.Add(household);
        return Task.CompletedTask;
    }

    public Task<Household?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_households.FirstOrDefault(x => x.Id == id && x.OwnerId == userId));

    public Task<IReadOnlyCollection<Household>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Household>>(_households.Where(x => x.OwnerId == userId).ToList());

    public Task UpdateAsync(Household household, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
