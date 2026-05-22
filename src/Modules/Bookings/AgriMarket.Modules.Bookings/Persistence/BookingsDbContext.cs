using AgriMarket.Modules.Bookings.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Bookings.Persistence;

internal sealed class BookingsDbContext : DbContext
{
    public BookingsDbContext(DbContextOptions<BookingsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; } = default!;
    public DbSet<Payment> Payments { get; set; } = default!;
    public DbSet<Review> Reviews { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("bookings");

        // Payment → Booking: one-to-one, cascade delete
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Booking)
            .WithOne(b => b.Payment)
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Review → Booking: one-to-one, cascade delete
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Booking)
            .WithOne(b => b.Review)
            .HasForeignKey<Review>(r => r.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index on Booking.Status for query performance
        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.Status);
    }
}
