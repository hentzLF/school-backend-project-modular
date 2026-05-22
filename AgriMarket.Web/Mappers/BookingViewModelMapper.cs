using AgriMarket.Modules.Bookings.Dtos.Bookings;
using AgriMarket.Web.Areas.Admin.ViewModels;
using AgriMarket.Web.Areas.Client.ViewModels.Bookings;
using AgriMarket.Web.Areas.Client.ViewModels.MyListings;

namespace AgriMarket.Web.Mappers;

internal static class BookingViewModelMapper
{
    public static CreateBookingDto ToCreateBookingDto(this CreateBookingViewModel vm)
    {
        return new CreateBookingDto
        {
            ServiceListingId = vm.ServiceListingId,
            AvailabilityId = vm.AvailabilityId,
            AreaInHectares = vm.AreaInHectares,
            Notes = vm.Notes
        };
    }

    public static BookingIndexItemViewModel ToClientIndexItem(this BookingDto dto)
    {
        return new BookingIndexItemViewModel
        {
            Id = dto.Id,
            ListingTitle = dto.ListingTitle,
            Status = dto.Status,
            TotalPrice = dto.TotalPrice,
            AreaInHectares = dto.AreaInHectares,
            CreatedAt = dto.CreatedAt
        };
    }

    public static BookingDetailsViewModel ToClientDetailsVm(this BookingDto dto)
    {
        return new BookingDetailsViewModel
        {
            Id = dto.Id,
            Status = dto.Status,
            TotalPrice = dto.TotalPrice,
            AreaInHectares = dto.AreaInHectares,
            CreatedAt = dto.CreatedAt,
            Notes = dto.Notes,
            ListingTitle = dto.ListingTitle,
            ListingId = dto.ServiceListingId,
            AvailabilityStart = dto.AvailabilityStart,
            AvailabilityEnd = dto.AvailabilityEnd,
            ProviderProfileId = dto.ProviderProfileId,
            ClientProfileId = dto.ClientProfileId
        };
    }

    public static BookingListItemViewModel ToAdminListItem(this BookingDto dto)
    {
        return new BookingListItemViewModel
        {
            Id = dto.Id,
            ClientName = dto.ClientName,
            ListingTitle = dto.ListingTitle,
            Status = dto.Status,
            TotalPrice = dto.TotalPrice,
            AreaInHectares = dto.AreaInHectares,
            CreatedAt = dto.CreatedAt
        };
    }

    public static BookingDetailViewModel ToAdminDetailVm(this BookingDto dto)
    {
        return new BookingDetailViewModel
        {
            Id = dto.Id,
            Status = dto.Status,
            TotalPrice = dto.TotalPrice,
            AreaInHectares = dto.AreaInHectares,
            CreatedAt = dto.CreatedAt,
            Notes = dto.Notes,
            ClientName = dto.ClientName,
            ClientProfileId = dto.ClientProfileId,
            ListingTitle = dto.ListingTitle,
            ListingId = dto.ServiceListingId,
            AvailabilityStart = dto.AvailabilityStart,
            AvailabilityEnd = dto.AvailabilityEnd
        };
    }

    public static BookingsForListingItemViewModel ToMyListingBookingItem(this BookingSummaryDto dto)
    {
        return new BookingsForListingItemViewModel
        {
            Id = dto.Id,
            ClientName = dto.ClientName,
            Status = dto.Status,
            AreaInHectares = dto.AreaInHectares,
            TotalPrice = dto.TotalPrice,
            CreatedAt = dto.CreatedAt
        };
    }
}
