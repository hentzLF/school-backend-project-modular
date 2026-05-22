using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Modules.Bookings.Services;

internal interface IPaymentService
{
    Task<IEnumerable<Payment>> GetAllAsync(PaymentStatus? status);
    Task<Payment?> GetByIdAsync(Guid id);
    Task ResolveDisputeAsync(Guid paymentId, PaymentResolution resolution);
}
