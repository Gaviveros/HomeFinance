using HomeFinance.Domain.Entities;
using HomeFinance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeFinance.Infrastructure.Repositories;

public sealed class PostgresHouseholdRepository(PersistenceDbContext dbContext) : IHouseholdRepository
{
    public async Task AddAsync(Household household, CancellationToken cancellationToken = default)
    {
        dbContext.Households.Add(household);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Household?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.Households.SingleOrDefaultAsync(
            household => household.Id == id && household.OwnerId == userId,
            cancellationToken);

    public async Task<IReadOnlyCollection<Household>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await dbContext.Households
            .Where(household => household.OwnerId == userId)
            .ToListAsync(cancellationToken);

    public async Task UpdateAsync(Household household, CancellationToken cancellationToken = default)
    {
        dbContext.Households.Update(household);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
