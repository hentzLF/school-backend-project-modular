using AgriMarket.Modules.Bookings.Dtos.Payments;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Persistence;
using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Persistence;
using MediatR;
using BookingConfirmedEvent = AgriMarket.Modules.Bookings.Contracts.BookingConfirmedEvent;

namespace AgriMarket.Modules.Bookings.Services;

internal sealed class ClientPaymentService(
    IBookingRepository bookingRepo,
    IRepository<Payment> paymentRepo,
    IUnitOfWork uow,
    IQueryMaterializer mat,
    ICatalogModule catalog,
    IMediator mediator) : IClientPaymentService
{
    private const decimal PlatformFeeRate = 0.05m;

    public async Task<PaymentReceiptDto> PayAsync(Guid callerProfileId, PayRequest request, CancellationToken ct = default)
    {
        var booking = await bookingRepo.GetForUpdateAsync(request.BookingId, ct)
            ?? throw new KeyNotFoundException($"Booking {request.BookingId} not found.");

        if (booking.ClientProfileId != callerProfileId)
            throw new UnauthorizedAccessException("You are not the client of this booking.");

        if (booking.Status != BookingStatus.AwaitingPayment)
            throw new BusinessRuleException("Booking is not in a payable state.");

        if (!Enum.IsDefined(request.Method))
            throw new BusinessRuleException("Invalid payment method.");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Held,
            Amount = booking.TotalPrice,
            PlatformFee = booking.TotalPrice * PlatformFeeRate,
            CreatedAt = DateTime.UtcNow,
            BookingId = booking.Id,
            Method = request.Method
        };
        paymentRepo.Add(payment);
        booking.Status = BookingStatus.Confirmed;

        await uow.SaveChangesAsync(ct);

        var listing = await catalog.GetListingSummaryAsync(booking.ServiceListingId, ct);
        await mediator.Publish(
            new BookingConfirmedEvent(
                booking.Id,
                booking.ClientProfileId,
                listing?.UserProfileId ?? Guid.Empty,
                booking.ServiceListingId),
            ct);

        return new PaymentReceiptDto(
            payment.Id,
            payment.BookingId,
            payment.Amount,
            payment.PlatformFee,
            payment.Amount,
            payment.Method.ToString(),
            payment.Status.ToString(),
            payment.CreatedAt);
    }

    public async Task<List<PaymentHistoryItemDto>> GetHistoryAsync(Guid callerProfileId, CancellationToken ct = default)
    {
        var listingIds = (await catalog.GetListingsByProviderAsync(callerProfileId, ct))
            .Select(l => l.Id).ToList();

        var query = paymentRepo.Query()
            .Where(p => p.Booking!.ClientProfileId == callerProfileId
                     || listingIds.Contains(p.Booking!.ServiceListingId))
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentRow(
                p.Id,
                p.BookingId,
                p.Booking!.ServiceListingId,
                p.Amount,
                p.PlatformFee,
                p.Method,
                p.Status,
                p.CreatedAt,
                p.ReleasedAt));

        var rows = await mat.ToListAsync(query, ct);
        if (rows.Count == 0)
            return [];

        var listings = await catalog.GetListingSummariesAsync(
            rows.Select(r => r.ServiceListingId).Distinct().ToList(), ct);

        return rows.Select(r => new PaymentHistoryItemDto(
            r.PaymentId,
            r.BookingId,
            listings.TryGetValue(r.ServiceListingId, out var l) ? l.Title : "Unknown",
            r.Amount,
            r.PlatformFee,
            r.Method.ToString(),
            r.Status.ToString(),
            r.CreatedAt,
            r.ReleasedAt)).ToList();
    }

    private sealed record PaymentRow(
        Guid PaymentId,
        Guid BookingId,
        Guid ServiceListingId,
        decimal Amount,
        decimal PlatformFee,
        PaymentMethod Method,
        PaymentStatus Status,
        DateTime CreatedAt,
        DateTime? ReleasedAt);
}
