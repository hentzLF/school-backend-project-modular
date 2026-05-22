using AgriMarket.Modules.Messaging.Hubs;
using AgriMarket.Modules.Messaging.Persistence;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AgriMarket.Tests.Hubs;

public class MessageHubTests
{
    private readonly Mock<IConversationRepository> _conversationRepo = new();
    private readonly MessageHub _hub;
    private readonly Mock<IGroupManager> _groups = new();
    private readonly Mock<IHubCallerClients> _clients = new();
    private readonly Mock<HubCallerContext> _context = new();

    private static readonly Guid ProfileId = Guid.NewGuid();
    private const string ConnectionId = "test-connection-id";

    public MessageHubTests()
    {
        _hub = new MessageHub(_conversationRepo.Object);

        _context.Setup(c => c.ConnectionId).Returns(ConnectionId);
        _context.Setup(c => c.User).Returns(CreateClaimsPrincipal(ProfileId));
        _context.Setup(c => c.ConnectionAborted).Returns(CancellationToken.None);

        _hub.Context = _context.Object;
        _hub.Groups = _groups.Object;
        _hub.Clients = _clients.Object;
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(Guid profileId)
    {
        var claims = new[] { new Claim("profileId", profileId.ToString()) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    [Fact]
    public void GroupName_ReturnsExpectedFormat()
    {
        var conversationId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var result = MessageHub.GroupName(conversationId);
        Assert.Equal("conversation-12345678-1234-1234-1234-123456789012", result);
    }

    // ===== OnConnectedAsync =====

    [Fact]
    public async Task OnConnectedAsync_JoinsAllConversationGroups()
    {
        var conv1 = Guid.NewGuid();
        var conv2 = Guid.NewGuid();
        var conv3 = Guid.NewGuid();
        _conversationRepo
            .Setup(r => r.GetConversationIdsAsync(ProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([conv1, conv2, conv3]);

        await _hub.OnConnectedAsync();

        _groups.Verify(g => g.AddToGroupAsync(ConnectionId, $"conversation-{conv1}", It.IsAny<CancellationToken>()), Times.Once);
        _groups.Verify(g => g.AddToGroupAsync(ConnectionId, $"conversation-{conv2}", It.IsAny<CancellationToken>()), Times.Once);
        _groups.Verify(g => g.AddToGroupAsync(ConnectionId, $"conversation-{conv3}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_NoConversations_JoinsNoGroups()
    {
        _conversationRepo
            .Setup(r => r.GetConversationIdsAsync(ProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _hub.OnConnectedAsync();

        _groups.Verify(
            g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ===== JoinConversation =====

    [Fact]
    public async Task JoinConversation_Participant_JoinsGroup()
    {
        var conversationId = Guid.NewGuid();
        _conversationRepo
            .Setup(r => r.IsParticipantAsync(conversationId, ProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _hub.JoinConversation(conversationId);

        _groups.Verify(g => g.AddToGroupAsync(ConnectionId, $"conversation-{conversationId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task JoinConversation_NonParticipant_ThrowsHubException()
    {
        var conversationId = Guid.NewGuid();
        _conversationRepo
            .Setup(r => r.IsParticipantAsync(conversationId, ProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<HubException>(() => _hub.JoinConversation(conversationId));

        _groups.Verify(
            g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ===== SendTyping =====

    [Fact]
    public async Task SendTyping_Participant_BroadcastsToOthersInGroup()
    {
        var conversationId = Guid.NewGuid();
        _conversationRepo
            .Setup(r => r.IsParticipantAsync(conversationId, ProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var clientProxy = new Mock<ISingleClientProxy>();
        _clients
            .Setup(c => c.OthersInGroup($"conversation-{conversationId}"))
            .Returns(clientProxy.Object);

        await _hub.SendTyping(conversationId);

        clientProxy.Verify(
            p => p.SendCoreAsync("UserTyping", It.Is<object?[]>(args => args.Length == 1), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendTyping_NonParticipant_ThrowsHubException()
    {
        var conversationId = Guid.NewGuid();
        _conversationRepo
            .Setup(r => r.IsParticipantAsync(conversationId, ProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<HubException>(() => _hub.SendTyping(conversationId));
    }
}
