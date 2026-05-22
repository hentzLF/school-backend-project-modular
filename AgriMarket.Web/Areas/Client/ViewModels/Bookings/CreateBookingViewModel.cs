using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Web.Areas.Client.ViewModels.Bookings;

internal class CreateBookingViewModel
{
    [Required]
    public Guid ServiceListingId { get; set; }

    [Required]
    public Guid AvailabilityId { get; set; }

    [Required]
    [Range(0.1, 10000, ErrorMessage = "Area must be between 0.1 and 10000 hectares")]
    public decimal AreaInHectares { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
