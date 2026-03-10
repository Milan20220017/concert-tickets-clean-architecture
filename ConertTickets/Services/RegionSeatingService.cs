using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.Services;

public class RegionSeatingService
{
    private readonly IRegionSeatingRepository _regions;
    private readonly ILocationRepository _locations;
    private readonly ITicketPriceRepository _prices;
    private readonly IReservationRepository _reservations;

    public RegionSeatingService(IRegionSeatingRepository regions, ILocationRepository locations, ITicketPriceRepository prices, IReservationRepository reservations )
    {
        _regions = regions;
        _locations = locations;
        _prices = prices;
        _reservations = reservations;
    }

    public Task<List<RegionSeating>> GetByLocationAsync(int locationId, CancellationToken ct = default)
        => _regions.GetByLocationAsync(locationId, ct);

    public async Task<RegionSeating> CreateAsync(int locationId, string name, int capacity, CancellationToken ct = default)
    {
        name = (name ?? "").Trim();
        if (name.Length < 2) throw new ArgumentException("Naziv regiona je prekratak.");
        if (capacity <= 0) throw new ArgumentException("Kapacitet mora biti veći od 0.");

        var loc = await _locations.GetByIdAsync(locationId, includeRegions: false, ct);
        if (loc is null) throw new ArgumentException("Lokacija ne postoji.");

        return await _regions.AddAsync(new RegionSeating
        {
            LocationId = locationId,
            Name = name,
            Capacity = capacity
        }, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var usedInPrices = await _prices.ExistsForRegionAsync(id, ct);
        if (usedInPrices)
            throw new ArgumentException("Region sjedenja se ne može obrisati jer je povezan sa cijenama karata.");

        var usedInReservations = await _reservations.ExistsForRegionAsync(id, ct);
        if (usedInReservations)
            throw new ArgumentException("Region sjedenja se ne može obrisati jer postoje rezervacije za taj region.");

        return await _regions.DeleteAsync(id, ct);
    }
}
