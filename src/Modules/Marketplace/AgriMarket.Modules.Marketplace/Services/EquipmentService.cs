using AgriMarket.Modules.Marketplace.Dtos.Equipment;
using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Modules.Marketplace.Enums;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Persistence;
using Microsoft.Extensions.Logging;

namespace AgriMarket.Modules.Marketplace.Services;

internal sealed class EquipmentService(
    IRepository<Equipment> equipments,
    IRepository<ServiceListing> listings,
    IRepository<ServiceListingEquipment> listingEquipments,
    IUnitOfWork uow,
    IQueryMaterializer mat,
    ILogger<EquipmentService> logger) : IEquipmentService
{
    public async Task<IReadOnlyList<EquipmentDto>> GetByProviderAsync(Guid profileId, CancellationToken ct = default)
    {
        var query = equipments.Query()
            .Where(e => e.UserProfileId == profileId)
            .OrderBy(e => e.Name)
            .Select(e => ToDto(e));

        return await mat.ToListAsync(query, ct);
    }

    public async Task<EquipmentDto?> GetByIdAsync(Guid profileId, Guid equipmentId, CancellationToken ct = default)
    {
        var equipment = await equipments.FirstOrDefaultAsync(
            e => e.Id == equipmentId && e.UserProfileId == profileId, ct);

        return equipment is null ? null : ToDto(equipment);
    }

    public async Task<EquipmentDto> CreateAsync(Guid profileId, CreateEquipmentDto dto, CancellationToken ct = default)
    {
        var equipment = new Equipment
        {
            Id = Guid.NewGuid(),
            UserProfileId = profileId,
            Name = dto.Name,
            Make = dto.Make,
            Model = dto.Model,
            ManufactureYear = dto.ManufactureYear,
            HorsePower = dto.HorsePower,
            Condition = dto.Condition,
            Status = EquipmentStatus.Available,
            Description = dto.Description
        };

        equipments.Add(equipment);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Equipment {EquipmentId} created for provider {ProfileId}", equipment.Id, profileId);
        return ToDto(equipment);
    }

    public async Task<EquipmentDto> UpdateAsync(Guid profileId, Guid equipmentId, UpdateEquipmentDto dto, CancellationToken ct = default)
    {
        var equipment = await GetOwnedEquipmentOrThrow(profileId, equipmentId, ct);

        equipment.Name = dto.Name;
        equipment.Make = dto.Make;
        equipment.Model = dto.Model;
        equipment.ManufactureYear = dto.ManufactureYear;
        equipment.HorsePower = dto.HorsePower;
        equipment.Condition = dto.Condition;
        equipment.Description = dto.Description;

        equipments.Update(equipment);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Equipment {EquipmentId} updated by provider {ProfileId}", equipmentId, profileId);
        return ToDto(equipment);
    }

    public async Task DeleteAsync(Guid profileId, Guid equipmentId, CancellationToken ct = default)
    {
        var equipment = await GetOwnedEquipmentOrThrow(profileId, equipmentId, ct);

        var joinRows = await listingEquipments.FindAsync(sle => sle.EquipmentId == equipmentId, ct);
        foreach (var row in joinRows)
            listingEquipments.Remove(row);

        equipments.Remove(equipment);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Equipment {EquipmentId} deleted by provider {ProfileId}", equipmentId, profileId);
    }

    public async Task<EquipmentDto> UpdateStatusAsync(Guid profileId, Guid equipmentId, EquipmentStatus status, CancellationToken ct = default)
    {
        var equipment = await GetOwnedEquipmentOrThrow(profileId, equipmentId, ct);

        equipment.Status = status;
        equipments.Update(equipment);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Equipment {EquipmentId} status changed to {Status} by provider {ProfileId}",
            equipmentId, status, profileId);
        return ToDto(equipment);
    }

    public async Task<IReadOnlyList<EquipmentDto>> GetByListingAsync(Guid listingId, CancellationToken ct = default)
    {
        var query = listingEquipments.Query()
            .Where(sle => sle.ServiceListingId == listingId && sle.Equipment != null)
            .Select(sle => ToDto(sle.Equipment!));

        return await mat.ToListAsync(query, ct);
    }

    public async Task AssignToListingAsync(Guid profileId, Guid listingId, IReadOnlyList<Guid> equipmentIds, CancellationToken ct = default)
    {
        await VerifyListingOwnership(profileId, listingId, ct);

        var distinctIds = equipmentIds.Distinct().ToList();
        await VerifyEquipmentOwnership(profileId, distinctIds, ct);

        var existingRows = await listingEquipments.FindAsync(sle => sle.ServiceListingId == listingId, ct);
        foreach (var row in existingRows)
            listingEquipments.Remove(row);

        foreach (var equipmentId in distinctIds)
        {
            listingEquipments.Add(new ServiceListingEquipment
            {
                ServiceListingId = listingId,
                EquipmentId = equipmentId
            });
        }

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Assigned {Count} equipment to listing {ListingId} by provider {ProfileId}",
            distinctIds.Count, listingId, profileId);
    }

    private async Task<Equipment> GetOwnedEquipmentOrThrow(Guid profileId, Guid equipmentId, CancellationToken ct = default)
    {
        var equipment = await equipments.GetByIdAsync(equipmentId, ct)
            ?? throw new KeyNotFoundException($"Equipment {equipmentId} not found.");

        if (equipment.UserProfileId != profileId)
            throw new BusinessRuleException("You do not own this equipment.");

        return equipment;
    }

    private async Task VerifyListingOwnership(Guid profileId, Guid listingId, CancellationToken ct = default)
    {
        var listing = await listings.GetByIdAsync(listingId, ct)
            ?? throw new KeyNotFoundException($"Listing {listingId} not found.");

        if (listing.UserProfileId != profileId)
            throw new BusinessRuleException("You do not own this listing.");
    }

    private async Task VerifyEquipmentOwnership(Guid profileId, IReadOnlyList<Guid> equipmentIds, CancellationToken ct = default)
    {
        if (equipmentIds.Count == 0)
            return;

        var ownedCount = await equipments.CountAsync(
            e => equipmentIds.Contains(e.Id) && e.UserProfileId == profileId, ct);

        if (ownedCount != equipmentIds.Count)
            throw new BusinessRuleException("One or more equipment items do not belong to you.");
    }

    private static EquipmentDto ToDto(Equipment e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Make = e.Make,
        Model = e.Model,
        ManufactureYear = e.ManufactureYear,
        HorsePower = e.HorsePower,
        Condition = e.Condition,
        Status = e.Status,
        Description = e.Description
    };
}
