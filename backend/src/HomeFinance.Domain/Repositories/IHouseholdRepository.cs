using HomeFinance.Domain.Entities;

namespace HomeFinance.Domain.Repositories;

public interface IHouseholdRepository
{
    void Add(Household household);
    Household? GetById(Guid id);
    IReadOnlyCollection<Household> GetForUser(Guid userId);
}
