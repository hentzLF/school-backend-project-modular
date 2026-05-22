using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Modules.Bookings.Dtos.Payments;

public sealed record PayRequest(Guid BookingId, PaymentMethod Method);
