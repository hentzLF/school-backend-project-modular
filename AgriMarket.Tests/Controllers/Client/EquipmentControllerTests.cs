using AgriMarket.Modules.Marketplace.Dtos.Equipment;
using AgriMarket.Modules.Marketplace.Dtos.Listings;
using AgriMarket.Modules.Marketplace.Enums;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Tests.Helpers;
using AgriMarket.Web.Areas.Client.Controllers;
using AgriMarket.Web.Areas.Client.ViewModels.Equipment;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Controllers.Client;

public class EquipmentControllerTests
{
    private readonly Mock<IEquipmentService> _equipmentService = new();
    private readonly Mock<IListingService> _listingService = new();
    private readonly Mock<IUserService> _userService = new();

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProfileId = Guid.NewGuid();

    private EquipmentController CreateController(Guid userId)
    {
        var controller = new EquipmentController(
            _equipmentService.Object, _listingService.Object, _userService.Object);
        controller.ControllerContext = ControllerContextFactory.WithAuthenticatedUser(userId);
        return controller;
    }

    private void SetupProfile(Guid userId, Guid profileId)
    {
        _userService
            .Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync(new UserProfileDto { Id = profileId });
    }

    // --- Index ---

    [Fact]
    public async Task Index_UserNotFound_ReturnsNotFound()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Index();
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Index_ReturnsViewWithEquipmentList()
    {
        SetupProfile(UserId, ProfileId);
        _equipmentService.Setup(x => x.GetByProviderAsync(ProfileId, default))
            .ReturnsAsync(new List<EquipmentDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Test Tractor", Make = "John Deere", Status = EquipmentStatus.Available }
            });

        var controller = CreateController(UserId);
        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EquipmentIndexViewModel>(viewResult.Model);
        Assert.Single(model.Equipments);
    }

    [Fact]
    public async Task Index_ReturnsEmptyListForNewProvider()
    {
        SetupProfile(UserId, ProfileId);
        _equipmentService.Setup(x => x.GetByProviderAsync(ProfileId, default))
            .ReturnsAsync(new List<EquipmentDto>());

        var controller = CreateController(UserId);
        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EquipmentIndexViewModel>(viewResult.Model);
        Assert.Empty(model.Equipments);
    }

    // --- Create GET ---

    [Fact]
    public void Create_Get_ReturnsView()
    {
        var controller = CreateController(UserId);
        var result = controller.Create();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<EquipmentCreateViewModel>(viewResult.Model);
    }

    // --- Create POST ---

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        var controller = CreateController(UserId);
        controller.ModelState.AddModelError("Name", "Required");

        var result = await controller.Create(new EquipmentCreateViewModel());
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_UserNotFound_ReturnsNotFound()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var model = new EquipmentCreateViewModel
        {
            Name = "Test Tractor",
            Make = "John Deere",
            Condition = EquipmentCondition.Good
        };

        var result = await controller.Create(model);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Post_ValidModel_RedirectsToIndex()
    {
        SetupProfile(UserId, ProfileId);
        _equipmentService
            .Setup(x => x.CreateAsync(ProfileId, It.IsAny<CreateEquipmentDto>(), default))
            .ReturnsAsync(new EquipmentDto { Id = Guid.NewGuid(), Name = "Test Tractor", Make = "John Deere" });

        var controller = CreateController(UserId);
        var model = new EquipmentCreateViewModel
        {
            Name = "Test Tractor",
            Make = "John Deere",
            Condition = EquipmentCondition.Good
        };

        var result = await controller.Create(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(EquipmentController.Index), redirect.ActionName);
    }

    // --- Edit GET ---

    [Fact]
    public async Task Edit_Get_UserNotFound_ReturnsNotFound()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Edit(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_EquipmentNotFound_ReturnsNotFound()
    {
        SetupProfile(UserId, ProfileId);
        _equipmentService.Setup(x => x.GetByIdAsync(ProfileId, It.IsAny<Guid>(), default))
            .ReturnsAsync((EquipmentDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Edit(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsViewWithPopulatedModel()
    {
        var equipmentId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _equipmentService.Setup(x => x.GetByIdAsync(ProfileId, equipmentId, default))
            .ReturnsAsync(new EquipmentDto
            {
                Id = equipmentId,
                Name = "Test Tractor",
                Make = "John Deere",
                Condition = EquipmentCondition.Good
            });

        var controller = CreateController(UserId);
        var result = await controller.Edit(equipmentId);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EquipmentEditViewModel>(viewResult.Model);
        Assert.Equal(equipmentId, model.Id);
        Assert.Equal("Test Tractor", model.Name);
    }

    // --- Edit POST ---

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsView()
    {
        var equipmentId = Guid.NewGuid();
        var controller = CreateController(UserId);
        controller.ModelState.AddModelError("Name", "Required");

        var result = await controller.Edit(equipmentId, new EquipmentEditViewModel { Id = equipmentId });
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_RedirectsToIndex()
    {
        var equipmentId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _equipmentService
            .Setup(x => x.UpdateAsync(ProfileId, equipmentId, It.IsAny<UpdateEquipmentDto>(), default))
            .ReturnsAsync(new EquipmentDto { Id = equipmentId, Name = "Updated Tractor", Make = "Valtra" });

        var controller = CreateController(UserId);
        var model = new EquipmentEditViewModel
        {
            Id = equipmentId,
            Name = "Updated Tractor",
            Make = "Valtra",
            Condition = EquipmentCondition.Excellent
        };

        var result = await controller.Edit(equipmentId, model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(EquipmentController.Index), redirect.ActionName);
    }

    // --- Delete GET ---

    [Fact]
    public async Task Delete_Get_UserNotFound_ReturnsNotFound()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Delete(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_EquipmentNotOwned_ReturnsNotFound()
    {
        SetupProfile(UserId, ProfileId);
        _equipmentService.Setup(x => x.GetByIdAsync(ProfileId, It.IsAny<Guid>(), default))
            .ReturnsAsync((EquipmentDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Delete(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ReturnsConfirmationView()
    {
        var equipmentId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _equipmentService.Setup(x => x.GetByIdAsync(ProfileId, equipmentId, default))
            .ReturnsAsync(new EquipmentDto { Id = equipmentId, Name = "Test Tractor", Make = "John Deere" });

        var controller = CreateController(UserId);
        var result = await controller.Delete(equipmentId);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EquipmentDeleteViewModel>(viewResult.Model);
        Assert.Equal(equipmentId, model.Id);
    }

    // --- DeleteConfirmed ---

    [Fact]
    public async Task DeleteConfirmed_UserNotFound_ReturnsNotFound()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.DeleteConfirmed(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesEquipmentAndRedirects()
    {
        var equipmentId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _equipmentService.Setup(x => x.DeleteAsync(ProfileId, equipmentId, default))
            .Returns(Task.CompletedTask);

        var controller = CreateController(UserId);
        var result = await controller.DeleteConfirmed(equipmentId);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(EquipmentController.Index), redirect.ActionName);
        _equipmentService.Verify(x => x.DeleteAsync(ProfileId, equipmentId, default), Times.Once);
    }

    // --- UpdateStatus ---

    [Fact]
    public async Task UpdateStatus_UserNotFound_ReturnsNotFound()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.UpdateStatus(Guid.NewGuid(), EquipmentStatus.UnderMaintenance);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_ValidStatus_RedirectsToIndex()
    {
        var equipmentId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _equipmentService
            .Setup(x => x.UpdateStatusAsync(ProfileId, equipmentId, EquipmentStatus.UnderMaintenance, default))
            .ReturnsAsync(new EquipmentDto { Id = equipmentId, Status = EquipmentStatus.UnderMaintenance, Name = "Tractor", Make = "JD" });

        var controller = CreateController(UserId);
        var result = await controller.UpdateStatus(equipmentId, EquipmentStatus.UnderMaintenance);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(EquipmentController.Index), redirect.ActionName);
    }

    // --- AssignToListing GET ---

    [Fact]
    public async Task AssignToListing_Get_UserNotFound_ReturnsNotFound()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.AssignToListing(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AssignToListing_Get_ListingNotFound_ReturnsNotFound()
    {
        SetupProfile(UserId, ProfileId);
        _listingService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((ListingDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.AssignToListing(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AssignToListing_Get_OtherProviderListing_ReturnsNotFound()
    {
        var listingId = Guid.NewGuid();
        var otherProfileId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _listingService.Setup(x => x.GetByIdAsync(listingId, default))
            .ReturnsAsync(new ListingDto { Id = listingId, UserProfileId = otherProfileId, Title = "Other" });

        var controller = CreateController(UserId);
        var result = await controller.AssignToListing(listingId);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AssignToListing_Get_ReturnsViewWithEquipmentList()
    {
        var listingId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _listingService.Setup(x => x.GetByIdAsync(listingId, default))
            .ReturnsAsync(new ListingDto { Id = listingId, UserProfileId = ProfileId, Title = "Tractor Listing" });
        _equipmentService.Setup(x => x.GetByProviderAsync(ProfileId, default))
            .ReturnsAsync(new List<EquipmentDto>
            {
                new() { Id = equipmentId, Name = "Tractor", Make = "JD", Status = EquipmentStatus.Available }
            });
        _equipmentService.Setup(x => x.GetByListingAsync(listingId, default))
            .ReturnsAsync(new List<EquipmentDto>());

        var controller = CreateController(UserId);
        var result = await controller.AssignToListing(listingId);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EquipmentAssignViewModel>(viewResult.Model);
        Assert.Equal(listingId, model.ListingId);
        Assert.Single(model.Equipment);
    }

    // --- AssignToListing POST ---

    [Fact]
    public async Task AssignToListing_Post_AssignsEquipmentAndRedirects()
    {
        var listingId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _listingService.Setup(x => x.GetByIdAsync(listingId, default))
            .ReturnsAsync(new ListingDto { Id = listingId, UserProfileId = ProfileId, Title = "Tractor Listing" });
        _equipmentService
            .Setup(x => x.AssignToListingAsync(ProfileId, listingId, It.IsAny<IReadOnlyList<Guid>>(), default))
            .Returns(Task.CompletedTask);

        var controller = CreateController(UserId);
        var result = await controller.AssignToListing(listingId, new List<Guid> { equipmentId });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("MyListings", redirect.ControllerName);
    }
}
