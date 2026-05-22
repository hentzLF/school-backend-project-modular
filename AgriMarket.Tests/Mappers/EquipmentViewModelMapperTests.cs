using AgriMarket.Modules.Marketplace.Dtos.Equipment;
using AgriMarket.Modules.Marketplace.Enums;
using AgriMarket.Web.Mappers;
using FluentAssertions;
using Xunit;

namespace AgriMarket.Tests.Mappers;

public class EquipmentViewModelMapperTests
{
    [Fact]
    public void ToListItem_MapsCorrectly()
    {
        var dto = new EquipmentDto
        {
            Id = Guid.NewGuid(),
            Name = "Tractor",
            Make = "John Deere",
            Model = "6130M",
            ManufactureYear = 2022,
            HorsePower = 130,
            Condition = EquipmentCondition.Good,
            Status = EquipmentStatus.Available
        };

        var vm = dto.ToListItem();

        vm.Name.Should().Be("Tractor");
        vm.Make.Should().Be("John Deere");
    }
}
