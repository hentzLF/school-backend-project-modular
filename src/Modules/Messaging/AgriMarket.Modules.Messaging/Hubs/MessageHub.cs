using AgriMarket.Modules.Messaging.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AgriMarket.Modules.Messaging.Hubs;

[Authorize]
internal sealed class MessageHub(IConversationRepository conversationRepo) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var profileId = GetProfileId();
        var conversationIds = await conversationRepo.GetConversationIdsAsync(profileId, Context.ConnectionAborted);

        foreach (var conversationId in conversationIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationId), Context.ConnectionAborted);

        await base.OnConnectedAsync();
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var profileId = GetProfileId();
        var isParticipant = await conversationRepo.IsParticipantAsync(conversationId, profileId, Context.ConnectionAborted);
        if (!isParticipant)
            throw new HubException("You are not a participant of this conversation.");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationId), Context.ConnectionAborted);
    }

    public async Task SendTyping(Guid conversationId)
    {
        var profileId = GetProfileId();
        var isParticipant = await conversationRepo.IsParticipantAsync(conversationId, profileId, Context.ConnectionAborted);
        if (!isParticipant)
            throw new HubException("You are not a participant of this conversation.");

        await Clients.OthersInGroup(GroupName(conversationId))
            .SendAsync("UserTyping", new { conversationId, profileId }, Context.ConnectionAborted);
    }

    public static string GroupName(Guid conversationId) => $"conversation-{conversationId}";

    private Guid GetProfileId()
    {
        var value = Context.User?.FindFirst("profileId")?.Value;
        return Guid.TryParse(value, out var profileId)
            ? profileId
            : throw new HubException("Invalid profile identity.");
    }
}
