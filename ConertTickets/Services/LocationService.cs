using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.Services;

public class LocationService
{
    private readonly ILocationRepository _locations;
    public LocationService(ILocationRepository locations) => _locations = locations;

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
    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    => _locations.DeleteAsync(id, ct);

}
