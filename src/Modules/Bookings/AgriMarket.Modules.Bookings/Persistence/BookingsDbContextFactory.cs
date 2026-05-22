using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AgriMarket.Modules.Bookings.Persistence;

internal sealed class BookingsDbContextFactory : IDesignTimeDbContextFactory<BookingsDbContext>
{
    public BookingsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseNpgsql("Host=localhost;Database=agrimarket;Username=postgres;Password=postgres",
                b => b.MigrationsHistoryTable("__EFMigrationsHistory", "bookings"))
            .Options;

        return new BookingsDbContext(options);
    }
}
