namespace AgriMarket.Modules.Marketplace.Entities;

public sealed class Location
{
    public Guid Id { get; set; }

    public Guid MunicipalityId { get; set; }

    public string? Address { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    // Navigation
    public Municipality? Municipality { get; set; }
}
