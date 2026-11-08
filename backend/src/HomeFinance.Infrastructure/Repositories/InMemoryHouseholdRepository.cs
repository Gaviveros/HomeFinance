using HomeFinance.Domain.Entities;
using HomeFinance.Domain.Repositories;

namespace HomeFinance.Infrastructure.Repositories;

public class InMemoryHouseholdRepository : IHouseholdRepository
{
    private readonly List<Household> _households = new();

    public void Add(Household household)
    {
        _households.Add(household);
    }

    public Household? GetById(Guid id)
    {
        return _households.FirstOrDefault(x => x.Id == id);
    }

    public IReadOnlyCollection<Household> GetForUser(Guid userId)
    {
        return _households.Where(x => x.OwnerId == userId).ToList();
    }
}
