namespace AgriMarket.Web.Areas.Client.ViewModels.Equipment;

internal class EquipmentListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Make { get; set; } = default!;
    public string? Model { get; set; }
    public int? ManufactureYear { get; set; }
    public int? HorsePower { get; set; }
    public string Condition { get; set; } = default!;
    public string Status { get; set; } = default!;
}
