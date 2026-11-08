using HomeFinance.Domain.Entities;
using HomeFinance.Domain.Repositories;

namespace HomeFinance.Infrastructure.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public void Add(User user)
    {
        _users.Add(user);
    }

    public User? GetByEmail(string email)
    {
        return _users.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public User? GetById(Guid id)
    {
        return _users.FirstOrDefault(x => x.Id == id);
    }
}
