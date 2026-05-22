using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Persistence;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Modules.Marketplace.Persistence;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Shared.Persistence;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using BookingsRepo = AgriMarket.Modules.Bookings.Persistence.EfRepository<AgriMarket.Modules.Bookings.Entities.Review>;
using BookingsUow = AgriMarket.Modules.Bookings.Persistence.EfUnitOfWork;
using MarketplaceUow = AgriMarket.Modules.Marketplace.Persistence.EfUnitOfWork;

namespace AgriMarket.Tests.Helpers;

internal static class TestServiceFactory
{
    public static ReviewService CreateReviewService(BookingsDbContext db) =>
        new(new AgriMarket.Modules.Bookings.Persistence.EfRepository<Review>(db),
            new EfBookingRepository(db),
            new AgriMarket.Modules.Bookings.Persistence.EfUnitOfWork(db),
            new EfQueryMaterializer(),
            Mock.Of<ICatalogModule>());

    public static EquipmentService CreateEquipmentService(MarketplaceDbContext db) =>
        new(new AgriMarket.Modules.Marketplace.Persistence.EfRepository<Equipment>(db),
            new AgriMarket.Modules.Marketplace.Persistence.EfRepository<ServiceListing>(db),
            new AgriMarket.Modules.Marketplace.Persistence.EfRepository<ServiceListingEquipment>(db),
            new AgriMarket.Modules.Marketplace.Persistence.EfUnitOfWork(db),
            new EfQueryMaterializer(),
            NullLogger<EquipmentService>.Instance);

    public static ClientPaymentService CreateClientPaymentService(BookingsDbContext db) =>
        new(new EfBookingRepository(db),
            new AgriMarket.Modules.Bookings.Persistence.EfRepository<Payment>(db),
            new AgriMarket.Modules.Bookings.Persistence.EfUnitOfWork(db),
            new EfQueryMaterializer(),
            Mock.Of<ICatalogModule>(),
            Mock.Of<IMediator>());

    public static PaymentService CreatePaymentService(BookingsDbContext db) =>
        new(new EfPaymentRepository(db),
            new AgriMarket.Modules.Bookings.Persistence.EfUnitOfWork(db),
            new EfQueryMaterializer(),
            NullLogger<PaymentService>.Instance);
}
