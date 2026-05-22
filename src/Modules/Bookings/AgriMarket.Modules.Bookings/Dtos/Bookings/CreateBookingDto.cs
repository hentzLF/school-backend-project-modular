using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Modules.Bookings.Dtos.Bookings;

internal sealed class CreateBookingDto
{
    [Required]
    public Guid ServiceListingId { get; init; }

    [Required]
    public Guid AvailabilityId { get; init; }

    [Range(0.0001, (double)decimal.MaxValue, ErrorMessage = "AreaInHectares must be positive.")]
    public decimal AreaInHectares { get; init; }

    public string? Notes { get; init; }
}
