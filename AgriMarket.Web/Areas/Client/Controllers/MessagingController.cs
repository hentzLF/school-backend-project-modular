using AgriMarket.Modules.Messaging.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Modules.Messaging.Services;
using AgriMarket.Web.Areas.Client.ViewModels.Messaging;
using AgriMarket.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriMarket.Web.Areas.Client.Controllers;

[Area("Client")]
[Authorize]
[Route("Client/Messages")]
internal class MessagingController(
    IMessagingService messagingService,
    IUserService userService) : Controller
{
    private const int DefaultPageSize = 20;

    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var profileId = await GetProfileIdAsync();
        if (profileId == null) return Unauthorized();

        var conversations = await messagingService.GetConversationsAsync(profileId.Value, page, DefaultPageSize);
        var totalPages = conversations.TotalCount > 0
            ? (int)Math.Ceiling((double)conversations.TotalCount / conversations.PageSize)
            : 1;

        var viewModel = new ConversationListViewModel
        {
            Conversations = conversations.Items.Select(c => c.ToListItem()).ToList(),
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = DefaultPageSize
        };

        return View(viewModel);
    }

    [HttpGet("Details/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, int page = 1)
    {
        var profileId = await GetProfileIdAsync();
        if (profileId == null) return Unauthorized();

        try
        {
            await messagingService.MarkAllAsReadAsync(profileId.Value, id);
            var conversation = await messagingService.GetConversationAsync(profileId.Value, id, page, DefaultPageSize);
            var viewModel = conversation.ToDetailViewModel(profileId.Value);
            return View(viewModel);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("SendMessage")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(SendMessageViewModel model)
    {
        var profileId = await GetProfileIdAsync();
        if (profileId == null) return Unauthorized();

        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Details), new { id = model.ConversationId });

        try
        {
            var dto = new SendMessageDto { Content = model.Content };
            await messagingService.SendMessageAsync(profileId.Value, model.ConversationId, dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { id = model.ConversationId });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid participantProfileId, Guid? bookingId)
    {
        var profileId = await GetProfileIdAsync();
        if (profileId == null) return Unauthorized();

        var dto = new CreateConversationDto
        {
            ParticipantProfileIds = new List<Guid> { profileId.Value, participantProfileId },
            BookingId = bookingId
        };

        var (conversation, _) = await messagingService.CreateConversationAsync(profileId.Value, dto);
        return RedirectToAction(nameof(Details), new { id = conversation.Id });
    }

    private async Task<Guid?> GetProfileIdAsync()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return null;

        var profile = await userService.GetProfileByUserIdAsync(userId);
        return profile?.Id;
    }
}
