using AgriMarket.Modules.Bookings.Dtos.Bookings;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Persistence;
using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using BookingConfirmedEvent = AgriMarket.Modules.Bookings.Contracts.BookingConfirmedEvent;

namespace AgriMarket.Modules.Bookings.Services;

/// <summary>
/// Booking lifecycle service. All profile-id parameters are UserProfile ids;
/// the API/MVC layer resolves AppUser id → UserProfile id before calling.
/// </summary>
internal sealed class BookingService(
    IBookingRepository bookingRepo,
    IRepository<Payment> paymentRepo,
    IUnitOfWork uow,
    ICatalogModule catalog,
    IUsersModule users,
    IMediator mediator,
    ILogger<BookingService> logger) : IBookingService
{
    private const decimal PlatformFeeRate = 0.05m;

    private static readonly BookingStatus[] ActiveStatuses =
    [
        BookingStatus.Pending,
        BookingStatus.AwaitingPayment,
        BookingStatus.Confirmed,
        BookingStatus.InProgress,
        BookingStatus.ProviderCompleted
    ];

    public async Task<IEnumerable<BookingDto>> GetAllAsync(BookingStatus? status = null, CancellationToken ct = default)
    {
        var bookings = await bookingRepo.ListWithDetailsAsync(
            status.HasValue ? b => b.Status == status.Value : null, ct: ct);
        return await BuildBookingDtosAsync(bookings, ct);
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var booking = await bookingRepo.GetByIdWithDetailsAsync(id, ct);
        if (booking is null)
            return null;

        var dtos = await BuildBookingDtosAsync([booking], ct);
        return dtos[0];
    }

    public async Task<IEnumerable<BookingDto>> GetByClientAsync(Guid clientProfileId, CancellationToken ct = default)
    {
        var bookings = await bookingRepo.ListWithDetailsAsync(b => b.ClientProfileId == clientProfileId, ct: ct);
        return await BuildBookingDtosAsync(bookings, ct);
    }

    public async Task<IEnumerable<BookingDto>> GetByProviderAsync(Guid providerProfileId, CancellationToken ct = default)
    {
        var listingIds = (await catalog.GetListingsByProviderAsync(providerProfileId, ct))
            .Select(l => l.Id).ToList();
        if (listingIds.Count == 0)
            return [];

        var bookings = await bookingRepo.ListWithDetailsAsync(b => listingIds.Contains(b.ServiceListingId), ct: ct);
        return await BuildBookingDtosAsync(bookings, ct);
    }

    public async Task<BookingDto> CreateAsync(Guid clientProfileId, CreateBookingDto dto, CancellationToken ct = default)
    {
        var listing = await catalog.GetListingSummaryAsync(dto.ServiceListingId, ct)
            ?? throw new KeyNotFoundException($"ServiceListing {dto.ServiceListingId} not found.");

        if (listing.UserProfileId == clientProfileId)
            throw new BusinessRuleException("Providers cannot book their own services.");

        var availability = await catalog.GetAvailabilityAsync(dto.AvailabilityId, ct);
        if (availability is null || availability.ServiceListingId != dto.ServiceListingId)
            throw new BusinessRuleException("Availability does not belong to the selected listing.");

        if (availability.IsBooked)
            throw new BusinessRuleException("The selected availability is no longer available.");

        var reserved = await catalog.TryReserveAvailabilityAsync(dto.AvailabilityId, ct);
        if (!reserved)
            throw new BusinessRuleException("The selected availability was booked by someone else.");

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            Status = BookingStatus.Pending,
            TotalPrice = dto.AreaInHectares * listing.PricePerHectare,
            AreaInHectares = dto.AreaInHectares,
            CreatedAt = DateTime.UtcNow,
            Notes = dto.Notes,
            ServiceListingId = dto.ServiceListingId,
            ClientProfileId = clientProfileId,
            AvailabilityId = dto.AvailabilityId
        };

        bookingRepo.Add(booking);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Booking {BookingId} created for listing {ListingId}", booking.Id, dto.ServiceListingId);
        return (await GetByIdAsync(booking.Id, ct))!;
    }

    public async Task<BookingDto> UpdateStatusAsync(
        Guid id, BookingStatus status, Guid? callerProfileId = null, CancellationToken ct = default)
    {
        var booking = await bookingRepo.GetForUpdateAsync(id, ct)
            ?? throw new KeyNotFoundException($"Booking {id} not found.");

        var listing = await catalog.GetListingSummaryAsync(booking.ServiceListingId, ct);
        var providerProfileId = listing?.UserProfileId ?? Guid.Empty;

        if (callerProfileId.HasValue)
        {
            var isClient = booking.ClientProfileId == callerProfileId.Value;
            var isProvider = providerProfileId == callerProfileId.Value;

            if (!isClient && !isProvider)
                throw new UnauthorizedAccessException("You are not a party to this booking.");

            var allowed = GetAllowedTransitions(booking.Status, isClient, isProvider);
            if (!allowed.Contains(status))
                throw new BusinessRuleException($"Transition from {booking.Status} to {status} is not permitted for your role.");
        }

        var previousStatus = booking.Status;
        booking.Status = status;

        if (previousStatus == BookingStatus.AwaitingPayment && status == BookingStatus.Confirmed)
        {
            paymentRepo.Add(new Payment
            {
                Id = Guid.NewGuid(),
                Status = PaymentStatus.Held,
                Amount = booking.TotalPrice,
                PlatformFee = booking.TotalPrice * PlatformFeeRate,
                CreatedAt = DateTime.UtcNow,
                BookingId = booking.Id
            });
        }

        if (status == BookingStatus.ClientConfirmed && booking.Payment is { Status: PaymentStatus.Held })
        {
            booking.Payment.Status = PaymentStatus.Released;
            booking.Payment.ReleasedAt = DateTime.UtcNow;
        }

        if (status == BookingStatus.Cancelled && booking.Payment is { Status: PaymentStatus.Held })
            booking.Payment.Status = PaymentStatus.Refunded;

        if (status == BookingStatus.Disputed && booking.Payment is not null)
            booking.Payment.Status = PaymentStatus.Disputed;

        await uow.SaveChangesAsync(ct);

        if (previousStatus != BookingStatus.Confirmed && status == BookingStatus.Confirmed)
        {
            await mediator.Publish(
                new BookingConfirmedEvent(booking.Id, booking.ClientProfileId, providerProfileId, booking.ServiceListingId),
                ct);
        }

        return (await GetByIdAsync(id, ct))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var booking = await bookingRepo.GetByIdAsync(id, ct);
        if (booking is null)
            return;

        bookingRepo.Remove(booking);
        await uow.SaveChangesAsync(ct);
    }

    public Task<int> GetCountByListingAsync(Guid listingId, CancellationToken ct = default)
        => bookingRepo.CountAsync(b => b.ServiceListingId == listingId, ct);

    public Task<bool> HasActiveBookingsAsync(Guid listingId, CancellationToken ct = default)
        => bookingRepo.AnyAsync(b => b.ServiceListingId == listingId && ActiveStatuses.Contains(b.Status), ct);

    public async Task<IEnumerable<BookingSummaryDto>> GetByListingAsync(Guid listingId, CancellationToken ct = default)
    {
        var bookings = await bookingRepo.ListSummariesByListingAsync(listingId, ct);
        if (bookings.Count == 0)
            return [];

        var clients = await users.GetProfilesAsync(
            bookings.Select(b => b.ClientProfileId).Distinct().ToList(), ct);

        return bookings.Select(b =>
        {
            clients.TryGetValue(b.ClientProfileId, out var client);
            return new BookingSummaryDto
            {
                Id = b.Id,
                ClientName = client is null ? "Unknown" : $"{client.FirstName} {client.LastName}",
                Status = b.Status,
                AreaInHectares = b.AreaInHectares,
                TotalPrice = b.TotalPrice,
                CreatedAt = b.CreatedAt,
                PaymentStatus = b.Payment is null ? null : (int)b.Payment.Status
            };
        }).ToList();
    }

    public async Task<(IEnumerable<BookingDto> Items, int TotalCount)> GetAllForProfileAsync(
        Guid profileId, int page, int pageSize, CancellationToken ct = default)
    {
        var listingIds = (await catalog.GetListingsByProviderAsync(profileId, ct))
            .Select(l => l.Id).ToList();

        System.Linq.Expressions.Expression<Func<Booking, bool>> predicate =
            b => b.ClientProfileId == profileId || listingIds.Contains(b.ServiceListingId);

        var totalCount = await bookingRepo.CountWithDetailsAsync(predicate, ct);
        var bookings = await bookingRepo.ListWithDetailsAsync(predicate, (page - 1) * pageSize, pageSize, ct);

        var dtos = await BuildBookingDtosAsync(bookings, ct);
        return (dtos, totalCount);
    }

    private async Task<List<BookingDto>> BuildBookingDtosAsync(IReadOnlyList<Booking> bookings, CancellationToken ct)
    {
        if (bookings.Count == 0)
            return [];

        var listings = await catalog.GetListingSummariesAsync(
            bookings.Select(b => b.ServiceListingId).Distinct().ToList(), ct);
        var clients = await users.GetProfilesAsync(
            bookings.Select(b => b.ClientProfileId).Distinct().ToList(), ct);

        var availabilities = new Dictionary<Guid, AvailabilityDto>();
        foreach (var availabilityId in bookings.Select(b => b.AvailabilityId).Distinct())
        {
            var availability = await catalog.GetAvailabilityAsync(availabilityId, ct);
            if (availability is not null)
                availabilities[availabilityId] = availability;
        }

        return bookings.Select(b => ToBookingDto(b, listings, clients, availabilities)).ToList();
    }

    private static BookingDto ToBookingDto(
        Booking b,
        IReadOnlyDictionary<Guid, ListingSummaryDto> listings,
        IReadOnlyDictionary<Guid, UserProfileDto> clients,
        IReadOnlyDictionary<Guid, AvailabilityDto> availabilities)
    {
        listings.TryGetValue(b.ServiceListingId, out var listing);
        clients.TryGetValue(b.ClientProfileId, out var client);
        availabilities.TryGetValue(b.AvailabilityId, out var availability);

        return new BookingDto
        {
            Id = b.Id,
            Status = b.Status,
            TotalPrice = b.TotalPrice,
            AreaInHectares = b.AreaInHectares,
            CreatedAt = b.CreatedAt,
            Notes = b.Notes,
            ServiceListingId = b.ServiceListingId,
            ClientProfileId = b.ClientProfileId,
            AvailabilityId = b.AvailabilityId,
            ClientName = client is null ? "Unknown" : $"{client.FirstName} {client.LastName}",
            ListingTitle = listing?.Title ?? "Unknown",
            ProviderProfileId = listing?.UserProfileId ?? Guid.Empty,
            AvailabilityStart = availability?.StartTime ?? default,
            AvailabilityEnd = availability?.EndTime ?? default,
            PaymentStatus = b.Payment is null ? null : (int)b.Payment.Status,
            PaymentAmount = b.Payment?.Amount,
            PaymentPlatformFee = b.Payment?.PlatformFee
        };
    }

    private static IReadOnlySet<BookingStatus> GetAllowedTransitions(BookingStatus current, bool isClient, bool isProvider)
    {
        var result = new HashSet<BookingStatus>();

        if (isClient)
        {
            if (current == BookingStatus.Pending) result.Add(BookingStatus.Cancelled);
            if (current == BookingStatus.AwaitingPayment)
            {
                result.Add(BookingStatus.Confirmed);
                result.Add(BookingStatus.Cancelled);
            }
            if (current == BookingStatus.Confirmed) result.Add(BookingStatus.Cancelled);
            if (current == BookingStatus.ProviderCompleted) result.Add(BookingStatus.ClientConfirmed);
        }

        if (isProvider)
        {
            if (current == BookingStatus.Pending)
            {
                result.Add(BookingStatus.AwaitingPayment);
                result.Add(BookingStatus.Cancelled);
            }
            if (current == BookingStatus.AwaitingPayment) result.Add(BookingStatus.Cancelled);
            if (current == BookingStatus.Confirmed) result.Add(BookingStatus.InProgress);
            if (current == BookingStatus.InProgress) result.Add(BookingStatus.ProviderCompleted);
            var terminal = new[] { BookingStatus.Cancelled, BookingStatus.ClientConfirmed, BookingStatus.Disputed };
            if (!terminal.Contains(current)) result.Add(BookingStatus.Disputed);
        }

        return result;
    }
}
