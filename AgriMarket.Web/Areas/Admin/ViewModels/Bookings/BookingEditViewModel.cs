using System.ComponentModel.DataAnnotations;
using AgriMarket.Modules.Bookings.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

public class BookingEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [EnumDataType(typeof(BookingStatus))]
    public BookingStatus Status { get; set; }

    public string ListingTitle { get; set; } = default!;
    public string ClientName { get; set; } = default!;
    public IEnumerable<SelectListItem> Statuses { get; set; } = [];
}
