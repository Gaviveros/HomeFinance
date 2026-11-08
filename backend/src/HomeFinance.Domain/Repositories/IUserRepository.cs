using HomeFinance.Domain.Entities;

namespace HomeFinance.Domain.Repositories;

public interface IUserRepository
{
    void Add(User user);
    User? GetByEmail(string email);
    User? GetById(Guid id);
}
