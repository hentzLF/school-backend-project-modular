using System.Linq.Expressions;
using AgriMarket.Modules.Bookings.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Bookings.Persistence;

internal sealed class EfBookingRepository(BookingsDbContext db)
    : EfRepository<Booking>(db), IBookingRepository
{
    private IQueryable<Booking> BuildBaseQuery()
        => db.Set<Booking>()
            .AsNoTracking()
            .Include(b => b.Payment)
            .Include(b => b.Review);

    public async Task<List<Booking>> ListWithDetailsAsync(
        Expression<Func<Booking, bool>>? predicate = null,
        int? skip = null,
        int? take = null,
        CancellationToken ct = default)
    {
        var query = BuildBaseQuery();
        if (predicate is not null)
            query = query.Where(predicate);
        query = query.OrderByDescending(b => b.CreatedAt);
        if (skip.HasValue)
            query = query.Skip(skip.Value);
        if (take.HasValue)
            query = query.Take(take.Value);
        return await query.ToListAsync(ct);
    }

    public async Task<int> CountWithDetailsAsync(
        Expression<Func<Booking, bool>>? predicate = null,
        CancellationToken ct = default)
    {
        var query = BuildBaseQuery();
        if (predicate is not null)
            query = query.Where(predicate);
        return await query.CountAsync(ct);
    }

    public async Task<Booking?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await BuildBaseQuery().FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<Booking?> GetForUpdateAsync(Guid id, CancellationToken ct = default)
        => await db.Set<Booking>()
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<List<Booking>> ListSummariesByListingAsync(Guid listingId, CancellationToken ct = default)
        => await db.Set<Booking>()
            .AsNoTracking()
            .Include(b => b.Payment)
            .Where(b => b.ServiceListingId == listingId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);
}
