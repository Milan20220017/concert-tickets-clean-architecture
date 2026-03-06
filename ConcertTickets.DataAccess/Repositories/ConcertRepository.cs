using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.DataAccess.Context;
using ConcertTickets_API.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcertTickets_API.DataAccess.Repositories;

public class ConcertRepository : IConcertRepository
{
    private readonly AppDbContext _db;
    public ConcertRepository(AppDbContext db) => _db = db;

    public async Task<List<Concert>> GetAllAsync(bool includeRefs = false, CancellationToken ct = default)
    {
        var q = _db.Concerts.AsNoTracking().AsQueryable();
        if (includeRefs) q = q.Include(c => c.Category).Include(c => c.Location);
        return await q.OrderBy(c => c.Date).ToListAsync(ct);
    }

    public async Task<Concert?> GetByIdAsync(int id, bool includeRefs = false, CancellationToken ct = default)
    {
        var q = _db.Concerts.AsNoTracking().AsQueryable();
        if (includeRefs) q = q.Include(c => c.Category).Include(c => c.Location);
        return await q.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Concert> AddAsync(Concert concert, CancellationToken ct = default)
    {
        _db.Concerts.Add(concert);
        await _db.SaveChangesAsync(ct);
        return concert;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Concerts.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (entity is null) return false;

        _db.Concerts.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }
    public async Task<List<Concert>> GetFilteredAsync(
        bool includeRefs,
        int? categoryId,
        int? locationId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken ct = default)
    {
        IQueryable<Concert> q = _db.Concerts;

        if (includeRefs)
        {
            q = q
                .Include(c => c.Category)
                .Include(c => c.Location);
        }

        if (categoryId.HasValue)
            q = q.Where(c => c.CategoryId == categoryId.Value);

        if (locationId.HasValue)
            q = q.Where(c => c.LocationId == locationId.Value);

        if (dateFrom.HasValue)
            q = q.Where(c => c.Date >= dateFrom.Value);

        if (dateTo.HasValue)
            q = q.Where(c => c.Date <= dateTo.Value);

        return await q
            .OrderBy(c => c.Date)
            .ToListAsync(ct);
    }
}
