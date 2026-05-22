namespace AgriMarket.Web.Areas.Client.ViewModels.Equipment;

internal class EquipmentAssignItemViewModel
{
    public Guid EquipmentId { get; set; }
    public string Name { get; set; } = default!;
    public string Make { get; set; } = default!;
    public string? Model { get; set; }
    public string Status { get; set; } = default!;
    public bool IsSelected { get; set; }
}
