using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;
using ConcertTickets_API.DTO;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ConcertTickets_API.Services;

public class ConcertService
{

    private readonly IReservationRepository _reservations;
    private readonly IConcertRepository _concerts;
    private readonly ILocationRepository _locations;
    private readonly ICategoryRepository _categories;
    private readonly IRegionSeatingRepository _regionSeating;
    private readonly IDistributedCache _cache;

    public ConcertService(
        IReservationRepository reservations,
        IConcertRepository concerts,
        ICategoryRepository categories,
        ILocationRepository locations,
        IRegionSeatingRepository regions,
        IDistributedCache cache)
    {
        _reservations = reservations;
        _concerts = concerts;
        _categories = categories;
        _locations = locations;
        _regionSeating = regions;
        _cache = cache;
    }

    public Task<List<Concert>> GetAllAsync(bool includeRefs, CancellationToken ct = default)
        => _concerts.GetAllAsync(includeRefs, ct);

    public Task<Concert?> GetByIdAsync(int id, bool includeRefs, CancellationToken ct = default)
        => _concerts.GetByIdAsync(id, includeRefs, ct);
    public async Task<ConcertListItemDto?> GetListItemByIdAsync(int id, CancellationToken ct = default)
    {
        var concert = await _concerts.GetByIdAsync(id, includeRefs: true, ct);

        if (concert is null)
            return null;

        return MapToListItemDto(concert);
    }
    public async Task<List<ConcertListItemDto>> GetAllListItemsAsync(CancellationToken ct = default)
    {
        var concerts = await _concerts.GetAllAsync(includeRefs: true, ct);

        return concerts.Select(MapToListItemDto).ToList();
    }

    public async Task<List<ConcertListItemDto>> GetFilteredListItemsAsync(
        int? categoryId,
        int? locationId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken ct = default)
    {
        var concerts = await _concerts.GetFilteredAsync(
            includeRefs: true,
            categoryId,
            locationId,
            dateFrom,
            dateTo,
            onlyPublished: true,
            ct);

        return concerts.Select(MapToListItemDto).ToList();
    }

    public async Task<Concert> CreateAsync(
    string name,
    DateTime date,
    int categoryId,
    int locationId,
    DateTime? earlyBirdDiscountUntil,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Naziv koncerta je obavezan.");

        var category = await _categories.GetByIdAsync(categoryId, ct);
        if (category is null)
            throw new ArgumentException("Kategorija ne postoji.");

        var location = await _locations.GetByIdAsync(locationId, includeRegions: false, ct);
        if (location is null)
            throw new ArgumentException("Lokacija ne postoji.");

        var regions = await _regionSeating.GetByLocationAsync(locationId, ct);
        if (regions == null || regions.Count == 0)
            throw new ArgumentException("Lokacija mora imati bar jedan region sjedenja prije kreiranja koncerta.");

        DateTime concertUtc;
        if (date.Kind == DateTimeKind.Unspecified)
            concertUtc = DateTime.SpecifyKind(date, DateTimeKind.Local).ToUniversalTime();
        else
            concertUtc = date.ToUniversalTime();

        DateTime? earlyBirdUtc = null;
        if (earlyBirdDiscountUntil.HasValue)
        {
            var eb = earlyBirdDiscountUntil.Value;

            if (eb.Kind == DateTimeKind.Unspecified)
                earlyBirdUtc = DateTime.SpecifyKind(eb, DateTimeKind.Local).ToUniversalTime();
            else
                earlyBirdUtc = eb.ToUniversalTime();

            if (earlyBirdUtc.Value > concertUtc)
                throw new ArgumentException("Datum za early bird popust mora biti prije datuma koncerta.");
        }

        var concert = new Concert
        {
            Name = name.Trim(),
            Date = concertUtc,
            CategoryId = categoryId,
            LocationId = locationId,
            EarlyBirdDiscountUntil = earlyBirdUtc
        };

        return await _concerts.AddAsync(concert, ct);

    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var deleted = await _concerts.DeleteAsync(id, ct);

        if (deleted)
            await _cache.RemoveAsync("home_published_concerts", ct);

        return deleted;
    }

    //public Task<List<Concert>> GetFilteredAsync(
    //    bool includeRefs,
    //    int? categoryId,
    //    int? locationId,
    //    DateTime? dateFrom,
    //    DateTime? dateTo,
    //    CancellationToken ct = default)
    //{
    //    return _concerts.GetFilteredAsync(includeRefs, categoryId, locationId, dateFrom, dateTo, ct);
    //}

    private static ConcertListItemDto MapToListItemDto(Concert concert)
    {
        return new ConcertListItemDto
        {
            Id = concert.Id,
            Name = concert.Name,
            Date = concert.Date,
            CategoryId = concert.CategoryId,
            CategoryName = concert.Category?.Name ?? string.Empty,
            LocationId = concert.LocationId,
            LocationName = concert.Location?.Name ?? string.Empty,
            EarlyBirdDiscountUntil = concert.EarlyBirdDiscountUntil,
            IsPublished = concert.isPublished
        };
    }

    public async Task<List<ConcertListItemDto>> GetFilteredListItemsForAdminAsync(
    int? categoryId,
    int? locationId,
    DateTime? dateFrom,
    DateTime? dateTo,
    CancellationToken ct = default)
    {
        var concerts = await _concerts.GetFilteredAsync(
            includeRefs: true,
            categoryId,
            locationId,
            dateFrom,
            dateTo,
            onlyPublished: false,
            ct
        );

        return concerts.Select(c => new ConcertListItemDto
        {
            Id = c.Id,
            Name = c.Name,
            Date = c.Date,
            CategoryId = c.CategoryId,
            CategoryName = c.Category?.Name ?? string.Empty,
            LocationId = c.LocationId,
            LocationName = c.Location?.Name ?? string.Empty,
            EarlyBirdDiscountUntil = c.EarlyBirdDiscountUntil,
            IsPublished = c.isPublished
        }).ToList();
    }
    public async Task<List<ConcertListItemDto>> GetHomeConcertsAsync(CancellationToken ct = default)
    {
        var cacheKey = "home_published_concerts";

        var cached = await _cache.GetStringAsync(cacheKey, ct);

        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<List<ConcertListItemDto>>(cached)!;
        }

        var concerts = await _concerts.GetFilteredAsync(
            includeRefs: true,
            categoryId: null,
            locationId: null,
            dateFrom: null,
            dateTo: null,
            onlyPublished: true,
            ct
        );

        var result = concerts.Select(MapToListItemDto).ToList();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            options,
            ct
        );

        return result;
    }
    public async Task<List<RegionAvailabilityDto>> GetRegionAvailabilityAsync(
    int concertId,
    CancellationToken ct = default)
    {
        var concert = await _concerts.GetByIdAsync(concertId, includeRefs: false, ct);
        if (concert is null)
            throw new ArgumentException("Koncert ne postoji.");

        var regions = await _regionSeating.GetByLocationAsync(concert.LocationId, ct);

        var result = new List<RegionAvailabilityDto>();

        foreach (var region in regions)
        {
            var reservedSeats = await _reservations.GetReservedCountAsync(concertId, region.Id, ct);
            var availableSeats = Math.Max(region.Capacity - reservedSeats, 0);

            result.Add(new RegionAvailabilityDto
            {
                Id = region.Id,
                Name = region.Name,
                Capacity = region.Capacity,
                ReservedSeats = reservedSeats,
                AvailableSeats = availableSeats
            });
        }

        return result;
    }
}