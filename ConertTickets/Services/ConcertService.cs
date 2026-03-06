using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.Services;

public class ConcertService
{
    private readonly IConcertRepository _concerts;
    private readonly ILocationRepository _locations;
    private readonly ICategoryRepository _categories;

    public ConcertService(
        IConcertRepository concerts,
        ILocationRepository locations,
        ICategoryRepository categories)
    {
        _concerts = concerts;
        _locations = locations;
        _categories = categories;
    }

    public Task<List<Concert>> GetAllAsync(bool includeRefs, CancellationToken ct = default)
        => _concerts.GetAllAsync(includeRefs, ct);

    public Task<Concert?> GetByIdAsync(int id, bool includeRefs, CancellationToken ct = default)
        => _concerts.GetByIdAsync(id, includeRefs, ct);

    public async Task<Concert> CreateAsync(string name, DateTime date, int categoryId, int locationId, CancellationToken ct = default)
    {
        if (date.Kind == DateTimeKind.Unspecified)
            date = DateTime.SpecifyKind(date, DateTimeKind.Local).ToUniversalTime();
        else
            date = date.ToUniversalTime();
        name = (name ?? "").Trim();
        if (name.Length < 2) throw new ArgumentException("Naziv koncerta je prekratak.");

        
        if (date == default) throw new ArgumentException("Datum nije validan.");

        
        var cat = await _categories.GetByIdAsync(categoryId, ct);
        if (cat is null) throw new ArgumentException("Kategorija ne postoji.");

        var loc = await _locations.GetByIdAsync(locationId, includeRegions: false, ct);
        if (loc is null) throw new ArgumentException("Lokacija ne postoji.");

        var concert = new Concert
        {
            Name = name,
            Date = date,
            CategoryId = categoryId,
            LocationId = locationId
        };

        return await _concerts.AddAsync(concert, ct);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => _concerts.DeleteAsync(id, ct);

    public Task<List<Concert>> GetFilteredAsync(
     bool includeRefs,
     int? categoryId,
     int? locationId,
     DateTime? dateFrom,
     DateTime? dateTo,
     CancellationToken ct = default)
    {
        return _concerts.GetFilteredAsync(includeRefs, categoryId, locationId, dateFrom, dateTo, ct);
    }
}
