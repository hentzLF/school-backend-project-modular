namespace AgriMarket.Web.Areas.Client.ViewModels.Equipment;

internal class EquipmentAssignViewModel
{
    public Guid ListingId { get; set; }
    public string ListingTitle { get; set; } = default!;
    public List<EquipmentAssignItemViewModel> Equipment { get; set; } = [];
}
