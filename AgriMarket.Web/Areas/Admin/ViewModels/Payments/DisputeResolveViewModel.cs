using System.ComponentModel.DataAnnotations;
using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

internal class DisputeResolveViewModel
{
    public Guid PaymentId { get; set; }

    [Required]
    public PaymentResolution Resolution { get; set; }
}
