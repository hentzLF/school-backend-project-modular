using AgriMarket.Modules.Bookings.Dtos.Reviews;

namespace AgriMarket.Modules.Bookings.Mappers;

internal static class ReviewApiMapper
{
    public static UpdateReviewDto WithRouteId(this UpdateReviewDto dto, Guid id)
    {
        return new UpdateReviewDto
        {
            Id = id,
            Rating = dto.Rating,
            Comment = dto.Comment
        };
    }
}
