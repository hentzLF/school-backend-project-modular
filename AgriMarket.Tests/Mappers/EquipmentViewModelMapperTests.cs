using AgriMarket.Modules.Marketplace.Dtos.Equipment;
using AgriMarket.Modules.Marketplace.Enums;
using AgriMarket.Web.Areas.Client.ViewModels.Equipment;
using AgriMarket.Web.Mappers;
using FluentAssertions;
using Xunit;

namespace AgriMarket.Tests.Mappers;

public class EquipmentViewModelMapperTests
{
    private static EquipmentDto CreateSampleDto() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Tractor",
        Make = "John Deere",
        Model = "6130M",
        ManufactureYear = 2022,
        HorsePower = 130,
        Condition = EquipmentCondition.Good,
        Status = EquipmentStatus.Available,
        Description = "A good tractor"
    };

    [Fact]
    public void ToListItem_MapsAllProperties()
    {
        // Arrange
        var dto = CreateSampleDto();

        // Act
        var result = dto.ToListItem();

        // Assert
        result.Id.Should().Be(dto.Id);
        result.Name.Should().Be("Test Tractor");
        result.Make.Should().Be("John Deere");
        result.Model.Should().Be("6130M");
        result.ManufactureYear.Should().Be(2022);
        result.HorsePower.Should().Be(130);
        result.Condition.Should().Be("Good");
        result.Status.Should().Be("Available");
    }

    [Fact]
    public void ToListItem_HandlesNullOptionalFields()
    {
        // Arrange
        var dto = new EquipmentDto
        {
            Id = Guid.NewGuid(),
            Name = "Basic Equipment",
            Make = "Generic",
            Condition = EquipmentCondition.Fair,
            Status = EquipmentStatus.InUse
        };

        // Act
        var result = dto.ToListItem();

        // Assert
        result.Model.Should().BeNull();
        result.ManufactureYear.Should().BeNull();
        result.HorsePower.Should().BeNull();
    }

    [Fact]
    public void ToEditViewModel_MapsAllProperties()
    {
        // Arrange
        var dto = CreateSampleDto();

        // Act
        var result = dto.ToEditViewModel();

        // Assert
        result.Id.Should().Be(dto.Id);
        result.Name.Should().Be("Test Tractor");
        result.Make.Should().Be("John Deere");
        result.Model.Should().Be("6130M");
        result.ManufactureYear.Should().Be(2022);
        result.HorsePower.Should().Be(130);
        result.Condition.Should().Be(EquipmentCondition.Good);
        result.Description.Should().Be("A good tractor");
    }

    [Fact]
    public void ToDeleteViewModel_MapsIdentifyingProperties()
    {
        // Arrange
        var dto = CreateSampleDto();

        // Act
        var result = dto.ToDeleteViewModel();

        // Assert
        result.Id.Should().Be(dto.Id);
        result.Name.Should().Be("Test Tractor");
        result.Make.Should().Be("John Deere");
        result.Model.Should().Be("6130M");
    }

    [Fact]
    public void ToCreateDto_MapsAllFields()
    {
        // Arrange
        var vm = new EquipmentCreateViewModel
        {
            Name = "New Combine",
            Make = "CLAAS",
            Model = "LEXION 8900",
            ManufactureYear = 2024,
            HorsePower = 790,
            Condition = EquipmentCondition.New,
            Description = "Brand new combine"
        };

        // Act
        var result = vm.ToCreateDto();

        // Assert
        result.Name.Should().Be("New Combine");
        result.Make.Should().Be("CLAAS");
        result.Model.Should().Be("LEXION 8900");
        result.ManufactureYear.Should().Be(2024);
        result.HorsePower.Should().Be(790);
        result.Condition.Should().Be(EquipmentCondition.New);
        result.Description.Should().Be("Brand new combine");
    }

    [Fact]
    public void ToUpdateDto_MapsAllFields()
    {
        // Arrange
        var vm = new EquipmentEditViewModel
        {
            Id = Guid.NewGuid(),
            Name = "Updated Tractor",
            Make = "Valtra",
            Model = "T254",
            ManufactureYear = 2023,
            HorsePower = 254,
            Condition = EquipmentCondition.Excellent,
            Description = "Updated description"
        };

        // Act
        var result = vm.ToUpdateDto();

        // Assert
        result.Name.Should().Be("Updated Tractor");
        result.Make.Should().Be("Valtra");
        result.Model.Should().Be("T254");
        result.ManufactureYear.Should().Be(2023);
        result.HorsePower.Should().Be(254);
        result.Condition.Should().Be(EquipmentCondition.Excellent);
        result.Description.Should().Be("Updated description");
    }

    [Fact]
    public void ToCreateDto_HandlesNullOptionalFields()
    {
        // Arrange
        var vm = new EquipmentCreateViewModel
        {
            Name = "Simple Tool",
            Make = "Generic",
            Condition = EquipmentCondition.Good
        };

        // Act
        var result = vm.ToCreateDto();

        // Assert
        result.Model.Should().BeNull();
        result.ManufactureYear.Should().BeNull();
        result.HorsePower.Should().BeNull();
        result.Description.Should().BeNull();
    }
}
