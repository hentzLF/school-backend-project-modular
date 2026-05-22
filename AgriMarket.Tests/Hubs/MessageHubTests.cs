using AgriMarket.Modules.Messaging.Hubs;
using AgriMarket.Modules.Messaging.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Hubs;

public class MessageHubTests
{
    [Fact]
    public void GroupName_ReturnsExpectedFormat()
    {
        var conversationId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var result = MessageHub.GroupName(conversationId);
        Assert.Equal("conversation-12345678-1234-1234-1234-123456789012", result);
    }
}
