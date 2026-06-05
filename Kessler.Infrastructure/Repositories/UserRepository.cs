using Kessler.Domain.Entities;
using Kessler.Domain.Interfaces;
using Kessler.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kessler.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(KesslerDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant().Trim(), ct);
}
