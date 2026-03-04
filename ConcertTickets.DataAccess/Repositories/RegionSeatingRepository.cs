using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.DataAccess.Context;
using ConcertTickets_API.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcertTickets_API.DataAccess.Repositories;

public class RegionSeatingRepository : IRegionSeatingRepository
{
    private readonly AppDbContext _db;
    public RegionSeatingRepository(AppDbContext db) => _db = db;

    public async Task<List<RegionSeating>> GetByLocationAsync(int locationId, CancellationToken ct = default)
        => await _db.RegionSeatings.AsNoTracking()
            .Where(r => r.LocationId == locationId)
            .OrderBy(r => r.Id)
            .ToListAsync(ct);

    public async Task<RegionSeating> AddAsync(RegionSeating region, CancellationToken ct = default)
    {
        _db.RegionSeatings.Add(region);
        await _db.SaveChangesAsync(ct);
        return region;
    }
    public Task<RegionSeating?> GetByIdAsync(int id, CancellationToken ct = default)
    => _db.RegionSeatings.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
}