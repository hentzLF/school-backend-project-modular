using System.ComponentModel.DataAnnotations;
using AgriMarket.Modules.Marketplace.Dtos.Locations;
using FluentAssertions;
using Xunit;

namespace AgriMarket.Tests.Dtos;

public class LocationDtoValidationTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void CreateLocationDto_ValidMinimal_PassesValidation()
    {
        // Arrange
        var dto = new CreateLocationDto { MunicipalityId = Guid.NewGuid() };

        // Act
        var results = Validate(dto);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CreateLocationDto_ValidWithCoordinates_PassesValidation()
    {
        // Arrange
        var dto = new CreateLocationDto
        {
            MunicipalityId = Guid.NewGuid(),
            Address = "Tammsaare tee 56",
            Latitude = 59.437,
            Longitude = 24.7536
        };

        // Act
        var results = Validate(dto);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CreateLocationDto_LatitudeWithoutLongitude_FailsValidation()
    {
        // Arrange
        var dto = new CreateLocationDto
        {
            MunicipalityId = Guid.NewGuid(),
            Latitude = 59.437
        };

        // Act
        var results = Validate(dto);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("Latitude and Longitude must be provided together");
    }

    [Fact]
    public void CreateLocationDto_LongitudeWithoutLatitude_FailsValidation()
    {
        // Arrange
        var dto = new CreateLocationDto
        {
            MunicipalityId = Guid.NewGuid(),
            Longitude = 24.7536
        };

        // Act
        var results = Validate(dto);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("Latitude and Longitude must be provided together");
    }

    [Theory]
    [InlineData(-91.0)]
    [InlineData(91.0)]
    public void CreateLocationDto_InvalidLatitude_FailsValidation(double latitude)
    {
        // Arrange
        var dto = new CreateLocationDto
        {
            MunicipalityId = Guid.NewGuid(),
            Latitude = latitude,
            Longitude = 24.0
        };

        // Act
        var results = Validate(dto);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateLocationDto.Latitude)));
    }

    [Theory]
    [InlineData(-181.0)]
    [InlineData(181.0)]
    public void CreateLocationDto_InvalidLongitude_FailsValidation(double longitude)
    {
        // Arrange
        var dto = new CreateLocationDto
        {
            MunicipalityId = Guid.NewGuid(),
            Latitude = 59.0,
            Longitude = longitude
        };

        // Act
        var results = Validate(dto);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateLocationDto.Longitude)));
    }

    [Fact]
    public void CreateLocationDto_BoundaryLatitude_PassesValidation()
    {
        // Arrange
        var dto = new CreateLocationDto
        {
            MunicipalityId = Guid.NewGuid(),
            Latitude = 90.0,
            Longitude = 180.0
        };

        // Act
        var results = Validate(dto);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CreateLocationDto_NegativeBoundary_PassesValidation()
    {
        // Arrange
        var dto = new CreateLocationDto
        {
            MunicipalityId = Guid.NewGuid(),
            Latitude = -90.0,
            Longitude = -180.0
        };

        // Act
        var results = Validate(dto);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void UpdateLocationDto_ValidMinimal_PassesValidation()
    {
        // Arrange
        var dto = new UpdateLocationDto { MunicipalityId = Guid.NewGuid() };

        // Act
        var results = Validate(dto);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void UpdateLocationDto_LatitudeWithoutLongitude_FailsValidation()
    {
        // Arrange
        var dto = new UpdateLocationDto
        {
            MunicipalityId = Guid.NewGuid(),
            Latitude = 59.437
        };

        // Act
        var results = Validate(dto);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("Latitude and Longitude must be provided together");
    }

    [Fact]
    public void CreateLocationDto_EmptyMunicipalityId_FailsValidation()
    {
        // Arrange
        var dto = new CreateLocationDto { MunicipalityId = Guid.Empty };

        // Act
        var results = Validate(dto);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateLocationDto.MunicipalityId)));
    }

    [Fact]
    public void UpdateLocationDto_EmptyMunicipalityId_FailsValidation()
    {
        // Arrange
        var dto = new UpdateLocationDto { MunicipalityId = Guid.Empty };

        // Act
        var results = Validate(dto);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains(nameof(UpdateLocationDto.MunicipalityId)));
    }
}
