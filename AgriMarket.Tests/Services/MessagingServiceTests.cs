using AgriMarket.Modules.Messaging.Contracts;
using AgriMarket.Modules.Messaging.Entities;
using AgriMarket.Modules.Messaging.Persistence;
using AgriMarket.Modules.Messaging.Services;
using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Shared.Persistence;
using AgriMarket.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Services;

public class MessagingServiceTests
{
    private static MessagingService CreateService(MessagingDbContext db)
    {
        return new MessagingService(
            new EfConversationRepository(db),
            new AgriMarket.Modules.Messaging.Persistence.EfRepository<Conversation>(db),
            new AgriMarket.Modules.Messaging.Persistence.EfRepository<Message>(db),
            new AgriMarket.Modules.Messaging.Persistence.EfRepository<MessageRead>(db),
            new AgriMarket.Modules.Messaging.Persistence.EfUnitOfWork(db),
            Mock.Of<IUsersModule>(),
            Mock.Of<IMessageNotifier>());
    }

    [Fact]
    public async Task GetUnreadCountAsync_NoMessages_ReturnsZero()
    {
        var db = TestDbContextFactory.CreateMessagingDb();
        var service = CreateService(db);

        var result = await service.GetUnreadCountAsync(Guid.NewGuid());

        result.UnreadCount.Should().Be(0);
    }
}
