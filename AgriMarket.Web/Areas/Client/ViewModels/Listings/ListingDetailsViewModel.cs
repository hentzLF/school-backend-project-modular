using AgriMarket.Web.Areas.Client.ViewModels.Equipment;
using AgriMarket.Web.Areas.Client.ViewModels.Reviews;

namespace AgriMarket.Web.Areas.Client.ViewModels.Listings;

internal class ListingDetailsViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public decimal PricePerHectare { get; set; }
    public string CategoryName { get; set; } = default!;
    public string ProviderName { get; set; } = default!;
    public Guid ProviderProfileId { get; set; }
    public IEnumerable<AvailabilityOptionViewModel> Availabilities { get; set; } = [];
    public bool IsOwnListing { get; set; }
    public RatingStatsViewModel? RatingStats { get; set; }
    public List<EquipmentListItemViewModel> Equipment { get; set; } = [];
}

internal class AvailabilityOptionViewModel
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
