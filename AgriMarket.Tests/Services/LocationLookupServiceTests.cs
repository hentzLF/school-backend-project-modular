using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Modules.Marketplace.Persistence;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Shared.Persistence;
using AgriMarket.Tests.Helpers;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace AgriMarket.Tests.Services;

public class LocationLookupServiceTests
{
    private readonly Mock<IRepository<County>> _counties = new();
    private readonly Mock<IRepository<Municipality>> _municipalities = new();
    private readonly LocationLookupService _sut;

    private static readonly Guid HarjuId = new("a0000000-0000-0000-0000-000000000001");
    private static readonly Guid TartuId = new("a0000000-0000-0000-0000-00000000000c");

    public LocationLookupServiceTests()
    {
        _sut = new LocationLookupService(_counties.Object, _municipalities.Object);
    }

    // -------------------------------------------------------------------------
    // GetAllCountiesAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllCountiesAsync_ReturnsCountiesOrderedByName()
    {
        // Arrange
        var countiesData = new List<County>
        {
            new() { Id = TartuId, Name = "Tartu maakond", EhakCode = "0078" },
            new() { Id = HarjuId, Name = "Harju maakond", EhakCode = "0037" }
        };
        _counties
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<County, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(countiesData);

        // Act
        var result = await _sut.GetAllCountiesAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Harju maakond");
        result[1].Name.Should().Be("Tartu maakond");
    }

    [Fact]
    public async Task GetAllCountiesAsync_MapsFieldsCorrectly()
    {
        // Arrange
        var county = new County { Id = HarjuId, Name = "Harju maakond", EhakCode = "0037" };
        _counties
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<County, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<County> { county });

        // Act
        var result = await _sut.GetAllCountiesAsync();

        // Assert
        var dto = result.Single();
        dto.Id.Should().Be(HarjuId);
        dto.Name.Should().Be("Harju maakond");
        dto.EhakCode.Should().Be("0037");
    }

    // -------------------------------------------------------------------------
    // GetMunicipalitiesByCountyAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetMunicipalitiesByCountyAsync_ReturnsMunicipalitiesOrderedByName()
    {
        // Arrange
        var muns = new List<Municipality>
        {
            new() { Id = Guid.NewGuid(), Name = "Viimsi vald", EhakCode = "0890", CountyId = HarjuId },
            new() { Id = Guid.NewGuid(), Name = "Anija vald", EhakCode = "0141", CountyId = HarjuId }
        };
        _municipalities
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Municipality, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(muns);

        // Act
        var result = await _sut.GetMunicipalitiesByCountyAsync(HarjuId);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Anija vald");
        result[1].Name.Should().Be("Viimsi vald");
    }

    [Fact]
    public async Task GetMunicipalitiesByCountyAsync_MapsFieldsCorrectly()
    {
        // Arrange
        var munId = Guid.NewGuid();
        var municipality = new Municipality
        {
            Id = munId, Name = "Tallinn", EhakCode = "0784", CountyId = HarjuId
        };
        _municipalities
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Municipality, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Municipality> { municipality });

        // Act
        var result = await _sut.GetMunicipalitiesByCountyAsync(HarjuId);

        // Assert
        var dto = result.Single();
        dto.Id.Should().Be(munId);
        dto.Name.Should().Be("Tallinn");
        dto.EhakCode.Should().Be("0784");
        dto.CountyId.Should().Be(HarjuId);
    }

    [Fact]
    public async Task GetMunicipalitiesByCountyAsync_NoMunicipalities_ReturnsEmptyList()
    {
        // Arrange
        _municipalities
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Municipality, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Municipality>());

        // Act
        var result = await _sut.GetMunicipalitiesByCountyAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // CountyExistsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CountyExistsAsync_Exists_ReturnsTrue()
    {
        // Arrange
        _counties
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<County, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.CountyExistsAsync(HarjuId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CountyExistsAsync_DoesNotExist_ReturnsFalse()
    {
        // Arrange
        _counties
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<County, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.CountyExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // DB-based: seeded data
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllCountiesAsync_ReturnsSeededCounties()
    {
        var db = TestDbContextFactory.CreateMarketplaceDb();
        var service = new LocationLookupService(
            new EfRepository<County>(db),
            new EfRepository<Municipality>(db));

        var result = await service.GetAllCountiesAsync();

        result.Should().HaveCount(15);
    }
}
