using System.Linq.Expressions;

namespace LateralCms.Application.Interfaces.Persistence;

public interface IRepository<T> where T : class
{
    IEnumerable<T> GetAll();
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    T? FirstOrDefault(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    T? SingleOrDefault(Expression<Func<T, bool>> predicate);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    bool Any(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    void Add(T entity);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void AddRange(IEnumerable<T> range);
    Task AddRangeAsync(IEnumerable<T> range, CancellationToken cancellationToken = default);

    void Remove(T entity);
    void RemoveRange(IEnumerable<T> range);

    IQueryable<T> Query();
}