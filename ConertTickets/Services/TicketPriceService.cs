using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace ConcertTickets_API.Services;

public class TicketPriceService
{
    private readonly ITicketPriceRepository _prices;
    private readonly IConcertRepository _concerts;
    private readonly IRegionSeatingRepository _regions;
    private readonly ICurrencyRepository _currencies;
    private readonly IDistributedCache _cache;

    public TicketPriceService(
        ITicketPriceRepository prices,
        IConcertRepository concerts,
        IRegionSeatingRepository regions,
        ICurrencyRepository currencies,
        IDistributedCache cache)
    {
        _prices = prices;
        _concerts = concerts;
        _regions = regions;
        _currencies = currencies;
        _cache = cache;
    }

    public Task<List<TicketPrice>> GetByConcertAsync(int concertId, CancellationToken ct = default)
        => _prices.GetByConcertAsync(concertId, ct);

    public async Task<TicketPrice> UpsertAsync(
    int concertId,
    int regionSeatingId,
    int currencyId,
    decimal amount,
    CancellationToken ct = default)
    {
        if (amount <= 0)
            throw new ArgumentException("Iznos mora biti veći od 0.");

        var concert = await _concerts.GetByIdForUpdateAsync(concertId, ct);
        if (concert is null)
            throw new ArgumentException("Koncert ne postoji.");

        var currency = await _currencies.GetByIdAsync(currencyId, ct);
        if (currency is null)
            throw new ArgumentException("Valuta ne postoji.");

        var regionsForLocation = await _regions.GetByLocationAsync(concert.LocationId, ct);
        var regionExists = regionsForLocation.Any(r => r.Id == regionSeatingId);

        if (!regionExists)
            throw new ArgumentException("Region sjedenja ne pripada lokaciji ovog koncerta.");

        var savedPrice = await _prices.UpsertAsync(new TicketPrice
        {
            ConcertId = concertId,
            RegionSeatingId = regionSeatingId,
            CurrencyId = currencyId,
            Amount = amount
        }, ct);

        var eur = await _currencies.GetByCodeAsync("EUR", ct);
        if (eur is null)
            throw new ArgumentException("Bazna valuta EUR nije definisana.");

        var allPricesForConcert = await _prices.GetByConcertAsync(concertId, ct);

        var eurRegionIds = allPricesForConcert
            .Where(p => p.CurrencyId == eur.Id)
            .Select(p => p.RegionSeatingId)
            .Distinct()
            .ToHashSet();

        var allRegionIds = regionsForLocation
            .Select(r => r.Id)
            .ToHashSet();

        concert.isPublished =
            allRegionIds.Count > 0 &&
            allRegionIds.All(id => eurRegionIds.Contains(id));

        await _concerts.SaveAsync(ct);
        await _cache.RemoveAsync("home_published_concerts", ct);

        return savedPrice;
    }
}