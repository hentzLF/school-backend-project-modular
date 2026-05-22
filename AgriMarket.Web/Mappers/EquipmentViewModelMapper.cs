using AgriMarket.Modules.Marketplace.Dtos.Equipment;
using AgriMarket.Web.Areas.Client.ViewModels.Equipment;

namespace AgriMarket.Web.Mappers;

internal static class EquipmentViewModelMapper
{
    public static EquipmentListItemViewModel ToListItem(this EquipmentDto dto)
    {
        return new EquipmentListItemViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Make = dto.Make,
            Model = dto.Model,
            ManufactureYear = dto.ManufactureYear,
            HorsePower = dto.HorsePower,
            Condition = dto.Condition.ToString(),
            Status = dto.Status.ToString()
        };
    }

    public static EquipmentEditViewModel ToEditViewModel(this EquipmentDto dto)
    {
        return new EquipmentEditViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Make = dto.Make,
            Model = dto.Model,
            ManufactureYear = dto.ManufactureYear,
            HorsePower = dto.HorsePower,
            Condition = dto.Condition,
            Description = dto.Description
        };
    }

    public static EquipmentDeleteViewModel ToDeleteViewModel(this EquipmentDto dto)
    {
        return new EquipmentDeleteViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Make = dto.Make,
            Model = dto.Model
        };
    }

    public static CreateEquipmentDto ToCreateDto(this EquipmentCreateViewModel vm)
    {
        return new CreateEquipmentDto
        {
            Name = vm.Name,
            Make = vm.Make,
            Model = vm.Model,
            ManufactureYear = vm.ManufactureYear,
            HorsePower = vm.HorsePower,
            Condition = vm.Condition,
            Description = vm.Description
        };
    }

    public static UpdateEquipmentDto ToUpdateDto(this EquipmentEditViewModel vm)
    {
        return new UpdateEquipmentDto
        {
            Name = vm.Name,
            Make = vm.Make,
            Model = vm.Model,
            ManufactureYear = vm.ManufactureYear,
            HorsePower = vm.HorsePower,
            Condition = vm.Condition,
            Description = vm.Description
        };
    }
}
