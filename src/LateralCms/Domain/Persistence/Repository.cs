using LateralCms.Application.Interfaces.Persistence;
using LateralCms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LateralCms.Infrastructure.Persistence;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbSet<T> _entities;

    public Repository(LateralCmsContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _entities = context.Set<T>();
    }

    public IEnumerable<T> GetAll()
    {
        return _entities.ToList();
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _entities.ToListAsync(cancellationToken);
    }

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        return _entities.FirstOrDefault(predicate);
    }

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _entities.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public T? SingleOrDefault(Expression<Func<T, bool>> predicate)
    {
        return _entities.SingleOrDefault(predicate);
    }

    public async Task<T?> SingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _entities.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public bool Any(Expression<Func<T, bool>> predicate)
    {
        return _entities.Any(predicate);
    }

    public Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return _entities.AnyAsync(predicate, cancellationToken);
    }

    public void Add(T entity)
    {
        _entities.Add(entity);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _entities.AddAsync(entity, cancellationToken);
    }

    public void AddRange(IEnumerable<T> range)
    {
        _entities.AddRange(range);
    }

    public Task AddRangeAsync(
        IEnumerable<T> range,
        CancellationToken cancellationToken = default)
    {
        return _entities.AddRangeAsync(range, cancellationToken);
    }

    public void Remove(T entity)
    {
        _entities.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> range)
    {
        _entities.RemoveRange(range);
    }

    public IQueryable<T> Query()
    {
        return _entities.AsQueryable();
    }
}
