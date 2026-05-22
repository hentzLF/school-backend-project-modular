using AgriMarket.Modules.Users.Services;
using AgriMarket.Modules.Users.Security;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Tests.Helpers;
using AgriMarket.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Controllers.Admin;

public class AccountControllerTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();

    private AccountController CreateController(Guid userId)
    {
        var controller = new AccountController(_userService.Object, _passwordHasher.Object);
        controller.ControllerContext = ControllerContextFactory.WithAuthenticatedUser(userId);
        return controller;
    }

    [Fact]
    public void Login_Get_ReturnsView()
    {
        var controller = CreateController(Guid.NewGuid());
        var result = controller.Login();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsView()
    {
        _userService.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync((AgriMarket.Modules.Users.Entities.AppUser?)null);

        var controller = CreateController(Guid.NewGuid());
        var result = await controller.Login(new AgriMarket.Web.ViewModels.LoginViewModel
        {
            Email = "test@test.com",
            Password = "wrong"
        });
        Assert.IsType<ViewResult>(result);
    }
}
