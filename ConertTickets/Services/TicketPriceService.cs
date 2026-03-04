using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.Services;

public class TicketPriceService
{
    private readonly ITicketPriceRepository _prices;
    private readonly IConcertRepository _concerts;
    private readonly IRegionSeatingRepository _regions;
    private readonly ICurrencyRepository _currencies;

    public TicketPriceService(
        ITicketPriceRepository prices,
        IConcertRepository concerts,
        IRegionSeatingRepository regions,
        ICurrencyRepository currencies)
    {
        _prices = prices;
        _concerts = concerts;
        _regions = regions;
        _currencies = currencies;
    }

    public Task<List<TicketPrice>> GetByConcertAsync(int concertId, CancellationToken ct = default)
        => _prices.GetByConcertAsync(concertId, ct);

    public async Task<TicketPrice> UpsertAsync(int concertId, int regionSeatingId, int currencyId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0) throw new ArgumentException("Iznos mora biti veći od 0.");

        var concert = await _concerts.GetByIdAsync(concertId, includeRefs: false, ct);
        if (concert is null) throw new ArgumentException("Koncert ne postoji.");

        var currency = await _currencies.GetByIdAsync(currencyId, ct);
        if (currency is null) throw new ArgumentException("Valuta ne postoji.");

        var regionsForLocation = await _regions.GetByLocationAsync(concert.LocationId, ct);
        var regionExists = regionsForLocation.Any(r => r.Id == regionSeatingId);
        if (!regionExists) throw new ArgumentException("Region sjedenja ne pripada lokaciji ovog koncerta.");

        return await _prices.UpsertAsync(new TicketPrice
        {
            ConcertId = concertId,
            RegionSeatingId = regionSeatingId,
            CurrencyId = currencyId,
            Amount = amount
        }, ct);
    }
}
