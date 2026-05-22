using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Bookings.Persistence;

internal interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetWithBookingDetailsAsync(Guid id, CancellationToken ct = default);
}
