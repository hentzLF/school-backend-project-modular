using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Modules.Marketplace.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Marketplace.Persistence;

internal sealed class MarketplaceDbContext : DbContext
{
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options)
        : base(options)
    {
    }

    public DbSet<ServiceCategory> ServiceCategories { get; set; } = default!;
    public DbSet<ServiceListing> ServiceListings { get; set; } = default!;
    public DbSet<Equipment> Equipments { get; set; } = default!;
    public DbSet<ServiceListingEquipment> ServiceListingEquipments { get; set; } = default!;
    public DbSet<Location> Locations { get; set; } = default!;
    public DbSet<County> Counties { get; set; } = default!;
    public DbSet<Municipality> Municipalities { get; set; } = default!;
    public DbSet<Availability> Availabilities { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("marketplace");

        // ServiceListingEquipment: N:M join table
        modelBuilder.Entity<ServiceListingEquipment>()
            .HasKey(sle => new { sle.ServiceListingId, sle.EquipmentId });

        modelBuilder.Entity<ServiceListingEquipment>()
            .HasOne(sle => sle.ServiceListing)
            .WithMany(sl => sl.ServiceListingEquipments)
            .HasForeignKey(sle => sle.ServiceListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ServiceListingEquipment>()
            .HasOne(sle => sle.Equipment)
            .WithMany(e => e.ServiceListingEquipments)
            .HasForeignKey(sle => sle.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Availability → ServiceListing
        modelBuilder.Entity<Availability>()
            .HasOne(a => a.ServiceListing)
            .WithMany(sl => sl.Availabilities)
            .HasForeignKey(a => a.ServiceListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Availability>()
            .Property(a => a.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<Availability>()
            .HasIndex(a => a.IsBooked);

        // ServiceCategory: name must be unique
        modelBuilder.Entity<ServiceCategory>()
            .HasIndex(sc => sc.Name)
            .IsUnique();

        // ServiceListing → ServiceCategory: restrict delete
        modelBuilder.Entity<ServiceListing>()
            .HasOne(sl => sl.ServiceCategory)
            .WithMany()
            .HasForeignKey(sl => sl.ServiceCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ServiceListing → Location: cascade delete
        modelBuilder.Entity<ServiceListing>()
            .HasOne(sl => sl.Location)
            .WithMany()
            .HasForeignKey(sl => sl.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ServiceListing>()
            .HasIndex(sl => sl.IsActive);

        // County / Municipality
        modelBuilder.Entity<County>()
            .HasIndex(c => c.EhakCode)
            .IsUnique();

        modelBuilder.Entity<Municipality>()
            .HasIndex(m => m.EhakCode)
            .IsUnique();

        modelBuilder.Entity<Municipality>()
            .HasOne(m => m.County)
            .WithMany(c => c.Municipalities)
            .HasForeignKey(m => m.CountyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Location → Municipality: restrict delete
        modelBuilder.Entity<Location>()
            .HasOne(l => l.Municipality)
            .WithMany(m => m.Locations)
            .HasForeignKey(l => l.MunicipalityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seeding
        modelBuilder.Entity<ServiceCategory>().HasData(ServiceCategorySeedData.GetAll());
        modelBuilder.Entity<County>().HasData(CountySeedData.GetAll());
        modelBuilder.Entity<Municipality>().HasData(MunicipalitySeedData.GetAll());
    }
}
