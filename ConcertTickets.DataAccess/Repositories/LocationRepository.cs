using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.DataAccess.Context;
using ConcertTickets_API.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcertTickets_API.DataAccess.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _db;
    public LocationRepository(AppDbContext db) => _db = db;

    public async Task<List<Location>> GetAllAsync(CancellationToken ct = default)
        => await _db.Locations.AsNoTracking().OrderBy(l => l.Id).ToListAsync(ct);

    public async Task<Location?> GetByIdAsync(int id, bool includeRegions = false, CancellationToken ct = default)
    {
        var q = _db.Locations.AsNoTracking().AsQueryable();
        if (includeRegions) q = q.Include(l => l.Regions);
        return await q.FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task<Location> AddAsync(Location location, CancellationToken ct = default)
    {
        _db.Locations.Add(location);
        await _db.SaveChangesAsync(ct);
        return location;
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Locations.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (entity is null) return false;

        _db.Locations.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
