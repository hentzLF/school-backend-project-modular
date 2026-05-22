namespace AgriMarket.Web.Areas.Client.ViewModels.Equipment;

internal class EquipmentDeleteViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Make { get; set; } = default!;
    public string? Model { get; set; }
}
