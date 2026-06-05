using Kessler.Domain.Interfaces;
using Kessler.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kessler.Infrastructure.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly KesslerDbContext _context;

    protected BaseRepository(KesslerDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Set<T>().FindAsync(new object[] { id }, ct);

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _context.Set<T>().ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _context.Set<T>().AddAsync(entity, ct);

    public void Update(T entity)
        => _context.Set<T>().Update(entity);

    public void Delete(T entity)
        => _context.Set<T>().Remove(entity);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
