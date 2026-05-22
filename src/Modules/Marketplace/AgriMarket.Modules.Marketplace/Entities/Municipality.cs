namespace AgriMarket.Modules.Marketplace.Entities;

internal sealed class Municipality
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string EhakCode { get; set; } = default!;

    public Guid CountyId { get; set; }

    // Navigation
    public County? County { get; set; }

    public ICollection<Location>? Locations { get; set; }
}
