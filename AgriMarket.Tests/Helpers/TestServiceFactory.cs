using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Persistence;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Modules.Marketplace.Persistence;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Modules.Messaging.Contracts;
using AgriMarket.Modules.Messaging.Persistence;
using AgriMarket.Modules.Messaging.Services;
using AgriMarket.Modules.Users.Contracts;
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

    public static ReviewService CreateReviewService(BookingsDbContext db, ICatalogModule catalog) =>
        new(new AgriMarket.Modules.Bookings.Persistence.EfRepository<Review>(db),
            new EfBookingRepository(db),
            new AgriMarket.Modules.Bookings.Persistence.EfUnitOfWork(db),
            new EfQueryMaterializer(),
            catalog);

    public static MessagingService CreateMessagingService(MessagingDbContext db, IUsersModule users) =>
        new(new EfConversationRepository(db),
            new AgriMarket.Modules.Messaging.Persistence.EfRepository<AgriMarket.Modules.Messaging.Entities.Conversation>(db),
            new AgriMarket.Modules.Messaging.Persistence.EfRepository<AgriMarket.Modules.Messaging.Entities.Message>(db),
            new AgriMarket.Modules.Messaging.Persistence.EfRepository<AgriMarket.Modules.Messaging.Entities.MessageRead>(db),
            new AgriMarket.Modules.Messaging.Persistence.EfUnitOfWork(db),
            users,
            Mock.Of<IMessageNotifier>());

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
