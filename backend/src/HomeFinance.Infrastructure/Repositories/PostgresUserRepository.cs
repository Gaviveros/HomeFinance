using HomeFinance.Domain.Entities;
using HomeFinance.Domain.Exceptions;
using HomeFinance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HomeFinance.Infrastructure.Repositories;

public sealed class PostgresUserRepository(PersistenceDbContext dbContext) : IUserRepository
{
    private const string NormalizedEmailIndex = "IX_Users_NormalizedEmail";

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Add(user);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateEmail(exception))
        {
            throw new DuplicateEmailException();
        }
    }

    public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsDuplicateEmail(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: NormalizedEmailIndex
        };
}
