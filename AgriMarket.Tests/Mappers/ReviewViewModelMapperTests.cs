using AgriMarket.Modules.Bookings.Dtos.Reviews;
using AgriMarket.Web.Mappers;
using FluentAssertions;
using Xunit;

namespace AgriMarket.Tests.Mappers;

public class ReviewViewModelMapperTests
{
    [Fact]
    public void ToViewModel_MapsCorrectly()
    {
        var dto = new ReviewDto
        {
            Id = Guid.NewGuid(),
            Rating = 4,
            Comment = "Good service",
            CreatedAt = DateTime.UtcNow
        };

        var vm = dto.ToViewModel();

        vm.Rating.Should().Be(4);
        vm.Comment.Should().Be("Good service");
    }
}
