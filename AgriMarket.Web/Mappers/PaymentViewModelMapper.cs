using AgriMarket.Modules.Bookings.Dtos.Payments;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Web.Areas.Admin.ViewModels;
using AgriMarket.Web.Areas.Client.ViewModels.Payments;

namespace AgriMarket.Web.Mappers;

public static class PaymentViewModelMapper
{
    public static PaymentListItemViewModel ToAdminListItem(this Payment p)
    {
        return new PaymentListItemViewModel
        {
            Id = p.Id,
            BookingId = p.BookingId,
            Amount = p.Amount,
            PlatformFee = p.PlatformFee,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            ReleasedAt = p.ReleasedAt
        };
    }

    public static ReceiptViewModel ToReceiptViewModel(this PaymentReceiptDto dto)
    {
        return new ReceiptViewModel
        {
            PaymentId = dto.PaymentId,
            BookingId = dto.BookingId,
            Amount = dto.Amount,
            PlatformFee = dto.PlatformFee,
            TotalCharged = dto.TotalCharged,
            Method = dto.Method,
            Status = dto.Status,
            PaidAt = dto.PaidAt
        };
    }

    public static PaymentHistoryItemViewModel ToHistoryItemViewModel(this PaymentHistoryItemDto dto)
    {
        return new PaymentHistoryItemViewModel
        {
            PaymentId = dto.PaymentId,
            BookingId = dto.BookingId,
            ListingTitle = dto.ListingTitle,
            Amount = dto.Amount,
            PlatformFee = dto.PlatformFee,
            Method = dto.Method,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt,
            ReleasedAt = dto.ReleasedAt
        };
    }
}
