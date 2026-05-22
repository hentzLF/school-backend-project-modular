using AgriMarket.Modules.Marketplace.Dtos.Equipment;
using AgriMarket.Modules.Marketplace.Enums;

namespace AgriMarket.Modules.Marketplace.Services;

internal interface IEquipmentService
{
    Task<IReadOnlyList<EquipmentDto>> GetByProviderAsync(Guid profileId, CancellationToken ct = default);
    Task<EquipmentDto?> GetByIdAsync(Guid profileId, Guid equipmentId, CancellationToken ct = default);
    Task<EquipmentDto> CreateAsync(Guid profileId, CreateEquipmentDto dto, CancellationToken ct = default);
    Task<EquipmentDto> UpdateAsync(Guid profileId, Guid equipmentId, UpdateEquipmentDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid profileId, Guid equipmentId, CancellationToken ct = default);
    Task<EquipmentDto> UpdateStatusAsync(Guid profileId, Guid equipmentId, EquipmentStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<EquipmentDto>> GetByListingAsync(Guid listingId, CancellationToken ct = default);
    Task AssignToListingAsync(Guid profileId, Guid listingId, IReadOnlyList<Guid> equipmentIds, CancellationToken ct = default);
}
