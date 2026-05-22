using AgriMarket.Modules.Messaging.Dtos;
using AgriMarket.Modules.Messaging.Services;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Shared.Dtos;
using AgriMarket.Tests.Helpers;
using AgriMarket.Web.Areas.Client.Controllers;
using AgriMarket.Web.Areas.Client.ViewModels.Messaging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Controllers.Client;

public class MessagingControllerTests
{
    private readonly Mock<IMessagingService> _messagingService = new();
    private readonly Mock<IUserService> _userService = new();

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProfileId = Guid.NewGuid();
    private static readonly Guid OtherProfileId = Guid.NewGuid();
    private static readonly Guid ConversationId = Guid.NewGuid();

    private MessagingController CreateController(Guid userId)
    {
        var controller = new MessagingController(_messagingService.Object, _userService.Object);
        controller.ControllerContext = ControllerContextFactory.WithAuthenticatedUser(userId);
        return controller;
    }

    private void SetupProfile(Guid userId, Guid profileId)
    {
        _userService
            .Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync(new UserProfileDto { Id = profileId, FirstName = "Test", LastName = "User" });
    }

    private static PaginatedResponse<ConversationSummaryDto> EmptyConversations() =>
        new() { Items = new List<ConversationSummaryDto>(), Page = 1, PageSize = 20, TotalCount = 0 };

    private static PaginatedResponse<MessageDto> EmptyMessages() =>
        new() { Items = new List<MessageDto>(), Page = 1, PageSize = 20, TotalCount = 0 };

    // --- Index ---

    [Fact]
    public async Task Index_UserNotFound_ReturnsUnauthorized()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Index();
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Index_WithConversations_ReturnsView()
    {
        SetupProfile(UserId, ProfileId);
        _messagingService.Setup(x => x.GetConversationsAsync(ProfileId, 1, 20, default))
            .ReturnsAsync(new PaginatedResponse<ConversationSummaryDto>
            {
                Items = new List<ConversationSummaryDto>(),
                TotalCount = 0, Page = 1, PageSize = 20
            });

        var controller = CreateController(UserId);
        var result = await controller.Index();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Index_CallsGetConversationsAsync_ReturnsView()
    {
        SetupProfile(UserId, ProfileId);
        _messagingService
            .Setup(s => s.GetConversationsAsync(ProfileId, 1, 20, default))
            .ReturnsAsync(new PaginatedResponse<ConversationSummaryDto>
            {
                Items = new List<ConversationSummaryDto>
                {
                    new()
                    {
                        Id = ConversationId,
                        OtherParticipant = new ParticipantDto { ProfileId = OtherProfileId, FullName = "Other User" },
                        UnreadCount = 2,
                        CreatedAt = DateTime.UtcNow
                    }
                },
                Page = 1,
                PageSize = 20,
                TotalCount = 1
            });

        var controller = CreateController(UserId);
        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ConversationListViewModel>(viewResult.Model);
        Assert.Single(model.Conversations);
        Assert.Equal("Other User", model.Conversations[0].ParticipantName);
    }

    [Fact]
    public async Task Index_EmptyList_ReturnsViewWithEmptyModel()
    {
        SetupProfile(UserId, ProfileId);
        _messagingService
            .Setup(s => s.GetConversationsAsync(ProfileId, 1, 20, default))
            .ReturnsAsync(EmptyConversations());

        var controller = CreateController(UserId);
        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ConversationListViewModel>(viewResult.Model);
        Assert.Empty(model.Conversations);
    }

    // --- Details ---

    [Fact]
    public async Task Details_UserNotFound_ReturnsUnauthorized()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Details(ConversationId);
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Details_ConversationNotFound_ReturnsNotFound()
    {
        SetupProfile(UserId, ProfileId);
        _messagingService.Setup(x => x.MarkAllAsReadAsync(ProfileId, ConversationId, default))
            .ThrowsAsync(new KeyNotFoundException());

        var controller = CreateController(UserId);
        var result = await controller.Details(ConversationId);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_CallsMarkAllAsReadAndGetConversation_ReturnsView()
    {
        SetupProfile(UserId, ProfileId);
        _messagingService.Setup(x => x.MarkAllAsReadAsync(ProfileId, ConversationId, default))
            .ReturnsAsync(0);
        _messagingService
            .Setup(s => s.GetConversationAsync(ProfileId, ConversationId, 1, 20, default))
            .ReturnsAsync(new ConversationDto
            {
                Id = ConversationId,
                Participants = new List<ParticipantDto>
                {
                    new() { ProfileId = ProfileId, FullName = "Test User" },
                    new() { ProfileId = OtherProfileId, FullName = "Other User" }
                },
                Messages = EmptyMessages()
            });

        var controller = CreateController(UserId);
        var result = await controller.Details(ConversationId);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ConversationDetailViewModel>(viewResult.Model);
        Assert.Equal(ConversationId, model.ConversationId);
        Assert.Equal("Other User", model.ParticipantName);
        _messagingService.Verify(s => s.MarkAllAsReadAsync(ProfileId, ConversationId, default), Times.Once);
    }

    // --- SendMessage ---

    [Fact]
    public async Task SendMessage_UserNotFound_ReturnsUnauthorized()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var model = new SendMessageViewModel { ConversationId = ConversationId, Content = "Hello!" };
        var result = await controller.SendMessage(model);
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task SendMessage_InvalidModel_RedirectsToDetails()
    {
        SetupProfile(UserId, ProfileId);
        var controller = CreateController(UserId);
        controller.ModelState.AddModelError("Content", "Required");

        var model = new SendMessageViewModel { ConversationId = ConversationId };
        var result = await controller.SendMessage(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
    }

    [Fact]
    public async Task SendMessage_ValidModel_CallsSendAndRedirects()
    {
        SetupProfile(UserId, ProfileId);
        _messagingService
            .Setup(s => s.SendMessageAsync(ProfileId, ConversationId, It.IsAny<SendMessageDto>(), default))
            .ReturnsAsync(new MessageDto
            {
                Id = Guid.NewGuid(),
                ConversationId = ConversationId,
                SenderProfileId = ProfileId,
                SenderName = "Test User",
                Content = "Hello!",
                SentAt = DateTime.UtcNow
            });

        var controller = CreateController(UserId);
        var model = new SendMessageViewModel { ConversationId = ConversationId, Content = "Hello!" };
        var result = await controller.SendMessage(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        _messagingService.Verify(s => s.SendMessageAsync(ProfileId, ConversationId, It.IsAny<SendMessageDto>(), default), Times.Once);
    }

    // --- Create ---

    [Fact]
    public async Task Create_UserNotFound_ReturnsUnauthorized()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Create(OtherProfileId, null);
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Create_CallsCreateConversationAndRedirects()
    {
        var bookingId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _messagingService
            .Setup(s => s.CreateConversationAsync(ProfileId, It.IsAny<CreateConversationDto>(), default))
            .ReturnsAsync((new ConversationDto
            {
                Id = ConversationId,
                BookingId = bookingId,
                Participants = new List<ParticipantDto>(),
                Messages = EmptyMessages()
            }, true));

        var controller = CreateController(UserId);
        var result = await controller.Create(OtherProfileId, bookingId);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        _messagingService.Verify(s => s.CreateConversationAsync(ProfileId, It.Is<CreateConversationDto>(
            d => d.ParticipantProfileIds.Contains(OtherProfileId) && d.BookingId == bookingId), default), Times.Once);
    }

    [Fact]
    public async Task Create_WithoutBookingId_CallsCreateWithNullBookingId()
    {
        SetupProfile(UserId, ProfileId);
        _messagingService
            .Setup(s => s.CreateConversationAsync(ProfileId, It.IsAny<CreateConversationDto>(), default))
            .ReturnsAsync((new ConversationDto
            {
                Id = ConversationId,
                Participants = new List<ParticipantDto>(),
                Messages = EmptyMessages()
            }, true));

        var controller = CreateController(UserId);
        var result = await controller.Create(OtherProfileId, null);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        _messagingService.Verify(s => s.CreateConversationAsync(ProfileId, It.Is<CreateConversationDto>(
            d => d.BookingId == null), default), Times.Once);
    }
}
