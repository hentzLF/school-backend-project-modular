using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Modules.Bookings.Dtos.Payments;

internal sealed record PayRequest(Guid BookingId, PaymentMethod Method);
