using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Persistence;
using AgriMarket.Shared.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgriMarket.Modules.Bookings.Services;

internal sealed class PaymentService(
    IPaymentRepository payments,
    [FromKeyedServices("bookings")] IUnitOfWork uow,
    IQueryMaterializer mat,
    ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<IEnumerable<Payment>> GetAllAsync(PaymentStatus? status)
    {
        var query = payments.Query();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        return await mat.ToListAsync(query.OrderByDescending(p => p.CreatedAt));
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await payments.GetWithBookingDetailsAsync(id);
    }

    public async Task ResolveDisputeAsync(Guid paymentId, PaymentResolution resolution)
    {
        var payment = await payments.GetByIdAsync(paymentId);
        if (payment != null && payment.Status == PaymentStatus.Disputed)
        {
            if (resolution == PaymentResolution.Release)
            {
                payment.Status = PaymentStatus.Released;
                payment.ReleasedAt = DateTime.UtcNow;
            }
            else if (resolution == PaymentResolution.Refund)
            {
                payment.Status = PaymentStatus.Refunded;
            }
            await uow.SaveChangesAsync();
        }
    }
}
