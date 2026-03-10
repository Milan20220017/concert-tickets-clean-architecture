using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.Services;

public class LocationService
{
    private readonly ILocationRepository _locations;
    private readonly IConcertRepository _concerts;
    private readonly IRegionSeatingRepository _regions;
    public LocationService(ILocationRepository locations, IConcertRepository concerts, IRegionSeatingRepository regions) =>
        (_locations, _concerts, _regions) = (locations, concerts, regions);

    public Task<List<Location>> GetAllAsync(CancellationToken ct = default)
        => _locations.GetAllAsync(ct);

    public Task<Location?> GetByIdAsync(int id, bool includeRegions, CancellationToken ct = default)
        => _locations.GetByIdAsync(id, includeRegions, ct);

    public async Task<Location> CreateAsync(string name, string address, CancellationToken ct = default)
    {
        name = (name ?? "").Trim();
        address = (address ?? "").Trim();

        if (name.Length < 2) throw new ArgumentException("Naziv lokacije je prekratak.");
        if (address.Length < 3) throw new ArgumentException("Adresa lokacije je prekratka.");

        return await _locations.AddAsync(new Location { Name = name, Address = address }, ct);
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var concerts = await _concerts.GetFilteredAsync(
            includeRefs: false,
            categoryId: null,
            locationId: id,
            dateFrom: null,
            dateTo: null,
            onlyPublished: false,
            ct
        );

        if (concerts.Any())
            throw new ArgumentException("Lokacija se ne može obrisati jer postoje koncerti zakazani na toj lokaciji.");

        var regions = await _regions.GetByLocationAsync(id, ct);
        if (regions.Any())
            throw new ArgumentException("Lokacija se ne može obrisati jer sadrži regione sjedenja.");

        return await _locations.DeleteAsync(id, ct);
    }

}
