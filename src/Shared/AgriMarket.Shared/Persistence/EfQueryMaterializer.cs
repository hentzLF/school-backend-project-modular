using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Shared.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IQueryMaterializer"/>. Context-agnostic —
/// shared by every module.
/// </summary>
public sealed class EfQueryMaterializer : IQueryMaterializer
{
    public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken ct = default)
        => EntityFrameworkQueryableExtensions.ToListAsync(query, ct);

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken ct = default)
        => EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(query, ct);

    public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken ct = default)
        => EntityFrameworkQueryableExtensions.CountAsync(query, ct);

    public async Task<decimal> SumAsync<T>(
        IQueryable<T> query,
        Expression<Func<T, decimal?>> selector,
        CancellationToken ct = default)
        => await EntityFrameworkQueryableExtensions.SumAsync(query, selector, ct) ?? 0m;

    public async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TSource, TKey, TValue>(
        IQueryable<TSource> query,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector,
        CancellationToken ct = default) where TKey : notnull
    {
        var list = await EntityFrameworkQueryableExtensions.ToListAsync(query, ct);
        return list.ToDictionary(keySelector, valueSelector);
    }
}
