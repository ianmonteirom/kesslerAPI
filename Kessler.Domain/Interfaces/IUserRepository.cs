using Kessler.Domain.Entities;

namespace Kessler.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}
