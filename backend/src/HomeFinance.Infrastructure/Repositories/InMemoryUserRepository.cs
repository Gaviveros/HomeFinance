using HomeFinance.Domain.Entities;
using HomeFinance.Domain.Exceptions;
using HomeFinance.Domain.Repositories;

namespace HomeFinance.Infrastructure.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        if (_users.Any(existing => existing.NormalizedEmail == user.NormalizedEmail))
        {
            throw new DuplicateEmailException();
        }

        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.FirstOrDefault(x => x.NormalizedEmail == normalizedEmail));

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.FirstOrDefault(x => x.Id == id));

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
