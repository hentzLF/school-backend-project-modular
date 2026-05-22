namespace AgriMarket.Modules.Marketplace.Contracts;

public sealed record ListingSummaryDto(
    Guid Id,
    string Title,
    decimal PricePerHectare,
    bool IsActive,
    Guid UserProfileId,
    Guid ServiceCategoryId,
    Guid? LocationId);
