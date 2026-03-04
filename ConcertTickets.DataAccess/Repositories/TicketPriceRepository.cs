using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.DataAccess.Context;
using ConcertTickets_API.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcertTickets_API.DataAccess.Repositories;

public class TicketPriceRepository : ITicketPriceRepository
{
    private readonly AppDbContext _db;
    public TicketPriceRepository(AppDbContext db) => _db = db;

    public async Task<List<TicketPrice>> GetByConcertAsync(int concertId, CancellationToken ct = default)
        => await _db.TicketPrices.AsNoTracking()
            .Where(p => p.ConcertId == concertId)
            .Include(p => p.RegionSeating)
            .Include(p => p.Currency)
            .OrderBy(p => p.RegionSeatingId)
            .ToListAsync(ct);

    public async Task<TicketPrice> UpsertAsync(TicketPrice price, CancellationToken ct = default)
    {
        var existing = await _db.TicketPrices.FirstOrDefaultAsync(p =>
            p.ConcertId == price.ConcertId &&
            p.RegionSeatingId == price.RegionSeatingId &&
            p.CurrencyId == price.CurrencyId, ct);

        if (existing is null)
        {
            _db.TicketPrices.Add(price);
        }
        else
        {
            existing.Amount = price.Amount;
        }

        await _db.SaveChangesAsync(ct);
        return existing ?? price;
    }
}
