using AgriMarket.Modules.Bookings.Contracts;
using AgriMarket.Modules.Marketplace.Dtos.Listings;
using AgriMarket.Modules.Marketplace.Dtos.Locations;
using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Modules.Marketplace.Persistence;
using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Persistence;
using Microsoft.Extensions.Logging;

namespace AgriMarket.Modules.Marketplace.Services;

/// <summary>
/// Manages service listings. All Guid "userId" / "ownerId" parameters accepted by
/// write methods are UserProfile IDs (not AppUser IDs). The API/MVC layer is
/// responsible for resolving AppUser ID → UserProfile ID before invoking this service.
/// </summary>
internal sealed class ListingService(
    IListingRepository listingRepository,
    IAvailabilityRepository availabilityRepository,
    IRepository<Location> locations,
    IRepository<Municipality> municipalities,
    IUnitOfWork uow,
    IUsersModule usersModule,
    IBookingsModule bookingsModule,
    ILogger<ListingService> logger) : IListingService
{
    public async Task<IEnumerable<ListingSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var listings = await listingRepository.ListWithSummaryAsync(predicate: null, ct);
        return await BuildListingSummaryDtosAsync(listings, ct);
    }

    public async Task<ListingDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var listing = await listingRepository.GetWithFullDetailsAsync(id, ct);
        return listing is null ? null : await BuildListingDtoAsync(listing, ct);
    }

    public async Task<IEnumerable<ListingSummaryDto>> GetByProviderAsync(Guid providerProfileId, CancellationToken ct = default)
    {
        var listings = await listingRepository.ListWithSummaryAsync(l => l.UserProfileId == providerProfileId, ct);
        return await BuildListingSummaryDtosAsync(listings, ct);
    }

    public async Task<IEnumerable<ListingSummaryDto>> GetActiveListingsAsync(CancellationToken ct = default)
    {
        var listings = await listingRepository.ListWithSummaryAsync(l => l.IsActive, ct);
        return await BuildListingSummaryDtosAsync(listings, ct);
    }

    /// <param name="userId">UserProfile ID of the authenticated provider.</param>
    public async Task<ListingDto> CreateAsync(Guid userId, CreateListingDto dto, CancellationToken ct = default)
    {
        var listing = new ServiceListing
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            ServiceCategoryId = dto.ServiceCategoryId,
            PricePerHectare = dto.PricePerHectare,
            UserProfileId = userId,
            IsActive = false
        };

        if (dto.Location is not null)
        {
            await ValidateMunicipalityExistsAsync(dto.Location.MunicipalityId, ct);
            ValidateCoordinates(dto.Location.Latitude, dto.Location.Longitude);

            var location = BuildLocation(
                dto.Location.MunicipalityId,
                dto.Location.Address,
                dto.Location.Latitude,
                dto.Location.Longitude);

            locations.Add(location);
            listing.LocationId = location.Id;
        }

        listingRepository.Add(listing);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Listing {ListingId} created by profile {UserProfileId}", listing.Id, userId);

        return (await GetByIdAsync(listing.Id, ct))!;
    }

    /// <param name="userId">UserProfile ID of the authenticated provider.</param>
    public async Task<ListingDto> UpdateAsync(Guid userId, UpdateListingDto dto, CancellationToken ct = default)
    {
        var listing = await listingRepository.FirstOrDefaultAsync(l => l.Id == dto.Id, ct)
            ?? throw new KeyNotFoundException($"ServiceListing {dto.Id} not found.");

        if (listing.UserProfileId != userId)
            throw new BusinessRuleException("You do not own this listing.");

        listing.Title = dto.Title;
        listing.Description = dto.Description;
        listing.ServiceCategoryId = dto.ServiceCategoryId;
        listing.PricePerHectare = dto.PricePerHectare;
        listing.IsActive = dto.IsActive;
        await UpdateListingLocationAsync(listing, dto.Location, ct);
        await uow.SaveChangesAsync(ct);

        return (await GetByIdAsync(listing.Id, ct))!;
    }

    /// <param name="userId">UserProfile ID of the authenticated provider.</param>
    public async Task DeleteAsync(Guid userId, Guid listingId, CancellationToken ct = default)
    {
        var listing = await listingRepository.FirstOrDefaultAsync(l => l.Id == listingId, ct)
            ?? throw new KeyNotFoundException($"ServiceListing {listingId} not found.");

        if (listing.UserProfileId != userId)
            throw new BusinessRuleException("You do not own this listing.");

        var hasActiveBookings = await bookingsModule.HasActiveBookingsAsync(listingId, ct);
        if (hasActiveBookings)
            throw new BusinessRuleException("Cannot delete listing with active bookings.");

        listingRepository.Remove(listing);
        await uow.SaveChangesAsync(ct);
    }

    /// <param name="userId">UserProfile ID of the authenticated provider.</param>
    public async Task ToggleActiveAsync(Guid userId, Guid listingId, CancellationToken ct = default)
    {
        var listing = await listingRepository.FirstOrDefaultAsync(l => l.Id == listingId, ct)
            ?? throw new KeyNotFoundException($"ServiceListing {listingId} not found.");

        if (listing.UserProfileId != userId)
            throw new BusinessRuleException("You do not own this listing.");

        listing.IsActive = !listing.IsActive;
        await uow.SaveChangesAsync(ct);
    }

    /// <param name="userId">UserProfile ID of the authenticated provider.</param>
    public async Task<AvailabilityDto> AddAvailabilityAsync(Guid userId, CreateAvailabilityDto dto, CancellationToken ct = default)
    {
        if (dto.StartTime >= dto.EndTime)
            throw new BusinessRuleException("Start time must be before end time.");

        var listing = await listingRepository.FirstOrDefaultAsync(l => l.Id == dto.ListingId, ct)
            ?? throw new KeyNotFoundException($"ServiceListing {dto.ListingId} not found.");

        if (listing.UserProfileId != userId)
            throw new BusinessRuleException("You do not own this listing.");

        var availability = new Availability
        {
            Id = Guid.NewGuid(),
            ServiceListingId = dto.ListingId,
            StartTime = DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Utc),
            EndTime = DateTime.SpecifyKind(dto.EndTime, DateTimeKind.Utc),
            IsBooked = false
        };

        availabilityRepository.Add(availability);
        await uow.SaveChangesAsync(ct);

        return ToAvailabilityDto(availability);
    }

    /// <param name="userId">UserProfile ID of the authenticated provider.</param>
    public async Task DeleteAvailabilityAsync(Guid userId, Guid availabilityId, CancellationToken ct = default)
    {
        var availability = await availabilityRepository.GetWithListingAsync(availabilityId, ct)
            ?? throw new KeyNotFoundException($"Availability {availabilityId} not found.");

        if (availability.ServiceListing?.UserProfileId != userId)
            throw new BusinessRuleException("You do not own this availability.");

        if (availability.IsBooked)
            throw new BusinessRuleException("Cannot delete a booked availability slot.");

        availabilityRepository.Remove(availability);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<AvailabilityDto?> GetAvailabilityByIdAsync(Guid id, CancellationToken ct = default)
    {
        var availability = await availabilityRepository.FirstOrDefaultAsync(a => a.Id == id, ct);
        return availability is null ? null : ToAvailabilityDto(availability);
    }

    public async Task<ListingDto> AdminUpdateAsync(UpdateListingDto dto, CancellationToken ct = default)
    {
        var listing = await listingRepository.FirstOrDefaultAsync(l => l.Id == dto.Id, ct)
            ?? throw new KeyNotFoundException($"ServiceListing {dto.Id} not found.");

        listing.Title = dto.Title;
        listing.Description = dto.Description;
        listing.ServiceCategoryId = dto.ServiceCategoryId;
        listing.PricePerHectare = dto.PricePerHectare;
        listing.IsActive = dto.IsActive;
        await UpdateListingLocationAsync(listing, dto.Location, ct);
        await uow.SaveChangesAsync(ct);

        return (await GetByIdAsync(listing.Id, ct))!;
    }

    public async Task AdminDeleteAsync(Guid listingId, CancellationToken ct = default)
    {
        var listing = await listingRepository.FirstOrDefaultAsync(l => l.Id == listingId, ct)
            ?? throw new KeyNotFoundException($"ServiceListing {listingId} not found.");

        var hasActiveBookings = await bookingsModule.HasActiveBookingsAsync(listingId, ct);
        if (hasActiveBookings)
            throw new BusinessRuleException("Cannot delete listing with active bookings.");

        listingRepository.Remove(listing);
        await uow.SaveChangesAsync(ct);
    }

    public async Task AdminToggleActiveAsync(Guid listingId, CancellationToken ct = default)
    {
        var listing = await listingRepository.FirstOrDefaultAsync(l => l.Id == listingId, ct)
            ?? throw new KeyNotFoundException($"ServiceListing {listingId} not found.");

        listing.IsActive = !listing.IsActive;
        await uow.SaveChangesAsync(ct);
    }

    // --- Private helpers ---

    private async Task<IEnumerable<ListingSummaryDto>> BuildListingSummaryDtosAsync(
        IEnumerable<ServiceListing> listings,
        CancellationToken ct)
    {
        var listingsList = listings.ToList();
        if (listingsList.Count == 0)
            return [];

        var listingIds = listingsList.Select(l => l.Id).ToList();
        var ratingMap = await bookingsModule.GetListingRatingsAsync(listingIds, ct);

        var profileIds = listingsList.Select(l => l.UserProfileId).Distinct().ToList();
        var profileMap = await usersModule.GetProfilesAsync(profileIds, ct);

        return listingsList.Select(l =>
        {
            ratingMap.TryGetValue(l.Id, out var stats);
            profileMap.TryGetValue(l.UserProfileId, out var profile);
            return ToListingSummaryDto(l, profile, stats);
        }).ToList();
    }

    private async Task<ListingDto> BuildListingDtoAsync(ServiceListing listing, CancellationToken ct)
    {
        var stats = await bookingsModule.GetListingRatingAsync(listing.Id, ct);
        var profile = await usersModule.GetProfileAsync(listing.UserProfileId, ct);
        return ToListingDto(listing, profile, stats);
    }

    private static ListingSummaryDto ToListingSummaryDto(
        ServiceListing listing,
        UserProfileDto? profile,
        RatingStatsDto? stats) =>
        new()
        {
            Id = listing.Id,
            Title = listing.Title,
            CategoryName = listing.ServiceCategory?.Name ?? "Unknown",
            ProviderName = profile is null ? "Unknown" : $"{profile.FirstName} {profile.LastName}",
            PricePerHectare = listing.PricePerHectare,
            IsActive = listing.IsActive,
            AverageRating = stats?.AverageRating ?? 0,
            ReviewCount = stats?.ReviewCount ?? 0
        };

    private static ListingDto ToListingDto(
        ServiceListing listing,
        UserProfileDto? profile,
        RatingStatsDto? stats) =>
        new()
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            PricePerHectare = listing.PricePerHectare,
            IsActive = listing.IsActive,
            UserProfileId = listing.UserProfileId,
            ServiceCategoryId = listing.ServiceCategoryId,
            Location = ToLocationDto(listing.Location),
            CategoryName = listing.ServiceCategory?.Name ?? "Unknown",
            ProviderName = profile is null ? "Unknown" : $"{profile.FirstName} {profile.LastName}",
            ProviderUserId = profile?.AppUserId,
            Availabilities = (listing.Availabilities ?? [])
                .OrderBy(a => a.StartTime)
                .Select(ToAvailabilityDto)
                .ToList(),
            Equipments = (listing.ServiceListingEquipments ?? [])
                .Where(sle => sle.Equipment is not null)
                .Select(sle => ToListingEquipmentDto(sle.Equipment!))
                .ToList(),
            AverageRating = stats?.AverageRating ?? 0,
            ReviewCount = stats?.ReviewCount ?? 0
        };

    private static LocationDto? ToLocationDto(Location? location)
    {
        if (location?.Municipality is null)
            return null;

        return new LocationDto(
            location.Id,
            location.MunicipalityId,
            location.Municipality.Name,
            location.Municipality.CountyId,
            location.Municipality.County?.Name ?? "Unknown",
            location.Address,
            location.Latitude,
            location.Longitude);
    }

    private static AvailabilityDto ToAvailabilityDto(Availability availability) =>
        new()
        {
            Id = availability.Id,
            StartTime = availability.StartTime,
            EndTime = availability.EndTime,
            IsBooked = availability.IsBooked,
            ServiceListingId = availability.ServiceListingId
        };

    private static ListingEquipmentDto ToListingEquipmentDto(Equipment equipment) =>
        new()
        {
            Id = equipment.Id,
            Name = equipment.Name,
            Make = equipment.Make,
            Model = equipment.Model,
            ManufactureYear = equipment.ManufactureYear,
            HorsePower = equipment.HorsePower,
            Condition = equipment.Condition,
            Status = equipment.Status,
            Description = equipment.Description
        };

    private async Task UpdateListingLocationAsync(
        ServiceListing listing,
        UpdateLocationDto? locationDto,
        CancellationToken ct)
    {
        if (locationDto is null)
        {
            await RemoveExistingLocationAsync(listing, ct);
            return;
        }

        await ValidateMunicipalityExistsAsync(locationDto.MunicipalityId, ct);
        ValidateCoordinates(locationDto.Latitude, locationDto.Longitude);

        if (listing.LocationId.HasValue)
        {
            var existing = await locations.GetByIdAsync(listing.LocationId.Value, ct);
            if (existing is not null)
            {
                existing.MunicipalityId = locationDto.MunicipalityId;
                existing.Address = locationDto.Address;
                existing.Latitude = locationDto.Latitude;
                existing.Longitude = locationDto.Longitude;
                locations.Update(existing);
                return;
            }
        }

        var newLocation = BuildLocation(
            locationDto.MunicipalityId,
            locationDto.Address,
            locationDto.Latitude,
            locationDto.Longitude);

        locations.Add(newLocation);
        listing.LocationId = newLocation.Id;
    }

    private async Task RemoveExistingLocationAsync(ServiceListing listing, CancellationToken ct)
    {
        if (!listing.LocationId.HasValue)
            return;

        var existing = await locations.GetByIdAsync(listing.LocationId.Value, ct);
        if (existing is not null)
            locations.Remove(existing);

        listing.LocationId = null;
    }

    private async Task ValidateMunicipalityExistsAsync(Guid municipalityId, CancellationToken ct)
    {
        var exists = await municipalities.AnyAsync(m => m.Id == municipalityId, ct);
        if (!exists)
            throw new BusinessRuleException($"Municipality {municipalityId} does not exist.");
    }

    private static void ValidateCoordinates(double? latitude, double? longitude)
    {
        if (latitude.HasValue != longitude.HasValue)
            throw new BusinessRuleException("Both Latitude and Longitude must be provided together.");

        if (latitude.HasValue && (latitude.Value < -90 || latitude.Value > 90))
            throw new BusinessRuleException("Latitude must be between -90 and 90.");

        if (longitude.HasValue && (longitude.Value < -180 || longitude.Value > 180))
            throw new BusinessRuleException("Longitude must be between -180 and 180.");
    }

    private static Location BuildLocation(Guid municipalityId, string? address, double? latitude, double? longitude) =>
        new()
        {
            Id = Guid.NewGuid(),
            MunicipalityId = municipalityId,
            Address = address,
            Latitude = latitude,
            Longitude = longitude
        };
}
