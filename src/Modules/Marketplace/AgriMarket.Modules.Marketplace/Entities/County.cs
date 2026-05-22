namespace AgriMarket.Modules.Marketplace.Entities;

public sealed class County
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string EhakCode { get; set; } = default!;

    public ICollection<Municipality>? Municipalities { get; set; }
}
