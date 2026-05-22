namespace AgriMarket.Modules.Marketplace.Entities;

public sealed class ServiceCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Description { get; set; }
}
