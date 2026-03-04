using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.Services;

public class RegionSeatingService
{
    private readonly IRegionSeatingRepository _regions;
    private readonly ILocationRepository _locations;

    public RegionSeatingService(IRegionSeatingRepository regions, ILocationRepository locations)
    {
        _regions = regions;
        _locations = locations;
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
}
