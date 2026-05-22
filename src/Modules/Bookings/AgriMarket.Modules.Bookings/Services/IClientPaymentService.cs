using AgriMarket.Modules.Bookings.Dtos.Payments;

namespace AgriMarket.Modules.Bookings.Services;

internal interface IClientPaymentService
{
    /// <param name="callerProfileId">UserProfile id of the paying client.</param>
    Task<PaymentReceiptDto> PayAsync(Guid callerProfileId, PayRequest request, CancellationToken ct = default);

    /// <param name="callerProfileId">UserProfile id whose payment history (as client or provider) is requested.</param>
    Task<List<PaymentHistoryItemDto>> GetHistoryAsync(Guid callerProfileId, CancellationToken ct = default);
}
