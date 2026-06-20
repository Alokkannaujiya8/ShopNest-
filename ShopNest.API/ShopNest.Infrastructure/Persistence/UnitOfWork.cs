using System.Collections.Concurrent;
using ShopNest.Application.Interfaces;

namespace ShopNest.Infrastructure.Persistence;

public sealed class UnitOfWork(ShopNestDbContext context) : IUnitOfWork
{
    private readonly ConcurrentDictionary<string, object> _repositories = new();

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T).Name;
        return (IRepository<T>)_repositories.GetOrAdd(type, _ => new Repository<T>(context));
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public void Dispose() => context.Dispose();
}
