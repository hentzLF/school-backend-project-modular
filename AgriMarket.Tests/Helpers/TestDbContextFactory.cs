using AgriMarket.Modules.Users.Entities;
using AgriMarket.Modules.Users.Enums;
using AgriMarket.Modules.Users.Persistence;
using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Modules.Marketplace.Enums;
using AgriMarket.Modules.Marketplace.Persistence;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Persistence;
using AgriMarket.Modules.Messaging.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Tests.Helpers;

internal static class TestDbContextFactory
{
    public static UsersDbContext CreateUsersDb(string dbName = "test")
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(dbName + "_users_" + Guid.NewGuid())
            .Options;
        var db = new UsersDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public static MarketplaceDbContext CreateMarketplaceDb(string dbName = "test")
    {
        var options = new DbContextOptionsBuilder<MarketplaceDbContext>()
            .UseInMemoryDatabase(dbName + "_marketplace_" + Guid.NewGuid())
            .Options;
        var db = new MarketplaceDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public static BookingsDbContext CreateBookingsDb(string dbName = "test")
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(dbName + "_bookings_" + Guid.NewGuid())
            .Options;
        var db = new BookingsDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public static MessagingDbContext CreateMessagingDb(string dbName = "test")
    {
        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(dbName + "_messaging_" + Guid.NewGuid())
            .Options;
        var db = new MessagingDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public static (AppUser user, UserProfile profile) SeedClientUser(
        UsersDbContext db, string email, string password, RoleType role)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            AppUserId = user.Id
        };
        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            AppUserId = user.Id,
            Role = role
        };
        db.AppUsers.Add(user);
        db.UserProfiles.Add(profile);
        db.UserRoles.Add(userRole);
        db.SaveChanges();
        return (user, profile);
    }

    public static void EnsureServiceCategory(MarketplaceDbContext db)
    {
        var categoryId = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001");
        if (!db.ServiceCategories.Any(c => c.Id == categoryId))
        {
            db.ServiceCategories.Add(new ServiceCategory
            {
                Id = categoryId,
                Name = "Test Category"
            });
            db.SaveChanges();
        }
    }

    public static (ServiceListing listing, Availability availability) SeedListing(
        MarketplaceDbContext db, Guid providerProfileId)
    {
        EnsureServiceCategory(db);
        var categoryId = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001");
        var listing = new ServiceListing
        {
            Id = Guid.NewGuid(),
            Title = "Test Service",
            PricePerHectare = 50m,
            IsActive = true,
            UserProfileId = providerProfileId,
            ServiceCategoryId = categoryId
        };
        var availability = new Availability
        {
            Id = Guid.NewGuid(),
            ServiceListingId = listing.Id,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(2),
            IsBooked = false
        };
        db.ServiceListings.Add(listing);
        db.Availabilities.Add(availability);
        db.SaveChanges();
        return (listing, availability);
    }

    public static Equipment SeedEquipment(MarketplaceDbContext db, Guid providerProfileId)
    {
        var equipment = new Equipment
        {
            Id = Guid.NewGuid(),
            UserProfileId = providerProfileId,
            Name = "Test Tractor",
            Make = "John Deere",
            Model = "6130M",
            ManufactureYear = 2022,
            HorsePower = 130,
            Condition = EquipmentCondition.Good,
            Status = EquipmentStatus.Available
        };
        db.Equipments.Add(equipment);
        db.SaveChanges();
        return equipment;
    }

    public static Booking SeedBooking(
        BookingsDbContext db, Guid clientProfileId, Guid listingId, Guid availabilityId,
        BookingStatus status = BookingStatus.Pending)
    {
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            ClientProfileId = clientProfileId,
            ServiceListingId = listingId,
            AvailabilityId = availabilityId,
            Status = status,
            TotalPrice = 100m,
            AreaInHectares = 1.0m,
            CreatedAt = DateTime.UtcNow
        };
        db.Bookings.Add(booking);
        db.SaveChanges();
        return booking;
    }
}
