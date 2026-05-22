using AgriMarket.Modules.Messaging.Services;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Shared.Dtos;
using AgriMarket.Modules.Messaging.Dtos;
using AgriMarket.Tests.Helpers;
using AgriMarket.Web.Areas.Client.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Controllers.Client;

public class MessagingControllerTests
{
    private readonly Mock<IMessagingService> _messagingService = new();
    private readonly Mock<IUserService> _userService = new();

    private MessagingController CreateController(Guid userId)
    {
        var controller = new MessagingController(_messagingService.Object, _userService.Object);
        controller.ControllerContext = ControllerContextFactory.WithAuthenticatedUser(userId);
        return controller;
    }

    [Fact]
    public async Task Index_UserNotFound_ReturnsUnauthorized()
    {
        var userId = Guid.NewGuid();
        _userService.Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(userId);
        var result = await controller.Index();
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Index_WithConversations_ReturnsView()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        _userService.Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync(new UserProfileDto { Id = profileId });
        _messagingService.Setup(x => x.GetConversationsAsync(profileId, 1, 20, default))
            .ReturnsAsync(new PaginatedResponse<ConversationSummaryDto>
            {
                Items = new List<ConversationSummaryDto>(),
                TotalCount = 0, Page = 1, PageSize = 20
            });

        var controller = CreateController(userId);
        var result = await controller.Index();
        Assert.IsType<ViewResult>(result);
    }
}
