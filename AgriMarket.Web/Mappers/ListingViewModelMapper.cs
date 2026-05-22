using AgriMarket.Modules.Marketplace.Dtos.Listings;
using AgriMarket.Web.Areas.Admin.ViewModels;
using AgriMarket.Web.Areas.Client.ViewModels.Equipment;
using AgriMarket.Web.Areas.Client.ViewModels.Listings;
using AgriMarket.Web.Areas.Client.ViewModels.MyListings;

namespace AgriMarket.Web.Mappers;

internal static class ListingViewModelMapper
{
    public static ListingIndexItemViewModel ToClientIndexItem(this ListingSummaryDto dto)
    {
        return new ListingIndexItemViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            CategoryName = dto.CategoryName,
            ProviderName = dto.ProviderName,
            PricePerHectare = dto.PricePerHectare,
            IsActive = dto.IsActive
        };
    }

    public static MyListingIndexItemViewModel ToMyListingIndexItem(this ListingSummaryDto dto)
    {
        return new MyListingIndexItemViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            CategoryName = dto.CategoryName,
            PricePerHectare = dto.PricePerHectare,
            IsActive = dto.IsActive
        };
    }

    public static ListingListItemViewModel ToAdminListItem(this ListingSummaryDto dto)
    {
        return new ListingListItemViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            ProviderName = dto.ProviderName,
            CategoryName = dto.CategoryName,
            PricePerHectare = dto.PricePerHectare,
            IsActive = dto.IsActive
        };
    }

    public static ListingListItemViewModel ToAdminListItem(this ListingDto dto)
    {
        return new ListingListItemViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            ProviderName = dto.ProviderName,
            CategoryName = dto.CategoryName,
            PricePerHectare = dto.PricePerHectare,
            IsActive = dto.IsActive
        };
    }

    public static ListingDetailsViewModel ToClientDetails(this ListingDto dto, bool isOwnListing)
    {
        return new ListingDetailsViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            PricePerHectare = dto.PricePerHectare,
            CategoryName = dto.CategoryName,
            ProviderName = dto.ProviderName,
            IsOwnListing = isOwnListing,
            Availabilities = dto.Availabilities
                .Where(a => !a.IsBooked)
                .OrderBy(a => a.StartTime)
                .Select(a => new AvailabilityOptionViewModel
                {
                    Id = a.Id,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime
                })
                .ToList()
        };
    }

    public static MyListingDetailsViewModel ToMyListingDetails(this ListingDto dto, int bookingCount)
    {
        return new MyListingDetailsViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            CategoryName = dto.CategoryName,
            PricePerHectare = dto.PricePerHectare,
            IsActive = dto.IsActive,
            TotalBookingCount = bookingCount
        };
    }

    public static ManageAvailabilitiesViewModel ToAvailabilitiesVm(this ListingDto dto)
    {
        return new ManageAvailabilitiesViewModel
        {
            ListingId = dto.Id,
            ListingTitle = dto.Title,
            Availabilities = dto.Availabilities
                .OrderBy(a => a.StartTime)
                .Select(a => new AvailabilityItemViewModel
                {
                    Id = a.Id,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    IsBooked = a.IsBooked
                })
                .ToList(),
            AddStartTime = DateTime.Today.AddDays(1).AddHours(8),
            AddEndTime = DateTime.Today.AddDays(1).AddHours(17)
        };
    }

    public static CreateListingDto ToCreateListingDto(this MyListingCreateViewModel vm)
    {
        return new CreateListingDto
        {
            Title = vm.Title,
            Description = vm.Description,
            ServiceCategoryId = vm.ServiceCategoryId,
            PricePerHectare = vm.PricePerHectare
        };
    }

    public static UpdateListingDto ToUpdateListingDto(this MyListingEditViewModel vm)
    {
        return new UpdateListingDto
        {
            Id = vm.Id,
            Title = vm.Title,
            Description = vm.Description,
            ServiceCategoryId = vm.ServiceCategoryId,
            PricePerHectare = vm.PricePerHectare,
            IsActive = vm.IsActive
        };
    }

    public static ListingEditViewModel ToAdminEditVm(this ListingDto dto)
    {
        return new ListingEditViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            PricePerHectare = dto.PricePerHectare,
            IsActive = dto.IsActive,
            ServiceCategoryId = dto.ServiceCategoryId
        };
    }

    public static UpdateListingDto ToUpdateListingDto(this ListingEditViewModel vm)
    {
        return new UpdateListingDto
        {
            Id = vm.Id,
            Title = vm.Title,
            Description = vm.Description,
            PricePerHectare = vm.PricePerHectare,
            IsActive = vm.IsActive,
            ServiceCategoryId = vm.ServiceCategoryId
        };
    }

    public static ListingDetailViewModel ToAdminDetailVm(this ListingDto dto, int bookingCount)
    {
        return new ListingDetailViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            PricePerHectare = dto.PricePerHectare,
            IsActive = dto.IsActive,
            ProviderName = dto.ProviderName,
            ProviderProfileId = dto.UserProfileId,
            CategoryName = dto.CategoryName,
            CategoryId = dto.ServiceCategoryId,
            BookingsCount = bookingCount,
            Equipments = dto.Equipments.Select(e => new ListingEquipmentViewModel
            {
                Name = e.Name,
                Model = e.Model,
                ManufactureYear = e.ManufactureYear
            }),
            Availabilities = dto.Availabilities.Select(a => new ListingAvailabilityViewModel
            {
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsBooked = a.IsBooked
            }).OrderBy(a => a.StartTime)
        };
    }
}
