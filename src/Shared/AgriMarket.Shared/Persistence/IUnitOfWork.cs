namespace AgriMarket.Shared.Persistence;

/// <summary>
/// Coordinates persistence of changes for a single module's DbContext,
/// including explicit transaction control.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
