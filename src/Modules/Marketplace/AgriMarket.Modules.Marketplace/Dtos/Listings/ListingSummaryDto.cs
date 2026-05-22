namespace AgriMarket.Modules.Marketplace.Dtos.Listings;

internal sealed class ListingSummaryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string CategoryName { get; init; } = default!;
    public string ProviderName { get; init; } = default!;
    public decimal PricePerHectare { get; init; }
    public bool IsActive { get; init; }
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
}
