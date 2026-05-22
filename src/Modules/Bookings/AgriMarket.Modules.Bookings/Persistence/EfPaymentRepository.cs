using AgriMarket.Modules.Bookings.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Bookings.Persistence;

internal sealed class EfPaymentRepository(BookingsDbContext db)
    : EfRepository<Payment>(db), IPaymentRepository
{
    public async Task<Payment?> GetWithBookingDetailsAsync(Guid id, CancellationToken ct = default)
        => await db.Set<Payment>()
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
}
