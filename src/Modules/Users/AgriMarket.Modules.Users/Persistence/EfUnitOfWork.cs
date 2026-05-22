using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Users.Persistence;

internal sealed class EfUnitOfWork(UsersDbContext db) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException(ex.Message);
        }
    }

    public Task BeginTransactionAsync(CancellationToken ct = default)
        => db.Database.BeginTransactionAsync(ct);

    public Task CommitTransactionAsync(CancellationToken ct = default)
        => db.Database.CommitTransactionAsync(ct);

    public Task RollbackTransactionAsync(CancellationToken ct = default)
        => db.Database.RollbackTransactionAsync(ct);
}
