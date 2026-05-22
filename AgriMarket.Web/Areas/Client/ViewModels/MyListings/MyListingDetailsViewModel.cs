using AgriMarket.Web.Areas.Client.ViewModels.Equipment;

namespace AgriMarket.Web.Areas.Client.ViewModels.MyListings
{
    public class MyListingDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public string CategoryName { get; set; } = default!;
        public decimal PricePerHectare { get; set; }
        public bool IsActive { get; set; }
        public int TotalBookingCount { get; set; }
        public bool HasActiveBookings { get; set; }
        public List<EquipmentListItemViewModel> AssignedEquipment { get; set; } = [];
        public int AssignedEquipmentCount => AssignedEquipment.Count;
    }
}