using AgriMarket.Modules.Bookings.Dtos.Reviews;
using AgriMarket.Web.Areas.Client.ViewModels.Reviews;

namespace AgriMarket.Web.Mappers;

internal static class ReviewViewModelMapper
{
    public static CreateReviewDto ToCreateDto(this CreateReviewViewModel vm)
    {
        return new CreateReviewDto
        {
            BookingId = vm.BookingId,
            Rating = vm.Rating,
            Comment = vm.Comment
        };
    }

    public static UpdateReviewDto ToUpdateDto(this EditReviewViewModel vm)
    {
        return new UpdateReviewDto
        {
            Id = vm.ReviewId,
            Rating = vm.Rating,
            Comment = vm.Comment
        };
    }

    public static ReviewViewModel ToViewModel(this ReviewDto dto, string reviewerName = "")
    {
        return new ReviewViewModel
        {
            Id = dto.Id,
            BookingId = dto.BookingId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            ReviewerName = reviewerName,
            CreatedAt = dto.CreatedAt
        };
    }

    public static EditReviewViewModel ToEditViewModel(this ReviewDto dto, string bookingTitle)
    {
        return new EditReviewViewModel
        {
            ReviewId = dto.Id,
            BookingId = dto.BookingId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            BookingTitle = bookingTitle
        };
    }

    public static DeleteReviewViewModel ToDeleteViewModel(this ReviewDto dto, string reviewerName = "")
    {
        return new DeleteReviewViewModel
        {
            ReviewId = dto.Id,
            BookingId = dto.BookingId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            ReviewerName = reviewerName,
            CreatedAt = dto.CreatedAt
        };
    }

    public static RatingStatsViewModel ToRatingStatsViewModel(this RatingStatsDto dto)
    {
        return new RatingStatsViewModel
        {
            AverageRating = dto.AverageRating,
            ReviewCount = dto.ReviewCount
        };
    }

    public static ReviewListViewModel ToReviewListViewModel(
        IEnumerable<ReviewDto> reviews,
        RatingStatsDto stats,
        Guid profileId,
        string providerName,
        int currentPage,
        int totalPages)
    {
        return new ReviewListViewModel
        {
            Reviews = reviews.Select(r => r.ToViewModel()),
            ProfileId = profileId,
            ProviderName = providerName,
            AverageRating = stats.AverageRating,
            ReviewCount = stats.ReviewCount,
            CurrentPage = currentPage,
            TotalPages = totalPages
        };
    }
}
